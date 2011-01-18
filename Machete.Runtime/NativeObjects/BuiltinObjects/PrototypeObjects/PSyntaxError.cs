using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PSyntaxError : LObject
    {
        public PSyntaxError(IEnvironment environment)
            : base(environment)
        {
            Class = "Error";
            Extensible = true;
            DefineOwnProperty("name", environment.CreateDataDescriptor(environment.CreateString("SyntaxError"), true, false, true), false);
            DefineOwnProperty("message", environment.CreateDataDescriptor(environment.CreateString(""), true, false, true), false);
        }
    }
}
