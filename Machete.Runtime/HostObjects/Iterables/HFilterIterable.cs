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
            var createIterator = _iterable.Get("createIterator") as ICallable;
            if (createIterator == null)
                throw environment.CreateTypeError("");
            var iterator = createIterator.Call(environment, _iterable, environment.EmptyArgs).ConvertToObject();
            if (!iterator.HasProperty("current"))
                throw environment.CreateTypeError("");
            var next = iterator.Get("next") as ICallable;
            if (next == null)
                throw environment.CreateTypeError("");
            return new HFilterIterator(environment, iterator, next, _predicate);
        }
    }
}
