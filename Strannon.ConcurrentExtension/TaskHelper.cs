using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension
{
    public static class TaskHelper
    {
        public static TaskCompletionSource<T> CreateTaskCompletitionSourceWithAsyncContinuation<T>()
        {
            return new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public static TaskCompletionSource<object> CreateTaskCompletitionSource()
        {
            return new TaskCompletionSource<object>();
        }

        public static TaskCompletionSource<object> CreateTaskCompletitionSourceWithAsyncContinuation()
        {
            return new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
