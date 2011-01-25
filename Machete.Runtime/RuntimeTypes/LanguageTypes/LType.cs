using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.NativeObjects;
using Machete.Core;
using System.Dynamic;

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
            var r = RelationalComparison(Environment, true, this, other);
            return r.TypeCode == LanguageTypeCode.Undefined ? Environment.False : r;
        }

        public virtual IDynamic Op_Greaterthan(IDynamic other)
        {
            var r = RelationalComparison(Environment, false, other, this);
            return r.TypeCode == LanguageTypeCode.Undefined ? Environment.False : r;
        }

        public virtual IDynamic Op_LessthanOrEqual(IDynamic other)
        {
            var r = RelationalComparison(Environment, false, other, this);
            if (r.TypeCode == LanguageTypeCode.Undefined || ((LBoolean)r).BaseValue)
            {
                return Environment.False;
            }
            return Environment.True;
        }

        public virtual IDynamic Op_GreaterthanOrEqual(IDynamic other)
        {
            var r = RelationalComparison(Environment, true, this, other);
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

        private static IDynamic RelationalComparison(IEnvironment environment, bool leftFirst, IDynamic left, IDynamic right)
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
                    return environment.False;
                }
                else if (sy.StartsWith(sx))
                {
                    return environment.True;
                }
                else
                {
                    return environment.CreateBoolean(sx.CompareTo(sy) < 0);
                }
            }
            else
            {
                double nx = (double)px.ConvertToNumber().BaseValue, ny = (double)py.ConvertToNumber().BaseValue;
                if (double.IsNaN(nx) || double.IsNaN(ny))
                {
                    return environment.Undefined;
                }
                else if (nx != ny)
                {
                    if (double.IsPositiveInfinity(nx))
                    {
                        return environment.False;
                    }
                    else if (double.IsPositiveInfinity(ny))
                    {
                        return environment.True;
                    }
                    else if (double.IsNegativeInfinity(ny))
                    {
                        return environment.False;
                    }
                    else if (double.IsNegativeInfinity(nx))
                    {
                        return environment.True;
                    }
                    else
                    {
                        return environment.CreateBoolean(nx < ny);
                    }
                }
                return environment.False;
            }
        }        
    }







    //public static INumber ConvertToUInt16(IEnvironment environment, IDynamic value)
    //{
    //    var number = (double)value.ConvertToNumber().BaseValue;
    //    if (number == 0.0 || double.IsNaN(number) || double.IsInfinity(number))
    //    {
    //        return environment.CreateNumber(0);
    //    }
    //    var posInt = Math.Sign(number) * Math.Floor(Math.Abs(number));
    //    var int16Bit = posInt % ushort.MaxValue;
    //    return environment.CreateNumber(int16Bit);
    //}

    //private static IDynamic RelationalComparison(IEnvironment environment, bool leftFirst, IDynamic left, IDynamic right)
    //{
    //    IDynamic px, py;
    //    if (leftFirst)
    //    {
    //        px = left.ConvertToPrimitive("Number");
    //        py = right.ConvertToPrimitive("Number");
    //    }
    //    else
    //    {
    //        py = right.ConvertToPrimitive("Number");
    //        px = left.ConvertToPrimitive("Number");
    //    }
    //    if (px.TypeCode == LanguageTypeCode.String && py.TypeCode == LanguageTypeCode.String)
    //    {
    //        string sx = (string)px.ConvertToString().BaseValue, sy = (string)py.ConvertToString().BaseValue;
    //        if (sx.StartsWith(sy))
    //        {
    //            return environment.False;
    //        }
    //        else if (sy.StartsWith(sx))
    //        {
    //            return environment.True;
    //        }
    //        else
    //        {
    //            return environment.CreateBoolean(sx.CompareTo(sy) < 0);
    //        }
    //    }
    //    else
    //    {
    //        double nx = (double)px.ConvertToNumber().BaseValue, ny = (double)py.ConvertToNumber().BaseValue;
    //        if (double.IsNaN(nx) || double.IsNaN(ny))
    //        {
    //            return environment.Undefined;
    //        }
    //        else if (nx != ny)
    //        {
    //            if (double.IsPositiveInfinity(nx))
    //            {
    //                return environment.False;
    //            }
    //            else if (double.IsPositiveInfinity(ny))
    //            {
    //                return environment.True;
    //            }
    //            else if (double.IsNegativeInfinity(ny))
    //            {
    //                return environment.False;
    //            }
    //            else if (double.IsNegativeInfinity(nx))
    //            {
    //                return environment.True;
    //            }
    //            else
    //            {
    //                return environment.CreateBoolean(nx < ny);
    //            }
    //        }
    //        return environment.False;
    //    }
    //}

    //public static IDynamic GetValue(IEnvironment environment, LBoolean value, string name, bool strict)
    //{
    //    return GetValue(environment, (IDynamic)value, name, strict);
    //}

    //public static IDynamic GetValue(IEnvironment environment, LString value, string name, bool strict)
    //{
    //    return GetValue(environment, (IDynamic)value, name, strict);
    //}

    //public static IDynamic GetValue(IEnvironment environment, LNumber value, string name, bool strict)
    //{
    //    return GetValue(environment, (IDynamic)value, name, strict);
    //}

    //private static IDynamic GetValue(IEnvironment environment, IDynamic value, string name, bool strict)
    //{
    //    var obj = value.ConvertToObject();
    //    var desc = obj.GetProperty(name);
    //    if (desc == null) return environment.Undefined;
    //    if (desc.IsDataDescriptor) return desc.Value;
    //    if (desc.Get is LUndefined) return environment.Undefined;
    //    return desc.Get.Op_Call(new SArgs(environment));
    //}

    //public static void SetValue(IEnvironment environment, LBoolean of, string name, IDynamic value, bool strict)
    //{
    //    SetValue(environment, (IDynamic)of, name, value, strict);
    //}

    //public static void SetValue(IEnvironment environment, LString of, string name, IDynamic value, bool strict)
    //{
    //    SetValue(environment, (IDynamic)of, name, value, strict);
    //}

    //public static void SetValue(IEnvironment environment, LNumber of, string name, IDynamic value, bool strict)
    //{
    //    SetValue(environment, (IDynamic)of, name, value, strict);
    //}

    //private static void SetValue(IEnvironment environment, IDynamic of, string name, IDynamic value, bool strict)
    //{
    //    var obj = value.ConvertToObject();
    //    if (!obj.CanPut(name))
    //    {
    //        if (strict)
    //        {
    //            throw environment.CreateTypeError("");
    //        }
    //        return;
    //    }
    //    var ownDesc = obj.GetOwnProperty(name);
    //    if (ownDesc.IsDataDescriptor)
    //    {
    //        if (strict)
    //        {
    //            throw environment.CreateTypeError("");
    //        }
    //        return;
    //    }
    //    var desc = obj.GetProperty(name);
    //    if (desc.IsAccessorDescriptor)
    //    {
    //        desc.Set.Op_Call(new SArgs(environment, value));
    //        return;
    //    }
    //    else
    //    {
    //        if (strict)
    //        {
    //            throw environment.CreateTypeError("");
    //        }
    //        return;
    //    }
    //}
    //public static INumber ConvertToInteger(IEnvironment environment, IDynamic value)
    //{
    //    var number = value.ConvertToNumber();
    //    if (double.IsNaN((double)number.BaseValue))
    //    {
    //        return environment.CreateNumber(0);
    //    }
    //    else if (number.BaseValue == 0.0 || double.IsInfinity((double)number.BaseValue))
    //    {
    //        return number;
    //    }
    //    else
    //    {
    //        var sign = Math.Sign((double)number.BaseValue);
    //        var abs = Math.Abs((double)number.BaseValue);
    //        var floor = Math.Floor(abs);
    //        return environment.CreateNumber(sign * floor);
    //    }
    //}

    //public static INumber ConvertToInt32(IEnvironment environment, IDynamic value)
    //{
    //    var number = (double)value.ConvertToNumber().BaseValue;
    //    if (number == 0.0 || double.IsNaN(number) || double.IsInfinity(number))
    //    {
    //        return environment.CreateNumber(0);
    //    }
    //    var posInt = Math.Sign(number) * Math.Floor(Math.Abs(number));
    //    var int32Bit = posInt % uint.MaxValue;
    //    if (int32Bit > int.MaxValue)
    //    {
    //        return environment.CreateNumber(int32Bit - uint.MaxValue);
    //    }
    //    return environment.CreateNumber(int32Bit);
    //}

    //public static INumber ConvertToUInt32(IEnvironment environment, IDynamic value)
    //{
    //    var number = (double)value.ConvertToNumber().BaseValue;
    //    if (number == 0.0 || double.IsNaN(number) || double.IsInfinity(number))
    //    {
    //        return environment.CreateNumber(0);
    //    }
    //    var posInt = Math.Sign(number) * Math.Floor(Math.Abs(number));
    //    var int32Bit = posInt % uint.MaxValue;
    //    return environment.CreateNumber(int32Bit);
    //}
    //public static IDynamic Op_GetProperty(IEnvironment environment, IDynamic value, IDynamic name)
    //{
    //    throw new NotImplementedException();
    //}

    //public static void Op_SetProperty(IEnvironment environment, IDynamic of, IDynamic name, IDynamic value)
    //{
    //    throw new NotImplementedException();
    //}

    //public static IDynamic Op_Call(IEnvironment environment, IDynamic value, IArgs args)
    //{
    //    var callable = value as ICallable;
    //    if (callable == null)
    //    {
    //        throw environment.CreateTypeError("");
    //    }
    //    return callable.Call(environment, environment.Undefined, args);
    //}

    //public static IObject Op_Construct(IEnvironment environment, IDynamic value, IArgs args)
    //{
    //    throw new NotImplementedException();
    //}

    //public static void Op_Throw(IEnvironment environment, IDynamic value)
    //{
    //    environment.ThrowRuntimeException(value);
    //}

    //public static IDynamic Op_LogicalOr(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    return left.ConvertToBoolean().BaseValue ? left : right;
    //}

    //public static IDynamic Op_LogicalAnd(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    return !left.ConvertToBoolean().BaseValue ? left : right;
    //}

    //public static IDynamic Op_BitwiseOr(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var lnum = (int)left.ConvertToInt32().BaseValue;
    //    var rnum = (int)(double)right.ConvertToInt32().BaseValue;
    //    return environment.CreateNumber((double)(lnum | rnum));
    //}

    //public static IDynamic Op_BitwiseXor(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var lnum = (int)(double)left.ConvertToInt32().BaseValue;
    //    var rnum = (int)(double)right.ConvertToInt32().BaseValue;
    //    return environment.CreateNumber((double)(lnum ^ rnum));
    //}

    //public static IDynamic Op_BitwiseAnd(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var lnum = (int)(double)left.ConvertToInt32().BaseValue;
    //    var rnum = (int)(double)right.ConvertToInt32().BaseValue;
    //    return environment.CreateNumber((double)(lnum & rnum));
    //}

    //public static IDynamic Op_DoesNotEquals(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    return left.Op_Equals(right).Op_LogicalNot();
    //}

    //public static IDynamic Op_StrictDoesNotEquals(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    return left.Op_StrictEquals(right).Op_LogicalNot();
    //}

    //public static IDynamic Op_Lessthan(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var r = RelationalComparison(environment, true, left, right);
    //    if (r.TypeCode == LanguageTypeCode.Undefined)
    //    {
    //        return environment.False;
    //    }
    //    return r;
    //}

    //public static IDynamic Op_Greaterthan(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var r = RelationalComparison(environment, false, right, left);
    //    if (r.TypeCode == LanguageTypeCode.Undefined)
    //    {
    //        return environment.False;
    //    }
    //    return r;
    //}

    //public static IDynamic Op_LessthanOrEqual(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var r = RelationalComparison(environment, false, right, left);
    //    if (r.TypeCode == LanguageTypeCode.Undefined || (bool)(LBoolean)r)
    //    {
    //        return environment.False;
    //    }
    //    return environment.True;
    //}

    //public static IDynamic Op_GreaterthanOrEqual(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var r = RelationalComparison(environment, true, left, right);
    //    if (r.TypeCode == LanguageTypeCode.Undefined || (bool)(LBoolean)r)
    //    {
    //        return environment.False;
    //    }
    //    return environment.True;
    //}

    //public static IDynamic Op_Instanceof(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var func = right.Value as IHasInstance;
    //    if (func == null)
    //    {
    //        throw environment.CreateTypeError("");
    //    }
    //    return environment.CreateBoolean(func.HasInstance(left));
    //}

    //public static IDynamic Op_In(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    if (right.TypeCode != LanguageTypeCode.Object)
    //    {
    //        throw environment.CreateTypeError("");
    //    }
    //    var obj = right.ConvertToObject();
    //    var prop = left.ConvertToString().BaseValue;
    //    return environment.CreateBoolean(obj.HasProperty(prop));
    //}

    //public static IDynamic Op_LeftShift(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var lnum = (int)(double)left.ConvertToInt32().BaseValue;
    //    var rnum = (uint)(double)right.ConvertToUInt32().BaseValue;
    //    var shiftCount = (int)(rnum & (uint)0x1F);
    //    return environment.CreateNumber((double)(lnum << shiftCount));
    //}

    //public static IDynamic Op_SignedRightShift(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var lnum = (int)(double)left.ConvertToInt32().BaseValue;
    //    var rnum = (uint)(double)right.ConvertToUInt32().BaseValue;
    //    var shiftCount = (int)(rnum & (uint)0x1F);
    //    return environment.CreateNumber((double)(lnum >> shiftCount));
    //}

    //public static IDynamic Op_UnsignedRightShift(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var lnum = (uint)(double)left.ConvertToUInt32().BaseValue;
    //    var rnum = (uint)(double)right.ConvertToUInt32().BaseValue;
    //    var shiftCount = (int)(rnum & (uint)0x1F);
    //    return environment.CreateNumber((double)unchecked(lnum >> shiftCount));
    //}

    //public static IDynamic Op_Addition(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var lprim = left.ConvertToPrimitive(null);
    //    var rprim = right.ConvertToPrimitive(null);
    //    if (lprim.TypeCode == LanguageTypeCode.String || rprim.TypeCode == LanguageTypeCode.String)
    //    {
    //        var lstr = (string)lprim.ConvertToString().BaseValue;
    //        var rstr = (string)rprim.ConvertToString().BaseValue;
    //        return environment.CreateString(lstr + rstr);
    //    }
    //    else
    //    {
    //        var lnum = (double)lprim.ConvertToNumber().BaseValue;
    //        var rnum = (double)rprim.ConvertToNumber().BaseValue;
    //        return environment.CreateNumber(lnum + rnum);
    //    }
    //}

    //public static IDynamic Op_Subtraction(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var lnum = (double)left.ConvertToNumber().BaseValue;
    //    var rnum = (double)right.ConvertToNumber().BaseValue;
    //    return environment.CreateNumber(lnum - rnum);
    //}

    //public static IDynamic Op_Multiplication(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var lnum = (double)left.ConvertToNumber().BaseValue;
    //    var rnum = (double)right.ConvertToNumber().BaseValue;
    //    return environment.CreateNumber(lnum * rnum);
    //}

    //public static IDynamic Op_Division(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var lnum = (double)left.ConvertToNumber().BaseValue;
    //    var rnum = (double)right.ConvertToNumber().BaseValue;
    //    return environment.CreateNumber(lnum / rnum);
    //}

    //public static IDynamic Op_Modulus(IEnvironment environment, IDynamic left, IDynamic right)
    //{
    //    var lnum = (double)left.ConvertToNumber().BaseValue;
    //    var rnum = (double)right.ConvertToNumber().BaseValue;
    //    return environment.CreateNumber(lnum % rnum);
    //}

    //public static IDynamic Op_Plus(IEnvironment environment, IDynamic value)
    //{
    //    return value.ConvertToNumber();
    //}

    //public static IDynamic Op_Minus(IEnvironment environment, IDynamic value)
    //{
    //    var r = (double)value.ConvertToNumber().BaseValue;
    //    if (double.IsNaN(r))
    //    {
    //        return environment.CreateNumber(double.NaN);
    //    }
    //    return environment.CreateNumber(-r);
    //}

    //public static IDynamic Op_BitwiseNot(IEnvironment environment, IDynamic value)
    //{
    //    return environment.CreateNumber((double)(~(int)(double)value.ConvertToInt32().BaseValue));
    //}

    //public static IDynamic Op_LogicalNot(IEnvironment environment, IDynamic value)
    //{
    //    return environment.CreateBoolean(!value.ConvertToBoolean().BaseValue);
    //}
}