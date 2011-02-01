using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Core;
using Machete.Runtime.NativeObjects.BuiltinObjects;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.HostObjects.Iterables
{
    public sealed class HIterable : LObject
    {
        public BFunction IterateBuiltinFunction { get; private set; }
        public BFunction FilterStringBuiltinFunction { get; private set; }

        public HIterable(IEnvironment environment)
            : base(environment)
        {
            Class = "Iterable";
            Prototype = environment.ObjectPrototype;
            Extensible = true;

            IterateBuiltinFunction = new BFunction(environment, Iterate, new ReadOnlyList<string>("iterable", "callback"));
            FilterStringBuiltinFunction = new BFunction(environment, Filter, new ReadOnlyList<string>("iterable", "predicate"));

            new LObject.Builder(this)
            .SetAttributes(false, false, false)
            .AppendDataProperty("iterate", IterateBuiltinFunction)
            .AppendDataProperty("filter", FilterStringBuiltinFunction);
        }

        internal static IDynamic Iterate(IEnvironment environment, IArgs args)
        {
            var iterable = args[0].ConvertToObject();
            var createIterator = iterable.Get("createIterator") as ICallable;
            if (createIterator == null)
                throw environment.CreateTypeError("");
            var iterator = createIterator.Call(environment, iterable, environment.EmptyArgs).ConvertToObject();
            if (!iterator.HasProperty("current"))
                throw environment.CreateTypeError("");
            var next = iterator.Get("next") as ICallable;
            if (next == null)
                throw environment.CreateTypeError("");
            var callback = args[1] as ICallable;
            if (callback == null)
                throw environment.CreateTypeError("");
            while (next.Call(environment, iterator, environment.EmptyArgs).ConvertToBoolean().BaseValue)
            {
                var callArgs = environment.CreateArgs(new[] { iterator.Get("current") });
                callback.Call(environment, environment.Undefined, callArgs);
            }
            return environment.Undefined;
        }

        internal static IDynamic Filter(IEnvironment environment, IArgs args)
        {
            var iterable = args[0].ConvertToObject();
            var predicate = args[1].ConvertToObject() as ICallable;
            if (predicate == null)
                throw environment.CreateTypeError("");
            return new HFilterIterable(environment, iterable, predicate);
        }
    }
}