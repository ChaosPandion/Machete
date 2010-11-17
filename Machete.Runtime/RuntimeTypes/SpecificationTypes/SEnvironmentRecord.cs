using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public abstract class SEnvironmentRecord : SType
    {
        public abstract bool HasBinding(string n);
        public abstract void CreateMutableBinding(string n, bool d);
        public abstract void SetMutableBinding(string n, LType v, bool s);
        public abstract LType GetBindingValue(string n, bool s);
        public abstract bool DeleteBinding(string n);
        public abstract LType ImplicitThisValue();
    }
}
