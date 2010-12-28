using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PError : LObject
    {
        public PError(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Error";
            Extensible = true;
            DefineOwnProperty("name", Environment.CreateDataDescriptor(Environment.CreateString("Error"), true, false, true), false);
            DefineOwnProperty("message", Environment.CreateDataDescriptor(Environment.CreateString(""), true, false, true), false);
            base.Initialize();
        }


        [NativeFunction("toString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var name = obj.Get("name").ConvertToString().BaseValue;
            var message = obj.Get("message").ConvertToString().BaseValue;

            if (name == "undefined")
            {
                if (message == "undefined")
                {
                    return environment.CreateString("Error");
                }
                return environment.CreateString("Error: " + message);
            }
            else
            {
                if (message == "undefined")
                {
                    return environment.CreateString(name);
                }
                return environment.CreateString(name + ": " + message);
            }
        }
    }
}