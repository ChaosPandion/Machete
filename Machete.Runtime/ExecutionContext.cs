using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using System.Reflection;
using Machete.Interfaces;

namespace Machete.Runtime
{
    public sealed class ExecutionContext : IExecutionContext
    {
        public ILexicalEnvironment LexicalEnviroment { get; set; }
        public ILexicalEnvironment VariableEnviroment { get; set; }
        public IDynamic ThisBinding { get; set; }


        public ExecutionContext(ILexicalEnvironment enviroment, IDynamic thisBinding)
        {
            LexicalEnviroment = enviroment;
            VariableEnviroment = enviroment;
            ThisBinding = thisBinding;
        }
    }
}
