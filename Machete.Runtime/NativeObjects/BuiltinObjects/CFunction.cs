using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Text;
using Machete.Compiler;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CFunction : BConstructor
    {
        public CFunction(IEnvironment environment)
            : base(environment)
        {

        }

        public sealed override void Initialize()
        {
            base.Initialize();
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(1.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.FunctionPrototype, false, false, false), false);
        }

        protected sealed override IDynamic Call(IEnvironment environment, IArgs args)
        {
            return Construct(environment, args);
        }

        public override IObject Construct(IEnvironment environment, IArgs args)
        {
            string formalParametersString, functionBody;

            if (args.IsEmpty)
            {
                formalParametersString = "";
                functionBody = "";
            }
            else if (args.Count == 1)
            {
                formalParametersString = "";
                functionBody = args[0].ConvertToString().BaseValue;
            }
            else
            {
                var sb = new StringBuilder();
                var limit = args.Count - 1;
                for (int i = 0; i < limit; i++)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append(args[i].ConvertToString().BaseValue);
                }
                formalParametersString = sb.ToString();
                functionBody = args[limit].ConvertToString().BaseValue;
            }

            var compiler = new CompilerService(environment);
            var executableCode = compiler.CompileFunctionCode(functionBody, environment.Context.Strict);
            var formalParameters = compiler.CompileFormalParameterList(formalParametersString);
            var func = environment.CreateFunction(executableCode, formalParameters, environment.GlobalEnvironment);
            return func;
        }
    }
}