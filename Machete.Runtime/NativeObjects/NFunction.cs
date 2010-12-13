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
        private readonly string[] _formalParameterList;
        //private readonly Lazy<Code> _code;
        private readonly SLexicalEnvironment _scope;


        public SLexicalEnvironment Scope { get; set; }
        public string[] FormalParameters { get; set; }
        //internal Code Code { get; set; }
        public object TargetFunction { get; set; }
        public object BoundThis { get; set; }
        public object BoundArguments { get; set; }


        internal NFunction(string[] formalParameterList, Func<object> getCode)
        {
            //_formalParameterList = formalParameterList ?? new string[0];
            //_code = new Lazy<Code>(getCode);
            //_scope = Environment.Instance.Value.GlobalEnvironment;
        }

        internal NFunction(string[] formalParameterList, Func<object> getCode, SLexicalEnvironment scope)
        {
            //_formalParameterList = formalParameterList ?? new string[0];
            //_code = new Lazy<Code>(getCode);
            //_scope = scope;
        }


        IDynamic ICallable.Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            throw new NotImplementedException();
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
