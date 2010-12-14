using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Interfaces;

namespace Machete.Runtime.NativeObjects.BuiltinObjects
{
    public sealed class BJson : LObject
    {
        public BJson(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
