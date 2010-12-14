using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Interfaces;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NString : LObject, IPrimitiveWrapper
    {
        public IDynamic PrimitiveValue { get; set; }

        public NString(IEnvironment environment, LString value)
            : base(environment)
        {
            PrimitiveValue = value;
        }
    }
}
