using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.NativeObjects;
using Machete.Interfaces;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public class LObject : IObject
    {
        private readonly Dictionary<string, SPropertyDescriptor> _map = new Dictionary<string, SPropertyDescriptor>();

        public static readonly LString ObjectString = new LString("object");

        
        public IObject Prototype { get; set; }

        public string Class { get; set; }

        public bool Extensible { get; set; }

        public LanguageTypeCode TypeCode
        {
            get { return LanguageTypeCode.Object; }
        }

        public bool IsPrimitive
        {
            get { return false; }
        }

        public IDynamic Value
        {
            get { return this; }
            set { }
        }
        

        public virtual IPropertyDescriptor GetOwnProperty(string p)
        {
            SPropertyDescriptor value;
            if (_map.TryGetValue(p, out value))
            {
                return value.Copy();
            }
            else
            {
                return null;
            }
        }

        public virtual IPropertyDescriptor GetProperty(string p)
        {
            var prop = GetOwnProperty(p);
            if (prop != null)
            {
                return prop;
            }
            else if (Prototype == null)
            {
                return null;
            }
            else
            {
                return ((LObject)Prototype).GetProperty(p);
            }
        }

        public virtual IDynamic Get(string p)
        {
            var desc = GetProperty(p);
            if (desc == null)
            {
                return LUndefined.Instance;
            }
            else if (desc.IsDataDescriptor)
            {
                return desc.Value;
            }
            else if (desc.Get is LUndefined)
            {
                return LUndefined.Instance;
            }
            else
            {
                return ((ICallable)desc.Get).Call(null, this, SArgs.Empty);
            }
        }

        public virtual bool CanPut(string p)
        {
            var desc = GetOwnProperty(p);
            if (desc != null)
            {
                if (desc.IsAccessorDescriptor)
                {
                    return !(desc.Set is LUndefined);
                }
                else
                {
                    return true;
                }
            }
            else if (Prototype == null)
            {
                return Extensible;
            }

            var inherited = Prototype.GetProperty(p);
            if (inherited == null)
            {
                return Extensible;
            }
            else if (inherited.IsAccessorDescriptor)
            {
                return !(inherited.Set is LUndefined);
            }
            else if (Extensible)
            {
                return inherited.Writable.Value;
            }
            else
            {
                return false;
            }
        }

        public virtual void Put(string p, IDynamic value, bool @throw)
        {
            if (!CanPut(p))
            {
                if (@throw)
                {
                    throw Environment.ThrowTypeError();
                }
                return;
            }
            var ownDesc = GetOwnProperty(p);
            if (ownDesc.IsDataDescriptor)
            {
                var valueDesc = new SPropertyDescriptor() { Value = value };
                DefineOwnProperty(p, valueDesc, @throw);
            }
            var desc = GetProperty(p);
            if (desc.IsAccessorDescriptor)
            {
                ((ICallable)desc.Set).Call(null, this, new SArgs(value));
            }
            var newDesc = new SPropertyDescriptor(value, true, true, true);
            DefineOwnProperty(p, newDesc, @throw);
        }

        public virtual bool HasProperty(string p)
        {
            var desc = GetProperty(p);
            return desc != null;
        }

        public virtual bool Delete(string p, bool @throw)
        {
            var desc = GetOwnProperty(p);
            if (desc == null)
            {
                return true;
            }
            else if (desc.Configurable.GetValueOrDefault())
            {
                return _map.Remove(p);
            }
            else if (@throw)
            {
                throw Environment.ThrowTypeError();
            }
            else
            {
                return false;
            }
        }

        public virtual IDynamic DefaultValue(string hint)
        {
            if (hint == "String")
            {
                var func = Get("toString") as ICallable ?? Get("valueOf") as ICallable;
                if (func != null)
                {
                    var result = func.Call(null, this, SArgs.Empty);
                    if (result.IsPrimitive)
                    {
                        return result;
                    }
                }
                throw Environment.ThrowTypeError();
            }
            else
            {
                var func = Get("valueOf") as ICallable ?? Get("toString") as ICallable;
                if (func != null)
                {
                    var result = func.Call(null, this, SArgs.Empty);
                    if (result.IsPrimitive)
                    {
                        return result;
                    }
                }
                throw Environment.ThrowTypeError();  
            }
        }

        public virtual bool DefineOwnProperty(string p, IPropertyDescriptor desc, bool @throw)
        {
            var reject = new Func<bool>(() => {
                if (!@throw) return false;
                throw Environment.ThrowTypeError();
            });
            var current = GetOwnProperty(p);
            if (current == null)
            {
                if (!Extensible)
                {
                    return reject();
                }
                _map.Add(p,
                    desc.IsGenericDescriptor || desc.IsDataDescriptor 
                    ? new SPropertyDescriptor()
                        {
                            Value = desc.Value ?? LUndefined.Instance,
                            Writable = desc.Writable ?? false,
                            Enumerable = desc.Enumerable ?? false,
                            Configurable = desc.Configurable ?? false
                        } 
                    : new SPropertyDescriptor()
                        {
                            Get = desc.Get ?? LUndefined.Instance,
                            Set = desc.Set ?? LUndefined.Instance,
                            Enumerable = desc.Enumerable ?? false,
                            Configurable = desc.Configurable ?? false
                        }
                );
                return true;
            }
            else if (desc.IsEmpty || ((SPropertyDescriptor)current).Matches(desc))
            {
                return true;
            }
            else if (!current.Configurable.GetValueOrDefault())
            {
                if (desc.Configurable.GetValueOrDefault())
                {
                    return reject();
                }
                else if (desc.Enumerable != null && desc.Enumerable.GetValueOrDefault() ^ current.Enumerable.GetValueOrDefault())
                {
                    return reject();
                }
            }
            else if (!desc.IsGenericDescriptor)
            {
                if (current.IsDataDescriptor ^ desc.IsDataDescriptor)
                {
                    if (!current.Configurable.Value)
                    {
                        return reject();
                    }
                    else if (current.IsDataDescriptor)
                    {
                        current.Value = null;
                        current.Writable = null;
                        current.Get = desc.Get;
                        current.Set = desc.Set;
                    }
                    else
                    {
                        current.Value = desc.Value;
                        current.Writable = desc.Writable;
                        current.Get = null;
                        current.Set = null;
                    }
                }
                else if (current.IsDataDescriptor && desc.IsDataDescriptor)
                {
                    if (!current.Configurable.Value)
                    {
                        if (!desc.Writable.Value && current.Writable.Value)
                        {
                            return reject();
                        }
                        else if (!current.Writable.Value && current.Value != desc.Value)
                        {
                            return reject();
                        }
                    }
                }
                else
                {
                    if (!current.Configurable.Value)
                    {
                        if (desc.Set != null && desc.Set != current.Set)
                        {
                            return reject();
                        }
                        else if (desc.Get != null && desc.Get != current.Get)
                        {
                            return reject();
                        }
                    }
                }
            }
            current.Value = desc.Value ?? current.Value;
            current.Writable = desc.Writable ?? current.Writable;
            current.Get = desc.Get ?? current.Get;
            current.Set = desc.Set ?? current.Set;
            current.Enumerable = desc.Enumerable ?? current.Enumerable;
            current.Configurable = desc.Configurable ?? current.Configurable;
            return true;
        }
        
        public IDynamic Op_LogicalNot()
        {
            return LType.Op_LogicalNot(this);
        }

        public IDynamic Op_LogicalOr(IDynamic other)
        {
            return LType.Op_LogicalOr(this, other);
        }

        public IDynamic Op_LogicalAnd(IDynamic other)
        {
            return LType.Op_LogicalAnd(this, other);
        }

        public IDynamic Op_BitwiseNot()
        {
            return LType.Op_BitwiseNot(this);
        }

        public IDynamic Op_BitwiseOr(IDynamic other)
        {
            return LType.Op_BitwiseOr(this, other);
        }

        public IDynamic Op_BitwiseXor(IDynamic other)
        {
            return LType.Op_BitwiseXor(this, other);
        }

        public IDynamic Op_BitwiseAnd(IDynamic other)
        {
            return LType.Op_BitwiseAnd(this, other);
        }

        public IDynamic Op_Equals(IDynamic other)
        {
            switch (other.TypeCode)
            {
                case LanguageTypeCode.String:
                case LanguageTypeCode.Number:
                    return ConvertToPrimitive().Op_Equals(other);
                case LanguageTypeCode.Object:
                    return (LBoolean)(this == other);
                default:
                    return LBoolean.False;
            }
        }

        public IDynamic Op_DoesNotEquals(IDynamic other)
        {
            return LType.Op_DoesNotEquals(this, other);
        }

        public IDynamic Op_StrictEquals(IDynamic other)
        {
            switch (other.TypeCode)
            {
                case LanguageTypeCode.Object:
                    return (LBoolean)(this == other);
                default:
                    return LBoolean.False;
            }
        }

        public IDynamic Op_StrictDoesNotEquals(IDynamic other)
        {
            return LType.Op_StrictDoesNotEquals(this, other);
        }

        public IDynamic Op_Lessthan(IDynamic other)
        {
            return LType.Op_Lessthan(this, other);
        }

        public IDynamic Op_Greaterthan(IDynamic other)
        {
            return LType.Op_Greaterthan(this, other);
        }

        public IDynamic Op_LessthanOrEqual(IDynamic other)
        {
            return LType.Op_LessthanOrEqual(this, other);
        }

        public IDynamic Op_GreaterthanOrEqual(IDynamic other)
        {
            return LType.Op_GreaterthanOrEqual(this, other);
        }

        public IDynamic Op_Instanceof(IDynamic other)
        {
            return LType.Op_GreaterthanOrEqual(this, other);
        }

        public IDynamic Op_In(IDynamic other)
        {
            return LType.Op_In(this, other);
        }

        public IDynamic Op_LeftShift(IDynamic other)
        {
            return LType.Op_LeftShift(this, other);
        }

        public IDynamic Op_SignedRightShift(IDynamic other)
        {
            return LType.Op_SignedRightShift(this, other);
        }

        public IDynamic Op_UnsignedRightShift(IDynamic other)
        {
            return LType.Op_UnsignedRightShift(this, other);
        }

        public IDynamic Op_Addition(IDynamic other)
        {
            return LType.Op_Addition(this, other);
        }

        public IDynamic Op_Subtraction(IDynamic other)
        {
            return LType.Op_Subtraction(this, other);
        }

        public IDynamic Op_Multiplication(IDynamic other)
        {
            return LType.Op_Multiplication(this, other);
        }

        public IDynamic Op_Division(IDynamic other)
        {
            return LType.Op_Division(this, other);
        }

        public IDynamic Op_Modulus(IDynamic other)
        {
            return LType.Op_Modulus(this, other);
        }

        public IDynamic Op_Delete()
        {
            return LBoolean.True;
        }

        public IDynamic Op_Void()
        {
            return LUndefined.Instance;
        }

        public IDynamic Op_Typeof()
        {
            return LObject.ObjectString;
        }

        public IDynamic Op_PrefixIncrement()
        {
            throw Environment.ThrowReferenceError();
        }

        public IDynamic Op_PrefixDecrement()
        {
            throw Environment.ThrowReferenceError();
        }

        public IDynamic Op_Plus()
        {
            return ConvertToNumber();
        }

        public IDynamic Op_Minus()
        {
            return LType.Op_Minus(this);
        }

        public IDynamic Op_PostfixIncrement()
        {
            throw Environment.ThrowReferenceError();
        }

        public IDynamic Op_PostfixDecrement()
        {
            throw Environment.ThrowReferenceError();
        }

        public IDynamic Op_GetProperty(IDynamic name)
        {
            return LType.Op_GetProperty(this, name);
        }

        public void Op_SetProperty(IDynamic name, IDynamic value)
        {
            LType.Op_SetProperty(this, name, value);
        }

        public IDynamic Op_Call(IArgs args)
        {
            return LType.Op_Call(this, args);
        }

        public IObject Op_Construct(IArgs args)
        {
            return LType.Op_Construct(this, args);
        }

        public void Op_Throw()
        {
            LType.Op_Throw(this);
        }

        public IDynamic ConvertToPrimitive(string preferredType = null)
        {
            return DefaultValue(preferredType);
        }

        public IBoolean ConvertToBoolean()
        {
            return LBoolean.True;
        }

        public INumber ConvertToNumber()
        {
            return DefaultValue("Number").ConvertToNumber();
        }

        public IString ConvertToString()
        {
            return DefaultValue("String").ConvertToString();
        }

        public IObject ConvertToObject()
        {
            return this;
        }

        public INumber ConvertToInteger()
        {
            return LType.ConvertToInteger(this);
        }

        public INumber ConvertToInt32()
        {
            return LType.ConvertToInt32(this);
        }

        public INumber ConvertToUInt32()
        {
            return LType.ConvertToUInt32(this);
        }

        public INumber ConvertToUInt16()
        {
            return LType.ConvertToUInt16(this);
        }


        IDynamic IReferenceBase.Get(string name, bool strict)
        {
            return Get(name);
        }

        void IReferenceBase.Set(string name, IDynamic value, bool strict)
        {
            Put(name, value, strict);
        }


        public override string ToString()
        {
            return (string)ConvertToString().BaseValue;
        }
    }
}
