using System.Collections.Generic;

namespace Machete.Core
{
    public interface IArgs : IEnumerable<IDynamic>
    {
        IDynamic this[int index] { get; }
        int Count { get; }
        bool IsEmpty { get; }
    }
}
