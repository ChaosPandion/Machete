using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.NativeObjects.BuiltinObjects;
using Machete.Runtime.NativeObjects;
using Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects;

namespace Machete.Runtime.RuntimeTypes.SpecificationTypes
{
    public class SReference : SType
    {
        private readonly RType _base;
        private readonly string _referencedName;
        private readonly bool _strictReference;


        public bool HasPrimitiveBase
        {
            get { return _base is LBoolean || _base is LString || _base is LNumber; }
        }

        public bool IsPropertyReference
        {
            get { return _base is LObject || HasPrimitiveBase; }
        }

        public bool IsUnresovableReference
        {
            get { return _base is LUndefined; }
        }

        public override LType Value
        {
            get
            {
                if (IsUnresovableReference)
                {
                    Engine.ThrowReferenceError();
                }

                if (!IsPropertyReference)
                {
                    return ((SEnvironmentRecord)_base).GetBindingValue(_referencedName, _strictReference);
                }
                else
                {
                    if (!HasPrimitiveBase)
                    {
                        return ((LObject)_base).Get(_referencedName);
                    }

                    var o = ((LType)_base).ConvertToObject();
                    var desc = o.GetProperty(_referencedName);
                    if (desc == null)
                    {
                        return LUndefined.Value;
                    }
                    else
                    {
                        if (desc.IsDataDescriptor)
                        {
                            return desc.Value;
                        }
                        else if (desc.Get is LUndefined)
                        {
                            return LUndefined.Value;
                        }
                        else
                        {
                            return ((LObject)desc.Get).Call((LType)_base, SList.Empty);
                        }
                    }
                }
            }
            set
            {
                if (IsUnresovableReference)
                {
                    if (_strictReference)
                    {
                        Engine.ThrowReferenceError();
                    }
                    Engine.Instance.Value.GlobalObject.Put(_referencedName, value, false);
                }
                else if (!IsPropertyReference)
                {
                    ((SEnvironmentRecord)_base).SetMutableBinding(_referencedName, value, _strictReference);
                }
                else
                {
                    if (!HasPrimitiveBase)
                    {
                        ((LObject)_base).Put(_referencedName, value, _strictReference);
                    }
                }

                var o = ((LType)_base).ConvertToObject();
                if (!o.CanPut(_referencedName))
                {
                    if (!_strictReference) return;
                    Engine.ThrowTypeError();
                }
                var ownDesc = o.GetOwnProperty(_referencedName);
                if (ownDesc == null || ownDesc.IsDataDescriptor)
                {
                    if (!_strictReference) return;
                    Engine.ThrowTypeError();
                }
                var desc = o.GetProperty(_referencedName);
                if (desc.IsAccessorDescriptor)
                {
                    ((LObject)desc.Set).Call((LType)_base, new SList(value));
                }

                if (_strictReference)
                {
                    Engine.ThrowTypeError();
                }
            }
        }


        public SReference(RType @base, string referencedName, bool strictReference)
        {
            _base = @base;
            _referencedName = referencedName;
            _strictReference = strictReference;
        }
    }
}