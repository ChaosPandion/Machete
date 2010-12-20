using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NTypeError : LObject
    {
        public NTypeError(IEnvironment environment)
            : base(environment)
        {
            Class = "Error";
            Extensible = true;
        }
    }
}
