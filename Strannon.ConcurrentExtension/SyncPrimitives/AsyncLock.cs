using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.SyncPrimitives
{
    public sealed class AsyncLock : SynchronizationPrimitive<AsyncAutoResetEvent>
    {
        private readonly AsyncMonitor _am;

        public AsyncLock()
        {
            _am = new AsyncMonitor();
        }

        public override bool IsSignaled => _am.IsSignaled;

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
            return LockAsync(Consts.EternityTimeSpan, CancellationToken.None);
        }

        public Task<IDisposable> LockAsync(TimeSpan timeout)
        {
            return LockAsync(timeout, CancellationToken.None);
        }

        public Task<IDisposable> LockAsync(CancellationToken token)
        {
            return LockAsync(Consts.EternityTimeSpan, token);
        }

        public Task<IDisposable> LockAsync(TimeSpan timeout, CancellationToken token)
        {
            var tcs = new AsyncTaskCompletionSource<IDisposable>();

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
