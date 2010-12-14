using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.NativeObjects.BuiltinObjects;
using Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects;
using Machete.Interfaces;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public sealed class SDeclarativeEnvironmentRecord : IDeclarativeEnvironmentRecord
    {
        private readonly IEnvironment _environment;
        private readonly Dictionary<string, Binding> _bindings = new Dictionary<string, Binding>();


        public SDeclarativeEnvironmentRecord(IEnvironment environment)
        {
            _environment = environment;
        }


        public bool HasBinding(string n)
        {
            return _bindings.ContainsKey(n);
        }

        public void CreateMutableBinding(string n, bool d)
        {
            _bindings.Add(n, new Binding(_environment.Undefined, d ? BFlags.Deletable : BFlags.None));
        }

        public void SetMutableBinding(string n, IDynamic v, bool s)
        {
            var binding = _bindings[n];
            if ((binding.Flags & BFlags.Immutable) == BFlags.Immutable)
            {
                _environment.CreateTypeError().Op_Throw();
            }
            binding.Value = v;
        }

        public IDynamic GetBindingValue(string n, bool s)
        {
            var binding = _bindings[n];
            if ((binding.Flags & BFlags.Uninitialized) == BFlags.Uninitialized)
            {
                if (!s) return _environment.Undefined;
                _environment.CreateReferenceError().Op_Throw();
                return null;
            }
            return binding.Value;
        }

        public bool DeleteBinding(string n)
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

        public IDynamic ImplicitThisValue()
        {
            return _environment.Undefined;
        }

        public IDynamic Get(string name, bool strict)
        {
            return GetBindingValue(name, strict);
        }

        public void Set(string name, IDynamic value, bool strict)
        {
            SetMutableBinding(name, value, strict);
        }

        public void CreateImmutableBinding(string n)
        {
            _bindings.Add(n, new Binding(_environment.Undefined, BFlags.Immutable | BFlags.Uninitialized));
        }

        public void InitializeImmutableBinding(string n, IDynamic v)
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
            public IDynamic Value;
            public BFlags Flags;

            public Binding(IDynamic value, BFlags flags)
            {
                Value = value;
                Flags = flags;
            }
        }
    }
}
