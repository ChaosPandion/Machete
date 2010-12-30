using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NString : LObject, IPrimitiveWrapper
    {
        public IDynamic PrimitiveValue { get; set; }

        public NString(IEnvironment environment)
            : base(environment)
        {

        }

        public override IPropertyDescriptor GetOwnProperty(string p)
        {
            // 15.5.5.2 [[GetOwnProperty]] ( P ) 

            var desc = base.GetOwnProperty(p);
            if (desc != null)
            {
                return desc;
            }
            var pString = Environment.CreateString(p);
            var index = pString.ConvertToUInt32();
            if (index.ConvertToString().BaseValue == p && index.BaseValue != 4294967295)
            {
                var str = ((IString)PrimitiveValue).BaseValue;
                var intIndex = (int)index.BaseValue;
                if (str.Length > intIndex)
                {
                    var resultStr = Environment.CreateString(str.Substring(intIndex, 1));
                    return Environment.CreateDataDescriptor(resultStr, false, true, false);
                }
            }
            return null;
        }
    }
}
