using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.NativeObjects;
using Machete.Interfaces;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    internal static class LType
    {
        public static IDynamic Op_LogicalOr(IDynamic left, IDynamic right)
        {
            return left.ConvertToBoolean().BaseValue ? left : right;
        }

        public static IDynamic Op_LogicalAnd(IDynamic left, IDynamic right)
        {
            return !left.ConvertToBoolean().BaseValue ? left : right;
        }

        public static IDynamic Op_BitwiseOr(IDynamic left, IDynamic right)
        {
            var lnum = (int)left.ConvertToInt32().BaseValue;
            var rnum = (int)(double)right.ConvertToInt32().BaseValue;
            return (LNumber)(double)(lnum | rnum);
        }

        public static IDynamic Op_BitwiseXor(IDynamic left, IDynamic right)
        {
            var lnum = (int)(double)left.ConvertToInt32().BaseValue;
            var rnum = (int)(double)right.ConvertToInt32().BaseValue;
            return (LNumber)(double)(lnum ^ rnum);
        }

        public static IDynamic Op_BitwiseAnd(IDynamic left, IDynamic right)
        {
            var lnum = (int)(double)left.ConvertToInt32().BaseValue;
            var rnum = (int)(double)right.ConvertToInt32().BaseValue;
            return (LNumber)(double)(lnum & rnum);
        }

        public static IDynamic Op_DoesNotEquals(IDynamic left, IDynamic right)
        {
            return left.Op_Equals(right).Op_LogicalNot();
        }

        public static IDynamic Op_StrictDoesNotEquals(IDynamic left, IDynamic right)
        {
            return left.Op_StrictEquals(right).Op_LogicalNot();
        }

        public static IDynamic Op_Lessthan(IDynamic left, IDynamic right)
        {
            var r = RelationalComparison(true, left, right);
            if (r.TypeCode == LanguageTypeCode.Undefined)
            {
                return LBoolean.False;
            }
            return r;
        }

        public static IDynamic Op_Greaterthan(IDynamic left, IDynamic right)
        {
            var r = RelationalComparison(false, left, right);
            if (r.TypeCode == LanguageTypeCode.Undefined)
            {
                return LBoolean.False;
            }
            return r;
        }

        public static IDynamic Op_LessthanOrEqual(IDynamic left, IDynamic right)
        {
            var r = RelationalComparison(false, left, right);
            if (r.TypeCode == LanguageTypeCode.Undefined || (bool)(LBoolean)r)
            {
                return LBoolean.True;
            }
            return LBoolean.False;
        }

        public static IDynamic Op_GreaterthanOrEqual(IDynamic left, IDynamic right)
        {
            var r = RelationalComparison(true, left, right);
            if (r.TypeCode == LanguageTypeCode.Undefined || (bool)(LBoolean)r)
            {
                return LBoolean.False;
            }
            return LBoolean.True;
        }

        public static IDynamic Op_Instanceof(IDynamic left, IDynamic right)
        {
            var func = right as NFunction;
            if (func == null)
            {
                throw Environment.ThrowTypeError();
            }
            return (LBoolean)func.HasInstance(left);
        }

        public static IDynamic Op_In(IDynamic left, IDynamic right)
        {
            var obj = right as LObject;
            if (obj == null)
            {
                throw Environment.ThrowTypeError();
            }
            return (LBoolean)obj.HasProperty((string)left.ConvertToString().BaseValue);
        }

        public static IDynamic Op_LeftShift(IDynamic left, IDynamic right)
        {
            var lnum = (int)(double)left.ConvertToInt32().BaseValue;
            var rnum = (uint)(double)right.ConvertToUInt32().BaseValue;
            var shiftCount = (int)(rnum & (uint)0x1F);
            return (LNumber)(double)(lnum << shiftCount);
        }

        public static IDynamic Op_SignedRightShift(IDynamic left, IDynamic right)
        {
            var lnum = (int)(double)left.ConvertToInt32().BaseValue;
            var rnum = (uint)(double)right.ConvertToUInt32().BaseValue;
            var shiftCount = (int)(rnum & (uint)0x1F);
            return (LNumber)(double)(lnum >> shiftCount);
        }

        public static IDynamic Op_UnsignedRightShift(IDynamic left, IDynamic right)
        {
            var lnum = (uint)(double)left.ConvertToUInt32().BaseValue;
            var rnum = (uint)(double)right.ConvertToUInt32().BaseValue;
            var shiftCount = (int)(rnum & (uint)0x1F);
            return (LNumber)(double)unchecked(lnum >> shiftCount);
        }

        public static IDynamic Op_Addition(IDynamic left, IDynamic right)
        {
            var lprim = left.ConvertToPrimitive(null);
            var rprim = right.ConvertToPrimitive(null);
            if (lprim.TypeCode == LanguageTypeCode.String || rprim.TypeCode == LanguageTypeCode.String)
            {
                var lstr = (string)lprim.ConvertToString().BaseValue;
                var rstr = (string)rprim.ConvertToString().BaseValue;
                return (LString)(lstr + rstr);
            }
            else
            {
                var lnum = (double)lprim.ConvertToNumber().BaseValue;
                var rnum = (double)rprim.ConvertToNumber().BaseValue;
                return (LNumber)(lnum + rnum);
            }
        }

        public static IDynamic Op_Subtraction(IDynamic left, IDynamic right)
        {
            var lnum = (double)left.ConvertToNumber().BaseValue;
            var rnum = (double)right.ConvertToNumber().BaseValue;
            return (LNumber)(lnum - rnum);
        }

        public static IDynamic Op_Multiplication(IDynamic left, IDynamic right)
        {
            var lnum = (double)left.ConvertToNumber().BaseValue;
            var rnum = (double)right.ConvertToNumber().BaseValue;
            return (LNumber)(lnum * rnum);
        }

        public static IDynamic Op_Division(IDynamic left, IDynamic right)
        {
            var lnum = (double)left.ConvertToNumber().BaseValue;
            var rnum = (double)right.ConvertToNumber().BaseValue;
            return (LNumber)(lnum / rnum);
        }

        public static IDynamic Op_Modulus(IDynamic left, IDynamic right)
        {
            var lnum = (double)left.ConvertToNumber().BaseValue;
            var rnum = (double)right.ConvertToNumber().BaseValue;
            return (LNumber)(lnum % rnum);
        }

        public static IDynamic Op_Plus(IDynamic value)
        {
            return value.ConvertToNumber();
        }

        public static IDynamic Op_Minus(IDynamic value)
        {
            var r = (double)value.ConvertToNumber().BaseValue;
            if (double.IsNaN(r))
            {
                return LNumber.NaN;
            }
            return (LNumber)(-r);
        }

        public static IDynamic Op_BitwiseNot(IDynamic value)
        {
            return (LNumber)(double)(~(int)(double)value.ConvertToInt32().BaseValue);
        }

        public static IDynamic Op_LogicalNot(IDynamic value)
        {
            return (LBoolean)(!(bool)value.ConvertToBoolean().BaseValue);
        }

        public static IDynamic Op_GetProperty(IDynamic value, IDynamic name)
        {
            throw new NotImplementedException();
        }

        public static void Op_SetProperty(IDynamic of, IDynamic name, IDynamic value)
        {
            throw new NotImplementedException();
        }

        public static IDynamic Op_Call(IDynamic value, IArgs args)
        {
            throw new NotImplementedException();
        }

        public static IObject Op_Construct(IDynamic value, IArgs args)
        {
            throw new NotImplementedException();
        }

        public static void Op_Throw(IDynamic value)
        {
            throw new NotImplementedException();
        }

        public static INumber ConvertToInteger(IDynamic value)
        {
            var number = value.ConvertToNumber();
            if (double.IsNaN((double)number.BaseValue))
            {
                return LNumber.Zero;
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
                return (LNumber)(sign * floor);
            }
        }

        public static INumber ConvertToInt32(IDynamic value)
        {
            var number = (double)value.ConvertToNumber().BaseValue;
            if (number == 0.0 || double.IsNaN(number) || double.IsInfinity(number))
            {
                return LNumber.Zero;
            }
            var posInt = Math.Sign(number) * Math.Floor(Math.Abs(number));
            var int32Bit = posInt % uint.MaxValue;
            if (int32Bit > int.MaxValue)
            {
                return (LNumber)(int32Bit - uint.MaxValue);
            }
            return (LNumber)int32Bit;
        }

        public static INumber ConvertToUInt32(IDynamic value)
        {
            var number = (double)value.ConvertToNumber().BaseValue;
            if (number == 0.0 || double.IsNaN(number) || double.IsInfinity(number))
            {
                return LNumber.Zero;
            }
            var posInt = Math.Sign(number) * Math.Floor(Math.Abs(number));
            var int32Bit = posInt % uint.MaxValue;
            return (LNumber)int32Bit;
        }

        public static INumber ConvertToUInt16(IDynamic value)
        {
            var number = (double)value.ConvertToNumber().BaseValue;
            if (number == 0.0 || double.IsNaN(number) || double.IsInfinity(number))
            {
                return LNumber.Zero;
            }
            var posInt = Math.Sign(number) * Math.Floor(Math.Abs(number));
            var int16Bit = posInt % ushort.MaxValue;
            return (LNumber)int16Bit;
        }

        private static IDynamic RelationalComparison(bool leftFirst, IDynamic left, IDynamic right)
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
                    return LBoolean.False;
                }
                else if (sy.StartsWith(sx))
                {
                    return LBoolean.True;
                }
                else
                {
                    return (LBoolean)(sx.CompareTo(sy) < 0);
                }
            }
            else
            {
                double nx = (double)px.ConvertToNumber().BaseValue, ny = (double)py.ConvertToNumber().BaseValue;
                if (double.IsNaN(nx) || double.IsNaN(ny))
                {
                    return LUndefined.Instance;
                }
                else if (nx != ny)
                {
                    if (double.IsPositiveInfinity(nx))
                    {
                        return LBoolean.False;
                    }
                    else if (double.IsPositiveInfinity(ny))
                    {
                        return LBoolean.True;
                    }
                    else if (double.IsNegativeInfinity(ny))
                    {
                        return LBoolean.False;
                    }
                    else if (double.IsNegativeInfinity(nx))
                    {
                        return LBoolean.True;
                    }
                }
                return LBoolean.False;
            }
        }

        public static IDynamic GetValue(LBoolean value, string name, bool strict)
        {
            return GetValue((IDynamic)value, name, strict);
        }

        public static IDynamic GetValue(LString value, string name, bool strict)
        {
            return GetValue((IDynamic)value, name, strict);
        }

        public static IDynamic GetValue(LNumber value, string name, bool strict)
        {
            return GetValue((IDynamic)value, name, strict);
        }

        private static IDynamic GetValue(IDynamic value, string name, bool strict)
        {
            var obj = value.ConvertToObject();
            var desc = obj.GetProperty(name);
            if (desc == null) return LUndefined.Instance;
            if (desc.IsDataDescriptor) return desc.Value;
            if (desc.Get is LUndefined) return LUndefined.Instance;
            return desc.Get.Op_Call(SArgs.Empty);
        }

        public static void SetValue(LBoolean of, string name, IDynamic value, bool strict)
        {
            SetValue((IDynamic)of, name, value, strict);
        }

        public static void SetValue(LString of, string name, IDynamic value, bool strict)
        {
            SetValue((IDynamic)of, name, value, strict);
        }

        public static void SetValue(LNumber of, string name, IDynamic value, bool strict)
        {
            SetValue((IDynamic)of, name, value, strict);
        }

        private static void SetValue(IDynamic of, string name, IDynamic value, bool strict)
        {
            var obj = value.ConvertToObject();
            if (!obj.CanPut(name))
            {
                if (strict)
                {
                    throw Environment.ThrowTypeError();
                }
                return;
            }
            var ownDesc = obj.GetOwnProperty(name);
            if (ownDesc.IsDataDescriptor)
            {
                if (strict)
                {
                    throw Environment.ThrowTypeError();
                }
                return;
            }
            var desc = obj.GetProperty(name);
            if (desc.IsAccessorDescriptor)
            {
                desc.Set.Op_Call(new SArgs(value));
                return;
            }
            else
            {
                if (strict)
                {
                    throw Environment.ThrowTypeError();
                }
                return;
            }
        }
    }
}