namespace Machete.Core
{
    public interface IDeclarativeEnvironmentRecord : IEnvironmentRecord
    {
        void CreateImmutableBinding(string name);
        void InitializeImmutableBinding(string name, IDynamic value);
    }
}