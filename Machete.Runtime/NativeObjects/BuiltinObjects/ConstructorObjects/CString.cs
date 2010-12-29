using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Text;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CString : LObject, IBuiltinFunction
    {
        public CString(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Function";
            Extensible = true;
            Prototype = Environment.FunctionPrototype;
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(1.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.StringPrototype, false, false, false), false);
            base.Initialize();
        }

        public IDynamic Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            if (args.Count > 0)
            {
                return args[0].ConvertToString();
            }
            return environment.CreateString("");
        }

        public IObject Construct(IEnvironment environment, IArgs args)
        {
            var obj = new NString(environment);
            obj.Class = "String";
            obj.Extensible = true;
            obj.Prototype = environment.StringPrototype;
            if (args.Count > 0)
            {
                obj.PrimitiveValue = args[0].ConvertToString();
            }
            else
            {
                obj.PrimitiveValue = environment.CreateString("");
            }
            return obj;
        }

        public bool HasInstance(IDynamic value)
        {
            return Environment.Instanceof(value, this);
        }

        [NativeFunction("fromCharCode", "char0"), DataDescriptor(true, false, true)]
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
