using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.Interfaces;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NNumber : LObject, IPrimitiveWrapper
    {
        IDynamic PrimitiveValue { get; set; }

        public NNumber(LNumber value)
        {
            PrimitiveValue = value;
        }
    }
}
