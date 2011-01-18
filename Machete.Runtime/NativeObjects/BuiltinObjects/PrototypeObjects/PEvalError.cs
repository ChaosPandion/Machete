using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PEvalError : LObject
    {
        public PEvalError(IEnvironment environment)
            : base(environment)
        {
            Class = "Error";
            Extensible = true;
            DefineOwnProperty("name", environment.CreateDataDescriptor(environment.CreateString("EvalError"), true, false, true), false);
            DefineOwnProperty("message", environment.CreateDataDescriptor(environment.CreateString(""), true, false, true), false);
        }
    }
}