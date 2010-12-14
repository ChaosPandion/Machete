using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PBoolean : LObject
    {
        public PBoolean(IEnvironment environment)
            : base(environment)
        {

        }
        //internal PBoolean()
        //{
        //    Class = "Boolean";
        //    Extensible = true;
        //    //DefineOwnProperty("toString", SPropertyDescriptor.Create(new NFunction(null, () => ToString)), false);
        //    //DefineOwnProperty("valueOf", SPropertyDescriptor.Create(new NFunction(null, () => ValueOf)), false);
        //}


        //private IDynamic ToString(ExecutionContext context, SList args)
        //{
        //    return context.ThisBinding.ConvertToBoolean().ConvertToString();
        //}

        //private IDynamic ValueOf(ExecutionContext context, SList args)
        //{
        //    return context.ThisBinding.ConvertToBoolean();
        //}
    }
}

