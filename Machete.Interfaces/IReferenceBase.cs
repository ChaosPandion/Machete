namespace Machete.Interfaces
{
    public interface IReferenceBase
    {
        IDynamic Get(string name, bool strict);
        void Set(string name, IDynamic value, bool strict);
    }
}