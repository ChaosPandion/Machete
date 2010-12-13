using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.NativeObjects;
using Machete.Interfaces;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public struct LBoolean : IBoolean, IEquatable<LBoolean>
    {
        private readonly bool _value;
        public static readonly LBoolean True = new LBoolean(true);
        public static readonly LBoolean False = new LBoolean(false);
        public static readonly LString TrueString = new LString("true");
        public static readonly LString FalseString = new LString("false");
        public static readonly LString BooleanString = new LString("boolean");
          

        private LBoolean(bool value)
        {
            _value = value;
        }


        public bool BaseValue
        {
            get { return _value; }
        }

        public LanguageTypeCode TypeCode
        {
            get { return LanguageTypeCode.Boolean; }
        }

        public bool IsPrimitive
        {
            get { return true; }
        }

        public IDynamic Value
        {
            get { return this; }
            set { }
        }


        public IDynamic Op_LogicalOr(IDynamic other)
        {
            return _value ? this : other;
        }

        public IDynamic Op_LogicalAnd(IDynamic other)
        {
            return !_value ? this : other;
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
                case LanguageTypeCode.Boolean:
                    return (LBoolean)(this._value == ((LBoolean)other)._value);
                default:
                    return this.ConvertToNumber().Op_Equals(other);
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
                case LanguageTypeCode.Boolean:
                    return (LBoolean)(this._value == ((LBoolean)other)._value);
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
            return LType.Op_Instanceof(this, other);
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
            return BooleanString;
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
            return LType.Op_Plus(this);
        }

        public IDynamic Op_Minus()
        {
            return LType.Op_Minus(this);
        }

        public IDynamic Op_BitwiseNot()
        {
            return LType.Op_BitwiseNot(this);
        }

        public IDynamic Op_LogicalNot()
        {
            return LType.Op_LogicalNot(this);
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
            return this;
        }

        public IBoolean ConvertToBoolean()
        {
            return this;
        }

        public INumber ConvertToNumber()
        {
            return _value ? LNumber.One : LNumber.Zero;
        }

        public IString ConvertToString()
        {
            return _value ? TrueString : FalseString;
        }

        public IObject ConvertToObject()
        {
            return new NBoolean(this);
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

        public IDynamic Get(string name, bool strict)
        {
            return LType.GetValue(this, name, strict);
        }

        public void Set(string name, IDynamic value, bool strict)
        {
            LType.SetValue(this, name, value, strict);
        }

        public override bool Equals(object obj)
        {
            return obj is LBoolean && Equals((LBoolean)obj);
        }
        
        public bool Equals(LBoolean other)
        {
            return _value == other._value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString().ToLower();
        }

        public static bool operator ==(LBoolean left, LBoolean right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LBoolean left, LBoolean right)
        {
            return !left.Equals(right);
        }

        public static explicit operator bool(LBoolean value)
        {
            return value._value;
        }

        public static explicit operator LBoolean(bool value)
        {
            return value ? LBoolean.True : LBoolean.False;
        }
    }
}