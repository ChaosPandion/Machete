using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.Interfaces;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NString : LObject, IPrimitiveWrapper
    {
        IDynamic PrimitiveValue { get; set; }

        public NString(LString value)
        {
            PrimitiveValue = value;
        }
    }
}
