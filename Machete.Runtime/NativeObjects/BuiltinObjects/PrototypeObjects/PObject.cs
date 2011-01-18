using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PObject : LObject
    {
        public PObject(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Object";
            Extensible = true;
            DefineOwnProperty("constructor", Environment.CreateDataDescriptor(Environment.ObjectConstructor), false);
            base.Initialize();
        }

        [BuiltinFunction("toString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            return environment.CreateString(string.Format("[object, {0}]", obj.Class));
        }

        [BuiltinFunction("toLocaleString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToLocaleString(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var func = obj.Get("toString") as ICallable;
            if (func == null)
            {
                throw environment.CreateTypeError("This object does not contain a `toString` method.");
            }
            return func.Call(environment, obj, environment.EmptyArgs);
        }

        [BuiltinFunction("valueOf"), DataDescriptor(true, false, true)]
        internal static IDynamic ValueOf(IEnvironment environment, IArgs args)
        {
            return environment.Context.ThisBinding.ConvertToObject();
        }

        [BuiltinFunction("hasOwnProperty", "V"), DataDescriptor(true, false, true)]
        internal static IDynamic HasOwnProperty(IEnvironment environment, IArgs args)
        {
            if (args.IsEmpty) return environment.False;
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var desc = obj.GetOwnProperty(args[0].ConvertToString().BaseValue);
            return environment.CreateBoolean(desc != null);
        }

        [BuiltinFunction("isPrototypeOf", "V"), DataDescriptor(true, false, true)]
        internal static IDynamic IsPrototypeOf(IEnvironment environment, IArgs args)
        {
            var value = args[0] as LObject;
            var thisObj = environment.Context.ThisBinding.ConvertToObject();
            return environment.CreateBoolean(value != null && value.Prototype != null && value.Prototype == thisObj);
        }

        [BuiltinFunction("propertyIsEnumerable", "V"), DataDescriptor(true, false, true)]
        internal static IDynamic PropertyIsEnumerable(IEnvironment environment, IArgs args)
        {
            if (args.IsEmpty) return environment.False;
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var desc = obj.GetOwnProperty(args[0].ConvertToString().ToString());
            return environment.CreateBoolean(desc != null && (desc.Enumerable ?? false));
        }
    }
}