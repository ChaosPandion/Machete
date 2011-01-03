using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PBoolean : LObject
    {
        public PBoolean(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Boolean"; 
            Extensible = true;
            Prototype = Environment.ObjectPrototype;
            DefineOwnProperty("constructor", Environment.CreateDataDescriptor(Environment.BooleanConstructor, true, false, true), false);
            base.Initialize();
        }

        [NativeFunction("toString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            var v = environment.Context.ThisBinding;
            switch (v.TypeCode)
            {
                case LanguageTypeCode.Boolean:
                    return environment.CreateString(((IBoolean)v).BaseValue ? "true" : "false");
                case LanguageTypeCode.Object:
                    var o = (IObject)v;
                    if (o.Class == "Boolean")
                    {
                        return environment.CreateString(((IBoolean)((IPrimitiveWrapper)o).PrimitiveValue).BaseValue ? "true" : "false");
                    }
                    break;
            }
            throw environment.CreateTypeError("");
        }

        [NativeFunction("valueOf"), DataDescriptor(true, false, true)]
        internal static IDynamic ValueOf(IEnvironment environment, IArgs args)
        {
            var v = environment.Context.ThisBinding;
            switch (v.TypeCode)
            {
                case LanguageTypeCode.Boolean:
                    return v;
                case LanguageTypeCode.Object:
                    var o = (IObject)v;
                    if (o.Class == "Boolean")
                    {
                        return ((IPrimitiveWrapper)o).PrimitiveValue;
                    }
                    break;
            }
            throw environment.CreateTypeError("");
        }
    }
}

