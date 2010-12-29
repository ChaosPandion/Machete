using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CDate : LObject, IBuiltinFunction
    {
        public CDate(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Function";
            Extensible = true;
            Prototype = Environment.FunctionPrototype;
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(7.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.DatePrototype, false, false, false), false);
            base.Initialize();
        }

        public IDynamic Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            throw new System.NotImplementedException();
        }

        public IObject Construct(IEnvironment environment, IArgs args)
        {
            throw new System.NotImplementedException();
        }

        public bool HasInstance(IDynamic value)
        {
            throw new System.NotImplementedException();
        }
    }
}
