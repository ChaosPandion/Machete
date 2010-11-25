using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.RuntimeTypes.Interfaces
{
    public interface IPrimitiveWrapper
    {
        IDynamic PrimitiveValue { get; set; }
    }
}
