using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NArray : LObject
    {
        public NArray(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Prototype = Environment.ArrayPrototype;
            Class = "Array";
            Extensible = true;
            base.DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(0.0), true, false, false), false);
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

                var newWritable = true;
                if (newLenDesc.Writable != null && !newLenDesc.Writable.Value)
                {
                    newWritable = false;
                    newLenDesc.Writable = true;
                }

                var succeeded = base.DefineOwnProperty("length", newLenDesc, @throw);

                if (!succeeded)
                {
                    return false;
                }

                var nl = newLen.BaseValue;
                var ol = oldLen.ConvertToUInt32().BaseValue;

                while (nl < ol)
                {
                    --ol;

                    var cannotDelete = Delete(ol.ToString(), false);
                    if (cannotDelete)
                    {
                        newLenDesc.Value = Environment.CreateNumber(ol + 1.0);
                        if (!newWritable)
                        {
                            newLenDesc.Writable = false;
                        }
                        base.DefineOwnProperty("length", newLenDesc, false);
                        if (!@throw) return false;
                        throw Environment.CreateTypeError("");
                    }
                }

                if (!newWritable)
                {
                    base.DefineOwnProperty("length", Environment.CreateDataDescriptor(null, false), false);
                }

                return true;
            }

            var index = Environment.CreateString(p).ConvertToUInt32();
            if (index.ConvertToString().BaseValue == p && index.BaseValue != 4294967295)
            {
                var oldLenVal = oldLen.ConvertToNumber().BaseValue;
                if (index.BaseValue >= oldLenVal && (!(oldLenDesc.Writable ?? false)))
                {
                    if (!@throw) return false;
                    throw Environment.CreateTypeError("");
                }

                var succeeded = base.DefineOwnProperty(p, desc, @throw);
                if (!succeeded)
                {
                    if (!@throw) return false;
                    throw Environment.CreateTypeError("");
                }

                if (index.BaseValue >= oldLenVal)
                {
                    oldLenDesc.Value = Environment.CreateNumber(index.BaseValue + 1.0);
                    base.DefineOwnProperty("length", oldLenDesc, false);
                }

                return true;
            }
            

            return base.DefineOwnProperty(p, desc, @throw);
        }
    }
}