using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Core;
using Machete.Runtime.NativeObjects.BuiltinObjects;

namespace Machete.Runtime.HostObjects.Iterables
{
    public sealed class HFilterIterable : HIterableBase
    {
        private readonly IObject _iterable;
        private readonly ICallable _predicate;
        
        public HFilterIterable(IEnvironment environment, IObject iterable, ICallable predicate)
            : base(environment)
        {
            _iterable = iterable;
            _predicate = predicate;
        }

        public override IDynamic CreateIterator(IEnvironment environment, IArgs args)
        {
            var iterator = new Iterator(environment, _iterable);
            return new HFilterIterator(environment, iterator, _predicate);
        }
    }
}
