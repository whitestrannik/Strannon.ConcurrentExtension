using System;
using System.Threading.Tasks;

namespace Strannon.ConcurrentExtension
{
    public sealed class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> factory) : base(() => Task.Run(factory))
        { }

        public AsyncLazy(Func<Task<T>> factory) : base(() => Task.Run(factory))
        { }
    }
}
