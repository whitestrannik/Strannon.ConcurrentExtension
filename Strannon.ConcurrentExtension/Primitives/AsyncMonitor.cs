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

        public void Enter(TimeSpan timeout)
        {
            _are.Wait(timeout);
        }

        public void Enter(CancellationToken token)
        {
            _are.Wait(token);
        }

        public void Enter(TimeSpan timeout, CancellationToken token)
        {
            _are.Wait(timeout, token);
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
