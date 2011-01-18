using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CError : BuiltinConstructor
    {
        public CError(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(1.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.ErrorPrototype, false, false, false), false);
            base.Initialize();
        }

        protected override IDynamic Call(IEnvironment environment, IArgs args)
        {
            return Construct(environment, args);
        }

        public override IObject Construct(IEnvironment environment, IArgs args)
        {
            var message = args[0].ConvertToString();
            if (message.BaseValue == "undefined")
            {
                message = environment.CreateString("");
            }
            var error = new NError(environment);
            error.Class = "Error";
            error.Extensible = true;
            error.Prototype = environment.ErrorPrototype;
            error.Put("message", message, false);
            return error;
        }
    }
}