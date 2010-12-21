using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NArray : LObject
    {
        public NArray(IEnvironment environment)
            : base(environment)
        {

        }

        public override bool DefineOwnProperty(string p, IPropertyDescriptor desc, bool @throw)
        {
            var oldLenDesc = GetOwnProperty("length");
            var oldLen = oldLenDesc.Value;

            if (p == "length")
            {
                if (desc.Value == null)
                {
                    return base.DefineOwnProperty(p, desc, @throw);
                }

                var newLenDesc = ((SPropertyDescriptor)desc).Copy();
                var newLen = desc.Value.ConvertToUInt32();

                if (desc.Value.ConvertToNumber().BaseValue != newLen.BaseValue)
                {
                    throw Environment.CreateRangeError("");
                }

                newLenDesc.Value = newLen;

                if (newLen.BaseValue >= oldLen.ConvertToNumber().BaseValue)
                {
                    return base.DefineOwnProperty(p, newLenDesc, @throw); 
                }

                if (oldLenDesc.Writable ?? false)
                {
                    if (!@throw) return false;
                    throw Environment.CreateTypeError("");
                }
            }

            throw Environment.CreateRangeError("");
        }
    }
}
