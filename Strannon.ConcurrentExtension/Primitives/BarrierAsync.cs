using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Primitives
{
    public sealed class BarrierAsync
    {
        private readonly int _initialClientsCount;
        private int _clientsCount;
        private TaskCompletionSource<object> _tcs;

        public BarrierAsync(int clientsCount)
        {
            if (clientsCount <= 0)
            {
                throw new ArgumentOutOfRangeException("clientsCount can not be less or equal to zero.");
            }

            _initialClientsCount = clientsCount;
            _tcs = TaskHelper.CreateTaskCompletitionSource();
        }

        public Task SignalAndWait()
        {
            var tcs = _tcs;
            var count = Interlocked.Decrement(ref _clientsCount);
            if (count == 0)
            {
                _clientsCount = _initialClientsCount;
                tcs.SetResult(null);
                _tcs = TaskHelper.CreateTaskCompletitionSource();
            }
            else if (count < 0)
            {
                throw new InvalidOperationException("Can not signaled if counter is already less then zero.");
            }

            return tcs.Task;
        }
    }
}
