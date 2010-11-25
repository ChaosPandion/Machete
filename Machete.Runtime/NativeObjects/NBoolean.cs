﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.Interfaces;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NBoolean : LObject, IPrimitiveWrapper
    {        
        IDynamic PrimitiveValue { get; set; }

        public NBoolean(LBoolean value)
        {
            PrimitiveValue = value;
        }
    }
}
