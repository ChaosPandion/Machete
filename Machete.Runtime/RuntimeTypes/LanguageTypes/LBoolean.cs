using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public sealed class LBoolean : LType, IEquatable<LBoolean>
    {
        private readonly bool _value;
        public static readonly LBoolean True = new LBoolean(true);
        public static readonly LBoolean False = new LBoolean(false);
        public static readonly LString TrueString = new LString("true");
        public static readonly LString FalseString = new LString("false");
          

        private LBoolean(bool value)
        {
            _value = value;
        }


        public override LTypeCode TypeCode
        {
            get { return LTypeCode.LBoolean; }
        }


        public override LType Op_LogicalOr(LType other)
        {
            return _value ? this : other;
        }

        public override LType Op_LogicalAnd(LType other)
        {
            return !_value ? this : other;
        }

        public override LType Op_BitwiseOr(LType other)
        {
            return ConvertToInt32().Op_BitwiseOr(other);
        }

        public override LType Op_BitwiseXor(LType other)
        {
            return ConvertToInt32().Op_BitwiseXor(other);
        }

        public override LType Op_BitwiseAnd(LType other)
        {
            return ConvertToInt32().Op_BitwiseAnd(other);
        }

        public override LType Op_Equals(LType other)
        {
            switch (other.TypeCode)
            {
                case LTypeCode.LBoolean:
                    return (LBoolean)(this == other);
                default:
                    return ConvertToNumber().Op_Equals(other);
            }
        }

        //public override LType Op_DoesNotEquals(LType other)
        //{
        //    return Op_Equals(other).Op_LogicalNot();
        //}

        public override LType Op_StrictEquals(LType other)
        {
            if (other.TypeCode == LTypeCode.LBoolean)
            {
                return (LBoolean)(this == other);
            }
            return LBoolean.False;
        }

        //public override LType Op_StrictDoesNotEquals(LType other)
        //{
        //    return Op_StrictEquals(other).Op_LogicalNot();
        //}

        //public override LType Op_Lessthan(LType other)
        //{
        //    var r = RelationalComparison(true, other);
        //    if (r.TypeCode == LTypeCode.LUndefined)
        //    {
        //        return LBoolean.False;
        //    }
        //    return r;
        //}

        //public override LType Op_Greaterthan(LType other)
        //{
        //    throw new NotImplementedException();
        //}

        //public override LType Op_LessthanOrEqual(LType other)
        //{
        //    throw new NotImplementedException();
        //}

        //public override LType Op_GreaterthanOrEqual(LType other)
        //{
        //    throw new NotImplementedException();
        //}

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
        
        public bool Equals(LBoolean other)
        {
            return _value == other._value;
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