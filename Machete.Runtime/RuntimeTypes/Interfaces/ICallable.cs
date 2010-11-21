using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;

namespace Machete.Runtime.RuntimeTypes.Interfaces
{
    public interface ICallable
    {
        LType Call(LType @this, SList args);
    }
}
