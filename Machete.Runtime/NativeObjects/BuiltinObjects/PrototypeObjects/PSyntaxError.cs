using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PSyntaxError : LObject
    {
        public PSyntaxError(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
