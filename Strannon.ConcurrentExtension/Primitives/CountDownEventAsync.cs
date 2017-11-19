using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Primitives
{
    public sealed class CountDownEventAsync
    {
        private int _initialCountDown;
        private readonly ManualResetEventAsync _event;

        public CountDownEventAsync(int countDown)
        {
            if (countDown <= 0)
            {
                throw new ArgumentOutOfRangeException("countDown can not be less or equal to zero.");
            }

            _initialCountDown = countDown;
            _event = new ManualResetEventAsync();
        }

        public void Wait()
        {
            _event.Wait();
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
            var countDown = Interlocked.Decrement(ref _initialCountDown);
            if (countDown < 0)
            {
                throw new InvalidOperationException("Can not signaled if counter is already less then zero.");
            }
            else if (countDown == 0)
            {
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
