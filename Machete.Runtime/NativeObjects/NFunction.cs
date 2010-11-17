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
        private readonly string _identifier;
        private readonly string[] _formalParameterList;
        private readonly Lazy<Code> _code;
        private readonly SLexicalEnvironment _scope;


        NFunction(string identifier, string[] formalParameterList, Func<Code> getCode, SLexicalEnvironment scope)
        {
            Debug.Assert(identifier != null);
            Debug.Assert(formalParameterList != null);
            Debug.Assert(getCode != null);
            Debug.Assert(scope != null);
            _identifier = identifier;
            _formalParameterList = formalParameterList;
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
