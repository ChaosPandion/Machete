namespace Machete.Interfaces
{
    public interface IReference : IDynamic
    {
        IReferenceBase Base { get; }
        string Name { get; }
        bool IsStrictReference { get; }
        bool HasPrimitiveBase { get; }
        bool IsPropertyReference { get; }
        bool IsUnresolvableReference { get; }
    }
}