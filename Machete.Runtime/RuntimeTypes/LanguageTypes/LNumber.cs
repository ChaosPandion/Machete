using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.RuntimeTypes.Interfaces;
using Machete.Runtime.NativeObjects;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public struct LNumber : IDynamic, IEquatable<LNumber>
    {
        private readonly double _value;
        public static readonly LString NumberString = new LString("number");
        public static readonly LString NaNString = new LString("NaN");
        public static readonly LString InfinityString = new LString("Infinity");
        public static readonly LString ZeroString = new LString("0");
        public static readonly LNumber NaN = new LNumber(double.NaN);
        public static readonly LNumber PositiveInfinity = new LNumber(double.PositiveInfinity);
        public static readonly LNumber NegativeInfinity = new LNumber(double.NegativeInfinity);
        public static readonly LNumber Zero = new LNumber(0.0);
        public static readonly LNumber One = new LNumber(1.0);       


        public LNumber(double value)
        {
            _value = value;
        }


        public IDynamic Value
        {
            get { return this; }
            set { Engine.ThrowReferenceError(); }
        }

        public LTypeCode TypeCode
        {
            get { return LTypeCode.LNumber; }
        }

        public bool IsPrimitive
        {
            get { return true; }
        }


        public IDynamic Op_LogicalOr(IDynamic other)
        {
            return LType.Op_LogicalOr(this, other);
        }

        public IDynamic Op_LogicalAnd(IDynamic other)
        {
            return LType.Op_LogicalAnd(this, other);
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
                    return this.Op_Equals(other.ConvertToNumber());
                case LTypeCode.LNumber:
                    var lnum = (LNumber)other;
                    return (LBoolean)(!(double.IsNaN(_value) || double.IsNaN(lnum._value)) && this._value == lnum._value);
                case LTypeCode.LObject:
                    return this.Op_Equals(other.ConvertToPrimitive());
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
                case LTypeCode.LNumber:
                    var lnum = (LNumber)other;
                    return (LBoolean)(!(double.IsNaN(_value) || double.IsNaN(lnum._value)) && this._value == lnum._value);
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
            return NumberString;
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
            return this;
        }

        public IDynamic Op_Minus()
        {
            if (!double.IsNaN(_value))
            {
                return (LNumber)(-_value);
            }
            return this;
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
            throw Engine.ThrowReferenceError();
        }

        public IDynamic Op_PostfixDecrement()
        {
            throw Engine.ThrowReferenceError();
        }

        public IDynamic Op_AccessProperty(IDynamic name)
        {
            return LType.Op_AccessProperty(this, name);
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
            return (LBoolean)(!double.IsNaN(_value) && _value != 0.0);
        }

        public LNumber ConvertToNumber()
        {
            return this;
        }

        public LString ConvertToString()
        {
            if (double.IsNaN(_value))
            {
                return NaNString;
            }
            else if (_value == 0.0)
            {
                return ZeroString;
            }
            else if (_value < 0.0)
            {
                return (LString)("-" + (string)((LNumber)(-_value)).ConvertToString());
            }
            else if (double.IsInfinity(_value))
            {
                return InfinityString;
            }

            const double epsilon = 0.0000001;

            int n = 0, k = 0, s = 0;
            int min = 0, max = 0;
            double r = 0.0, rv = 0.0, rmin = _value - epsilon, rmax = _value + epsilon;
            bool complete = false;

            for (int ki = 1; !complete && ki < int.MaxValue; k = ki, ki++)
            {
                min = (int)Math.Pow(10, ki - 1);
                max = (int)Math.Pow(10, ki);
                for (int si = min; !complete && si < max; s = si, si++)
                {
                    if (si % 10 == 0) continue;
                    r = Math.Log10(_value / si) + ki;
                    rv = si * Math.Pow(10, (int)r - ki);
                    complete = rv > rmin && rv < rmax;
                    n = (int)r;
                }
            }

            if (k <= n && n <= 21)
            {
                return (LString)(s.ToString().Substring(0, k).PadRight((2 * k) - n, '0'));
            }
            else if (n > 0 && n <= 21)
            {
                var sv = s.ToString();
                var left = sv.Substring(0, n);
                var right = sv.Substring(n, k - n);
                return (LString)(left + "." + right);
            }
            else if (n > -6 && n <= 0)
            {
                return (LString)("0.".PadRight(2 + -n, '0') + s.ToString().Substring(0, k));
            }
            else if (k == 1)
            {
                var v = n - 1;
                var sign = v < 0 ? "-" : "+";
                var sv = s.ToString();
                var nv = Math.Abs(v).ToString();
                return (LString)(sv + "e" + sign + nv);
            }
            else
            {
                var v = n - 1;
                var sign = v < 0 ? "-" : "+";
                var sv = s.ToString();
                var nv = Math.Abs(v).ToString();
                return (LString)(sv[0] + "." + sv.Substring(1) + "e" + sign + nv);
            }
        }

        public LObject ConvertToObject()
        {
            return new NNumber(this);
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

        public override bool Equals(object obj)
        {
            return obj is LNumber && Equals((LNumber)obj);
        }

        public bool Equals(LNumber other)
        {
            return this._value == other._value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public static bool operator ==(LNumber left, LNumber right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LNumber left, LNumber right)
        {
            return !left.Equals(right);
        }

        public static bool operator ==(LNumber left, double right)
        {
            return left._value == right;
        }

        public static bool operator !=(LNumber left, double right)
        {
            return left._value != right;
        }

        public static explicit operator double(LNumber value)
        {
            return value._value;
        }

        public static explicit operator LNumber(double value)
        {
            return new LNumber(value);
        }
    }
}