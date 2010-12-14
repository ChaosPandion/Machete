using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CEvalError : LObject
    {
        public CEvalError(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
