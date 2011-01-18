using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CNumber : BuiltinConstructor
    {
        public CNumber(IEnvironment environment)
            : base(environment)
        {

        }

        public sealed override void Initialize()
        {
            base.Initialize();
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(1.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.NumberPrototype, false, false, false), false);
            DefineOwnProperty("MAX_VALUE", Environment.CreateDataDescriptor(Environment.CreateNumber(double.MaxValue), false, false, false), false);
            DefineOwnProperty("MIN_VALUE", Environment.CreateDataDescriptor(Environment.CreateNumber(double.MinValue), false, false, false), false);
            DefineOwnProperty("NaN", Environment.CreateDataDescriptor(Environment.CreateNumber(double.NaN), false, false, false), false);
            DefineOwnProperty("NEGATIVE_INFINITY", Environment.CreateDataDescriptor(Environment.CreateNumber(double.NegativeInfinity), false, false, false), false);
            DefineOwnProperty("POSITIVE_INFINITY", Environment.CreateDataDescriptor(Environment.CreateNumber(double.PositiveInfinity), false, false, false), false);
        }

        protected sealed override IDynamic Call(IEnvironment environment, IArgs args)
        {
            if (args.Count > 0)
            {
                return args[0].ConvertToNumber();
            }
            return environment.CreateNumber(0.0);
        }

        public sealed override IObject Construct(IEnvironment environment, IArgs args)
        {
            var obj = new NNumber(environment);
            obj.Class = "Number";
            obj.Extensible = true;
            obj.Prototype = environment.NumberPrototype;
            if (args.Count > 0)
            {
                obj.PrimitiveValue = args[0].ConvertToNumber();
            }
            else
            {
                obj.PrimitiveValue = environment.CreateNumber(0.0);
            }
            return obj;
        }
    }
}