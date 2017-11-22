using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Primitives
{
    public sealed class AsyncBarrier
    {
        private readonly int _initialClientsCount;
        private volatile int _clientsCount;
        private volatile TaskCompletionSource<object> _tcs;
        private readonly TimeSpan _eternityTimeSpan;

        public AsyncBarrier(int clientsCount)
        {
            if (clientsCount <= 0)
            {
                throw new ArgumentOutOfRangeException("clientsCount can not be less or equal to zero.");
            }

            _initialClientsCount = _clientsCount = clientsCount;
            _tcs = TaskHelper.CreateTaskCompletitionSource();
            _eternityTimeSpan = TimeSpan.FromMilliseconds(-1);
        }

        public Task SignalAndWaitAsync()
        {
            return SignalAndWaitAsync(_eternityTimeSpan, CancellationToken.None);
        }

        public Task SignalAndWaitAsync(TimeSpan timeout)
        {
            return SignalAndWaitAsync(timeout, CancellationToken.None);
        }

        public Task SignalAndWaitAsync(CancellationToken token)
        {
            return SignalAndWaitAsync(_eternityTimeSpan, token);
        }

        public Task SignalAndWaitAsync(TimeSpan timeout, CancellationToken token)
        {
            var tcs = _tcs;
            var count = Interlocked.Decrement(ref _clientsCount);

            if (count < 0)
            {
                throw new InvalidOperationException("Can not signaled if counter is already less then zero.");
            }
            else if (count == 0)
            {
                _tcs = TaskHelper.CreateTaskCompletitionSourceWithAsyncContinuation();
                _clientsCount = _initialClientsCount;
                tcs.SetResult(null);
                return tcs.Task;
            }
            else
            {
                var cancellableTcs = TaskHelper.CreateTaskCompletitionSourceWithAsyncContinuation();
                tcs.Task.ContinueWith(t =>cancellableTcs.TrySetResult(null));
                return cancellableTcs.WaitWithTimeoutAndCancelAsync(timeout, token);
            }
        }
    }
}
