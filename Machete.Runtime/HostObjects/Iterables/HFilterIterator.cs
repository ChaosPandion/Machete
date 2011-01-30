using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Core;

namespace Machete.Runtime.HostObjects.Iterables
{
    public sealed class HFilterIterator : HIterator
    {
        private readonly IObject _iterator;
        private readonly ICallable _next;
        private readonly ICallable _predicate;
        private IDynamic _current;
        private bool _initialized;
        private bool _complete;
        
        public HFilterIterator(IEnvironment environment, IObject iterator, ICallable next, ICallable predicate)
            : base(environment)
        {
            _iterator = iterator;
            _next = next;
            _predicate = predicate;
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
            while (_next.Call(environment, _iterator, environment.EmptyArgs).ConvertToBoolean().BaseValue)
            {
                _current = _iterator.Get("current");
                var callArgs = environment.CreateArgs(new[] { _current });
                if (_predicate.Call(environment, environment.Undefined, callArgs).ConvertToBoolean().BaseValue)
                    return environment.True;
            }
            _complete = true;
            return environment.False;
        }
    }
}