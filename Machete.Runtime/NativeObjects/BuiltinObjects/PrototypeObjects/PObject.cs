using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.Interfaces;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PObject : LObject
    {
        private IDynamic ToString(ExecutionContext context, SList args)
        {
            return new LString(string.Format("[object, {0}]", context.ThisBinding.ConvertToObject().Class));
        }

        private IDynamic ToLocaleString(ExecutionContext context, SList args)
        {
            var obj = context.ThisBinding.ConvertToObject();
            var func = obj.Get("toString") as ICallable;
            if (func == null) Engine.ThrowTypeError();
            return func.Call(obj, SList.Empty);
        }

        private IDynamic ValueOf(ExecutionContext context, SList args)
        {
            var obj = context.ThisBinding.ConvertToObject();
            return obj;
        }

        private IDynamic HasOwnProperty(ExecutionContext context, SList args)
        {
            if (args.IsEmpty) return LBoolean.False;            
            var obj = context.ThisBinding.ConvertToObject();
            var desc = obj.GetOwnProperty((string)args[0].ConvertToString());
            return (LBoolean)(desc == null);
        }

        private IDynamic IsPrototypeOf(ExecutionContext context, SList args)
        {
            var value = args[0] as LObject;
            var thisObj = context.ThisBinding.ConvertToObject();
            return (LBoolean)(value != null && value.Prototype != null && value.Prototype == thisObj); 
        }

        private IDynamic PropertyIsEnumerable(ExecutionContext context, SList args)
        {
            if (args.IsEmpty) return LBoolean.False;            
            var obj = context.ThisBinding.ConvertToObject();
            var desc = obj.GetOwnProperty(args[0].ConvertToString().ToString());
            return (LBoolean)(desc != null && (desc.Enumerable ?? false));
        }
    }
}
