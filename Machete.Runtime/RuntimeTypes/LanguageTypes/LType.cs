using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public abstract class LType : RType
    {
        public abstract LTypeCode TypeCode { get; }

        public abstract LType Op_LogicalOr(LType other);
        public abstract LType Op_LogicalAnd(LType other);
        public abstract LType Op_BitwiseOr(LType other);
        public abstract LType Op_BitwiseXor(LType other);
        public abstract LType Op_BitwiseAnd(LType other);

        public abstract LType Op_Equals(LType other);

        public LType Op_DoesNotEquals(LType other)
        {
            return Op_Equals(other).Op_LogicalNot();
        }

        public abstract LType Op_StrictEquals(LType other);

        public LType Op_StrictDoesNotEquals(LType other)
        {
            return Op_StrictEquals(other).Op_LogicalNot();
        }

        public LType Op_Lessthan(LType other)
        {
            var r = this.RelationalComparison(true, other);
            if (r.TypeCode == LTypeCode.LUndefined)
            {
                return LBoolean.False;
            }
            return r;
        }

        public LType Op_Greaterthan(LType other)
        {
            var r = other.RelationalComparison(false, this);
            if (r.TypeCode == LTypeCode.LUndefined)
            {
                return LBoolean.False;
            }
            return r;
        }

        public LType Op_LessthanOrEqual(LType other)
        {
            var r = other.RelationalComparison(false, this);
            if (r.TypeCode == LTypeCode.LUndefined || (bool)(LBoolean)r)
            {
                return LBoolean.True;
            }
            return LBoolean.False;
        }

        public LType Op_GreaterthanOrEqual(LType other)
        {
            var r = this.RelationalComparison(true, other);
            if (r.TypeCode == LTypeCode.LUndefined || (bool)(LBoolean)r)
            {
                return LBoolean.False;
            }
            return LBoolean.True;
        }

        public abstract LType Op_Instanceof(LType other);
        public abstract LType Op_In(LType other);
        public abstract LType Op_LeftShift(LType other);
        public abstract LType Op_SignedRightShift(LType other);
        public abstract LType Op_UnsignedRightShift(LType other);
        public abstract LType Op_Addition(LType other);
        public abstract LType Op_Subtraction(LType other);
        public abstract LType Op_Multiplication(LType other);
        public abstract LType Op_Division(LType other);
        public abstract LType Op_Modulus(LType other);
        public abstract LType Op_Delete();
        public abstract LType Op_Void();
        public abstract LType Op_Typeof();
        public abstract LType Op_PrefixIncrement();
        public abstract LType Op_PrefixDecrement();
        public abstract LType Op_Plus();
        public abstract LType Op_Minus();
        public abstract LType Op_BitwiseNot();
        public abstract LType Op_LogicalNot();
        public abstract LType Op_PostfixIncrement();
        public abstract LType Op_PostfixDecrement();
        public abstract LType Op_AccessProperty(LType name);
        public abstract LType Op_Call(SList args);
        public abstract LType Op_Construct(SList args);
        public abstract void Op_Throw();

        public abstract LType ConvertToPrimitive(string preferredType);
        public abstract LBoolean ConvertToBoolean();
        public abstract LNumber ConvertToNumber();
        public abstract LString ConvertToString();
        public abstract LObject ConvertToObject();

        public LNumber ConvertToInteger()
        {
            var number = this.ConvertToNumber();
            if (double.IsNaN(number.Value))
            {
                return LNumber.Zero;
            }
            else if (number.Value == 0.0 || double.IsInfinity(number.Value))
            {
                return number;
            }
            else
            {
                var sign = Math.Sign(number.Value);
                var abs = Math.Abs(number.Value);
                var floor = Math.Floor(abs);
                return new LNumber(sign * floor);
            }
        }

        public LNumber ConvertToInt32()
        {
            var number = this.ConvertToNumber();
            if (number.Value == 0.0 || double.IsNaN(number.Value) || double.IsInfinity(number.Value))
            {
                return LNumber.Zero;
            }
            return LNumber.Zero;
        }

        public LNumber ConvertToUInt32()
        {
            return LNumber.Zero;
        }

        public LNumber ConvertToUInt16()
        {
            return LNumber.Zero;
        }


        protected LType RelationalComparison(bool leftFirst, LType other)
        {
            LType px, py;
            if (leftFirst)
            {
                px = this.ConvertToPrimitive("Number");
                py = other.ConvertToPrimitive("Number");
            }
            else
            {
                py = other.ConvertToPrimitive("Number");
                px = this.ConvertToPrimitive("Number");
            }
            if (px.TypeCode == LTypeCode.LString && py.TypeCode == LTypeCode.LString)
            {
                string sx = (string)(LString)px, sy = (string)(LString)py;
                if (sx.StartsWith(sy))
                {
                    return LBoolean.False;
                }
                else if (sy.StartsWith(sx))
                {
                    return LBoolean.True;
                }
                else
                {
                    return (LBoolean)(sx.CompareTo(sy) < 0);
                }
            }
            else
            {
                double nx = (double)px.ConvertToNumber(), ny = (double)py.ConvertToNumber();
                if (double.IsNaN(nx) || double.IsNaN(ny))
                {
                    return LUndefined.Value;
                }
                else if (nx != ny)
                {
                    if (double.IsPositiveInfinity(nx))
                    {
                        return LBoolean.False;
                    }
                    else if (double.IsPositiveInfinity(ny))
                    {
                        return LBoolean.True;
                    }
                    else if (double.IsNegativeInfinity(ny))
                    {
                        return LBoolean.False;
                    }
                    else if (double.IsNegativeInfinity(nx))
                    {
                        return LBoolean.True;
                    }
                }
                return LBoolean.False;
            }
        }
    }
}