using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Interfaces;

namespace Machete.Runtime.HostObjects
{
    public sealed class HInput : LObject
    {
        public HInput(IEnvironment environment)
            : base(environment)
        {
            Initialize();
        }

        public override void Initialize()
        {
            Class = "Input";
            Prototype = null;
            Extensible = true;
            base.Initialize();
        }

        public override IDynamic DefaultValue(string hint)
        {
            return Environment.CreateString("[object, Input]");
        }

        [NativeFunction("read"), DataDescriptor(false, false, false)]
        internal static IDynamic Read(IEnvironment environment, IArgs args)
        {
            return environment.CreateString(environment.Input.Read());
        }
    }
}