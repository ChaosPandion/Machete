using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CError : LObject
    {
        public CError(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
