using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using System.Reflection;
using Machete.Runtime.RuntimeTypes.Interfaces;

namespace Machete.Runtime
{
    class ExecutionContext
    {
        public SLexicalEnvironment LexicalEnviroment { get; set; }
        public SLexicalEnvironment VariableEnviroment { get; set; }
        public IDynamic ThisBinding { get; set; }


        public ExecutionContext(SLexicalEnvironment enviroment, IDynamic thisBinding)
        {
            LexicalEnviroment = enviroment;
            VariableEnviroment = enviroment;
            ThisBinding = thisBinding;
        }




    }
}
