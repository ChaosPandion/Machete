namespace Machete.Core
{
    public interface IBoolean : IDynamic, IReferenceBase
    {
        bool BaseValue { get; }
    }
}