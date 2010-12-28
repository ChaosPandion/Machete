using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PRegExp : LObject
    {
        public PRegExp(IEnvironment environment)
            : base(environment)
        {

        }


        [NativeFunction("exec", "string"), DataDescriptor(true, false, true)]
        internal static IDynamic Exec(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("test", "string"), DataDescriptor(true, false, true)]
        internal static IDynamic Test(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("toString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
