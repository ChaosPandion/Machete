using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Text;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CString : BConstructor
    {
        public CString(IEnvironment environment)
            : base(environment)
        {

        }

        public sealed override void Initialize()
        {
            base.Initialize();
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(1.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.StringPrototype, false, false, false), false);
        }

        protected sealed override IDynamic Call(IEnvironment environment, IArgs args)
        {
            if (args.Count > 0)
            {
                return args[0].ConvertToString();
            }
            return environment.CreateString("");
        }

        public sealed override IObject Construct(IEnvironment environment, IArgs args)
        {
            var obj = new NString(environment);
            {
                var str = args.Count > 0 ? args[0].ConvertToString() : environment.CreateString("");
                var len = environment.CreateNumber(str.BaseValue.Length);
                var lenDesc = environment.CreateDataDescriptor(len, false, false, false);

                obj.Class = "String";
                obj.Extensible = true;
                obj.Prototype = environment.StringPrototype;
                obj.PrimitiveValue = str;
                obj.DefineOwnProperty("length", lenDesc, false);
            }
            return obj;
        }

        [BuiltinFunction("fromCharCode", "char0"), DataDescriptor(true, false, true)]
        internal static IDynamic FromCharCode(IEnvironment environment, IArgs args)
        {
            var sb = new StringBuilder(args.Count);
            foreach (var arg in args)
            {
                sb.Append((char)arg.ConvertToUInt16().BaseValue);
            }
            return environment.CreateString(sb.ToString());
        }
    }
}