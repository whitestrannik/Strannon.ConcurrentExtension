using Microsoft.VisualStudio.TestTools.UnitTesting;
using Strannon.ConcurrentExtension.Primitives;
using Strannon.ConcurrentTestExtension;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Test
{
    [TestClass]
    public class AsyncManualResetEventFixture
    {
        [TestMethod]
        public void WhenEventIsSignaled_ItShouldNotMakeClientsWaitedAsync()
        {
            var sut = new AsyncManualResetEvent();

            sut.Set();

            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.WaitAsync());
        }

        [TestMethod]
        public void WhenEventIsNonSignaled_ItShouldMakeClientsWaitedAsyncWithCancel()
        {
            var sut = new AsyncManualResetEvent();

            sut.Reset();
            var ct = new CancellationTokenSource();
            var task = sut.WaitAsync(ct.Token);

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(task);

            ct.Cancel();

            ConcurrentAssert.EnsureThatTaskIsCompleted(task);
            Assert.ThrowsExceptionAsync<TaskCanceledException>(() => task);
        }

        [TestMethod]
        public void WhenEventIsNonSignaled_ItShouldMakeClientsWaitedAsyncWithTimeout()
        {
            var sut = new AsyncManualResetEvent();
            sut.Reset();

            var task = sut.WaitAsync(TimeSpan.FromMilliseconds(1000));

            Assert.ThrowsExceptionAsync<TimeoutException>(() => task);
        }

        [TestMethod]
        public void WhenEventIsNonSignaled_ItShouldMakeClientsWaitedAsyncWithTimeoutAndCancel()
        {
            var sut = new AsyncManualResetEvent();
            sut.Reset();
            var ct = new CancellationTokenSource();
            var task = sut.WaitAsync(TimeSpan.FromMilliseconds(1000), ct.Token);

            ConcurrentAssert.EnsureThatTaskIsNotCompletedIn(task, TimeSpan.FromMilliseconds(500));

            ct.Cancel();

            ConcurrentAssert.EnsureThatTaskIsCompleted(task);
            Assert.ThrowsExceptionAsync<TaskCanceledException>(() => task);
        }

        [TestMethod]
        public void WhenEventIsDefault_ItShouldBeNonSignaled()
        {
            var sut = new AsyncManualResetEvent();

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatActionIsNeverCompleted(sut.Wait);
            ConcurrentAssert.EnsureThatActionIsNeverCompleted(sut.Wait);
        }

        [TestMethod]
        public void WhenEventIsSignaledManyTimes_ItShouldNotMakeClientsWaitedAsync()
        {
            var sut = new AsyncManualResetEvent();

            sut.Set();
            sut.Set();

            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatActionIsCompleted(sut.Wait);
            ConcurrentAssert.EnsureThatActionIsCompleted(sut.Wait);

            sut.Set();
            sut.Set();

            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatActionIsCompleted(sut.Wait);
            ConcurrentAssert.EnsureThatActionIsCompleted(sut.Wait);
        }

        [TestMethod]
        public void WhenEventIsReset_ItShouldMakeClientsWaitedAsync()
        {
            var sut = new AsyncManualResetEvent();

            sut.Set();

            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.WaitAsync());

            sut.Reset();

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(sut.WaitAsync());
        }

        [TestMethod]
        public void WhenEventIsSignaled_ItShouldNotMakeClientsWaited()
        {
            var sut = new AsyncManualResetEvent();

            sut.Set();

            ConcurrentAssert.EnsureThatActionIsCompleted(sut.Wait);
            ConcurrentAssert.EnsureThatActionIsCompleted(sut.Wait);
        }

        [TestMethod]
        public void WhenEventIsNonSignaled_ItShouldMakeClientsWaitedWithCancel()
        {
            var sut = new AsyncManualResetEvent();
            sut.Reset();
            var ct = new CancellationTokenSource();

            ct.Cancel();

            Assert.ThrowsException<TaskCanceledException>(() => sut.Wait(ct.Token));
        }


        [TestMethod]
        public void WhenEventIsReset_ItShouldMakeClientsWaited()
        {
            var sut = new AsyncManualResetEvent();

            sut.Set();

            ConcurrentAssert.EnsureThatActionIsCompleted(sut.Wait);
            ConcurrentAssert.EnsureThatActionIsCompleted(sut.Wait);

            sut.Reset();

            ConcurrentAssert.EnsureThatActionIsNeverCompleted(sut.Wait);
            ConcurrentAssert.EnsureThatActionIsNeverCompleted(sut.Wait);
        }
    }
}
