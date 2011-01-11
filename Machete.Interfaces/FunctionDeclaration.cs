using System;

namespace Machete.Interfaces
{
    public sealed class FunctionDeclaration
    {
        public string Identifier { get; private set; }
        public ReadOnlyList<string> FormalParameterList { get; private set; }
        public ExecutableCode ExecutableCode { get; private set; }

        public FunctionDeclaration(string identifier, ReadOnlyList<string> formalParameterList, ExecutableCode executableCode)
        {
            Identifier = identifier;
            FormalParameterList = formalParameterList;
            ExecutableCode = executableCode;
        }
    }
}