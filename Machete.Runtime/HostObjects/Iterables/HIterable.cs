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
        public BFunction MapStringBuiltinFunction { get; private set; }

        public HIterable(IEnvironment environment)
            : base(environment)
        {
            Class = "Iterable";
            Prototype = environment.ObjectPrototype;
            Extensible = true;

            IterateBuiltinFunction = new BFunction(environment, Iterate, new ReadOnlyList<string>("callback", "iterable"));
            FilterStringBuiltinFunction = new BFunction(environment, Filter, new ReadOnlyList<string>("predicate", "iterable"));
            MapStringBuiltinFunction = new BFunction(environment, Map, new ReadOnlyList<string>("mapping", "iterable"));

            new LObject.Builder(this)
            .SetAttributes(false, false, false)
            .AppendDataProperty("iterate", IterateBuiltinFunction)
            .AppendDataProperty("filter", FilterStringBuiltinFunction)
            .AppendDataProperty("map", MapStringBuiltinFunction);
        }

        internal static IDynamic Iterate(IEnvironment environment, IArgs args)
        {
            var iterator = new Iterator(environment, args[1]);
            var callback = args[0] as ICallable;
            if (callback == null)
                throw environment.CreateTypeError("The argument 'callback' must be a callable function.");
            while (iterator.Next())
            {
                var callArgs = environment.CreateArgs(new[] { iterator.Current });
                callback.Call(environment, environment.Undefined, callArgs);
            }
            return environment.Undefined;
        }

        internal static IDynamic Filter(IEnvironment environment, IArgs args)
        {
            if (args.Count < 2)
                throw environment.CreateTypeError("The arguments 'iterable' and 'predicate' are required.");
            var iterable = args[1].ConvertToObject();
            var predicate = args[0].ConvertToObject() as ICallable;
            if (predicate == null)
                throw environment.CreateTypeError("The argument 'predicate' must be a callable function.");
            return new HFilterIterable(environment, iterable, predicate);
        }

        internal static IDynamic Map(IEnvironment environment, IArgs args)
        {
            if (args.Count < 2)
                throw environment.CreateTypeError("The arguments 'iterable' and 'mapping' are required.");
            var iterable = args[1].ConvertToObject();
            var mapping = args[0].ConvertToObject() as ICallable;
            if (mapping == null)
                throw environment.CreateTypeError("The argument 'mapping' must be a callable function.");
            return new HMapIterable(environment, iterable, mapping);
        }
    }
}