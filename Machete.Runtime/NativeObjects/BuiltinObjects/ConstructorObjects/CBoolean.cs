using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CBoolean : LObject, IBuiltinFunction
    {
        public CBoolean(IEnvironment environment)
            : base(environment)
        {
            Class = "Function";
            Extensible = true;
            DefineOwnProperty("length", environment.CreateDataDescriptor(environment.CreateNumber(1.0), true, false, true), false);
        }

        public override void Initialize()
        {
            Class = "Function";
            Extensible = true;
            Prototype = Environment.FunctionPrototype;
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(1.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.BooleanPrototype, false, false, false), false);
            base.Initialize();
        }

        public IDynamic Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            return args[0].ConvertToBoolean();
        }

        public IObject Construct(IEnvironment environment, IArgs args)
        {
            var obj = new NBoolean(environment);
            obj.Class = "Boolean";
            obj.Extensible = true;
            obj.PrimitiveValue = args[0].ConvertToBoolean();
            obj.Prototype = environment.BooleanPrototype;
            return obj;
        }

        public bool HasInstance(IDynamic value)
        {
            return Environment.Instanceof(value, this);
        }
    }
}
