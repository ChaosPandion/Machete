using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;

namespace Machete.Runtime.RuntimeTypes.Interfaces
{
    public interface IDynamic
    {
        LTypeCode TypeCode { get; }
        bool IsPrimitive { get; }

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
        IDynamic Op_Call(SList args);
        IDynamic Op_Construct(SList args);
        void Op_Throw();

        IDynamic ConvertToPrimitive(string preferredType = null);
        LBoolean ConvertToBoolean();
        LNumber ConvertToNumber();
        LString ConvertToString();
        LObject ConvertToObject();
        LNumber ConvertToInteger();
        LNumber ConvertToInt32();
        LNumber ConvertToUInt32();
        LNumber ConvertToUInt16();
    }
}
