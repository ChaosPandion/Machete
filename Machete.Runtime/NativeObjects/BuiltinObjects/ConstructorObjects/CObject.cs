using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CObject : LObject
    {
        public CObject(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
