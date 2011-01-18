using System;

namespace Machete.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class BuiltinFunctionAttribute : Attribute
    {
        public string Identifier { get; private set; }
        public ReadOnlyList<string> FormalParameterList { get; private set; }

        public BuiltinFunctionAttribute(string identifier, params string[] formalParameterList)
        {
            Identifier = identifier;
            FormalParameterList = new ReadOnlyList<string>(formalParameterList);
        }
    }
}
