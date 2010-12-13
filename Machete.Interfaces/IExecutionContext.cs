namespace Machete.Interfaces
{
    public interface IExecutionContext
    {
        ILexicalEnvironment LexicalEnviroment { get; set; }
        ILexicalEnvironment VariableEnviroment { get; set; }
        IDynamic ThisBinding { get; set; }
    }
}