using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CEvalError : BConstructor
    {
        public CEvalError(IEnvironment environment)
            : base(environment)
        {

        }

        public sealed override void Initialize()
        {
            base.Initialize();
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(1.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.EvalErrorPrototype, false, false, false), false);
        }

        protected sealed override IDynamic Call(IEnvironment environment, IArgs args)
        {
            return Construct(environment, args);
        }

        public sealed override IObject Construct(IEnvironment environment, IArgs args)
        {
            var message = args[0].ConvertToString();
            if (message.BaseValue == "undefined")
            {
                message = environment.CreateString("");
            }
            var error = new NEvalError(environment);
            error.Class = "Error";
            error.Extensible = true;
            error.Prototype = environment.EvalErrorPrototype;
            error.Put("message", message, false);
            return error;
        }
    }
}