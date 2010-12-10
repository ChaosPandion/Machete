using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.RuntimeTypes.Interfaces;

namespace Machete.Runtime
{
    public delegate IDynamic Code(ExecutionContext context, SList args);
}
