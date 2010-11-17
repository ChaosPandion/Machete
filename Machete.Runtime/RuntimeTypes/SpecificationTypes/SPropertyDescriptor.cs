using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public class SPropertyDescriptor : SType
    {
        public LType Value { get; set; }
        public bool? Writable { get; set; }
        public LType Get { get; set; }
        public LType Set { get; set; }
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
    }
}
