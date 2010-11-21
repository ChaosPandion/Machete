using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.NativeObjects.BuiltinObjects;
using Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public sealed class SDeclarativeEnvironmentRecord : SEnvironmentRecord
    {
        private readonly Dictionary<string, Binding> _bindings = new Dictionary<string, Binding>();


        public override bool HasBinding(string n)
        {
            return _bindings.ContainsKey(n);
        }

        public override void CreateMutableBinding(string n, bool d)
        {
            _bindings.Add(n, new Binding(d ? BFlags.Deletable : BFlags.None));
        }

        public override void SetMutableBinding(string n, LType v, bool s)
        {
            var binding = _bindings[n];
            if ((binding.Flags & BFlags.Immutable) == BFlags.Immutable)
            {
                Engine.ThrowTypeError();
            }
            binding.Value = v;
        }

        public override LType GetBindingValue(string n, bool s)
        {
            var binding = _bindings[n];
            if ((binding.Flags & BFlags.Uninitialized) == BFlags.Uninitialized)
            {
                if (!s) return LUndefined.Value;
                Engine.ThrowReferenceError();
            }
            return binding.Value;
        }

        public override bool DeleteBinding(string n)
        {
            var binding = default(Binding);
            if (!_bindings.TryGetValue(n, out binding))
            {
                return true;
            }
            if ((binding.Flags & BFlags.Deletable) != BFlags.Deletable)
            {
                return false;
            }
            _bindings.Remove(n);
            return true;
        }

        public override LType ImplicitThisValue()
        {
            return LUndefined.Value;
        }

        public void CreateImmutableBinding(string n)
        {
            _bindings.Add(n, new Binding(BFlags.Immutable | BFlags.Uninitialized));
        }

        public void InitializeImmutableBinding(string n, LType v)
        {
            _bindings[n].Value = v;
        }


        [Flags]
        private enum BFlags
        {
            None,
            Deletable,
            Immutable,
            Initialized,
            Uninitialized,
        }

        private sealed class Binding
        {
            public LType Value;
            public BFlags Flags;

            public Binding(BFlags flags)
            {
                Value = LUndefined.Value;
                Flags = flags;
            }
        }
    }
}
