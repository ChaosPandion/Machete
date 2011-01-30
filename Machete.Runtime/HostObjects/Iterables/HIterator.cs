using Machete.Core;
using Machete.Core.Generators;
using Machete.Runtime.NativeObjects.BuiltinObjects;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.HostObjects
{
    public abstract class HIterator : LObject
    {
        public BFunction CurrentBuiltinFunction { get; private set; }
        public BFunction NextBuiltinFunction { get; private set; }
        
        public HIterator(IEnvironment environment)
            : base(environment)
        {
            Class = "Iterator";
            Extensible = true;
            Prototype = environment.ObjectPrototype;

            CurrentBuiltinFunction = new BFunction(environment, Current, ReadOnlyList<string>.Empty);
            NextBuiltinFunction = new BFunction(environment, Next, ReadOnlyList<string>.Empty);

            new LObject.Builder(this)
            .SetAttributes(false, false, false)
            .AppendAccessorProperty("current", CurrentBuiltinFunction, null)
            .AppendDataProperty("next", NextBuiltinFunction);
        }

        public abstract IDynamic Current(IEnvironment environment, IArgs args);

        public abstract IDynamic Next(IEnvironment environment, IArgs args);
    }
}