using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Core;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NDate : LObject, IPrimitiveWrapper
    {
        public IDynamic PrimitiveValue { get; set; }

        public NDate(IEnvironment environment)
            : base(environment)
        {

        }

        public override IDynamic DefaultValue(string hint)
        {
            return base.DefaultValue(hint ?? "String");
        }
    }
}
