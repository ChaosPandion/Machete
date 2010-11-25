using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.Interfaces;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.NativeObjects;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public struct LString : IDynamic, IReferenceBase, IEquatable<LString>
    {
        private readonly string _value;
        public static readonly LString Empty = new LString(string.Empty);
        public static readonly LString StringString = new LString("string");


        public LString(string value)
        {
            _value = value;
        }


        public LTypeCode TypeCode
        {
            get { return LTypeCode.LString; }
        }

        public bool IsPrimitive
        {
            get { return true; }
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
                case LTypeCode.LString:
                    return (LBoolean)(this._value == ((LString)other)._value);
                case LTypeCode.LNumber:
                    return this.ConvertToNumber().Op_Equals(other);
                case LTypeCode.LObject:
                    return this.ConvertToPrimitive().Op_Equals(other);
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
                case LTypeCode.LString:
                    return (LBoolean)(this._value == ((LString)other)._value);
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
            return StringString;
        }

        public IDynamic Op_PrefixIncrement()
        {
            throw Engine.ThrowReferenceError();
        }

        public IDynamic Op_PrefixDecrement()
        {
            throw Engine.ThrowReferenceError();
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
            throw Engine.ThrowReferenceError();
        }

        public IDynamic Op_PostfixDecrement()
        {
            throw Engine.ThrowReferenceError();
        }

        public IDynamic Op_GetProperty(IDynamic name)
        {
            return LType.Op_GetProperty(this, name);
        }

        public void Op_SetProperty(IDynamic name, IDynamic value)
        {
            LType.Op_SetProperty(this, name, value);
        }

        public IDynamic Op_Call(SList args)
        {
            return LType.Op_Call(this, args);
        }

        public IDynamic Op_Construct(SList args)
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

        public LBoolean ConvertToBoolean()
        {
            return (LBoolean)(_value.Length > 0);
        }

        public LNumber ConvertToNumber()
        {
            return (LNumber)Machete.Compiler.StringNumericLiteral.eval(_value);
        }

        public LString ConvertToString()
        {
            return StringString;
        }

        public LObject ConvertToObject()
        {
            return new NString(this);
        }

        public LNumber ConvertToInteger()
        {
            return LType.ConvertToInteger(this);
        }

        public LNumber ConvertToInt32()
        {
            return LType.ConvertToInt32(this);
        }

        public LNumber ConvertToUInt32()
        {
            return LType.ConvertToUInt32(this);
        }

        public LNumber ConvertToUInt16()
        {
            return LType.ConvertToUInt16(this);
        }
        
        public IDynamic GetValue(string name, bool strict)
        {
            return LType.GetValue(this, name, strict);
        }

        public void SetValue(string name, IDynamic value, bool strict)
        {
            LType.SetValue(this, name, value, strict);
        }

        public override bool Equals(object obj)
        {
            return obj is LString && Equals((LString)obj);
        }

        public bool Equals(LString other)
        {
            return this._value == other._value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value;
        }

        public static bool operator ==(LString left, LString right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LString left, LString right)
        {
            return !left.Equals(right);
        }

        public static explicit operator string(LString value)
        {
            return value._value;
        }

        public static explicit operator LString(string value)
        {
            return new LString(value);
        }

    }
}