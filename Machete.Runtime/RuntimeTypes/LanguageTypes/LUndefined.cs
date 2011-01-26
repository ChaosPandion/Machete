using Machete.Core;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public sealed class LUndefined : LType, IUndefined
    {
        public override LanguageTypeCode TypeCode
        {
            get { return LanguageTypeCode.Undefined; }
        }

        public override bool IsPrimitive
        {
            get { return true; }
        }

        public LUndefined(IEnvironment environment)
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
                case LanguageTypeCode.Undefined:
                    return Environment.True;
                default:
                    return Environment.False;
            }
        }
        
        public override IDynamic Op_Typeof()
        {
            return Environment.CreateString("undefined");
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
            return Environment.CreateString("undefined");
        }

        public override IObject ConvertToObject()
        {
            throw Environment.CreateTypeError("");
        }
        
        public IDynamic Get(string name, bool strict)
        {
            throw Environment.CreateReferenceError("The name '" + name + "' could not be resolved.");
        }

        public void Set(string name, IDynamic value, bool strict)
        {
            if (strict) throw Environment.CreateReferenceError("The name '" + name + "' could not be resolved.");
            Environment.GlobalObject.Put(name, value, strict);
        }
    }
}