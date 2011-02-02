using Machete.Core;

namespace Machete.Runtime.HostObjects.Iterables
{
    public sealed class HArrayIterator : HIteratorBase
    {
        private readonly IObject _array;
        private readonly uint _length;
        private uint _index;
        private bool _initialized;
        private bool _complete;

        public HArrayIterator(IEnvironment environment, IObject array)
            : base(environment)
        {
            _array = array;
            _length = (uint)array.Get("length").ConvertToUInt32().BaseValue;
        }

        public override IDynamic Current(IEnvironment environment, IArgs args)
        {
            if (!_initialized)
                throw environment.CreateTypeError("");
            return _array.Get(_index.ToString());
        }

        public override IDynamic Next(IEnvironment environment, IArgs args)
        {
            if (_complete)
                return environment.False;
            if (!_initialized)
            {
                _initialized = true;
            }
            else
            {
                _index++;
            }
            do
            {
                if (_array.HasProperty(_index.ToString()))
                {
                    return environment.True;
                }
            }
            while (++_index < _length);
            _complete = true;
            return environment.False;
        }
    }
}