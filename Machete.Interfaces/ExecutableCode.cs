using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Machete.Interfaces
{
    public sealed class ExecutableCode
    {
        public readonly Code Code;
        public readonly ReadOnlyList<string> VariableDeclarations;
        public readonly ReadOnlyList<FunctionDeclaration> FunctionDeclarations;
        public readonly bool Strict;

        public ExecutableCode(
            Code code, 
            ReadOnlyList<string> variableDeclarations,
            ReadOnlyList<FunctionDeclaration> functionDeclarations,
            bool strict)
        {
            Code = code;
            VariableDeclarations = variableDeclarations;
            FunctionDeclarations = functionDeclarations;
            Strict = strict;
        }
    }
}