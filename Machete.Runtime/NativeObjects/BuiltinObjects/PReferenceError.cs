using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PReferenceError : LObject
    {
        public PReferenceError(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Error";
            Extensible = true;
            Prototype = Environment.ErrorPrototype;
            DefineOwnProperty("name", Environment.CreateDataDescriptor(Environment.CreateString("ReferenceError"), true, false, true), false);
            DefineOwnProperty("message", Environment.CreateDataDescriptor(Environment.CreateString(""), true, false, true), false);
        }
    }
}