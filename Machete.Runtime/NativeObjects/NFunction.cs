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
    public sealed class NFunction : LObject, ICallable, IConstructable
    {
        public ILexicalEnvironment Scope { get; set; }
        public string[] FormalParameterList { get; set; }
        public bool Strict { get; set; }
        public Lazy<Code> Code { get; set; }
        public IObject TargetFunction { get; set; }
        public IDynamic BoundThis { get; set; }
        public IArgs BoundArguments { get; set; }


        public NFunction(IEnvironment enviroment, string[] formalParameterList, bool strict, Lazy<Code> code, ILexicalEnvironment scope)
            : base(enviroment)
        {
            FormalParameterList = formalParameterList;
            Strict = strict;
            Code = code;
            Scope = scope;
        }



        IDynamic ICallable.Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            using (var c = environment.EnterContext())
            {
                c.LexicalEnviroment = c.VariableEnviroment = Scope.NewDeclarativeEnvironment();
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

                return null;
            }
        }

        IObject IConstructable.Construct(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        public bool HasInstance(IDynamic obj)
        {
            throw new NotImplementedException();
        }
    }
}
