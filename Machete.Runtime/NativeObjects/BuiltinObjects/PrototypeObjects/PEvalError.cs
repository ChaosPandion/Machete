using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PEvalError : LObject
    {
        public PEvalError(IEnvironment environment)
            : base(environment)
        {

        }
    }
}