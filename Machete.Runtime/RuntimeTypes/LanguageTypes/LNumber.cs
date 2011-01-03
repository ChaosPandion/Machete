using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.NativeObjects;
using Machete.Interfaces;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public struct LNumber : INumber, IEquatable<LNumber>
    {
        private readonly IEnvironment _environment;
        private readonly double _value;     


        public LNumber(IEnvironment environment, double value)
        {
            _environment = environment;
            _value = value;
        }


        public double BaseValue
        {
            get { return _value; }
        }

        public LanguageTypeCode TypeCode
        {
            get { return LanguageTypeCode.Number; }
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
            return LType.Op_LogicalOr(_environment,this, other);
        }

        public IDynamic Op_LogicalAnd(IDynamic other)
        {
            return LType.Op_LogicalAnd(_environment, this, other);
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
                    return this.Op_Equals(other.ConvertToNumber());
                case LanguageTypeCode.Number:
                    var lnum = (LNumber)other;
                    return _environment.CreateBoolean(!(double.IsNaN(_value) || double.IsNaN(lnum._value)) && this._value == lnum._value);
                case LanguageTypeCode.Object:
                    return this.Op_Equals(other.ConvertToPrimitive(null));
                default:
                    return _environment.False;
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
                case LanguageTypeCode.Number:
                    var lnum = (LNumber)other;
                    return _environment.CreateBoolean(!(double.IsNaN(_value) || double.IsNaN(lnum._value)) && this._value == lnum._value);
                default:
                    return _environment.False;
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
            var otherPrim = other.ConvertToPrimitive(null);
            switch (otherPrim.TypeCode)
            {
                case LanguageTypeCode.String:
                    return _environment.CreateString(ConvertToString().BaseValue + otherPrim.ConvertToString().BaseValue);
                default:
                    return _environment.CreateNumber(_value + otherPrim.ConvertToNumber().BaseValue);
            }
        }

        public IDynamic Op_Subtraction(IDynamic other)
        {
            return _environment.CreateNumber(_value - other.ConvertToNumber().BaseValue);
        }

        public IDynamic Op_Multiplication(IDynamic other)
        {
            return _environment.CreateNumber(_value * other.ConvertToNumber().BaseValue);
        }

        public IDynamic Op_Division(IDynamic other)
        {
            return _environment.CreateNumber(_value / other.ConvertToNumber().BaseValue);
        }

        public IDynamic Op_Modulus(IDynamic other)
        {
            return _environment.CreateNumber(_value % other.ConvertToNumber().BaseValue);
        }

        public IDynamic Op_Delete()
        {
            return _environment.True;
        }

        public IDynamic Op_Void()
        {
            return _environment.Undefined;
        }

        public IDynamic Op_Typeof()
        {
            return _environment.CreateString("number");
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
            return this;
        }

        public IDynamic Op_Minus()
        {
            if (!double.IsNaN(_value))
            {
                return _environment.CreateNumber(-_value);
            }
            return this;
        }

        public IDynamic Op_BitwiseNot()
        {
            return LType.Op_BitwiseNot(_environment, this);
        }

        public IDynamic Op_LogicalNot()
        {
            return LType.Op_LogicalNot(_environment, this);
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
            return LType.Op_GetProperty(_environment, this, name);
        }

        public void Op_SetProperty(IDynamic name, IDynamic value)
        {
            LType.Op_SetProperty(_environment, this, name, value);
        }

        public IDynamic Op_Call(IArgs args)
        {
            return LType.Op_Call(_environment, this, args);
        }

        public IObject Op_Construct(IArgs args)
        {
            return LType.Op_Construct(_environment, this, args);
        }

        public void Op_Throw()
        {
            LType.Op_Throw(_environment, this);
        }

        public IDynamic ConvertToPrimitive(string preferredType)
        {
            return this;
        }

        public IBoolean ConvertToBoolean()
        {
            return _environment.CreateBoolean(!double.IsNaN(_value) && _value != 0.0);
        }

        public INumber ConvertToNumber()
        {
            return this;
        }

        public IString ConvertToString()
        {
            if (double.IsNaN(_value))
            {
                return _environment.CreateString("NaN");
            }
            else if (_value == 0.0)
            {
                return _environment.CreateString("0");
            }
            else if (_value < 0.0)
            {
                return _environment.CreateString("-" + (string)(_environment.CreateNumber(-_value)).ConvertToString().BaseValue);
            }
            else if (double.IsInfinity(_value))
            {
                return _environment.CreateString("Infinity");
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
                return _environment.CreateString(s.ToString().Substring(0, k).PadRight((2 * k) - n, '0'));
            }
            else if (n > 0 && n <= 21)
            {
                var sv = s.ToString();
                var left = sv.Substring(0, n);
                var right = sv.Substring(n, k - n);
                return _environment.CreateString(left + "." + right);
            }
            else if (n > -6 && n <= 0)
            {
                return _environment.CreateString("0.".PadRight(2 + -n, '0') + s.ToString().Substring(0, k));
            }
            else if (k == 1)
            {
                var v = n - 1;
                var sign = v < 0 ? "-" : "+";
                var sv = s.ToString();
                var nv = Math.Abs(v).ToString();
                return _environment.CreateString(sv + "e" + sign + nv);
            }
            else
            {
                var v = n - 1;
                var sign = v < 0 ? "-" : "+";
                var sv = s.ToString();
                var nv = Math.Abs(v).ToString();
                return _environment.CreateString(sv[0] + "." + sv.Substring(1) + "e" + sign + nv);
            }
        }

        public IObject ConvertToObject()
        {
            return ((IConstructable)_environment.NumberConstructor).Construct(_environment, _environment.CreateArgs(new IDynamic[] { this }));
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


        public IDynamic Get(string name, bool strict)
        {
            return LType.GetValue(_environment, this, name, strict);
        }

        public void Set(string name, IDynamic value, bool strict)
        {
            LType.SetValue(_environment, this, name, value, strict);
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
    }
}