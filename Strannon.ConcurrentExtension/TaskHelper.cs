using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension
{
    public static class TaskHelper
    {
        public static TaskCompletionSource<T> CreateTaskCompletitionSource<T>()
        {
            return new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public static TaskCompletionSource<object> CreateTaskCompletitionSource()
        {
            return new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
