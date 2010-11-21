using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public sealed class LNumber : LType
    {
        public static readonly LNumber NaN = new LNumber(double.NaN);
        public static readonly LNumber PositiveInfinity = new LNumber(double.PositiveInfinity);
        public static readonly LNumber NegativeInfinity = new LNumber(double.NegativeInfinity);
        public static readonly LNumber Zero = new LNumber(0.0);
        public static readonly LNumber One = new LNumber(1.0);


        public override LTypeCode TypeCode
        {
            get { return LTypeCode.LNumber; }
        }

        public double Value { get; private set; }


        public LNumber(double value)
        {
            Value = value;
        }

        
        public override LType Op_LogicalOr(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_LogicalAnd(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_BitwiseOr(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_BitwiseXor(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_BitwiseAnd(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_Equals(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_DoesNotEquals(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_StrictEquals(LType other)
        {
            throw new NotImplementedException();
        }

        public override LType Op_StrictDoesNotEquals(LType other)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override LType Op_Construct(SList args)
        {
            throw new NotImplementedException();
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


        public static explicit operator double(LNumber value)
        {
            return value.Value;
        }

        public static explicit operator LNumber(double value)
        {
            return new LNumber(value);
        }
    }
}
