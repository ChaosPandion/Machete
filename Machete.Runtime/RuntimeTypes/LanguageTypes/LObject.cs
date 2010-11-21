using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.RuntimeTypes.Interfaces;
using Machete.Runtime.NativeObjects;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public class LObject : LType
    {
        private readonly Dictionary<string, SPropertyDescriptor> _map = new Dictionary<string, SPropertyDescriptor>();


        public override LTypeCode TypeCode
        {
            get { return LTypeCode.LObject; }
        }

        public LObject Prototype { get; set; }

        public string Class { get; set; }

        public bool Extensible { get; set; }
        

        public virtual SPropertyDescriptor GetOwnProperty(string p)
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

        public virtual SPropertyDescriptor GetProperty(string p)
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

        public virtual LType Get(string p)
        {
            var desc = GetProperty(p);
            if (desc == null)
            {
                return LUndefined.Value;
            }
            else if (desc.IsDataDescriptor)
            {
                return desc.Value;
            }
            else if (desc.Get is LUndefined)
            {
                return LUndefined.Value;
            }
            else
            {
                return ((ICallable)desc.Get).Call(this, SList.Empty);
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

        public virtual void Put(string p, LType value, bool @throw)
        {
            if (!CanPut(p))
            {
                if (@throw)
                {
                    throw Engine.ThrowTypeError();
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
                ((ICallable)desc.Set).Call(this, new SList(value));
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
                throw Engine.ThrowTypeError();
            }
            else
            {
                return false;
            }
        }

        public virtual LType DefaultValue(string hint)
        {
            if (hint == "String")
            {
                var func = Get("toString") as ICallable ?? Get("valueOf") as ICallable;
                if (func != null)
                {
                    var result = func.Call(this, SList.Empty);
                    if (result is IPrimitive)
                    {
                        return result;
                    }
                }
                throw Engine.ThrowTypeError();
            }
            else
            {
                var func = Get("valueOf") as ICallable ?? Get("toString") as ICallable;
                if (func != null)
                {
                    var result = func.Call(this, SList.Empty);
                    if (result is IPrimitive)
                    {
                        return result;
                    }
                }
                throw Engine.ThrowTypeError();  
            }
        }

        public virtual bool DefineOwnProperty(string p, SPropertyDescriptor desc, bool @throw)
        {
            var reject = new Func<bool>(() => {
                if (!@throw) return false;
                throw Engine.ThrowTypeError();
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
                            Value = desc.Value ?? LUndefined.Value,
                            Writable = desc.Writable ?? false,
                            Enumerable = desc.Enumerable ?? false,
                            Configurable = desc.Configurable ?? false
                        } 
                    : new SPropertyDescriptor()
                        {
                            Get = desc.Get ?? LUndefined.Value,
                            Set = desc.Set ?? LUndefined.Value,
                            Enumerable = desc.Enumerable ?? false,
                            Configurable = desc.Configurable ?? false
                        }
                );
                return true;
            }
            else if (desc.IsEmpty || current.Matches(desc))
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
        

        public override LType Op_LogicalOr(LType other)
        {
            return this;
        }

        public override LType Op_LogicalAnd(LType other)
        {
            return other;
        }

        public override LType Op_BitwiseOr(LType other)
        {
            return ConvertToInt32().Op_BitwiseOr(other);
        }

        public override LType Op_BitwiseXor(LType other)
        {
            return ConvertToInt32().Op_BitwiseXor(other);
        }

        public override LType Op_BitwiseAnd(LType other)
        {
            return ConvertToInt32().Op_BitwiseAnd(other);
        }

        public override LType Op_Equals(LType other)
        {
            switch (other.TypeCode)
            {
                case LTypeCode.LString:
                case LTypeCode.LNumber:
                    return ConvertToPrimitive().Op_Equals(other);
                case LTypeCode.LObject:
                    return (LBoolean)(this == other);
                default:
                    return LBoolean.False;
            }
        }

        public override LType Op_DoesNotEquals(LType other)
        {
            return Op_Equals(other).Op_LogicalNot();
        }

        public override LType Op_StrictEquals(LType other)
        {
            switch (other.TypeCode)
            {
                case LTypeCode.LObject:
                    return (LBoolean)(this == other);
                default:
                    return LBoolean.False;
            }
        }

        public override LType Op_StrictDoesNotEquals(LType other)
        {
            return Op_StrictEquals(other).Op_LogicalNot();
        }

        public override LType Op_Lessthan(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_Greaterthan(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_LessthanOrEqual(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_GreaterthanOrEqual(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_Instanceof(LType other)
        {
            var func = other as NFunction;
            if (func == null)
            {
                throw Engine.ThrowTypeError();
            }
            return (LBoolean)func.HasInstance(this);
        }

        public override LType Op_In(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_LeftShift(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_SignedRightShift(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_UnsignedRightShift(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_Addition(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_Subtraction(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_Multiplication(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_Division(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_Modulus(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_Delete()
        {
            throw new NotImplementedException();
        }

        public override LType Op_Void()
        {
            throw new NotImplementedException();
        }

        public override LType Op_Typeof()
        {
            throw new NotImplementedException();
        }

        public override LType Op_PrefixIncrement()
        {
            throw new NotImplementedException();
        }

        public override LType Op_PrefixDecrement()
        {
            throw new NotImplementedException();
        }

        public override LType Op_Plus()
        {
            throw new NotImplementedException();
        }

        public override LType Op_Minus()
        {
            throw new NotImplementedException();
        }

        public override LType Op_BitwiseNot()
        {
            throw new NotImplementedException();
        }

        public override LType Op_LogicalNot()
        {
            throw new NotImplementedException();
        }

        public override LType Op_PostfixIncrement()
        {
            throw new NotImplementedException();
        }

        public override LType Op_PostfixDecrement()
        {
            throw new NotImplementedException();
        }

        public override LType Op_AccessProperty(LType name)
        {
            throw new NotImplementedException();
        }

        public override LType Op_Call(SList args)
        {
            var v = this as ICallable;
            if (v == null)
            {
                Engine.ThrowTypeError();
            }
            return v.Call(this, args);
        }

        public override LType Op_Construct(SList args)
        {
            var v = this as IConstructable;
            if (v == null)
            {
                Engine.ThrowTypeError();
            }
            return v.Construct(args);
        }

        public override void Op_Throw()
        {
            throw new NotImplementedException();
        }

        public override LType ConvertToPrimitive(string preferredType)
        {
            throw new NotImplementedException();
        }

        public override LBoolean ConvertToBoolean()
        {
            throw new NotImplementedException();
        }

        public override LNumber ConvertToNumber()
        {
            throw new NotImplementedException();
        }

        public override LString ConvertToString()
        {
            throw new NotImplementedException();
        }

        public override LObject ConvertToObject()
        {
            throw new NotImplementedException();
        }
    }
}
