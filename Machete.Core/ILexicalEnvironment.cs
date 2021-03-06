﻿namespace Machete.Core
{
    public interface ILexicalEnvironment
    {
        IEnvironmentRecord Record { get; set; }
        ILexicalEnvironment Parent { get; set; }

        IReference GetIdentifierReference(string name, bool strict);
        ILexicalEnvironment NewDeclarativeEnvironment();
        ILexicalEnvironment NewObjectEnvironment(IObject bindingObject, bool provideThis);
    }
}