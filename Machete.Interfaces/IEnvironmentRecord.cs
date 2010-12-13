namespace Machete.Interfaces
{
    public interface IEnvironmentRecord : IReferenceBase
    {
        bool HasBinding(string name);
        void CreateMutableBinding(string name, bool deletable);
        void SetMutableBinding(string name, IDynamic value, bool strict);
        IDynamic GetBindingValue(string name, bool strict);
        bool DeleteBinding(string name);
        IDynamic ImplicitThisValue();
    }
}