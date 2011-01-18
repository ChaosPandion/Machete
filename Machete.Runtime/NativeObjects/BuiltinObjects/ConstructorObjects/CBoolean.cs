using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CBoolean : BConstructor
    {
        public CBoolean(IEnvironment environment)
            : base(environment)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(1.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.BooleanPrototype, false, false, false), false);
        }

        protected override IDynamic Call(IEnvironment environment, IArgs args)
        {
            return args[0].ConvertToBoolean();
        }

        public override IObject Construct(IEnvironment environment, IArgs args)
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