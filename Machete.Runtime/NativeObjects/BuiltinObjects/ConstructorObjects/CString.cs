using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CString : LObject
    {
        public CString(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
