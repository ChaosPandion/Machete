namespace Machete.Interfaces
{
    public interface IBoolean : IDynamic, IReferenceBase
    {
        bool BaseValue { get; }
    }
}