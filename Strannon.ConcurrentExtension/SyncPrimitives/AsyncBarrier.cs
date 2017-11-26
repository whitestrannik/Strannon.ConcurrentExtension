using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.SyncPrimitives
{
    public sealed class AsyncBarrier : SynchronizationPrimitive<AsyncAutoResetEvent>
    {
        private readonly int _initialClientsCount;
        private volatile int _clientsCount;
        private volatile AsyncTaskCompletionSource _tcs;

        public AsyncBarrier(int clientsCount)
        {
            if (clientsCount <= 0)
            {
                throw new ArgumentOutOfRangeException("clientsCount can not be less or equal to zero.");
            }

            _initialClientsCount = _clientsCount = clientsCount;
            _tcs = new AsyncTaskCompletionSource();
        }

        public override bool IsSignaled => false;

        public Task SignalAndWaitAsync()
        {
            return SignalAndWaitAsync(Consts.EternityTimeSpan, CancellationToken.None);
        }

        public Task SignalAndWaitAsync(TimeSpan timeout)
        {
            return SignalAndWaitAsync(timeout, CancellationToken.None);
        }

        public Task SignalAndWaitAsync(CancellationToken token)
        {
            return SignalAndWaitAsync(Consts.EternityTimeSpan, token);
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
                _tcs = new AsyncTaskCompletionSource();
                _clientsCount = _initialClientsCount;
                tcs.SetResult();
                return tcs.Task;
            }
            else
            {
                var cancellableTcs = new AsyncTaskCompletionSource();
                tcs.Task.ContinueWith(t =>cancellableTcs.TrySetResult());
                return cancellableTcs.WaitWithTimeoutAndCancelAsync(timeout, token);
            }
        }
    }
}
