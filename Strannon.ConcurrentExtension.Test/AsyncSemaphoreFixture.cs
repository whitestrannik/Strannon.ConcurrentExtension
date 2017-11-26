using Microsoft.VisualStudio.TestTools.UnitTesting;
using Strannon.ConcurrentExtension.SyncPrimitives;
using Strannon.ConcurrentTestExtension;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Test
{
    [TestClass]
    public sealed class AsyncSemaphoreFixture
    {
        [TestMethod]
        public void WhenClientsCountIsLessThen_SemaphoreShouldNotMakeClientsWaitedAsync()
        {
            var sut = new AsyncSemaphore(3);

            ConcurrentAssert.EnsureThatTaskIsCompleted(Task.WhenAll(sut.WaitAsync(), sut.WaitAsync()));
        }

        [TestMethod]
        public void WhenClientsCountIsGreaterThen_SemaphoreShouldMakeClientsWaitedAsync()
        {
            var sut = new AsyncSemaphore(2);
            Task.WhenAll(sut.WaitAsync(), sut.WaitAsync()).Wait();

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(sut.WaitAsync());
        }

        [TestMethod]
        public void WhenClientsCountIsGreaterThen_SemaphoreShouldMakeClientsWaitedAsyncWithCancel()
        {
            var sut = new AsyncSemaphore(2);
            Task.WhenAll(sut.WaitAsync(), sut.WaitAsync()).Wait();

            var cts = new CancellationTokenSource();
            var task = sut.WaitAsync(cts.Token);

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(task);
            cts.Cancel();

            ConcurrentAssert.EnsureThatTaskIsCompleted(task);
            Assert.ThrowsExceptionAsync<TaskCanceledException>(() => task);
        }

        [TestMethod]
        public void WhenClientsCountIsGreaterThen_SemaphoreShouldMakeClientsWaitedAsyncWithTimeout()
        {
            var sut = new AsyncSemaphore(2);
            Task.WhenAll(sut.WaitAsync(), sut.WaitAsync()).Wait();

            var task = sut.WaitAsync(TimeSpan.FromMilliseconds(300));
            Assert.ThrowsExceptionAsync<TimeoutException>(() => task);
        }

        [TestMethod]
        public void WhenClientRelease_SemaphoreShouldNotMakeOneMoreClientWaitedAsync()
        {
            var sut = new AsyncSemaphore(1);
            sut.Wait();
            var task = sut.WaitAsync();
            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(task);

            sut.Release();

            ConcurrentAssert.EnsureThatTaskIsCompleted(task);
        }
    }
}
