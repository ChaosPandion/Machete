using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.NativeObjects;
using Machete.Core;
using System.Dynamic;
using System.Linq.Expressions;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public abstract class LType : DynamicObject, IDynamic
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

        public virtual IDynamic Op_GetProperty(IDynamic name)
        {
            throw new NotImplementedException();
        }

        public virtual void Op_SetProperty(IDynamic name, IDynamic value)
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

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {           
            var b = this as IReferenceBase;
            if (b != null)
            {
                var r = Environment.CreateReference(binder.Name, b, true);
                result = r.Value;
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            switch (binder.Operation)
            {
                case ExpressionType.Add:
                    result = this.Op_Addition(MakeDynamic(arg));
                    return true;
                case ExpressionType.AddAssign:
                    break;
                case ExpressionType.AddAssignChecked:
                    break;
                case ExpressionType.AddChecked:
                    break;
                case ExpressionType.And:
                    break;
                case ExpressionType.AndAlso:
                    break;
                case ExpressionType.AndAssign:
                    break;
                case ExpressionType.ArrayIndex:
                    break;
                case ExpressionType.ArrayLength:
                    break;
                case ExpressionType.Assign:
                    break;
                case ExpressionType.Block:
                    break;
                case ExpressionType.Call:
                    break;
                case ExpressionType.Coalesce:
                    break;
                case ExpressionType.Conditional:
                    break;
                case ExpressionType.Constant:
                    break;
                case ExpressionType.Convert:
                    break;
                case ExpressionType.ConvertChecked:
                    break;
                case ExpressionType.DebugInfo:
                    break;
                case ExpressionType.Decrement:
                    break;
                case ExpressionType.Default:
                    break;
                case ExpressionType.Divide:
                    break;
                case ExpressionType.DivideAssign:
                    break;
                case ExpressionType.Dynamic:
                    break;
                case ExpressionType.Equal:
                    result = this.Op_StrictEquals(MakeDynamic(arg));
                    return true;
                case ExpressionType.ExclusiveOr:
                    break;
                case ExpressionType.ExclusiveOrAssign:
                    break;
                case ExpressionType.Extension:
                    break;
                case ExpressionType.Goto:
                    break;
                case ExpressionType.GreaterThan:
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    break;
                case ExpressionType.Increment:
                    break;
                case ExpressionType.Index:
                    break;
                case ExpressionType.Invoke:
                    break;
                case ExpressionType.IsFalse:
                    break;
                case ExpressionType.IsTrue:
                    break;
                case ExpressionType.Label:
                    break;
                case ExpressionType.Lambda:
                    break;
                case ExpressionType.LeftShift:
                    break;
                case ExpressionType.LeftShiftAssign:
                    break;
                case ExpressionType.LessThan:
                    break;
                case ExpressionType.LessThanOrEqual:
                    break;
                case ExpressionType.ListInit:
                    break;
                case ExpressionType.Loop:
                    break;
                case ExpressionType.MemberAccess:
                    break;
                case ExpressionType.MemberInit:
                    break;
                case ExpressionType.Modulo:
                    break;
                case ExpressionType.ModuloAssign:
                    break;
                case ExpressionType.Multiply:
                    break;
                case ExpressionType.MultiplyAssign:
                    break;
                case ExpressionType.MultiplyAssignChecked:
                    break;
                case ExpressionType.MultiplyChecked:
                    break;
                case ExpressionType.Negate:
                    break;
                case ExpressionType.NegateChecked:
                    break;
                case ExpressionType.New:
                    break;
                case ExpressionType.NewArrayBounds:
                    break;
                case ExpressionType.NewArrayInit:
                    break;
                case ExpressionType.Not:
                    break;
                case ExpressionType.NotEqual:
                    break;
                case ExpressionType.OnesComplement:
                    break;
                case ExpressionType.Or:
                    break;
                case ExpressionType.OrAssign:
                    break;
                case ExpressionType.OrElse:
                    break;
                case ExpressionType.Parameter:
                    break;
                case ExpressionType.PostDecrementAssign:
                    break;
                case ExpressionType.PostIncrementAssign:
                    break;
                case ExpressionType.Power:
                    break;
                case ExpressionType.PowerAssign:
                    break;
                case ExpressionType.PreDecrementAssign:
                    break;
                case ExpressionType.PreIncrementAssign:
                    break;
                case ExpressionType.Quote:
                    break;
                case ExpressionType.RightShift:
                    break;
                case ExpressionType.RightShiftAssign:
                    break;
                case ExpressionType.RuntimeVariables:
                    break;
                case ExpressionType.Subtract:
                    break;
                case ExpressionType.SubtractAssign:
                    break;
                case ExpressionType.SubtractAssignChecked:
                    break;
                case ExpressionType.SubtractChecked:
                    break;
                case ExpressionType.Switch:
                    break;
                case ExpressionType.Throw:
                    break;
                case ExpressionType.Try:
                    break;
                case ExpressionType.TypeAs:
                    break;
                case ExpressionType.TypeEqual:
                    break;
                case ExpressionType.TypeIs:
                    break;
                case ExpressionType.UnaryPlus:
                    break;
                case ExpressionType.Unbox:
                    break;
                default:
                    break;
            }
            return base.TryBinaryOperation(binder, arg, out result);
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            var callable = this as ICallable;
            if (callable != null)
            {
                var callArgs = Environment.CreateArgs(args.Select(MakeDynamic));
                callable.Call(Environment, this, callArgs);
            }
            return base.TryInvoke(binder, args, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var b = this as IReferenceBase;
            if (b != null)
            {
                var r = Environment.CreateReference(binder.Name, b, true);
                var callArgs = Environment.CreateArgs(args.Select(MakeDynamic));
                result = r.Op_Call(callArgs);
                return true;
            }
            return base.TryInvokeMember(binder, args, out result);
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            switch (Type.GetTypeCode(binder.Type))
            {
                case System.TypeCode.Boolean:
                    result = this.ConvertToBoolean().BaseValue;
                    return true;
                case System.TypeCode.DateTime:
                    break;
                case System.TypeCode.Byte:
                    result = (byte)this.ConvertToNumber().BaseValue;
                    return true;
                case System.TypeCode.Decimal:
                    result = (decimal)this.ConvertToNumber().BaseValue;
                    return true;
                case System.TypeCode.Double:
                    result = this.ConvertToNumber().BaseValue;
                    return true;
                case System.TypeCode.Int16:
                    result = (short)this.ConvertToNumber().BaseValue;
                    return true;
                case System.TypeCode.Int32:
                    result = (int)this.ConvertToNumber().BaseValue;
                    return true;
                case System.TypeCode.Int64:
                    result = (long)this.ConvertToNumber().BaseValue;
                    return true;
                case System.TypeCode.UInt16:
                    result = (ushort)this.ConvertToNumber().BaseValue;
                    return true;
                case System.TypeCode.UInt32:
                    result = (uint)this.ConvertToNumber().BaseValue;
                    return true;
                case System.TypeCode.UInt64:
                    result = (ulong)this.ConvertToNumber().BaseValue;
                    return true;
                case System.TypeCode.SByte:
                    result = (sbyte)this.ConvertToNumber().BaseValue;
                    return true;
                case System.TypeCode.Single:
                    result = (float)this.ConvertToNumber().BaseValue;
                    return true;
                case System.TypeCode.Empty:
                    {
                        if (this.TypeCode != LanguageTypeCode.Null)
                            throw new InvalidCastException();
                        result = null;
                        return true;
                    }
                case System.TypeCode.Object:
                    break;
                case System.TypeCode.Char:
                    {
                        var s = this.ConvertToString().BaseValue;
                        if (string.IsNullOrEmpty(s))
                            throw new InvalidCastException();
                        result = s[0];
                        return true;
                    }
                case System.TypeCode.String:
                    result = this.ConvertToString().BaseValue;
                    return true;
                default:
                    break;
            }
            return base.TryConvert(binder, out result);
        }

        private IDynamic MakeDynamic(object o)
        {
            if (o == null)
            {
                return Environment.Null;
            }

            var r = o as IDynamic;
            if (r != null)
            {
                return r;
            }

            switch (Type.GetTypeCode(o.GetType()))
            {
                case System.TypeCode.Byte:
                case System.TypeCode.Decimal:
                case System.TypeCode.Double:
                case System.TypeCode.Int16:
                case System.TypeCode.Int32:
                case System.TypeCode.Int64:
                case System.TypeCode.UInt16:
                case System.TypeCode.UInt32:
                case System.TypeCode.UInt64:
                case System.TypeCode.SByte:
                case System.TypeCode.Single:
                    return Environment.CreateNumber(Convert.ToDouble(o));
                case System.TypeCode.Boolean:
                    return Environment.CreateBoolean((bool)o);
                case System.TypeCode.Char:
                case System.TypeCode.String:
                    return Environment.CreateString(o.ToString());
                default:
                    throw new NotImplementedException();
            }
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