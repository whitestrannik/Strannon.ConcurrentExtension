using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension
{
    public sealed class AsyncManualResetEvent
    {
        private readonly TimeSpan _eternityTimeSpan;
        private volatile TaskCompletionSource<object> _tcs;

        public AsyncManualResetEvent()
        {
            _eternityTimeSpan = TimeSpan.FromMilliseconds(-1);
            _tcs = TaskHelper.CreateTaskCompletitionSourceWithAsyncContinuation();
        }

        public void Wait()
        {
            WaitAsync().Wait();
        }

        public void Wait(TimeSpan timeout)
        {
            WaitAsync(timeout).Wait();
        }

        public void Wait(CancellationToken token)
        {
            WaitAsync(token).Wait();
        }

        public void Wait(TimeSpan timeout, CancellationToken token)
        {
            WaitAsync(timeout, token).Wait();
        }

        public Task WaitAsync()
        {
            return WaitAsync(_eternityTimeSpan, CancellationToken.None);
        }

        public Task WaitAsync(TimeSpan timeout)
        {
            return WaitAsync(timeout, CancellationToken.None);
        }

        public Task WaitAsync(CancellationToken token)
        {
            return WaitAsync(_eternityTimeSpan, token);
        }

        public Task WaitAsync(TimeSpan timeOut, CancellationToken token)
        {
            var tcs = TaskHelper.CreateTaskCompletitionSourceWithAsyncContinuation();
            _tcs.Task.ContinueWith(t => tcs.TrySetResult(null));

            return tcs.WaitWithTimeoutAndCancelAsync(timeOut, token);
        }

        public void Set()
        {
            _tcs.TrySetResult(null);
        }

        public void Reset()
        {
            var tcs = _tcs;

            while (tcs.Task.IsCompleted && Interlocked.CompareExchange(ref _tcs, TaskHelper.CreateTaskCompletitionSourceWithAsyncContinuation(), tcs) != tcs)
            {
                tcs = _tcs;
            }
        }
    }
}
