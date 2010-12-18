using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Machete.Runtime.NativeObjects
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class NativeFunctionAttribute : Attribute
    {
        public string Identifier { get; set; }
        public string[] FormalParameterList { get; set; }

        public NativeFunctionAttribute(string identifier, params string[] formalParameterList)
        {
            Identifier = identifier;
            FormalParameterList = formalParameterList;
        }
    }
}
