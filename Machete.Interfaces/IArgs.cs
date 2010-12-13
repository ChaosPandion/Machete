namespace Machete.Interfaces
{
    public interface IArgs
    {
        IDynamic this[int index] { get; }
        int Count { get; }
        bool IsEmpty { get; }
    }
}
