using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.RuntimeTypes.Interfaces;
using Machete.Runtime.NativeObjects;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public class LObject : IDynamic
    {
        private readonly Dictionary<string, SPropertyDescriptor> _map = new Dictionary<string, SPropertyDescriptor>();
        public static readonly LString ObjectString = new LString("object");

        
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

        public virtual void Put(string p, IDynamic value, bool @throw)
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

        public virtual IDynamic DefaultValue(string hint)
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




        public IDynamic Value
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public LTypeCode TypeCode
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsPrimitive
        {
            get { throw new NotImplementedException(); }
        }

        public IDynamic Op_LogicalOr(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_LogicalAnd(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_BitwiseOr(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_BitwiseXor(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_BitwiseAnd(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Equals(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_DoesNotEquals(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_StrictEquals(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_StrictDoesNotEquals(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Lessthan(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Greaterthan(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_LessthanOrEqual(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_GreaterthanOrEqual(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Instanceof(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_In(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_LeftShift(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_SignedRightShift(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_UnsignedRightShift(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Addition(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Subtraction(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Multiplication(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Division(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Modulus(IDynamic other)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Delete()
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Void()
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Typeof()
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_PrefixIncrement()
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_PrefixDecrement()
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Plus()
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Minus()
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_BitwiseNot()
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_LogicalNot()
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_PostfixIncrement()
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_PostfixDecrement()
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_AccessProperty(IDynamic name)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Call(SList args)
        {
            throw new NotImplementedException();
        }

        public IDynamic Op_Construct(SList args)
        {
            throw new NotImplementedException();
        }

        public void Op_Throw()
        {
            throw new NotImplementedException();
        }

        public IDynamic ConvertToPrimitive(string preferredType = null)
        {
            throw new NotImplementedException();
        }

        public LBoolean ConvertToBoolean()
        {
            throw new NotImplementedException();
        }

        public LNumber ConvertToNumber()
        {
            throw new NotImplementedException();
        }

        public LString ConvertToString()
        {
            throw new NotImplementedException();
        }

        public LObject ConvertToObject()
        {
            throw new NotImplementedException();
        }

        public LNumber ConvertToInteger()
        {
            throw new NotImplementedException();
        }

        public LNumber ConvertToInt32()
        {
            throw new NotImplementedException();
        }

        public LNumber ConvertToUInt32()
        {
            throw new NotImplementedException();
        }

        public LNumber ConvertToUInt16()
        {
            throw new NotImplementedException();
        }
    }
}
