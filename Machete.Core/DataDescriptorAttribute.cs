using System;

namespace Machete.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class DataDescriptorAttribute : Attribute
    {
        public bool? Writable { get; private set; }
        public bool? Enumerable { get; private set; }
        public bool? Configurable { get; private set; }

        public DataDescriptorAttribute(bool writable)
        {
            Writable = writable;
        }

        public DataDescriptorAttribute(bool writable, bool enumerable)
        {
            Writable = writable;
            Enumerable = enumerable;
        }

        public DataDescriptorAttribute(bool writable, bool enumerable, bool configurable)
        {
            Writable = writable;
            Enumerable = enumerable;
            Configurable = configurable;
        }
    }
}
