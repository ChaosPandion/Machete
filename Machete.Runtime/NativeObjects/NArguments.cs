using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Interfaces;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NArguments : LObject
    {
        public LObject ParameterMap { get; set; }

        public NArguments(IEnvironment environment)
            : base(environment)
        {

        }
    }
}
