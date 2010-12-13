using Machete.Interfaces;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public struct LUndefined : IUndefined
    {
        public static readonly LUndefined Instance = new LUndefined();
        public static readonly LString UndefinedString = new LString("undefined");
        

        public LanguageTypeCode TypeCode
        {
            get { return LanguageTypeCode.Undefined; }
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
                case LanguageTypeCode.Null:
                case LanguageTypeCode.Undefined:
                    return LBoolean.True;
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
                case LanguageTypeCode.Undefined:
                    return LBoolean.True;
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
            return LObject.ObjectString;
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
            return ConvertToNumber();
        }

        public IDynamic Op_Minus()
        {
            return LType.Op_Minus(this);
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

        public IDynamic ConvertToPrimitive(string preferredType)
        {
            return this;
        }

        public IBoolean ConvertToBoolean()
        {
            return LBoolean.False;
        }

        public INumber ConvertToNumber()
        {
            return LNumber.Zero;
        }

        public IString ConvertToString()
        {
            return UndefinedString;
        }

        public IObject ConvertToObject()
        {
            throw Environment.ThrowTypeError();
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
            throw Environment.ThrowReferenceError();
        }

        public void Set(string name, IDynamic value, bool strict)
        {
            if (strict)
            {
                throw Environment.ThrowReferenceError();
            }
            Environment.Instance.Value.GlobalObject.Put(name, value, false);
        }

        public override string ToString()
        {
            return "undefined";
        }
    }
}