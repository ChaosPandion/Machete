using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.NativeObjects.BuiltinObjects;
using System.Diagnostics.Contracts;
using Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public sealed class SObjectEnvironmentRecord : SEnvironmentRecord
    {
        private readonly LObject _bindingObject;
        private readonly bool _provideThis;

        public SObjectEnvironmentRecord(LObject bindingObject, bool provideThis)
        {
            Contract.Assert(bindingObject != null);
            _bindingObject = bindingObject;
            _provideThis = provideThis;
        }

        public override bool HasBinding(string n)
        {
            Contract.Assert(n != null);
            return _bindingObject.HasProperty(n);
        }

        public override void CreateMutableBinding(string n, bool d)
        {
            Contract.Assert(n != null);
            var desc = new SPropertyDescriptor() { 
                Value = LUndefined.Value, 
                Writable = true, 
                Enumerable = true, 
                Configurable = d 
            };
            _bindingObject.DefineOwnProperty(n, desc, false);
        }

        public override void SetMutableBinding(string n, LType v, bool s)
        {
            Contract.Assert(n != null);
            Contract.Assert(v != null);
            _bindingObject.Put(n, v, s);
        }

        public override LType GetBindingValue(string n, bool s)
        {
            Contract.Assert(n != null);
            if (!_bindingObject.HasProperty(n))
            {
                if (!s) return LUndefined.Value;
                CReferenceError.Instance.Value.Op_Construct(SList.Empty).Op_Throw();
            }
            return _bindingObject.Get(n);
        }

        public override bool DeleteBinding(string n)
        {
            Contract.Assert(n != null);
            return _bindingObject.Delete(n, false);
        }

        public override LType ImplicitThisValue()
        {
            return _provideThis ? (LType)_bindingObject : (LType)LUndefined.Value;
        }
    }
}
