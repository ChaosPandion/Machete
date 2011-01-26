using System;
using Machete.Runtime.NativeObjects;
using Machete.Core;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public sealed class LString : LType, IString
    {
        public string BaseValue { get; private set; }

        public override LanguageTypeCode TypeCode
        {
            get { return LanguageTypeCode.String; }
        }

        public override bool IsPrimitive
        {
            get { return true; }
        }

        public LString(IEnvironment environment, string value)
            : base(environment)
        {
            BaseValue = value;
        }
                
        public override IDynamic Op_Equals(IDynamic other)
        {
            switch (other.TypeCode)
            {
                case LanguageTypeCode.String:
                    return Environment.CreateBoolean(this.BaseValue == ((LString)other).BaseValue);
                case LanguageTypeCode.Number:
                    return this.ConvertToNumber().Op_Equals(other);
                case LanguageTypeCode.Object:
                    return this.ConvertToPrimitive(null).Op_Equals(other);
                default:
                    return Environment.False;
            }
        }

        public override IDynamic Op_StrictEquals(IDynamic other)
        {
            switch (other.TypeCode)
            {
                case LanguageTypeCode.String:
                    return Environment.CreateBoolean(this.BaseValue == ((LString)other).BaseValue);
                default:
                    return Environment.False;
            }
        }        

        public override IDynamic Op_Typeof()
        {
            return Environment.CreateString("string");
        }
                
        public override IDynamic ConvertToPrimitive(string preferredType)
        {
            return this;
        }

        public override IBoolean ConvertToBoolean()
        {
            return Environment.CreateBoolean(BaseValue.Length > 0);
        }

        public override INumber ConvertToNumber()
        {
            return Environment.CreateNumber(Machete.Compiler.StringNumericLiteral.eval(BaseValue));
        }

        public override IString ConvertToString()
        {
            return this;
        }

        public override IObject ConvertToObject()
        {
            return ((IConstructable)Environment.StringConstructor).Construct(Environment, Environment.CreateArgs(new IDynamic[] { this }));
        }
    }
}