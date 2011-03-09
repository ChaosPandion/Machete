using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.NativeObjects;
using Machete.Core;
using System.Dynamic;
using System.Linq.Expressions;
using System.Diagnostics;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public abstract class LType : IDynamic
    {
        public IEnvironment Environment { get; private set; }

        public abstract LanguageTypeCode TypeCode { get; }

        public abstract bool IsPrimitive { get; }

        public virtual IDynamic Value
        {
            get { return this; }
            set { }
        }


        protected LType(IEnvironment environment)
        {
            Environment = environment;
        }


        public virtual IDynamic Op_LogicalNot()
        {
            return Environment.CreateBoolean(!this.ConvertToBoolean().BaseValue);
        }

        public virtual IDynamic Op_LogicalOr(IDynamic other)
        {
            return this.ConvertToBoolean().BaseValue ? this : other;
        }

        public virtual IDynamic Op_LogicalAnd(IDynamic other)
        {
            return !this.ConvertToBoolean().BaseValue ? this : other;
        }

        public virtual IDynamic Op_BitwiseNot()
        {
            return Environment.CreateNumber((double)(~(int)(double)this.ConvertToInt32().BaseValue));
        }

        public virtual IDynamic Op_BitwiseOr(IDynamic other)
        {
            var lnum = (int)this.ConvertToInt32().BaseValue;
            var rnum = (int)(double)other.ConvertToInt32().BaseValue;
            return Environment.CreateNumber((double)(lnum | rnum));
        }

        public virtual IDynamic Op_BitwiseXor(IDynamic other)
        {
            var lnum = (int)(double)this.ConvertToInt32().BaseValue;
            var rnum = (int)(double)other.ConvertToInt32().BaseValue;
            return Environment.CreateNumber((double)(lnum ^ rnum));
        }

        public virtual IDynamic Op_BitwiseAnd(IDynamic other)
        {
            var lnum = (int)(double)this.ConvertToInt32().BaseValue;
            var rnum = (int)(double)other.ConvertToInt32().BaseValue;
            return Environment.CreateNumber((double)(lnum & rnum));
        }

        public abstract IDynamic Op_Equals(IDynamic other);

        public virtual IDynamic Op_DoesNotEquals(IDynamic other)
        {
            return this.Op_Equals(other).Op_LogicalNot();
        }

        public abstract IDynamic Op_StrictEquals(IDynamic other);

        public virtual IDynamic Op_StrictDoesNotEquals(IDynamic other)
        {
            return this.Op_StrictEquals(other).Op_LogicalNot();
        }

        public virtual IDynamic Op_Lessthan(IDynamic other)
        {
            var r = RelationalComparison(true, this, other);
            return r.TypeCode == LanguageTypeCode.Undefined ? Environment.False : r;
        }

        public virtual IDynamic Op_Greaterthan(IDynamic other)
        {
            var r = RelationalComparison(false, other, this);
            return r.TypeCode == LanguageTypeCode.Undefined ? Environment.False : r;
        }

        public virtual IDynamic Op_LessthanOrEqual(IDynamic other)
        {
            var r = RelationalComparison(false, other, this);
            if (r.TypeCode == LanguageTypeCode.Undefined || ((LBoolean)r).BaseValue)
            {
                return Environment.False;
            }
            return Environment.True;
        }

        public virtual IDynamic Op_GreaterthanOrEqual(IDynamic other)
        {
            var r = RelationalComparison(true, this, other);
            if (r.TypeCode == LanguageTypeCode.Undefined || ((LBoolean)r).BaseValue)
            {
                return Environment.False;
            }
            return Environment.True;
        }

        public virtual IDynamic Op_Instanceof(IDynamic other)
        {
            var func = other.Value as IHasInstance;
            if (func == null)
            {
                throw Environment.CreateTypeError("");
            }
            return Environment.CreateBoolean(func.HasInstance(this));
        }

        public virtual IDynamic Op_In(IDynamic other)
        {
            if (other.TypeCode != LanguageTypeCode.Object)
            {
                throw Environment.CreateTypeError("");
            }
            var obj = other.ConvertToObject();
            var prop = this.ConvertToString().BaseValue;
            return Environment.CreateBoolean(obj.HasProperty(prop));
        }

        public virtual IDynamic Op_LeftShift(IDynamic other)
        {
            var lnum = (int)(double)this.ConvertToInt32().BaseValue;
            var rnum = (uint)(double)other.ConvertToUInt32().BaseValue;
            var shiftCount = (int)(rnum & (uint)0x1F);
            return Environment.CreateNumber((double)(lnum << shiftCount));
        }

        public virtual IDynamic Op_SignedRightShift(IDynamic other)
        {
            var lnum = (int)(double)this.ConvertToInt32().BaseValue;
            var rnum = (uint)(double)other.ConvertToUInt32().BaseValue;
            var shiftCount = (int)(rnum & (uint)0x1F);
            return Environment.CreateNumber((double)(lnum >> shiftCount));
        }

        public virtual IDynamic Op_UnsignedRightShift(IDynamic other)
        {
            var lnum = (uint)(double)this.ConvertToUInt32().BaseValue;
            var rnum = (uint)(double)other.ConvertToUInt32().BaseValue;
            var shiftCount = (int)(rnum & (uint)0x1F);
            return Environment.CreateNumber((double)unchecked(lnum >> shiftCount));
        }

        public virtual IDynamic Op_Addition(IDynamic other)
        {
            var lprim = this.ConvertToPrimitive(null);
            var rprim = other.ConvertToPrimitive(null);
            if (lprim.TypeCode == LanguageTypeCode.String || rprim.TypeCode == LanguageTypeCode.String)
            {
                var lstr = (string)lprim.ConvertToString().BaseValue;
                var rstr = (string)rprim.ConvertToString().BaseValue;
                return Environment.CreateString(lstr + rstr);
            }
            else
            {
                var lnum = (double)lprim.ConvertToNumber().BaseValue;
                var rnum = (double)rprim.ConvertToNumber().BaseValue;
                return Environment.CreateNumber(lnum + rnum);
            }
        }

        public virtual IDynamic Op_Subtraction(IDynamic other)
        {
            var lnum = (double)this.ConvertToNumber().BaseValue;
            var rnum = (double)other.ConvertToNumber().BaseValue;
            return Environment.CreateNumber(lnum - rnum);
        }

        public virtual IDynamic Op_Multiplication(IDynamic other)
        {
            var lnum = (double)this.ConvertToNumber().BaseValue;
            var rnum = (double)other.ConvertToNumber().BaseValue;
            return Environment.CreateNumber(lnum * rnum);
        }

        public virtual IDynamic Op_Division(IDynamic other)
        {
            var lnum = (double)this.ConvertToNumber().BaseValue;
            var rnum = (double)other.ConvertToNumber().BaseValue;
            return Environment.CreateNumber(lnum / rnum);
        }

        public virtual IDynamic Op_Modulus(IDynamic other)
        {
            var lnum = (double)this.ConvertToNumber().BaseValue;
            var rnum = (double)other.ConvertToNumber().BaseValue;
            return Environment.CreateNumber(lnum % rnum);
        }

        public virtual IDynamic Op_Delete()
        {
            return Environment.True;
        }

        public virtual IDynamic Op_Void()
        {
            return Environment.Undefined;
        }

        public abstract IDynamic Op_Typeof();

        public virtual IDynamic Op_PrefixIncrement()
        {
            throw new NotImplementedException();
        }

        public virtual IDynamic Op_PrefixDecrement()
        {
            throw new NotImplementedException();
        }

        public virtual IDynamic Op_Plus()
        {
            return this.ConvertToNumber();
        }

        public virtual IDynamic Op_Minus()
        {
            var r = (double)this.ConvertToNumber().BaseValue;
            if (double.IsNaN(r))
            {
                return Environment.CreateNumber(double.NaN);
            }
            return Environment.CreateNumber(-r);
        }

        public virtual IDynamic Op_PostfixIncrement()
        {
            throw new NotImplementedException();
        }

        public virtual IDynamic Op_PostfixDecrement()
        {
            throw new NotImplementedException();
        }
        
        public virtual IDynamic Op_Call(IArgs args)
        {
            throw Environment.CreateTypeError("This value is not callable.");
        }

        public virtual IObject Op_Construct(IArgs args)
        {
            var c = this as IConstructable;
            if (c != null)
            {
                return c.Construct(Environment, args);
            }
            throw Environment.CreateTypeError("This value is not constructable.");
        }

        public virtual void Op_Throw()
        {
            Environment.ThrowRuntimeException(this);
        }

        public abstract IDynamic ConvertToPrimitive(string preferredType);

        public abstract IBoolean ConvertToBoolean();

        public abstract INumber ConvertToNumber();

        public abstract IString ConvertToString();

        public abstract IObject ConvertToObject();

        public virtual INumber ConvertToInteger()
        {
            var number = this.ConvertToNumber();
            if (double.IsNaN((double)number.BaseValue))
            {
                return Environment.CreateNumber(0);
            }
            else if (number.BaseValue == 0.0 || double.IsInfinity((double)number.BaseValue))
            {
                return number;
            }
            else
            {
                var sign = Math.Sign((double)number.BaseValue);
                var abs = Math.Abs((double)number.BaseValue);
                var floor = Math.Floor(abs);
                return Environment.CreateNumber(sign * floor);
            }
        }

        public virtual INumber ConvertToInt32()
        {
            var number = (double)this.ConvertToNumber().BaseValue;
            if (number == 0.0 || double.IsNaN(number) || double.IsInfinity(number))
            {
                return Environment.CreateNumber(0);
            }
            var posInt = Math.Sign(number) * Math.Floor(Math.Abs(number));
            var int32Bit = posInt % uint.MaxValue;
            if (int32Bit > int.MaxValue)
            {
                return Environment.CreateNumber(int32Bit - uint.MaxValue);
            }
            return Environment.CreateNumber(int32Bit);
        }

        public virtual INumber ConvertToUInt32()
        {
            var number = (double)this.ConvertToNumber().BaseValue;
            if (number == 0.0 || double.IsNaN(number) || double.IsInfinity(number))
            {
                return Environment.CreateNumber(0);
            }
            var posInt = Math.Sign(number) * Math.Floor(Math.Abs(number));
            var int32Bit = posInt % uint.MaxValue;
            return Environment.CreateNumber(int32Bit);
        }

        public virtual INumber ConvertToUInt16()
        {
            var number = (double)this.ConvertToNumber().BaseValue;
            if (number == 0.0 || double.IsNaN(number) || double.IsInfinity(number))
            {
                return Environment.CreateNumber(0);
            }
            var posInt = Math.Sign(number) * Math.Floor(Math.Abs(number));
            var int16Bit = posInt % ushort.MaxValue;
            return Environment.CreateNumber(int16Bit);
        }
        
        public override string ToString()
        {
            return ConvertToString().BaseValue;
        }
               
        private IDynamic RelationalComparison(bool leftFirst, IDynamic left, IDynamic right)
        {
            IDynamic px, py;
            if (leftFirst)
            {
                px = left.ConvertToPrimitive("Number");
                py = right.ConvertToPrimitive("Number");
            }
            else
            {
                py = right.ConvertToPrimitive("Number");
                px = left.ConvertToPrimitive("Number");
            }
            if (px.TypeCode == LanguageTypeCode.String && py.TypeCode == LanguageTypeCode.String)
            {
                string sx = (string)px.ConvertToString().BaseValue, sy = (string)py.ConvertToString().BaseValue;
                if (sx.StartsWith(sy))
                {
                    return Environment.False;
                }
                else if (sy.StartsWith(sx))
                {
                    return Environment.True;
                }
                else
                {
                    return Environment.CreateBoolean(sx.CompareTo(sy) < 0);
                }
            }
            else
            {
                double nx = (double)px.ConvertToNumber().BaseValue, ny = (double)py.ConvertToNumber().BaseValue;
                if (double.IsNaN(nx) || double.IsNaN(ny))
                {
                    return Environment.Undefined;
                }
                else if (nx != ny)
                {
                    if (double.IsPositiveInfinity(nx))
                    {
                        return Environment.False;
                    }
                    else if (double.IsPositiveInfinity(ny))
                    {
                        return Environment.True;
                    }
                    else if (double.IsNegativeInfinity(ny))
                    {
                        return Environment.False;
                    }
                    else if (double.IsNegativeInfinity(nx))
                    {
                        return Environment.True;
                    }
                    else
                    {
                        return Environment.CreateBoolean(nx < ny);
                    }
                }
                return Environment.False;
            }
        }
    }
}