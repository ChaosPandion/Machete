using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NError : LObject
    {
        public NError(IEnvironment environment)
            : base(environment)
        {
            Class = "Error";
            Extensible = true;
        }
    }
}
