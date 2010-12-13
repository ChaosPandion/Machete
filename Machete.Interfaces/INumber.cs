namespace Machete.Interfaces
{
    public interface INumber : IDynamic, IReferenceBase
    {
        double BaseValue { get; }
    }
}