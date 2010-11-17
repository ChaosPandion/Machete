using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Diagnostics;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public class SLexicalEnvironment : SType
    {
        public SEnvironmentRecord Record { get; private set; }
        public SLexicalEnvironment Parent { get; private set; }


        public SLexicalEnvironment(SEnvironmentRecord record, SLexicalEnvironment parent)
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