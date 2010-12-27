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
        public static IDynamic Op_LogicalOr(IEnvironment environment, IDynamic left, IDynamic right)
        {
            return left.ConvertToBoolean().BaseValue ? left : right;
        }

        public static IDynamic Op_LogicalAnd(IEnvironment environment, IDynamic left, IDynamic right)
        {
            return !left.ConvertToBoolean().BaseValue ? left : right;
        }

        public static IDynamic Op_BitwiseOr(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var lnum = (int)left.ConvertToInt32().BaseValue;
            var rnum = (int)(double)right.ConvertToInt32().BaseValue;
            return environment.CreateNumber((double)(lnum | rnum));
        }

        public static IDynamic Op_BitwiseXor(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var lnum = (int)(double)left.ConvertToInt32().BaseValue;
            var rnum = (int)(double)right.ConvertToInt32().BaseValue;
            return environment.CreateNumber((double)(lnum ^ rnum));
        }

        public static IDynamic Op_BitwiseAnd(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var lnum = (int)(double)left.ConvertToInt32().BaseValue;
            var rnum = (int)(double)right.ConvertToInt32().BaseValue;
            return environment.CreateNumber((double)(lnum & rnum));
        }

        public static IDynamic Op_DoesNotEquals(IEnvironment environment, IDynamic left, IDynamic right)
        {
            return left.Op_Equals(right).Op_LogicalNot();
        }

        public static IDynamic Op_StrictDoesNotEquals(IEnvironment environment, IDynamic left, IDynamic right)
        {
            return left.Op_StrictEquals(right).Op_LogicalNot();
        }

        public static IDynamic Op_Lessthan(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var r = RelationalComparison(environment, true, left, right);
            if (r.TypeCode == LanguageTypeCode.Undefined)
            {
                return environment.False;
            }
            return r;
        }

        public static IDynamic Op_Greaterthan(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var r = RelationalComparison(environment, false, right, left);
            if (r.TypeCode == LanguageTypeCode.Undefined)
            {
                return environment.False;
            }
            return r;
        }

        public static IDynamic Op_LessthanOrEqual(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var r = RelationalComparison(environment, false, right, left);
            if (r.TypeCode == LanguageTypeCode.Undefined || (bool)(LBoolean)r)
            {
                return environment.False;
            }
            return environment.True;
        }

        public static IDynamic Op_GreaterthanOrEqual(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var r = RelationalComparison(environment, true, left, right);
            if (r.TypeCode == LanguageTypeCode.Undefined || (bool)(LBoolean)r)
            {
                return environment.False;
            }
            return environment.True;
        }

        public static IDynamic Op_Instanceof(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var func = right.Value as IHasInstance;
            if (func == null)
            {
                throw environment.CreateTypeError("");
            }
            return environment.CreateBoolean(func.HasInstance(left));
        }

        public static IDynamic Op_In(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var obj = right as LObject;
            if (obj == null)
            {
                throw environment.CreateTypeError("");
            }
            return environment.CreateBoolean(obj.HasProperty(left.ConvertToString().BaseValue));
        }

        public static IDynamic Op_LeftShift(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var lnum = (int)(double)left.ConvertToInt32().BaseValue;
            var rnum = (uint)(double)right.ConvertToUInt32().BaseValue;
            var shiftCount = (int)(rnum & (uint)0x1F);
            return environment.CreateNumber((double)(lnum << shiftCount));
        }

        public static IDynamic Op_SignedRightShift(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var lnum = (int)(double)left.ConvertToInt32().BaseValue;
            var rnum = (uint)(double)right.ConvertToUInt32().BaseValue;
            var shiftCount = (int)(rnum & (uint)0x1F);
            return environment.CreateNumber((double)(lnum >> shiftCount));
        }

        public static IDynamic Op_UnsignedRightShift(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var lnum = (uint)(double)left.ConvertToUInt32().BaseValue;
            var rnum = (uint)(double)right.ConvertToUInt32().BaseValue;
            var shiftCount = (int)(rnum & (uint)0x1F);
            return environment.CreateNumber((double)unchecked(lnum >> shiftCount));
        }

        public static IDynamic Op_Addition(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var lprim = left.ConvertToPrimitive(null);
            var rprim = right.ConvertToPrimitive(null);
            if (lprim.TypeCode == LanguageTypeCode.String || rprim.TypeCode == LanguageTypeCode.String)
            {
                var lstr = (string)lprim.ConvertToString().BaseValue;
                var rstr = (string)rprim.ConvertToString().BaseValue;
                return environment.CreateString(lstr + rstr);
            }
            else
            {
                var lnum = (double)lprim.ConvertToNumber().BaseValue;
                var rnum = (double)rprim.ConvertToNumber().BaseValue;
                return environment.CreateNumber(lnum + rnum);
            }
        }

        public static IDynamic Op_Subtraction(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var lnum = (double)left.ConvertToNumber().BaseValue;
            var rnum = (double)right.ConvertToNumber().BaseValue;
            return environment.CreateNumber(lnum - rnum);
        }

        public static IDynamic Op_Multiplication(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var lnum = (double)left.ConvertToNumber().BaseValue;
            var rnum = (double)right.ConvertToNumber().BaseValue;
            return environment.CreateNumber(lnum * rnum);
        }

        public static IDynamic Op_Division(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var lnum = (double)left.ConvertToNumber().BaseValue;
            var rnum = (double)right.ConvertToNumber().BaseValue;
            return environment.CreateNumber(lnum / rnum);
        }

        public static IDynamic Op_Modulus(IEnvironment environment, IDynamic left, IDynamic right)
        {
            var lnum = (double)left.ConvertToNumber().BaseValue;
            var rnum = (double)right.ConvertToNumber().BaseValue;
            return environment.CreateNumber(lnum % rnum);
        }

        public static IDynamic Op_Plus(IEnvironment environment, IDynamic value)
        {
            return value.ConvertToNumber();
        }

        public static IDynamic Op_Minus(IEnvironment environment, IDynamic value)
        {
            var r = (double)value.ConvertToNumber().BaseValue;
            if (double.IsNaN(r))
            {
                return environment.CreateNumber(double.NaN);
            }
            return environment.CreateNumber(-r);
        }

        public static IDynamic Op_BitwiseNot(IEnvironment environment, IDynamic value)
        {
            return environment.CreateNumber((double)(~(int)(double)value.ConvertToInt32().BaseValue));
        }

        public static IDynamic Op_LogicalNot(IEnvironment environment, IDynamic value)
        {
            return environment.CreateBoolean(!value.ConvertToBoolean().BaseValue);
        }

        public static IDynamic Op_GetProperty(IEnvironment environment, IDynamic value, IDynamic name)
        {
            throw new NotImplementedException();
        }

        public static void Op_SetProperty(IEnvironment environment, IDynamic of, IDynamic name, IDynamic value)
        {
            throw new NotImplementedException();
        }

        public static IDynamic Op_Call(IEnvironment environment, IDynamic value, IArgs args)
        {
            var callable = value as ICallable;
            if (callable == null)
            {
                throw environment.CreateTypeError("");
            }
            return callable.Call(environment, environment.Undefined, args);
        }

        public static IObject Op_Construct(IEnvironment environment, IDynamic value, IArgs args)
        {
            throw new NotImplementedException();
        }

        public static void Op_Throw(IEnvironment environment, IDynamic value)
        {
            environment.ThrowRuntimeException(value);
        }

        public static INumber ConvertToInteger(IEnvironment environment, IDynamic value)
        {
            var number = value.ConvertToNumber();
            if (double.IsNaN((double)number.BaseValue))
            {
                return environment.CreateNumber(0);
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
                return environment.CreateNumber(sign * floor);
            }
        }

        public static INumber ConvertToInt32(IEnvironment environment, IDynamic value)
        {
            var number = (double)value.ConvertToNumber().BaseValue;
            if (number == 0.0 || double.IsNaN(number) || double.IsInfinity(number))
            {
                return environment.CreateNumber(0);
            }
            var posInt = Math.Sign(number) * Math.Floor(Math.Abs(number));
            var int32Bit = posInt % uint.MaxValue;
            if (int32Bit > int.MaxValue)
            {
                return environment.CreateNumber(int32Bit - uint.MaxValue);
            }
            return environment.CreateNumber(int32Bit);
        }

        public static INumber ConvertToUInt32(IEnvironment environment, IDynamic value)
        {
            var number = (double)value.ConvertToNumber().BaseValue;
            if (number == 0.0 || double.IsNaN(number) || double.IsInfinity(number))
            {
                return environment.CreateNumber(0);
            }
            var posInt = Math.Sign(number) * Math.Floor(Math.Abs(number));
            var int32Bit = posInt % uint.MaxValue;
            return environment.CreateNumber(int32Bit);
        }

        public static INumber ConvertToUInt16(IEnvironment environment, IDynamic value)
        {
            var number = (double)value.ConvertToNumber().BaseValue;
            if (number == 0.0 || double.IsNaN(number) || double.IsInfinity(number))
            {
                return environment.CreateNumber(0);
            }
            var posInt = Math.Sign(number) * Math.Floor(Math.Abs(number));
            var int16Bit = posInt % ushort.MaxValue;
            return environment.CreateNumber(int16Bit);
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

        public static IDynamic GetValue(IEnvironment environment, LBoolean value, string name, bool strict)
        {
            return GetValue(environment, (IDynamic)value, name, strict);
        }

        public static IDynamic GetValue(IEnvironment environment, LString value, string name, bool strict)
        {
            return GetValue(environment, (IDynamic)value, name, strict);
        }

        public static IDynamic GetValue(IEnvironment environment, LNumber value, string name, bool strict)
        {
            return GetValue(environment, (IDynamic)value, name, strict);
        }

        private static IDynamic GetValue(IEnvironment environment, IDynamic value, string name, bool strict)
        {
            var obj = value.ConvertToObject();
            var desc = obj.GetProperty(name);
            if (desc == null) return environment.Undefined;
            if (desc.IsDataDescriptor) return desc.Value;
            if (desc.Get is LUndefined) return environment.Undefined;
            return desc.Get.Op_Call(new SArgs(environment));
        }

        public static void SetValue(IEnvironment environment, LBoolean of, string name, IDynamic value, bool strict)
        {
            SetValue(environment, (IDynamic)of, name, value, strict);
        }

        public static void SetValue(IEnvironment environment, LString of, string name, IDynamic value, bool strict)
        {
            SetValue(environment, (IDynamic)of, name, value, strict);
        }

        public static void SetValue(IEnvironment environment, LNumber of, string name, IDynamic value, bool strict)
        {
            SetValue(environment, (IDynamic)of, name, value, strict);
        }

        private static void SetValue(IEnvironment environment, IDynamic of, string name, IDynamic value, bool strict)
        {
            var obj = value.ConvertToObject();
            if (!obj.CanPut(name))
            {
                if (strict)
                {
                    throw environment.CreateTypeError("");
                }
                return;
            }
            var ownDesc = obj.GetOwnProperty(name);
            if (ownDesc.IsDataDescriptor)
            {
                if (strict)
                {
                    throw environment.CreateTypeError("");
                }
                return;
            }
            var desc = obj.GetProperty(name);
            if (desc.IsAccessorDescriptor)
            {
                desc.Set.Op_Call(new SArgs(environment, value));
                return;
            }
            else
            {
                if (strict)
                {
                    throw environment.CreateTypeError("");
                }
                return;
            }
        }
    }
}