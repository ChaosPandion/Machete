using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Threading;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects
{
    public sealed class BGlobal : LObject
    {
        internal BGlobal()
        {
            Class = "Global";
            Prototype = null;
            Extensible = true;
            DefineOwnProperty("NaN", new SPropertyDescriptor(LNumber.NaN), false);
            DefineOwnProperty("Infinity", new SPropertyDescriptor(LNumber.PositiveInfinity), false);
            DefineOwnProperty("undefined", new SPropertyDescriptor(LUndefined.Value), false);
            DefineOwnProperty("eval", new SPropertyDescriptor(new NFunction(new[] { "x" }, () => Eval)), false);
            DefineOwnProperty("parseInt", new SPropertyDescriptor(new NFunction(new[] { "string", "radix" }, () => ParseInt)), false);
            DefineOwnProperty("parseFloat", new SPropertyDescriptor(new NFunction(new[] { "string" }, () => ParseFloat)), false);
            DefineOwnProperty("isNaN", new SPropertyDescriptor(new NFunction(new[] { "number" }, () => IsNan)), false);
            DefineOwnProperty("isFinite", new SPropertyDescriptor(new NFunction(new[] { "number" }, () => IsFinite)), false);
            DefineOwnProperty("decodeURI", new SPropertyDescriptor(new NFunction(new[] { "encodedURI" }, () => DecodeUri)), false);
            DefineOwnProperty("decodeURIComponent", new SPropertyDescriptor(new NFunction(new[] { "encodedURIComponent" }, () => DecodeUriComponent)), false);
            DefineOwnProperty("encodeURI", new SPropertyDescriptor(new NFunction(new[] { "uri" }, () => EncodeUri)), false);
            DefineOwnProperty("encodeURIComponent", new SPropertyDescriptor(new NFunction(new[] { "uriComponent" }, () => EncodeUriComponent)), false);
        }

        private LType Eval(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
        }

        private LType ParseInt(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var inputString = (args[0].ConvertToString().Value ?? string.Empty).Trim().ToLowerInvariant();
            //var sign = 1;

            //if (inputString.Length > 0)
            //{
            //    var isNegative = inputString[0] == '-';
            //    if (isNegative)
            //    {
            //        sign = -1;
            //    }
            //    if (isNegative || inputString[0] == '+')
            //    {
            //        inputString = inputString.Substring(1);
            //    }
            //}

            //var radix = (int)args[1].ConvertToInt32().Value;
            //var stripPrefix = true;

            //if (radix == 0)
            //{
            //    radix = 10;
            //}
            //else
            //{
            //    if (radix < 2 || radix > 36)
            //    {
            //        return LNumber.NaN;
            //    }
            //    if (radix != 16)
            //    {
            //        stripPrefix = false;
            //    }
            //}

            //if (stripPrefix && inputString.Length > 2)
            //{
            //    if (inputString.StartsWith("0x"))
            //    {
            //        inputString = inputString.Substring(2);
            //    }
            //}

            //var map = RadixMap.GetHashSet(radix);
            //var z = new List<char>();
            //foreach (var c in inputString)
            //{
            //    if (!map.Contains(c))
            //    {
            //        break;
            //    }
            //    z.Add(c);
            //}

            //if (z.Count == 0)
            //{
            //    return LNumber.NaN;
            //}

            //var result = 0D;
            //var power = 0D;

            //z.Reverse();
            //foreach (var c in z)
            //{
            //    if (c < 'a')
            //    {
            //        result += ((c - '0') * (power > 0 ? Math.Pow(radix, power) : 1));
            //    }
            //    else
            //    {
            //        result += ((c - 'a' + 10) * Math.Pow(radix, power));
            //    }
            //    power++;
            //}

            //return (LNumber)(sign * result);
        }

        private LType ParseFloat(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var inputString = (args[0].ToStringPrimitive().ToString() ?? string.Empty).Trim().ToLowerInvariant();
            //if (string.IsNullOrEmpty(inputString))
            //{
            //    return new NumberPrimitive(double.NaN);
            //}

            //var sign = 1D;
            //if (inputString[0] == '-')
            //{
            //    sign = -1D;
            //    inputString = inputString.Substring(1);
            //}
            //else if (inputString[0] == '+')
            //{
            //    inputString = inputString.Substring(1);
            //}

            //var traverser = new StringTraverser(inputString);
            //var sb = new StringBuilder();
            //var c = default(char?);

            //while ((c = traverser.Next()) != null)
            //{
            //    if ((c >= '0' && c <= '9') || c == 'e' || c == 'E' || c == '.' || c == '+' || c == '-')
            //    {
            //        sb.Append(c.Value);
            //        continue;
            //    }
            //    break;
            //}

            //if (sb.Length == 0)
            //{
            //    return new NumberPrimitive(double.NaN);
            //}

            //var result = 0D;
            //while (!double.TryParse(sb.ToString(), out result))
            //{
            //    sb.Remove(sb.Length - 1, 1);
            //    if (sb.Length == 0)
            //    {
            //        return new NumberPrimitive(double.NaN);
            //    }
            //}
            //return new NumberPrimitive(sign * result);
        }

        private LType IsNan(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //return new BooleanPrimitive(double.IsNaN(args[0].ToNumberPrimitive()));
        }

        private LType IsFinite(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var num = args[0].ToNumberPrimitive();
            //if (double.IsNaN(num))
            //{
            //    return new BooleanPrimitive(true);
            //}
            //if (double.IsPositiveInfinity(num))
            //{
            //    return new BooleanPrimitive(true);
            //}
            //if (double.IsNegativeInfinity(num))
            //{
            //    return new BooleanPrimitive(true);
            //}
            //else
            //{
            //    return new BooleanPrimitive(false);
            //}
        }

        private LType DecodeUri(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
        }

        private LType DecodeUriComponent(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
        }

        private LType EncodeUri(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
        }

        private LType EncodeUriComponent(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
        }
    }
}