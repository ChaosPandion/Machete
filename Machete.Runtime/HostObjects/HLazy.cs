using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Core;

namespace Machete.Runtime.HostObjects
{
    public sealed class HLazy : LObject
    {
        private readonly Lazy<IObject> _underlying;

        public HLazy(IEnvironment environment, Func<IObject> createValue)
            : base(environment)
        {
            _underlying = new Lazy<IObject>(createValue);
        }

        public override IPropertyDescriptor GetOwnProperty(string p)
        {
            return _underlying.Value.GetOwnProperty(p);
        }

        public override IPropertyDescriptor GetProperty(string p)
        {
            return _underlying.Value.GetProperty(p);
        }

        public override IDynamic Get(string p)
        {
            return _underlying.Value.Get(p);
        }

        public override bool CanPut(string p)
        {
            return _underlying.Value.CanPut(p);
        }

        public override bool HasProperty(string p)
        {
            return _underlying.Value.HasProperty(p);
        }

        public override IDynamic DefaultValue(string hint)
        {
            return _underlying.Value.DefaultValue(hint);
        }

        public override bool DefineOwnProperty(string p, IPropertyDescriptor desc, bool @throw)
        {
            return _underlying.Value.DefineOwnProperty(p, desc, @throw);
        }
    }
}