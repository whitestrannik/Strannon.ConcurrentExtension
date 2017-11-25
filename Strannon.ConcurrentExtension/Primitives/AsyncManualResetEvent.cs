﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Primitives
{
    public sealed class AsyncManualResetEvent : SynchronizationPrimitive<AsyncAutoResetEvent>
    {
        private volatile TaskCompletionSource<object> _tcs;

        public AsyncManualResetEvent()
        {
            _tcs = TaskHelper.CreateTaskCompletitionSourceWithAsyncContinuation();
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
