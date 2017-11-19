using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension
{
    public sealed class ManualResetEventAsync
    {
        private readonly TimeSpan _eternityTimeSpan;
        private volatile TaskCompletionSource<object> _tcs;

        public ManualResetEventAsync()
        {
            _eternityTimeSpan = TimeSpan.FromMilliseconds(-1);
            _tcs = TaskHelper.CreateTaskCompletitionSource();
        }

        public void Wait()
        {
            _tcs.Task.Wait();
        }

        public Task WaitAsync()
        {
            return WaitAsync(_eternityTimeSpan, CancellationToken.None);
        }

        public Task WaitAsync(TimeSpan timeOut)
        {
            return WaitAsync(timeOut, CancellationToken.None);
        }

        public Task WaitAsync(CancellationToken token)
        {
            return WaitAsync(_eternityTimeSpan, token);
        }

        public async Task WaitAsync(TimeSpan timeOut, CancellationToken token)
        {
            var tcs = TaskHelper.CreateTaskCompletitionSource();
            _tcs.Task.ContinueWith(t => tcs.TrySetResult(null));

            using (token.Register(() => tcs.TrySetCanceled()))
            {
                await tcs.Task.WithTimeout(timeOut);
            }
        }

        public void Set()
        {
            _tcs.TrySetResult(null);
        }

        public void Reset()
        {
            var tcs = _tcs;

            while (tcs.Task.IsCompleted && Interlocked.CompareExchange(ref _tcs, TaskHelper.CreateTaskCompletitionSource(), tcs) != tcs)
            {
                tcs = _tcs;
            }
        }
    }
}
