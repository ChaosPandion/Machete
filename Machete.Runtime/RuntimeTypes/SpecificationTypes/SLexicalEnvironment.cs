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
        public IEnvironmentRecord Record { get; set; }
        public ILexicalEnvironment Parent { get; set; }


        public SLexicalEnvironment(IEnvironmentRecord record, ILexicalEnvironment parent)
        {
            Debug.Assert(record != null);
            Debug.Assert(parent != null);
            Record = record;
            Parent = parent;
        }


        public SLexicalEnvironment NewDeclarativeEnvironment()
        {
            return new SLexicalEnvironment(new SDeclarativeEnvironmentRecord(), this);
        }

        public SLexicalEnvironment NewObjectEnvironment(LObject o)
        {
            Debug.Assert(o != null);
            return new SLexicalEnvironment(new SDeclarativeEnvironmentRecord(), this);
        }
    }
}