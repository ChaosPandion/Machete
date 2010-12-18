using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Diagnostics;
using Machete.Interfaces;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public sealed class SLexicalEnvironment : ILexicalEnvironment
    {
        private readonly IEnvironment _environment;
        public IEnvironmentRecord Record { get; set; }
        public ILexicalEnvironment Parent { get; set; }


        public SLexicalEnvironment(IEnvironment environment, IEnvironmentRecord record, ILexicalEnvironment parent)
        {
            _environment = environment;
            Record = record;
            Parent = parent;
        }

        public IReference GetIdentifierReference(string name, bool strict)
        {
            if (Record.HasBinding(name))
            {
                return _environment.CreateReference(name, Record, strict);
            }
            else if (Parent == null)
            {
                return _environment.CreateReference(name, _environment.Undefined, strict);
            }
            else
            {
                return Parent.GetIdentifierReference(name, strict);
            }
        }

        public SLexicalEnvironment NewDeclarativeEnvironment()
        {
            return new SLexicalEnvironment(_environment, new SDeclarativeEnvironmentRecord(_environment), this);
        }

        public SLexicalEnvironment NewObjectEnvironment(LObject o)
        {
            Debug.Assert(o != null);
            return new SLexicalEnvironment(_environment, new SDeclarativeEnvironmentRecord(_environment), this);
        }


        ILexicalEnvironment ILexicalEnvironment.NewDeclarativeEnvironment()
        {
            return new SLexicalEnvironment(_environment, new SDeclarativeEnvironmentRecord(_environment), this);
        }


        public ILexicalEnvironment NewObjectEnvironment(IObject bindingObject, bool provideThis)
        {
            return new SLexicalEnvironment(_environment, new SObjectEnvironmentRecord(_environment, bindingObject, provideThis), this);
        }
    }
}