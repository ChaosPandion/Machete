using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.NativeObjects.BuiltinObjects;
using System.Diagnostics.Contracts;
using Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects;
using Machete.Interfaces;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public sealed class SObjectEnvironmentRecord : IObjectEnvironmentRecord
    {
        private readonly LObject _bindingObject;
        private readonly bool _provideThis;

        public SObjectEnvironmentRecord(LObject bindingObject, bool provideThis)
        {
            Contract.Assert(bindingObject != null);
            _bindingObject = bindingObject;
            _provideThis = provideThis;
        }

        public bool HasBinding(string n)
        {
            Contract.Assert(n != null);
            return _bindingObject.HasProperty(n);
        }

        public void CreateMutableBinding(string n, bool d)
        {
            Contract.Assert(n != null);
            var desc = new SPropertyDescriptor() { 
                Value = LUndefined.Instance, 
                Writable = true, 
                Enumerable = true, 
                Configurable = d 
            };
            _bindingObject.DefineOwnProperty(n, desc, false);
        }

        public void SetMutableBinding(string n, IDynamic v, bool s)
        {
            Contract.Assert(n != null);
            Contract.Assert(v != null);
            _bindingObject.Put(n, v, s);
        }

        public IDynamic GetBindingValue(string n, bool s)
        {
            Contract.Assert(n != null);
            if (!_bindingObject.HasProperty(n))
            {
                if (!s) return LUndefined.Instance;
                Environment.ThrowReferenceError();
            }
            return _bindingObject.Get(n);
        }

        public bool DeleteBinding(string n)
        {
            Contract.Assert(n != null);
            return _bindingObject.Delete(n, false);
        }

        public IDynamic ImplicitThisValue()
        {
            return _provideThis ? (IDynamic)_bindingObject : (IDynamic)LUndefined.Instance;
        }

        public IDynamic Get(string name, bool strict)
        {
            return GetBindingValue(name, strict);
        }

        public void Set(string name, IDynamic value, bool strict)
        {
            SetMutableBinding(name, value, strict);
        }
    }
}
