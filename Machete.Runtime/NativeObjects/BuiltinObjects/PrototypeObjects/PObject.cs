using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PObject : LObject
    {
        private LType ToString(ExecutionContext context, SList args)
        {
            return new LString(string.Format("[object, {0}]", context.ThisBinding.ConvertToObject().Class));
        }

        private LType ToLocaleString(ExecutionContext context, SList args)
        {
            var obj = context.ThisBinding.ConvertToObject();
            var func = obj.Get("toString") as NFunction;
            if (func == null) Engine.ThrowTypeError();
            return func.Call(obj, SList.Empty);
        }

        private LType ValueOf(ExecutionContext context, SList args)
        {
            var obj = context.ThisBinding.ConvertToObject();
            return obj.PrimitiveValue;
        }

        private LType HasOwnProperty(ExecutionContext context, SList args)
        {
            if (args.IsEmpty) return LBoolean.False;            
            var obj = context.ThisBinding.ConvertToObject();
            var desc = obj.GetOwnProperty(args[0].ConvertToString().Value);
            return (LBoolean)(desc == null);
        }

        private LType IsPrototypeOf(ExecutionContext context, SList args)
        {
            var value = args[0] as LObject;
            var thisObj = context.ThisBinding.ConvertToObject();
            return (LBoolean)(value != null && value.Prototype != null && value.Prototype == thisObj); 
        }

        private LType PropertyIsEnumerable(ExecutionContext context, SList args)
        {
            if (args.IsEmpty) return LBoolean.False;            
            var obj = context.ThisBinding.ConvertToObject();
            var desc = obj.GetOwnProperty(args[0].ConvertToString().ToString());
            return (LBoolean)(desc != null && (desc.Enumerable ?? false));
        }
    }
}
