using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NRegExp : LObject
    {
        public RegExp.RegExp RegExp { get; set; }


        public NRegExp(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Put(string p, IDynamic value, bool @throw)
        {
            if (p == "lastIndex")
            {
                var lastIndex = (int)value.Value.ConvertToInt32().BaseValue;
                RegExp.LastIndex = lastIndex;
            }
            base.Put(p, value, @throw);
        }
    }
}