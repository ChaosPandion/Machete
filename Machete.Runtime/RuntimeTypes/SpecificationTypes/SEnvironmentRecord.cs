using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.Interfaces;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public abstract class SEnvironmentRecord : IReferenceBase
    {
        public abstract bool HasBinding(string n);
        public abstract void CreateMutableBinding(string n, bool d);
        public abstract void SetMutableBinding(string n, IDynamic v, bool s);
        public abstract IDynamic GetBindingValue(string n, bool s);
        public abstract bool DeleteBinding(string n);
        public abstract IDynamic ImplicitThisValue();

        public IDynamic GetValue(string name, bool strict)
        {
            return GetBindingValue(name, strict);
        }

        public void SetValue(string name, IDynamic value, bool strict)
        {
            SetMutableBinding(name, value, strict);
        }
    }
}
