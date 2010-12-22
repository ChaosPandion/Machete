using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Machete.Interfaces
{
    public class ReadOnlyList<T> : IEnumerable<T>
    {
        private readonly T[] _items;
        public static readonly ReadOnlyList<T> Empty = new ReadOnlyList<T>(new T[0]);
        

        public ReadOnlyList(params T[] items)
        {
            _items = items;
        }

        public ReadOnlyList(IEnumerable<T> items)
        {
            _items = items.ToArray();
        }


        public T this[int index]
        {
            get
            {
                return _items[index];
            }
        }

        public int Count
        {
            get
            {
                return _items.Length;
            }
        }


        public IEnumerator<T> GetEnumerator()
        {
            return _items.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.Cast<T>().GetEnumerator();
        }
    }
}