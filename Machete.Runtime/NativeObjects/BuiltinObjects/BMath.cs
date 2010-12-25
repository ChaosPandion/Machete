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

        public override void Initialize()
        {
            Class = "Math";
            Extensible = true;
            base.Initialize();
        }

        [NativeFunction("fact", "n")]
        internal static IDynamic Fact(IEnvironment environment, IArgs args)
        {
            double r = 1.0, n = Math.Truncate(args[0].ConvertToNumber().BaseValue);
            for (double i = n; i > 1; --i)
            {
                r *= i;
            }
            return environment.CreateNumber(r);
        }
    }
}
