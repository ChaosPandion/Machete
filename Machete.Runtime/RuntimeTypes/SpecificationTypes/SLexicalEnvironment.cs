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


        public SLexicalEnvironment NewDeclarativeEnvironment()
        {
            return new SLexicalEnvironment(_environment, new SDeclarativeEnvironmentRecord(_environment), this);
        }

        public SLexicalEnvironment NewObjectEnvironment(LObject o)
        {
            Debug.Assert(o != null);
            return new SLexicalEnvironment(_environment, new SDeclarativeEnvironmentRecord(_environment), this);
        }
    }
}