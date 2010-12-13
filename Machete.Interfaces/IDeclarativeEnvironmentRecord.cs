namespace Machete.Interfaces
{
    public interface IDeclarativeEnvironmentRecord : IEnvironmentRecord
    {
        void CreateImmutableBinding(string name);
        void InitializeImmutableBinding(string name, IDynamic value);
    }
}