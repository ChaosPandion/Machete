using System;

namespace Machete.Core
{
    public interface IExecutionContext : IDisposable
    { 
        ILexicalEnvironment LexicalEnviroment { get; set; }
        ILexicalEnvironment VariableEnviroment { get; set; }
        IDynamic ThisBinding { get; set; }
        bool Strict { get; set; }
        string CurrentFunction { get; set; }
    }
}