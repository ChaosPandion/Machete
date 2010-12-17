using System.Collections.Generic;

namespace Machete.Interfaces
{
    public interface IArgs : IEnumerable<IDynamic>
    {
        IDynamic this[int index] { get; }
        int Count { get; }
        bool IsEmpty { get; }
    }
}
