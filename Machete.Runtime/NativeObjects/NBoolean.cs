using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NBoolean : LObject, IPrimitiveWrapper
    {
        public IDynamic PrimitiveValue { get; set; }

        public NBoolean(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
