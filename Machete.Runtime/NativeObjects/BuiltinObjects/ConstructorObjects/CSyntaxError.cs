using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CSyntaxError : LObject
    {
        public CSyntaxError(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
