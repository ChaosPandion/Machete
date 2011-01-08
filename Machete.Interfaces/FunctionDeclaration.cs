using System;

namespace Machete.Interfaces
{
    public sealed class FunctionDeclaration
    {
        public string Identifier { get; private set; }
        public ReadOnlyList<string> FormalParameterList { get; private set; }
        public Lazy<Code> Code { get; private set; }
        public bool Strict { get; private set; }

        public FunctionDeclaration(string identifier, ReadOnlyList<string> formalParameterList, Lazy<Code> code, bool strict)
        {
            Identifier = identifier;
            FormalParameterList = formalParameterList;
            Code = code;
            Strict = strict;
        }
    }
}