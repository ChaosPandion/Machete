namespace Machete.Interfaces
{
    public interface ILexicalEnvironment
    {
        IEnvironmentRecord Record { get; set; }
        ILexicalEnvironment Parent { get; set; }
    }
}