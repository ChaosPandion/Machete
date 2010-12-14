using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CBoolean : LObject
    {
        public CBoolean(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
