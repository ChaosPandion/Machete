using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PReferenceError : LObject
    {
        public PReferenceError(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
