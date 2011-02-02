using Machete.Core;

namespace Machete.Runtime.HostObjects.Iterables
{
    public sealed class HMapIterable : HIterableBase
    {
        private readonly IObject _iterable;
        private readonly ICallable _mapping;

        public HMapIterable(IEnvironment environment, IObject iterable, ICallable mapping)
            : base(environment)
        {
            _iterable = iterable;
            _mapping = mapping;
        }

        public override IDynamic CreateIterator(IEnvironment environment, IArgs args)
        {
            var iterator = new Iterator(environment, _iterable);
            return new HMapIterator(environment, iterator, _mapping);
        }
    }
}
