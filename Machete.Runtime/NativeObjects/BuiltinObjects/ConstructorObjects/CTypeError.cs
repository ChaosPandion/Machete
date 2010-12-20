using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CTypeError : LObject, ICallable, IConstructable
    {
        public CTypeError(IEnvironment environment)
            : base(environment)
        {
            Class = "Error";
            Extensible = true;
            DefineOwnProperty("length", environment.CreateDataDescriptor(environment.CreateNumber(1.0), true, false, true), false);
        }

        IDynamic ICallable.Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            return ((IConstructable)this).Construct(environment, args);
        }

        IObject IConstructable.Construct(IEnvironment environment, IArgs args)
        {
            var message = args[0].ConvertToString();
            if (message.BaseValue == "undefined")
            {
                message = environment.CreateString("");
            }
            var error = new NTypeError(environment);
            error.Class = "Error";
            error.Extensible = true;
            error.Prototype = environment.TypeErrorPrototype;
            error.Put("message", message, false);
            return error;
        }
    }
}
