using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Interfaces;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NRangeError : LObject
    {
        public NRangeError(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
