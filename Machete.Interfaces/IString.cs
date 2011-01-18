namespace Machete.Core
{
    public interface IString : IDynamic, IReferenceBase
    {
        string BaseValue { get; }
    }
}