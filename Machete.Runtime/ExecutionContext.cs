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
        private readonly Action _dispose;

        public bool Strict { get; set; }
        public IFunction CurrentFunction { get; set; }
        public ILexicalEnvironment LexicalEnviroment { get; set; }
        public ILexicalEnvironment VariableEnviroment { get; set; }
        public IDynamic ThisBinding { get; set; }

        public ExecutionContext(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            _dispose();
        }
    }
}
