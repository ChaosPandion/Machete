using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.NativeObjects;
using Machete.Core;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public sealed class LBoolean : LType, IBoolean
    {
        public bool BaseValue { get; private set; }
        
        public override LanguageTypeCode TypeCode
        {
            get { return LanguageTypeCode.Boolean; }
        }

        public override bool IsPrimitive
        {
            get { return true; }
        }

        public LBoolean(IEnvironment environment, bool value)
            : base(environment)
        {
            BaseValue = value;
        }

        public override IDynamic Op_Equals(IDynamic other)
        {
            switch (other.TypeCode)
            {
                case LanguageTypeCode.Boolean:
                    return Environment.CreateBoolean(this.BaseValue == ((LBoolean)other).BaseValue);
                default:
                    return this.ConvertToNumber().Op_Equals(other);
            }
        }

        public override IDynamic Op_StrictEquals(IDynamic other)
        {
            switch (other.TypeCode)
            {
                case LanguageTypeCode.Boolean:
                    return Environment.CreateBoolean(this.BaseValue == ((LBoolean)other).BaseValue);
                default:
                    return Environment.CreateBoolean(false);
            }
        }

        public override IDynamic Op_Typeof()
        {
            return Environment.CreateString("boolean");
        }

        public override IDynamic ConvertToPrimitive(string preferredType)
        {
            return this;
        }

        public override IBoolean ConvertToBoolean()
        {
            return this;
        }

        public override INumber ConvertToNumber()
        {
            return BaseValue ? Environment.CreateNumber(1) : Environment.CreateNumber(0);
        }

        public override IString ConvertToString()
        {
            return BaseValue ? Environment.CreateString("true") : Environment.CreateString("false");
        }

        public override IObject ConvertToObject()
        {
            return ((IConstructable)Environment.BooleanConstructor).Construct(Environment, Environment.CreateArgs(new IDynamic[] { this }));
        }
    }
}