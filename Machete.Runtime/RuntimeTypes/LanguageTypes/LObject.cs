using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public class LObject : LType
    {
        private readonly Dictionary<string, SPropertyDescriptor> _map = new Dictionary<string, SPropertyDescriptor>();


        public LType Prototype { get; set; }
        public string Class { get; set; }
        public bool Extensible { get; set; }
        public virtual object PrimitiveValue { get; set; }
        public virtual object Scope { get; set; }
        public virtual string[] FormalParameters { get; set; }
        public virtual object Code { get; set; }
        public virtual object TargetFunction { get; set; }
        public virtual object BoundThis { get; set; }
        public virtual object BoundArguments { get; set; }
        public virtual object ParameterMatch { get; set; }


        public virtual SPropertyDescriptor GetOwnProperty(string p)
        {
            SPropertyDescriptor value;
            if (_map.TryGetValue(p, out value))
            {
                return value.Copy();
            }
            return null;
        }

        public virtual SPropertyDescriptor GetProperty(string p)
        {
            var prop = GetOwnProperty(p);
            if (prop == null)
            {
                if (Prototype is LNull)
                {
                    return null;
                }
                return ((LObject)Prototype).GetProperty(p);
            }
            return prop;
        }

        public virtual LType Get(string p)
        {
            var desc = GetProperty(p);
            if (desc == null)
            {
                return LUndefined.Value;
            }
            var pd = (SPropertyDescriptor)desc;
            if (pd.IsDataDescriptor)
            {
                return pd.Value;
            }
            else
            {
                if (pd.Get is LUndefined)
                {
                    return LUndefined.Value;
                }
                return ((LObject)pd.Get).Call(this, SList.Empty);
            }
        }

        public virtual bool CanPut(string p)
        {
            throw new NotImplementedException();
        }

        public virtual void Put(string p, LType value, bool @throw)
        {
            throw new NotImplementedException();
        }

        public virtual bool HasProperty(string p)
        {
            throw new NotImplementedException();
        }

        public virtual bool Delete(string p, bool @throw)
        {
            throw new NotImplementedException();
        }

        public virtual object DefaultValue(string hint)
        {
            throw new NotImplementedException();
        }

        public virtual bool DefineOwnProperty(string p, SPropertyDescriptor desc, bool @throw)
        {
            throw new NotImplementedException();
        }

        public virtual LType Construct(SList args)
        {
            throw new NotImplementedException();
        }

        public virtual LType Call(LType @this, SList args)
        {
            throw new NotImplementedException();
        }

        public virtual bool HasInstance(object obj)
        {
            throw new NotImplementedException();
        }

        public virtual bool Match(string input, int index)
        {
            throw new NotImplementedException();
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

        public override LType ConvertToPrimitive()
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
