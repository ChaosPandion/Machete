using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CObject : LObject, ICallable, IConstructable
    {
        public CObject(IEnvironment environment)
            : base(environment)
        {

        }

        IDynamic ICallable.Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            throw new System.NotImplementedException();
        }

        IObject IConstructable.Construct(IEnvironment environment, IArgs args)
        {
            var obj = new LObject(environment);
            obj.Class = "Object";
            obj.Extensible = true;
            obj.Prototype = ((Environment)environment).ObjectPrototype;
            return obj;
        }
    }
}
