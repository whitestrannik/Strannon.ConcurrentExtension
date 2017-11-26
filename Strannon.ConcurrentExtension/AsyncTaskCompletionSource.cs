using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension
{
    public sealed class AsyncTaskCompletionSource
    {
        private readonly TaskCompletionSource<bool> _tcs;

        public AsyncTaskCompletionSource()
        {
            _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public Task Task
        {
            get
            {
                return _tcs.Task;
            }
        }

        public void SetResult()
        {
            _tcs.SetResult(true);
        }

        public void SetCanceled()
        {
            _tcs.SetCanceled();
        }

        public void SetException(Exception ex)
        {
            _tcs.SetException(ex);
        }

        public void SetException(IEnumerable<Exception> ex)
        {
            _tcs.SetException(ex);
        }

        public bool TrySetResult()
        {
            return _tcs.TrySetResult(true);
        }

        public bool TrySetCanceled()
        {
            return _tcs.TrySetCanceled();
        }

        public bool TrySetCanceled(CancellationToken token)
        {
            return _tcs.TrySetCanceled(token);
        }

        public bool TrySetException(Exception ex)
        {
            return _tcs.TrySetException(ex);
        }

        public bool TrySetException(IEnumerable<Exception> ex)
        {
            return _tcs.TrySetException(ex);
        }
    }

    public sealed class AsyncTaskCompletionSource<T>
    {
        private readonly TaskCompletionSource<T> _tcs;

        public AsyncTaskCompletionSource()
        {
            _tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public Task<T> Task
        {
            get
            {
                return _tcs.Task;
            }
        }

        public void SetResult(T result)
        {
            _tcs.SetResult(result);
        }

        public void SetCanceled()
        {
            _tcs.SetCanceled();
        }

        public void SetException(Exception ex)
        {
            _tcs.SetException(ex);
        }

        public void SetException(IEnumerable<Exception> ex)
        {
            _tcs.SetException(ex);
        }

        public bool TrySetResult(T result)
        {
            return _tcs.TrySetResult(result);
        }

        public bool TrySetCanceled()
        {
            return _tcs.TrySetCanceled();
        }

        public bool TrySetCanceled(CancellationToken token)
        {
            return _tcs.TrySetCanceled(token);
        }

        public bool TrySetException(Exception ex)
        {
            return _tcs.TrySetException(ex);
        }

        public bool TrySetException(IEnumerable<Exception> ex)
        {
            return _tcs.TrySetException(ex);
        }
    }
}
