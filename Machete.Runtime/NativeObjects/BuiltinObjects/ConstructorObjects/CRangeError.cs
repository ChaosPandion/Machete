using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CRangeError : BuiltinConstructor
    {
        public CRangeError(IEnvironment environment)
            : base(environment)
        {

        }

        public sealed override void Initialize()
        {
            base.Initialize();
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(1.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.RangeErrorPrototype, false, false, false), false);
        }

        protected sealed override IDynamic Call(IEnvironment environment, IArgs args)
        {
            return ((IConstructable)this).Construct(environment, args);
        }

        public sealed override IObject Construct(IEnvironment environment, IArgs args)
        {
            var message = args[0].ConvertToString();
            if (message.BaseValue == "undefined")
            {
                message = environment.CreateString("");
            }
            var error = new NRangeError(environment);
            error.Class = "Error";
            error.Extensible = true;
            error.Prototype = environment.RangeErrorPrototype;
            error.Put("message", message, false);
            return error;
        }
    }
}