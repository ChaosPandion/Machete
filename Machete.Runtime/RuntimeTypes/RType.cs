using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.NativeObjects.BuiltinObjects;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects;

namespace Machete.Runtime.RuntimeTypes
{
    public abstract class RType
    {
        public virtual LType Value
        {
            get
            {
                return (LType)this;
            }
            set
            {
                Engine.ThrowReferenceError();
            }
        }
    }
}