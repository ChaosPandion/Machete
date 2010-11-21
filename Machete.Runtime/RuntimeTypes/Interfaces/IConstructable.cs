using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;

namespace Machete.Runtime.RuntimeTypes.Interfaces
{
    public interface IConstructable
    {
        LObject Construct(SList args);
    }
}
