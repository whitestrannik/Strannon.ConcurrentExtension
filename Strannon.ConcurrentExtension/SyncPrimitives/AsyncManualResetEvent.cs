using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.SyncPrimitives
{
    public sealed class AsyncManualResetEvent : SynchronizationPrimitive<AsyncAutoResetEvent>
    {
        private volatile AsyncTaskCompletionSource _tcs;

        public AsyncManualResetEvent()
        {
            _tcs = new AsyncTaskCompletionSource();
        }

        public override bool IsSignaled => _tcs.Task.IsCompleted;

        public void Wait()
        {
            WaitAsync().WaitAndUnwrapException();
        }

        public void Wait(TimeSpan timeout)
        {
            WaitAsync(timeout).WaitAndUnwrapException();
        }

        public void Wait(CancellationToken token)
        {
            WaitAsync(token).WaitAndUnwrapException();
        }

        public void Wait(TimeSpan timeout, CancellationToken token)
        {
            WaitAsync(timeout, token).WaitAndUnwrapException();
        }

        public Task WaitAsync()
        {
            return WaitAsync(Consts.EternityTimeSpan, CancellationToken.None);
        }

        public Task WaitAsync(TimeSpan timeout)
        {
            return WaitAsync(timeout, CancellationToken.None);
        }

        public Task WaitAsync(CancellationToken token)
        {
            return WaitAsync(Consts.EternityTimeSpan, token);
        }

        public Task WaitAsync(TimeSpan timeOut, CancellationToken token)
        {
            var tcs = new AsyncTaskCompletionSource();
            _tcs.Task.ContinueWith(t => tcs.TrySetResult());

            return tcs.WaitWithTimeoutAndCancelAsync(timeOut, token);
        }

        public void Set()
        {
            _tcs.TrySetResult();
        }

        public void Reset()
        {
            var tcs = _tcs;

            while (tcs.Task.IsCompleted && Interlocked.CompareExchange(ref _tcs, new AsyncTaskCompletionSource(), tcs) != tcs)
            {
                tcs = _tcs;
            }
        }
    }
}
