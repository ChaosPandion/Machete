namespace Machete.Interfaces
{
    public interface ILexicalEnvironment
    {
        IEnvironmentRecord Record { get; set; }
        ILexicalEnvironment Parent { get; set; }

        IReference GetIdentifierReference(string name, bool strict);
    }
}