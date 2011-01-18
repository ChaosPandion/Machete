using System.Diagnostics;
using Machete.Core;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    [DebuggerStepThrough]
    public sealed class SPropertyDescriptor : IPropertyDescriptor
    {
        public IDynamic Value { get; set; }
        public bool? Writable { get; set; }
        public IDynamic Get { get; set; }
        public IDynamic Set { get; set; }
        public bool? Enumerable { get; set; }
        public bool? Configurable { get; set; }

        public bool IsAccessorDescriptor
        {
            get { return !(Get == null && Set == null); }
        }

        public bool IsDataDescriptor
        {
            get { return !(Value == null && Writable == null); }
        }

        public bool IsGenericDescriptor
        {
            get { return !IsAccessorDescriptor && !IsDataDescriptor; }
        }

        public bool IsEmpty
        {
            get { return Value == null && Writable == null && Get == null && Set == null && Enumerable == null && Configurable == null; }
        }
        

        public IPropertyDescriptor Copy()
        {
            return new SPropertyDescriptor()
            {
                Value = Value,
                Writable = Writable,
                Get = Get,
                Set = Set,
                Enumerable = Enumerable,
                Configurable = Configurable
            };
        }

        public bool Equals(IPropertyDescriptor other)
        {
            return Value == other.Value
                && Writable == other.Writable
                && Get == other.Get
                && Set == other.Set
                && Enumerable == other.Enumerable
                && Configurable == other.Configurable;
        }
    }
}