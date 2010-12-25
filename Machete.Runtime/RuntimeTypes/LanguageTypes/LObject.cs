using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.NativeObjects;
using Machete.Interfaces;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public class LObject : IObject
    {
        private readonly IEnvironment _environment;
        private readonly Dictionary<string, IPropertyDescriptor> _map = new Dictionary<string, IPropertyDescriptor>();


        public LObject(IEnvironment environment)
        {
            _environment = environment;
        }


        protected IEnvironment Environment
        {
            get { return _environment; }
        }
        
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
        

        public IPropertyDescriptor GetOwnProperty(string p)
        {
            IPropertyDescriptor value;
            if (_map.TryGetValue(p, out value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public IPropertyDescriptor GetProperty(string p)
        {
            IObject obj = this;
            IPropertyDescriptor value;

            do
            {
                value = obj.GetOwnProperty(p);
                if (value != null)
                {
                    return value;
                }
                obj = obj.Prototype;
            } while (obj != null);

            return null;
        }

        public virtual IDynamic Get(string p)
        {
            var desc = GetProperty(p);
            if (desc == null)
            {
                return Environment.Undefined;
            }
            else if (desc.IsDataDescriptor)
            {
                return desc.Value;
            }
            else if (desc.Get is LUndefined)
            {
                return Environment.Undefined;
            }
            else
            {
                return ((ICallable)desc.Get).Call(Environment, this, new SArgs(Environment));
            }
        }

        public bool CanPut(string p)
        {
            IPropertyDescriptor desc;
            if (_map.TryGetValue(p, out desc))
            {
                return desc.IsAccessorDescriptor ? !(desc.Set is IUndefined) : true; 
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
                return !(inherited.Set is IUndefined);
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
                    throw _environment.CreateTypeError("");
                }
                return;
            }

            var ownDesc = GetOwnProperty(p);
            if (ownDesc != null && ownDesc.IsDataDescriptor)
            {
                var valueDesc = new SPropertyDescriptor() { Value = value };
                DefineOwnProperty(p, valueDesc, @throw);
                return;
            }

            var desc = GetProperty(p);
            if (desc != null && desc.IsAccessorDescriptor)
            {
                ((ICallable)desc.Set).Call(Environment, this, new SArgs(_environment, value));
                return;
            }

            var newDesc = Environment.CreateDataDescriptor(value, true, true, true);
            DefineOwnProperty(p, newDesc, @throw);
        }

        public bool HasProperty(string p)
        {
            return _map.ContainsKey(p);
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
                throw _environment.CreateTypeError("");
            }
            else
            {
                return false;
            }
        }

        public virtual IDynamic DefaultValue(string hint)
        {
            var toString = Get("toString") as ICallable;
            var valueOf = Get("valueOf") as ICallable;
            var first = hint == "String" ? toString : valueOf;
            var second = hint == "String" ? valueOf : toString;

            if (first != null)
            {
                var result = first.Call(Environment, this, Environment.EmptyArgs);
                if (result.IsPrimitive)
                {
                    return result;
                }
            }

            if (second != null)
            {
                var result = second.Call(Environment, this, Environment.EmptyArgs);
                if (result.IsPrimitive)
                {
                    return result;
                }
            }

            throw _environment.CreateTypeError("No primitive value was found for object.");
        }

        public virtual bool DefineOwnProperty(string p, IPropertyDescriptor desc, bool @throw)
        {
            var current = GetOwnProperty(p);
            if (current == null)
            {
                if (!Extensible)
                {
                    return Reject(p, @throw);
                }
                if (desc.IsGenericDescriptor || desc.IsDataDescriptor )
                {
                    _map.Add(p, new SPropertyDescriptor()
                        {
                            Value = desc.Value ?? Environment.Undefined,
                            Writable = desc.Writable ?? false,
                            Enumerable = desc.Enumerable ?? false,
                            Configurable = desc.Configurable ?? false
                        });
                }
                else
                {
                    _map.Add(p, new SPropertyDescriptor()
                    {
                        Get = desc.Get ?? Environment.Undefined,
                        Set = desc.Set ?? Environment.Undefined,
                        Enumerable = desc.Enumerable ?? false,
                        Configurable = desc.Configurable ?? false
                    });
                }
                return true;
            }
            else if (desc.IsEmpty || current.Equals(desc))
            {
                return true;
            }
            else if (!current.Configurable.GetValueOrDefault())
            {
                if (desc.Configurable.GetValueOrDefault())
                {
                    return Reject(p, @throw);
                }
                else if (desc.Enumerable != null && desc.Enumerable.GetValueOrDefault() ^ current.Enumerable.GetValueOrDefault())
                {
                    return Reject(p, @throw);
                }
            }
            else if (!desc.IsGenericDescriptor)
            {
                if (current.IsDataDescriptor ^ desc.IsDataDescriptor)
                {
                    if (!current.Configurable.Value)
                    {
                        return Reject(p, @throw);
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
                            return Reject(p, @throw);
                        }
                        else if (!current.Writable.Value && current.Value != desc.Value)
                        {
                            return Reject(p, @throw);
                        }
                    }
                }
                else
                {
                    if (!current.Configurable.Value)
                    {
                        if (desc.Set != null && desc.Set != current.Set)
                        {
                            return Reject(p, @throw);
                        }
                        else if (desc.Get != null && desc.Get != current.Get)
                        {
                            return Reject(p, @throw);
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
            _map[p] = current;
            return true;
        }

        private bool Reject(string name, bool @throw)
        {
            if (!@throw) return false;
            throw _environment.CreateTypeError("");
        }


        public IDynamic Op_LogicalNot()
        {
            return LType.Op_LogicalNot(_environment, this);
        }

        public IDynamic Op_LogicalOr(IDynamic other)
        {
            return LType.Op_LogicalOr(_environment, this, other);
        }

        public IDynamic Op_LogicalAnd(IDynamic other)
        {
            return LType.Op_LogicalAnd(_environment, this, other);
        }

        public IDynamic Op_BitwiseNot()
        {
            return LType.Op_BitwiseNot(_environment, this);
        }

        public IDynamic Op_BitwiseOr(IDynamic other)
        {
            return LType.Op_BitwiseOr(_environment, this, other);
        }

        public IDynamic Op_BitwiseXor(IDynamic other)
        {
            return LType.Op_BitwiseXor(_environment, this, other);
        }

        public IDynamic Op_BitwiseAnd(IDynamic other)
        {
            return LType.Op_BitwiseAnd(_environment, this, other);
        }

        public IDynamic Op_Equals(IDynamic other)
        {
            switch (other.TypeCode)
            {
                case LanguageTypeCode.String:
                case LanguageTypeCode.Number:
                    return ConvertToPrimitive(null).Op_Equals(other);
                case LanguageTypeCode.Object:
                    return Environment.CreateBoolean(this == other);
                default:
                    return Environment.False;
            }
        }

        public IDynamic Op_DoesNotEquals(IDynamic other)
        {
            return LType.Op_DoesNotEquals(_environment, this, other);
        }

        public IDynamic Op_StrictEquals(IDynamic other)
        {
            switch (other.TypeCode)
            {
                case LanguageTypeCode.Object:
                    return _environment.CreateBoolean(this == other);
                default:
                    return _environment.CreateBoolean(false);
            }
        }

        public IDynamic Op_StrictDoesNotEquals(IDynamic other)
        {
            return LType.Op_StrictDoesNotEquals(_environment, this, other);
        }

        public IDynamic Op_Lessthan(IDynamic other)
        {
            return LType.Op_Lessthan(_environment, this, other);
        }

        public IDynamic Op_Greaterthan(IDynamic other)
        {
            return LType.Op_Greaterthan(_environment, this, other);
        }

        public IDynamic Op_LessthanOrEqual(IDynamic other)
        {
            return LType.Op_LessthanOrEqual(_environment, this, other);
        }

        public IDynamic Op_GreaterthanOrEqual(IDynamic other)
        {
            return LType.Op_GreaterthanOrEqual(_environment, this, other);
        }

        public IDynamic Op_Instanceof(IDynamic other)
        {
            return LType.Op_GreaterthanOrEqual(_environment, this, other);
        }

        public IDynamic Op_In(IDynamic other)
        {
            return LType.Op_In(_environment, this, other);
        }

        public IDynamic Op_LeftShift(IDynamic other)
        {
            return LType.Op_LeftShift(_environment, this, other);
        }

        public IDynamic Op_SignedRightShift(IDynamic other)
        {
            return LType.Op_SignedRightShift(_environment, this, other);
        }

        public IDynamic Op_UnsignedRightShift(IDynamic other)
        {
            return LType.Op_UnsignedRightShift(_environment, this, other);
        }

        public IDynamic Op_Addition(IDynamic other)
        {
            return LType.Op_Addition(_environment, this, other);
        }

        public IDynamic Op_Subtraction(IDynamic other)
        {
            return LType.Op_Subtraction(_environment, this, other);
        }

        public IDynamic Op_Multiplication(IDynamic other)
        {
            return LType.Op_Multiplication(_environment, this, other);
        }

        public IDynamic Op_Division(IDynamic other)
        {
            return LType.Op_Division(_environment, this, other);
        }

        public IDynamic Op_Modulus(IDynamic other)
        {
            return LType.Op_Modulus(_environment, this, other);
        }

        public IDynamic Op_Delete()
        {
            return Environment.CreateBoolean(true);
        }

        public IDynamic Op_Void()
        {
            return Environment.Undefined;
        }

        public IDynamic Op_Typeof()
        {
            return Environment.CreateString(this is ICallable ? "function" : "object");
        }

        public IDynamic Op_PrefixIncrement()
        {
            throw _environment.CreateReferenceError("");
        }

        public IDynamic Op_PrefixDecrement()
        {
            throw _environment.CreateReferenceError("");
        }

        public IDynamic Op_Plus()
        {
            return ConvertToNumber();
        }

        public IDynamic Op_Minus()
        {
            return LType.Op_Minus(_environment, this);
        }

        public IDynamic Op_PostfixIncrement()
        {
            throw _environment.CreateReferenceError("");
        }

        public IDynamic Op_PostfixDecrement()
        {
            throw _environment.CreateReferenceError("");
        }

        public IDynamic Op_GetProperty(IDynamic name)
        {
            var propertyName = name.ConvertToString().BaseValue;
            return Get(propertyName);
        }

        public void Op_SetProperty(IDynamic name, IDynamic value)
        {
            LType.Op_SetProperty(_environment, this, name, value);
        }

        public IDynamic Op_Call(IArgs args)
        {
            var c = this as ICallable;
            if (c == null)
            {
                throw Environment.CreateTypeError("");
            }
            return c.Call(Environment, this, args);
        }

        public IObject Op_Construct(IArgs args)
        {
            var c = this as IConstructable;
            if (c == null)
            {
                throw Environment.CreateTypeError("");
            }
            return c.Construct(Environment, args);
        }

        public void Op_Throw()
        {
            LType.Op_Throw(_environment, this);
        }

        public IDynamic ConvertToPrimitive(string preferredType)
        {
            return DefaultValue(preferredType);
        }

        public IBoolean ConvertToBoolean()
        {
            return Environment.CreateBoolean(true);
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
            return LType.ConvertToInteger(_environment, this);
        }

        public INumber ConvertToInt32()
        {
            return LType.ConvertToInt32(_environment, this);
        }

        public INumber ConvertToUInt32()
        {
            return LType.ConvertToUInt32(_environment, this);
        }

        public INumber ConvertToUInt16()
        {
            return LType.ConvertToUInt16(_environment, this);
        }


        IDynamic IReferenceBase.Get(string name, bool strict)
        {
            return Get(name);
        }

        void IReferenceBase.Set(string name, IDynamic value, bool strict)
        {
            Put(name, value, strict);
        }


        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            foreach (var prop in _map.Keys)
            {
                IPropertyDescriptor desc;
                if (_map.TryGetValue(prop, out desc))
                {
                    if (desc.Enumerable ?? false)
                    {
                        yield return prop;
                    }
                }
            }

            if (Prototype != null)
            {
                foreach (var prop in Prototype)
                {
                    yield return prop;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }


        public virtual void Initialize()
        {
            foreach (var m in this.GetType().GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
            {
                NativeFunctionAttribute nf = null;
                DataDescriptorAttribute dd = null;

                foreach (var a in m.GetCustomAttributes(false))
                {
                    nf = nf ?? a as NativeFunctionAttribute;
                    dd = dd ?? a as DataDescriptorAttribute;
                    if (nf != null && dd != null)
                    {
                        break;
                    }
                }

                // For now I want to assume native functions will always be stored in data descriptors.
                Debug.Assert(nf != null && dd != null);

                var code = (Code)Delegate.CreateDelegate(typeof(Code), m);
                var func = Environment.CreateFunction(nf.FormalParameterList, true,  new Lazy<Code>(() => code));
                var desc = Environment.CreateDataDescriptor(func, dd.Writable, dd.Enumerable, dd.Configurable);

                DefineOwnProperty(nf.Identifier, desc, false);
            }
        }

        public override string ToString()
        {
            return (string)ConvertToString().BaseValue;
        }

        
    }
}
