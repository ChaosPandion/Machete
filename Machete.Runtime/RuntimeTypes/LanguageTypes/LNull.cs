using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Core;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public sealed class LNull : LType, INull
    {
        public override LanguageTypeCode TypeCode
        {
            get { return LanguageTypeCode.Null; }
        }

        public override bool IsPrimitive
        {
            get { return true; }
        }

        public LNull(IEnvironment environment)
            : base(environment)
        {

        }

        public override IDynamic Op_Equals(IDynamic other)
        {
            switch (other.TypeCode)
            {
                case LanguageTypeCode.Null:
                case LanguageTypeCode.Undefined:
                    return Environment.True;
                default:
                    return Environment.False;
            }
        }

        public override IDynamic Op_StrictEquals(IDynamic other)
        {
            switch (other.TypeCode)
            {
                case LanguageTypeCode.Null:
                    return Environment.True;
                default:
                    return Environment.False;
            }
        }

        public override IDynamic Op_Typeof()
        {
            return Environment.CreateString("object");
        }

        public override IDynamic ConvertToPrimitive(string preferredType)
        {
            return this;
        }

        public override IBoolean ConvertToBoolean()
        {
            return Environment.False;
        }

        public override INumber ConvertToNumber()
        {
            return Environment.CreateNumber(0);
        }

        public override IString ConvertToString()
        {
            return Environment.CreateString("null");
        }

        public override IObject ConvertToObject()
        {
            throw Environment.CreateTypeError("");
        }
    }
}
