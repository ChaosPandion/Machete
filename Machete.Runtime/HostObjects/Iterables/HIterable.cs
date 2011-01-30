using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Core;
using Machete.Core.Generators;
using Machete.Runtime.NativeObjects.BuiltinObjects;

namespace Machete.Runtime.HostObjects.Iterables
{
    public abstract class HIterable : LObject
    {
        public BFunction CreateIteratorBuiltinFunction { get; private set; }

        public HIterable(IEnvironment environment)
            : base(environment)
        {
            Class = "Iterable";
            Extensible = true;
            Prototype = environment.ObjectPrototype;

            CreateIteratorBuiltinFunction = new BFunction(environment, CreateIterator, ReadOnlyList<string>.Empty);

            new LObject.Builder(this)
            .SetAttributes(false, false, false)
            .AppendDataProperty("createIterator", CreateIteratorBuiltinFunction);
        }

        public abstract IDynamic CreateIterator(IEnvironment environment, IArgs args);
    }
}