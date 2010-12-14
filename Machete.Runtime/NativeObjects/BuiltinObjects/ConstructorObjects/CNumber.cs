using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CNumber : LObject
    {
        public CNumber(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
