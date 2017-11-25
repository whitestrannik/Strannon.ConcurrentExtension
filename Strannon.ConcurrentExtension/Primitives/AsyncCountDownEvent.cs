using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Primitives
{
    public sealed class AsyncCountDownEvent : SynchronizationPrimitive<AsyncAutoResetEvent>
    {
        private readonly int _initialCountDown;
        private readonly AsyncManualResetEvent _event;
        private int _countDown;

        public AsyncCountDownEvent(int countDown)
        {
            if (countDown <= 0)
            {
                throw new ArgumentOutOfRangeException("countDown can not be less or equal to zero.");
            }

            _initialCountDown = _countDown = countDown;
            _event = new AsyncManualResetEvent();
        }

        public override bool IsSignaled => false;

        public void Wait()
        {
            _event.Wait();
        }

        public void Wait(TimeSpan timeOut)
        {
            _event.Wait(timeOut);
        }

        public void Wait(CancellationToken token)
        {
            _event.Wait(token);
        }

        public void Wait(TimeSpan timeOut, CancellationToken token)
        {
            _event.Wait(timeOut, token);
        }

        public Task WaitAsync()
        {
            return _event.WaitAsync();
        }

        public Task WaitAsync(TimeSpan timeOut)
        {
            return _event.WaitAsync(timeOut);
        }

        public Task WaitAsync(CancellationToken token)
        {
            return _event.WaitAsync(token);
        }

        public Task WaitAsync(TimeSpan timeOut, CancellationToken token)
        {
            return _event.WaitAsync(timeOut, token);
        }

        public void Signal()
        {
            var countDown = Interlocked.Decrement(ref _countDown);
            if (countDown < 0)
            {
                throw new InvalidOperationException("Can not signaled if counter is already less then zero.");
            }
            else if (countDown == 0)
            {
                _countDown = _initialCountDown;
                _event.Set();
            }
        }

        public Task SignalAndWait()
        {
            Signal();
            return WaitAsync();
        }
    }
}
