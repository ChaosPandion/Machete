using System;

namespace Machete.Interfaces
{
    public interface IExecutionContext : IDisposable
    {
        IFunction CurrentFunction { get; set; } 
        ILexicalEnvironment LexicalEnviroment { get; set; }
        ILexicalEnvironment VariableEnviroment { get; set; }
        IDynamic ThisBinding { get; set; }
    }
}