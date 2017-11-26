namespace Strannon.ConcurrentExtension.SyncPrimitives
{
    public abstract class SynchronizationPrimitive<T> where T : SynchronizationPrimitive<T>
    {
        private readonly long _id;

        protected SynchronizationPrimitive()
        {
            _id = IdManager<T>.GenerateId();
        }

        public long Id
        {
            get
            {
                return _id;
            }
        }

        public abstract bool IsSignaled { get; }
    }
}
