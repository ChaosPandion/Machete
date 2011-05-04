using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PObject : LObject
    {
        public BFunction ToStringBuiltinFunction { get; private set; }
        public BFunction ToLocaleStringBuiltinFunction { get; private set; }
        public BFunction ValueOfBuiltinFunction { get; private set; }
        public BFunction HasOwnPropertyBuiltinFunction { get; private set; }
        public BFunction IsPrototypeOfBuiltinFunction { get; private set; }
        public BFunction PropertyIsEnumerableBuiltinFunction { get; private set; }
        
        public PObject(IEnvironment environment)
            : base(environment)
        {

        }

        public sealed override void Initialize()
        {
            Class = "Object";
            Extensible = true;
            Prototype = null;

            ToStringBuiltinFunction = new BFunction(Environment, ToString, ReadOnlyList<string>.Empty);
            ToLocaleStringBuiltinFunction = new BFunction(Environment, ToLocaleString, ReadOnlyList<string>.Empty);
            ValueOfBuiltinFunction = new BFunction(Environment, ValueOf, ReadOnlyList<string>.Empty);
            HasOwnPropertyBuiltinFunction = new BFunction(Environment, HasOwnProperty, new ReadOnlyList<string>("V"));
            IsPrototypeOfBuiltinFunction = new BFunction(Environment, IsPrototypeOf, new ReadOnlyList<string>("V"));
            PropertyIsEnumerableBuiltinFunction = new BFunction(Environment, PropertyIsEnumerable, new ReadOnlyList<string>("V"));

            new LObject.Builder(this)
            .SetAttributes(true, false, true)
            .AppendDataProperty("constructor", Environment.ObjectConstructor)
            .AppendDataProperty("toString", ToStringBuiltinFunction)
            .AppendDataProperty("toLocaleString", ToLocaleStringBuiltinFunction)
            .AppendDataProperty("valueOf", ValueOfBuiltinFunction)
            .AppendDataProperty("hasOwnProperty", HasOwnPropertyBuiltinFunction)
            .AppendDataProperty("isPrototypeOf", IsPrototypeOfBuiltinFunction)
            .AppendDataProperty("propertyIsEnumerable", PropertyIsEnumerableBuiltinFunction);
        }

        private static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            switch (o.TypeCode)
            {
                case LanguageTypeCode.Undefined:
                    return environment.CreateString("[object, Undefined]");
                case LanguageTypeCode.Null:
                    return environment.CreateString("[object, Null]");
                default:
                    return environment.CreateString(string.Format("[object, {0}]", o.ConvertToObject().Class));
            }
        }

        private static IDynamic ToLocaleString(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var func = obj.Get("toString") as ICallable;
            if (func == null)
            {
                throw environment.CreateTypeError("This object does not contain a 'toString' method.");
            }
            return func.Call(environment, obj, environment.EmptyArgs);
        }

        private static IDynamic ValueOf(IEnvironment environment, IArgs args)
        {
            return environment.Context.ThisBinding.ConvertToObject();
        }

        private static IDynamic HasOwnProperty(IEnvironment environment, IArgs args)
        {
            if (args.IsEmpty) return environment.False;
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var desc = obj.GetOwnProperty(args[0].ConvertToString().BaseValue);
            return environment.CreateBoolean(desc != null);
        }

        private static IDynamic IsPrototypeOf(IEnvironment environment, IArgs args)
        {
            var value = args[0] as LObject;
            var thisObj = environment.Context.ThisBinding.ConvertToObject();
            return environment.CreateBoolean(value != null && value.Prototype != null && value.Prototype == thisObj);
        }

        private static IDynamic PropertyIsEnumerable(IEnvironment environment, IArgs args)
        {
            if (args.IsEmpty) return environment.False;
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var desc = obj.GetOwnProperty(args[0].ConvertToString().ToString());
            return environment.CreateBoolean(desc != null && (desc.Enumerable ?? false));
        }
    }
}