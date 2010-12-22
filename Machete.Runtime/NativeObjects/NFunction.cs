using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using System.Diagnostics;
using Machete.Interfaces;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NFunction : LObject, IFunction
    {
        public ILexicalEnvironment Scope { get; set; }
        public ReadOnlyList<string> FormalParameterList { get; set; }
        public Lazy<Code> Code { get; set; }
        public IObject TargetFunction { get; set; }
        public IDynamic BoundThis { get; set; }
        public IArgs BoundArguments { get; set; }
        public bool Strict { get; set; }
        public bool BindFunction { get; set; }


        public NFunction(IEnvironment enviroment)
            : base(enviroment)
        {

        }


        public override IDynamic Get(string p)
        {
            // 15.3.5.4 [[Get]] (P) 

            if (BindFunction)
            {
                // NOTE Function objects created using Function.prototype.bind 
                // use the default [[Get]] internal method. 

                return base.Get(p);
            }
            else
            {
                var v = base.Get(p);
                if (p == "callee")
                {
                    var vFunc = v as IFunction;
                    if (vFunc != null && vFunc.Strict)
                    {
                        throw Environment.CreateTypeError("");
                    }
                }
                return v;
            }
        }

        public IDynamic Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            if (BindFunction)
            {
                // 15.3.4.5.1 [[Call]] 

                var func = TargetFunction as IFunction;
                Debug.Assert(func != null);
                return func.Call(environment, BoundThis, environment.ConcatArgs(BoundArguments, args));
            }
            else
            {
                // 13.2.1 [[Call]] 

                var scope = Scope.NewDeclarativeEnvironment();
                using (var c = environment.EnterContext())
                {
                    c.CurrentFunction = this;
                    c.LexicalEnviroment = scope;
                    c.VariableEnviroment = scope;

                    if (Strict)
                    {
                        c.ThisBinding = thisBinding;
                    }
                    else
                    {
                        switch (thisBinding.TypeCode)
                        {
                            case LanguageTypeCode.Undefined:
                            case LanguageTypeCode.Null:
                                c.ThisBinding = environment.GlobalObject;
                                break;
                            default:
                                c.ThisBinding = thisBinding.ConvertToObject();
                                break;
                        }
                    }

                    return Code.Value(environment, args);
                }
            }
        }

        public IObject Construct(IEnvironment environment, IArgs args)
        {
            if (BindFunction)
            {
                // 15.3.4.5.2 [[Construct]]

                var func = TargetFunction as IFunction;
                if (func == null)
                {
                    throw Environment.CreateTypeError("");
                }
                return func.Construct(environment, environment.ConcatArgs(BoundArguments, args));
            }
            else
            {
                // 13.2.2 [[Construct]]

                var obj = new LObject(environment);
                obj.Class = "Object";
                obj.Extensible = true;

                var proto = Get("prototype");
                if (proto.TypeCode == LanguageTypeCode.Object)
                {
                    obj.Prototype = (IObject)proto;
                }
                else
                {
                    obj.Prototype = environment.ObjectPrototype;
                }

                var result = ((ICallable)this).Call(environment, obj, args);
                if (proto.TypeCode == LanguageTypeCode.Object)
                {
                    return (IObject)result;
                }

                return obj;
            }
        }

        public bool HasInstance(IDynamic value)
        {
            if (BindFunction)
            {
                // 15.3.4.5.3 [[HasInstance]] (V)

                var func = TargetFunction as IFunction;
                if (func == null)
                {
                    throw Environment.CreateTypeError("");
                }
                return func.HasInstance(value);
            }
            else
            {
                // 15.3.5.3 [[HasInstance]] (V)

                var oValue = value as IObject;
                if (oValue == null)
                {
                    return false;
                }

                var oPrototype = Get("prototype") as IObject;
                if (oPrototype == null)
                {
                    throw Environment.CreateTypeError("");
                }

                do
                {
                    oValue = oValue.Prototype;
                    if (oValue == oPrototype)
                    {
                        return true;
                    }
                }
                while (oValue.Prototype != null);

                return false;
            }
        }
    }
}