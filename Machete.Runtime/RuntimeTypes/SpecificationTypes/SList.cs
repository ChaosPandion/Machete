using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Diagnostics;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public class SList : SType
    {
        private readonly LType[] _items;
        public static readonly SList Empty = new SList();

        public LType this[int index]
        {
            get
            {
                Debug.Assert(index >= 0);
                if (index > _items.Length - 1)
                {
                    return LUndefined.Value;
                }
                return _items[index];
            }
        }

        public SList(params LType[] items)
        {
            Debug.Assert(items != null);
            Debug.Assert(!items.Any(o => o == null));
            _items = items;
        }
    }
}