using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Diagnostics;
using Machete.Core;
using System.Collections;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public sealed class SArgs : IArgs
    {
        private readonly IEnvironment _environment;
        private readonly IDynamic[] _items;


        public SArgs(IEnvironment environment, params IDynamic[] items)
        {
            _environment = environment;
            _items = items;
        }

        public SArgs(IEnvironment environment, IEnumerable<IDynamic> items)
        {
            _environment = environment;
            _items = items as IDynamic[];
            if (_items == null)
            {
                _items = items.ToArray();
            }
        }

        public SArgs(IEnvironment environment, IArgs head, IArgs tail)
        {
            _environment = environment;
            _items = head.Concat(tail).ToArray();
        }

        public SArgs(IEnvironment environment, IArgs head, params IDynamic[] items)
        {
            _environment = environment;
            _items = head.Concat(items).ToArray();
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


        public IEnumerator<IDynamic> GetEnumerator()
        {
            return _items.Cast<IDynamic>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}