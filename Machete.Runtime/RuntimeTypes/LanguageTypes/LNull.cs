using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public sealed class LNull : LType
    {
        public static readonly LNull Value = new LNull();

        public override LTypeCode TypeCode
        {
            get { return LTypeCode.LNull; }
        }

        private LNull()
        {

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

        public override LType Op_Call(SpecificationTypes.SList args)
        {
            throw new NotImplementedException();
        }

        public override LType Op_Construct(SpecificationTypes.SList args)
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
    }
}
