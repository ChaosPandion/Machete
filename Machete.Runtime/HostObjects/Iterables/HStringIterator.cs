using Machete.Core;

namespace Machete.Runtime.HostObjects.Iterables
{
    public sealed class HStringIterator : HIteratorBase
    {
        private readonly IString _value;
        private readonly int _length;
        private int _index;
        private bool _initialized;
        private bool _complete;

        public HStringIterator(IEnvironment environment, IString value)
            : base(environment)
        {
            _value = value;
            _length = value.BaseValue.Length;
            _index = -1;
        }

        public override IDynamic Current(IEnvironment environment, IArgs args)
        {
            if (!_initialized)
                throw environment.CreateTypeError("");
            return environment.CreateString(_value.BaseValue.Substring(_index, 1));
        }

        public override IDynamic Next(IEnvironment environment, IArgs args)
        {
            if (_complete)
                return environment.False;
            _initialized = true;
            if (++_index < _length)
                return environment.True;
            _complete = true;
            return environment.False;
        }
    }
}