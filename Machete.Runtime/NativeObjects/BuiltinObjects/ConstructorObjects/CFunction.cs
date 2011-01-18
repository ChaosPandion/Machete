using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CFunction : BuiltinConstructor
    {
        public CFunction(IEnvironment environment)
            : base(environment)
        {

        }

        public sealed override void Initialize()
        {
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(1.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.FunctionPrototype, false, false, false), false);
            base.Initialize();
        }

        protected sealed override IDynamic Call(IEnvironment environment, IArgs args)
        {
            return Construct(environment, args);
        }

        public override IObject Construct(IEnvironment environment, IArgs args)
        {
            throw new System.NotImplementedException();
        }
    }
}