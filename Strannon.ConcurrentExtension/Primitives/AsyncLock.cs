using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Primitives
{
    public sealed class AsyncLock
    {
        private readonly AsyncMonitor _am;
        private readonly TimeSpan _eternityTimeSpan;

        public AsyncLock()
        {
            _am = new AsyncMonitor();
            _eternityTimeSpan = TimeSpan.FromMilliseconds(-1);
        }

        public IDisposable Lock()
        {
            _am.Enter();
            return new Disposable(_am);
        }

        public IDisposable Lock(TimeSpan timeout)
        {
            _am.Enter(timeout);
            return new Disposable(_am);
        }

        public IDisposable Lock(CancellationToken token)
        {
            _am.Enter(token);
            return new Disposable(_am);
        }

        public IDisposable Lock(TimeSpan timeout, CancellationToken token)
        {
            _am.Enter(timeout, token);
            return new Disposable(_am);
        }

        public Task<IDisposable> LockAsync()
        {
            return LockAsync(_eternityTimeSpan, CancellationToken.None);
        }

        public Task<IDisposable> LockAsync(TimeSpan timeout)
        {
            return LockAsync(timeout, CancellationToken.None);
        }

        public Task<IDisposable> LockAsync(CancellationToken token)
        {
            return LockAsync(_eternityTimeSpan, token);
        }

            public Task<IDisposable> LockAsync(TimeSpan timeout, CancellationToken token)
        {
            var tcs = TaskHelper.CreateTaskCompletitionSourceWithAsyncContinuation<IDisposable>();

            _am.EnterAsync(timeout, token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    tcs.TrySetCanceled(token);
                }
                else if (t.IsFaulted)
                {
                    tcs.TrySetException(t.Exception);
                }
                else
                {
                    tcs.SetResult(new Disposable(_am));
                }
            });

            return tcs.Task;
        }

        private struct Disposable : IDisposable
        {
            private readonly AsyncMonitor _am;

            public Disposable(AsyncMonitor monitor)
            {
                _am = monitor;
            }

            public void Dispose()
            {
                _am.Exit();
            }
        }
    }
}
