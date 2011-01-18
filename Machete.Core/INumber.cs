namespace Machete.Core
{
    public interface INumber : IDynamic, IReferenceBase
    {
        double BaseValue { get; }
    }
}