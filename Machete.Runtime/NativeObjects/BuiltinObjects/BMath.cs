using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Interfaces;

namespace Machete.Runtime.NativeObjects.BuiltinObjects
{
    public sealed class BMath : LObject
    {
        public BMath(IEnvironment environment)
            : base(environment)
        {

        }

    }
}
