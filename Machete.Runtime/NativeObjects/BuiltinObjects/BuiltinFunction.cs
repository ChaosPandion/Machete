using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects
{
    public sealed class BuiltinFunction : LObject, ICallable
    {
        private readonly Code _code;
        private readonly ReadOnlyList<string> _formalParameters;

        public BuiltinFunction(IEnvironment environment, Code code, ReadOnlyList<string> formalParameters)
            : base (environment)
        {
            _code = code;
            _formalParameters = formalParameters;
        }

        public sealed override void Initialize()
        {
            Class = "Function";
            Extensible = true;
            Prototype = Environment.FunctionPrototype;
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(_formalParameters.Count), false, false, false), false);
            base.Initialize();
        }

        public IDynamic Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            using (var newContext = environment.EnterContext())
            {
                Environment.Context.ThisBinding = thisBinding;
                return _code(environment, args);
            }
        }
    }
}