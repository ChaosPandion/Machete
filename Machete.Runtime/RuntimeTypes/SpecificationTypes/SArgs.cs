using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Diagnostics;
using Machete.Interfaces;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public sealed class SArgs : IArgs
    {
        private readonly IEnvironment _environment;
        private readonly IDynamic[] _items;


        public SArgs(IEnvironment environment)
        {
            _environment = environment;
        }

        public IDynamic this[int index]
        {
            get
            {
                Debug.Assert(index >= 0);
                if (index > _items.Length - 1)
                {
                    return _environment.Undefined;
                }
                return _items[index];
            }
        }

        public int Count
        {
            get { return _items.Length; }
        }

        public bool IsEmpty
        {
            get { return _items.Length == 0; }
        }

        public SArgs(params IDynamic[] items)
        {
            Debug.Assert(items != null);
            Debug.Assert(!items.Any(o => o == null));
            _items = items;
        }
    }
}