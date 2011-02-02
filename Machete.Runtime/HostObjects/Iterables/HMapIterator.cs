using Machete.Core;

namespace Machete.Runtime.HostObjects.Iterables
{
    public sealed class HMapIterator : HIteratorBase
    {
        private readonly Iterator _iterator;
        private readonly ICallable _mapping;
        private IDynamic _current;
        private bool _initialized;
        private bool _complete;

        public HMapIterator(IEnvironment environment, Iterator iterator, ICallable mapping)
            : base(environment)
        {
            _iterator = iterator;
            _mapping = mapping;
        }

        public override IDynamic Current(IEnvironment environment, IArgs args)
        {
            if (!_initialized)
                throw environment.CreateTypeError("");
            return _current;
        }

        public override IDynamic Next(IEnvironment environment, IArgs args)
        {
            if (_complete)
                return environment.False;
            _initialized = true;
            while (_iterator.Next())
            {
                var callArgs = environment.CreateArgs(new[] { _iterator.Current });
                _current = _mapping.Call(environment, environment.Undefined, callArgs);
                return environment.True;
            }
            _complete = true;
            return environment.False;
        }
    }
}
