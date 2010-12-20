using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CBoolean : LObject, ICallable, IConstructable
    {
        public CBoolean(IEnvironment environment)
            : base(environment)
        {
            Class = "Function";
            Extensible = true;
            DefineOwnProperty("length", environment.CreateDataDescriptor(environment.CreateNumber(1.0), true, false, true), false);
        }

        IDynamic ICallable.Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            return args[0].ConvertToBoolean();
        }

        IObject IConstructable.Construct(IEnvironment environment, IArgs args)
        {
            var obj = new NBoolean(environment);
            obj.Class = "Boolean";
            obj.Extensible = true;
            obj.PrimitiveValue = args[0].ConvertToBoolean();
            obj.Prototype = environment.BooleanPrototype;
            return obj;
        }
    }
}
