using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Threading;

namespace Machete.Runtime.NativeObjects.BuiltinObjects
{
    public abstract class BObject<T> : LObject 
        where T : BObject<T> 
    {
        public static readonly ThreadLocal<T> Instance = new ThreadLocal<T>();
    }
}
