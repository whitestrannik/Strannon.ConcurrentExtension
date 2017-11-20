using Microsoft.VisualStudio.TestTools.UnitTesting;
using Strannon.ConcurrentTestExtension;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Test
{
    [TestClass]
    public class ManualResetEventAsyncFixture
    {
        [TestMethod]
        public void WhenEventIsSet_ItShouldNotMakeClientsWaitedAsync()
        {
            var sut = new AsyncManualResetEvent();

            sut.Set();

            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.WaitAsync());
        }

        [TestMethod]
        public void WhenEventIsReset_ItShouldMakeClientsWaitedAsyncWithCancel()
        {
            var sut = new AsyncManualResetEvent();

            sut.Reset();
            var ct = new CancellationTokenSource();
            var task = sut.WaitAsync(ct.Token);

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(task);

            ct.Cancel();

            ConcurrentAssert.EnsureThatTaskIsCompleted(task);
        }

        [TestMethod]
        public void WhenEventIsDefault_ItShouldMakeClientsWaitedAsync()
        {
            var sut = new AsyncManualResetEvent();

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(sut.WaitAsync());
        }

        [TestMethod]
        public void WhenEventIsSetManyTimes_ItShouldNotMakeClientsWaitedAsync()
        {
            var sut = new AsyncManualResetEvent();

            sut.Set();
            sut.Set();
            sut.Set();

            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.WaitAsync());
        }

        [TestMethod]
        public void WhenEventIsReset_ItShouldMakeClientsWaitedAsync()
        {
            var sut = new AsyncManualResetEvent();

            sut.Set();
            sut.Reset();

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(sut.WaitAsync());
            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(sut.WaitAsync());
        }

        [TestMethod]
        public void WhenEventIsSet_ItShouldNotMakeClientsWaited()
        {
            var sut = new AsyncManualResetEvent();

            sut.Set();

            ConcurrentAssert.EnsureThatActionIsCompleted(sut.Wait);
            ConcurrentAssert.EnsureThatActionIsCompleted(sut.Wait);
        }

        [TestMethod]
        public void WhenEventIsDefault_ItShouldMakeClientsWaited()
        {
            var sut = new AsyncManualResetEvent();

            ConcurrentAssert.EnsureThatActionIsNeverCompleted(sut.Wait);
            ConcurrentAssert.EnsureThatActionIsNeverCompleted(sut.Wait);
        }

        [TestMethod]
        public void WhenEventIsSetManyTimes_ItShouldMakeClientsWaited()
        {
            var sut = new AsyncManualResetEvent();

            sut.Set();
            sut.Set();
            sut.Set();

            ConcurrentAssert.EnsureThatActionIsCompleted(sut.Wait);
            ConcurrentAssert.EnsureThatActionIsCompleted(sut.Wait);
        }

        [TestMethod]
        public void WhenEventIsReset_ItShouldMakeClientsWaited()
        {
            var sut = new AsyncManualResetEvent();

            sut.Set();
            sut.Reset();

            ConcurrentAssert.EnsureThatActionIsNeverCompleted(sut.Wait);
            ConcurrentAssert.EnsureThatActionIsNeverCompleted(sut.Wait);
        }
    }
}
