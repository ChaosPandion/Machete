using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Interfaces;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NArray : LObject
    {
        public NArray(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
