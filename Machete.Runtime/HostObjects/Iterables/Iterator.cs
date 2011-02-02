using Machete.Core;

namespace Machete.Runtime.HostObjects.Iterables
{
    public sealed class Iterator
    {
        private readonly IEnvironment _environment;
        private readonly IObject _iterator;
        private readonly ICallable _next;

        public Iterator(IEnvironment environment, IDynamic iterable)
        {
            _environment = environment;

            var o = iterable.ConvertToObject();
            var createIterator = o.Get("createIterator") as ICallable;
            if (createIterator == null)
                throw environment.CreateTypeError("The object supplied does not contain a callable property named 'createIterator'.");
            _iterator = createIterator.Call(environment, iterable, environment.EmptyArgs).ConvertToObject();
            if (!_iterator.HasProperty("current"))
                throw environment.CreateTypeError("The object returned from the iterable supplied does not have a property named 'current'.");
            _next = _iterator.Get("next") as ICallable;
            if (_next == null)
                throw environment.CreateTypeError("The object returned from the iterable supplied does not have a callable property named 'next'.");
        }

        public IDynamic Current
        {
            get { return _iterator.Get("current"); }
        }

        public bool Next()
        {
            return _next.Call(_environment, _environment.Undefined, _environment.EmptyArgs).ConvertToBoolean().BaseValue;
        }
    }
}