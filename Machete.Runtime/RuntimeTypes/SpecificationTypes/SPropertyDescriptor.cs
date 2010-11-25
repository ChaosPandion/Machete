using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.Interfaces;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public class SPropertyDescriptor
    {
        public IDynamic Value { get; set; }
        public bool? Writable { get; set; }
        public IDynamic Get { get; set; }
        public IDynamic Set { get; set; }
        public bool? Enumerable { get; set; }
        public bool? Configurable { get; set; }

        public bool IsAccessorDescriptor
        {
            get { return Get != null && Set != null; }
        }

        public bool IsDataDescriptor
        {
            get { return Value != null && Writable != null; }
        }

        public bool IsGenericDescriptor
        {
            get { return !IsAccessorDescriptor && !IsDataDescriptor; }
        }

        public bool IsEmpty
        {
            get { return Value == null && Writable == null && Get == null && Set == null && Enumerable == null && Configurable == null; }
        }


        public SPropertyDescriptor()
        {

        }

        public SPropertyDescriptor(IDynamic value)
            : this(value, false, false, false)
        {

        }

        public SPropertyDescriptor(IDynamic value, bool? writable)
            : this(value, writable, false, false)
        {

        }

        public SPropertyDescriptor(IDynamic value, bool? writable, bool? enumerable)
            : this(value, writable, enumerable, false)
        {

        }

        public SPropertyDescriptor(IDynamic value, bool? writable, bool? enumerable, bool? configurable)
        {
            Value = value;
            Writable = writable;
            Enumerable = enumerable;
            Configurable = configurable;
        }


        public bool Matches(SPropertyDescriptor other)
        {
            return Value == other.Value 
                && Writable == other.Writable 
                && Get == other.Get 
                && Set == other.Set 
                && Enumerable == other.Enumerable 
                && Configurable == other.Configurable;
        }

        public SPropertyDescriptor Copy()
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

        public static SPropertyDescriptor Create(IDynamic value)
        {
            return new SPropertyDescriptor()
            {
                Value = value,
                Writable = false,
                Enumerable = false,
                Configurable = false
            };
        }
    }
}
