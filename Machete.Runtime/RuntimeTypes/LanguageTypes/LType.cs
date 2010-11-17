using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public abstract class LType : RType
    {
        public abstract LType Op_LogicalOr(LType other);
        public abstract LType Op_LogicalAnd(LType other);
        public abstract LType Op_BitwiseOr(LType other);
        public abstract LType Op_BitwiseXor(LType other);
        public abstract LType Op_BitwiseAnd(LType other);
        public abstract LType Op_Equals(LType other);
        public abstract LType Op_DoesNotEquals(LType other);
        public abstract LType Op_StrictEquals(LType other);
        public abstract LType Op_StrictDoesNotEquals(LType other);
        public abstract LType Op_Lessthan(LType other);
        public abstract LType Op_Greaterthan(LType other);
        public abstract LType Op_LessthanOrEqual(LType other);
        public abstract LType Op_GreaterthanOrEqual(LType other);
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


        public abstract LType ConvertToPrimitive();

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
    }
}