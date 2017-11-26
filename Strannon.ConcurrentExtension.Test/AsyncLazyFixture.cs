using Microsoft.VisualStudio.TestTools.UnitTesting;
using Strannon.ConcurrentTestExtension;
using System.Threading;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension.Test
{
    [TestClass]
    public sealed class AsyncLazyFixture
    {
        [TestMethod]
        public void WhenLazyValueIsNotReady_WeShouldWait()
        {
            var mre = new ManualResetEvent(false);
            var sut = new AsyncLazy<int>(
                () =>
                {
                    mre.WaitOne();
                    return 42;
                });

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(sut.Value);
        }

        [TestMethod]
        public void WhenLazyValueIsReady_WeShouldGetIt()
        {
            var mre = new ManualResetEvent(false);
            var sut = new AsyncLazy<int>(
                () =>
                {
                    mre.WaitOne();
                    return 42;
                });

            mre.Set();
            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.Value);
        }

        [TestMethod]
        public void WhenLazyValueCreatedWithTaskFactoryIsNotReady_WeShouldWait()
        {
            var tcs = new TaskCompletionSource<int>();
            var sut = new AsyncLazy<int>(() => tcs.Task);

            ConcurrentAssert.EnsureThatTaskIsNeverCompleted(sut.Value);
        }

        [TestMethod]
        public void WhenLazyValueCreatedWithTaskFactoryIsReady_WeShouldGetIt()
        {
            var tcs = new TaskCompletionSource<int>();
            var sut = new AsyncLazy<int>(() => tcs.Task);

            tcs.SetResult(42);
            ConcurrentAssert.EnsureThatTaskIsCompleted(sut.Value);
        }
    }
}
