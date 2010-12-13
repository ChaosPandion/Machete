namespace Machete.Interfaces
{
    public interface IString : IDynamic, IReferenceBase
    {
        string BaseValue { get; }
    }
}