using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CReferenceError : LObject
    {
        public CReferenceError(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
