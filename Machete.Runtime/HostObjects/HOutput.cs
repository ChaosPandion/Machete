using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.HostObjects
{
    public sealed class HOutput : LObject
    {
        public HOutput(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Output";
            Prototype = null;
            Extensible = true;
            base.Initialize();
        }

        public override IDynamic DefaultValue(string hint)
        {
            return Environment.CreateString("[object, Output]");
        }

        [NativeFunction("write", "value"), DataDescriptor(false, false, false)]
        internal static IDynamic Write(IEnvironment environment, IArgs args)
        {
            var value = args[0].ConvertToString().BaseValue;
            environment.Output.Write(value);
            return environment.Undefined;
        }
    }
}