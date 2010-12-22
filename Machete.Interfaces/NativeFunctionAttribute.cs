using System;
using System.Diagnostics.Contracts;

namespace Machete.Interfaces
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class NativeFunctionAttribute : Attribute
    {
        public string Identifier { get; private set; }
        public ReadOnlyList<string> FormalParameterList { get; private set; }

        public NativeFunctionAttribute(string identifier, params string[] formalParameterList)
        {
            Identifier = identifier;
            FormalParameterList = new ReadOnlyList<string>(formalParameterList);
        }
    }
}
