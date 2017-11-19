using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Primitives
{
    public sealed class AutoResetEventAsync
    {
        private readonly TimeSpan _eternityTimeSpan;
        private Queue<TaskCompletionSource<object>> _waitedClients;
        private readonly object _lock;
        private bool _isSignalState;

        public AutoResetEventAsync()
        {
            _eternityTimeSpan = TimeSpan.FromMilliseconds(-1);
            _lock = new object();
            _waitedClients = new Queue<TaskCompletionSource<object>>();
        }

        public void Wait()
        {
            var tcs = TaskHelper.CreateTaskCompletitionSource();

            lock (_lock)
            {
                _waitedClients.Enqueue(tcs);
                if (_isSignalState && _waitedClients.Count > 0)
                { 
                    _waitedClients.Dequeue().SetResult(null);
                    _isSignalState = false;
                }
            }

            tcs.Task.Wait();
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

            lock (_lock)
            {
                _waitedClients.Enqueue(tcs);
                if (_isSignalState && _waitedClients.Count > 0)
                {
                    _waitedClients.Dequeue().SetResult(null);
                    _isSignalState = false;
                }
            }

            using (token.Register(() => tcs.TrySetCanceled()))
            {
                await tcs.Task.WithTimeout(timeOut);
            }
        }

        public void Set()
        {
            lock(_lock)
            {
                if (!_isSignalState)
                {
                    if (_waitedClients.Count > 0)
                    {
                        _waitedClients.Dequeue().SetResult(null);
                    }
                    else
                    {
                        _isSignalState = true;
                    }
                }
            }
        }
    }
}
