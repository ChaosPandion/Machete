namespace Machete.Interfaces
{
    public interface IDynamic
    {
        LanguageTypeCode TypeCode { get; }
        bool IsPrimitive { get; }
        IDynamic Value { get; set; }

        IDynamic Op_LogicalNot();
        IDynamic Op_LogicalOr(IDynamic other);
        IDynamic Op_LogicalAnd(IDynamic other);

        IDynamic Op_BitwiseNot();
        IDynamic Op_BitwiseOr(IDynamic other);
        IDynamic Op_BitwiseXor(IDynamic other);
        IDynamic Op_BitwiseAnd(IDynamic other);

        IDynamic Op_Equals(IDynamic other);
        IDynamic Op_DoesNotEquals(IDynamic other);
        IDynamic Op_StrictEquals(IDynamic other);
        IDynamic Op_StrictDoesNotEquals(IDynamic other);

        IDynamic Op_Lessthan(IDynamic other);
        IDynamic Op_Greaterthan(IDynamic other);
        IDynamic Op_LessthanOrEqual(IDynamic other);
        IDynamic Op_GreaterthanOrEqual(IDynamic other);
        IDynamic Op_Instanceof(IDynamic other);
        IDynamic Op_In(IDynamic other);

        IDynamic Op_LeftShift(IDynamic other);
        IDynamic Op_SignedRightShift(IDynamic other);
        IDynamic Op_UnsignedRightShift(IDynamic other);

        IDynamic Op_Addition(IDynamic other);
        IDynamic Op_Subtraction(IDynamic other);
        IDynamic Op_Multiplication(IDynamic other);
        IDynamic Op_Division(IDynamic other);
        IDynamic Op_Modulus(IDynamic other);
        IDynamic Op_Delete();
        IDynamic Op_Void();
        IDynamic Op_Typeof();
        IDynamic Op_PrefixIncrement();
        IDynamic Op_PrefixDecrement();
        IDynamic Op_Plus();
        IDynamic Op_Minus();
        IDynamic Op_PostfixIncrement();
        IDynamic Op_PostfixDecrement();

        IDynamic Op_GetProperty(IDynamic name);
        void Op_SetProperty(IDynamic name, IDynamic value);
        IDynamic Op_Call(IArgs args);
        IObject Op_Construct(IArgs args);
        void Op_Throw();

        IDynamic ConvertToPrimitive(string preferredType);
        IBoolean ConvertToBoolean();
        INumber ConvertToNumber();
        IString ConvertToString();
        IObject ConvertToObject();
        INumber ConvertToInteger();
        INumber ConvertToInt32();
        INumber ConvertToUInt32();
        INumber ConvertToUInt16();
    }
}