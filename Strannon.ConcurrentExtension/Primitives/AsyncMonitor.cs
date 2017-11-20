using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Primitives
{
    public sealed class AsyncMonitor
    {
        private readonly AsyncAutoResetEvent _are;

        public AsyncMonitor()
        {
            _are = new AsyncAutoResetEvent();
        }

        public void Enter()
        {
            _are.Wait();
        }

        public Task EnterAsync()
        {
            return _are.WaitAsync();
        }

        public Task EnterAsync(TimeSpan timeOut)
        {
            return _are.WaitAsync(timeOut);
        }

        public Task EnterAsync(CancellationToken token)
        {
            return _are.WaitAsync(token);
        }

        public Task EnterAsync(TimeSpan timeOut, CancellationToken token)
        {
            return _are.WaitAsync(timeOut, token);
        }

        public void Exit()
        {
            _are.Set();
        }
    }
}
