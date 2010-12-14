using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CTypeError : LObject
    {
        public CTypeError(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
