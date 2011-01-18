using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PRangeError : LObject
    {
        public PRangeError(IEnvironment environment)
            : base(environment)
        {
            Class = "Error";
            Extensible = true;
            DefineOwnProperty("name", environment.CreateDataDescriptor(environment.CreateString("RangeError"), true, false, true), false);
            DefineOwnProperty("message", environment.CreateDataDescriptor(environment.CreateString(""), true, false, true), false);
        }
    }
}
