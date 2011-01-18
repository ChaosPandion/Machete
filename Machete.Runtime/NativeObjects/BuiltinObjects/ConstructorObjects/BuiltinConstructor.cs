using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public abstract class BuiltinConstructor : LObject, ICallable, IConstructable, IHasInstance
    {
        public BuiltinConstructor(IEnvironment environment)
            : base (environment)
        {

        }

        public override void Initialize()
        {
            Class = "Function";
            Extensible = true;
            Prototype = Environment.FunctionPrototype;
            base.Initialize();
        }

        IDynamic ICallable.Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            using (var newContext = environment.EnterContext())
            {
                Environment.Context.ThisBinding = thisBinding;
                return Call(environment, args);
            }
        }

        protected abstract IDynamic Call(IEnvironment environment, IArgs args);

        public abstract IObject Construct(IEnvironment environment, IArgs args);

        public bool HasInstance(IDynamic value)
        {
            return Environment.Instanceof(value, this);
        }
    }
}