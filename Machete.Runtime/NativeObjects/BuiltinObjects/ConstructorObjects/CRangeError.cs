using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CRangeError : LObject
    {
        public CRangeError(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
