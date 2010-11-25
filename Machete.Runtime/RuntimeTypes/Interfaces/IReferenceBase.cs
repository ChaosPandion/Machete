using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Machete.Runtime.RuntimeTypes.Interfaces
{
    public interface IReferenceBase
    {
        IDynamic GetValue(string name, bool strict);
        void SetValue(string name, IDynamic value, bool strict);
    }
}
