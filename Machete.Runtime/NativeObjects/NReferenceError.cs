using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NReferenceError : LObject
    {
        public NReferenceError(IEnvironment environment)
            : base(environment)
        {
            Class = "Error";
            Extensible = true;
        }
    }
}
