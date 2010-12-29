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
    }
}