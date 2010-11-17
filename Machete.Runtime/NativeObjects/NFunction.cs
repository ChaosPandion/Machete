using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using System.Diagnostics;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NFunction : LObject
    {
        private readonly string[] _formalParameterList;
        private readonly Lazy<Code> _code;
        private readonly SLexicalEnvironment _scope;


        internal NFunction(string[] formalParameterList, Func<Code> getCode)
        {
            _formalParameterList = formalParameterList ?? new string[0];
            _code = new Lazy<Code>(getCode);
            _scope = Engine.Instance.Value.GlobalEnvironment;
        }

        internal NFunction(string[] formalParameterList, Func<Code> getCode, SLexicalEnvironment scope)
        {
            _formalParameterList = formalParameterList ?? new string[0];
            _code = new Lazy<Code>(getCode);
            _scope = scope;
        }


        public override LType Call(LType @this, SList args)
        {
            Debug.Assert(@this != null);
            Debug.Assert(args != null);
            var enviroment = _scope.NewDeclarativeEnvironment();
            var context = new ExecutionContext(enviroment, @this);
            return _code.Value(context, args);
        }


    }
}
