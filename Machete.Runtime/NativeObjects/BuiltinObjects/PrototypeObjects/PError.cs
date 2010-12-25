using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PError : LObject
    {
        public PError(IEnvironment environment)
            : base(environment)
        {
            Class = "Error";
            Extensible = true;
            DefineOwnProperty("name", environment.CreateDataDescriptor(environment.CreateString("Error"), true, false, true), false);
            DefineOwnProperty("message", environment.CreateDataDescriptor(environment.CreateString(""), true, false, true), false);
        }


        [NativeFunction("toString")]
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