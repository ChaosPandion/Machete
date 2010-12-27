using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Threading;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Interfaces;

namespace Machete.Runtime.NativeObjects.BuiltinObjects
{
    public sealed class BGlobal : LObject
    {
        public BGlobal(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Global";
            Prototype = null;
            Extensible = true;
            DefineOwnProperty("NaN", Environment.CreateDataDescriptor(Environment.CreateNumber(double.NaN), false, false, false), false);
            DefineOwnProperty("Infinity", Environment.CreateDataDescriptor(Environment.CreateNumber(double.PositiveInfinity), false, false, false), false);
            DefineOwnProperty("undefined", Environment.CreateDataDescriptor(Environment.Undefined, false, false, false), false);
            DefineOwnProperty("Object", Environment.CreateDataDescriptor(Environment.ObjectConstructor, true, false, true), false);
            DefineOwnProperty("Function", Environment.CreateDataDescriptor(Environment.FunctionConstructor, true, false, true), false);
            DefineOwnProperty("Array", Environment.CreateDataDescriptor(Environment.ArrayConstructor, true, false, true), false);
            DefineOwnProperty("String", Environment.CreateDataDescriptor(Environment.StringConstructor, true, false, true), false);
            DefineOwnProperty("Boolean", Environment.CreateDataDescriptor(Environment.BooleanConstructor, true, false, true), false);
            DefineOwnProperty("Number", Environment.CreateDataDescriptor(Environment.NumberConstructor, true, false, true), false);
            DefineOwnProperty("Date", Environment.CreateDataDescriptor(Environment.DateConstructor, true, false, true), false);
            DefineOwnProperty("RegExp", Environment.CreateDataDescriptor(Environment.RegExpConstructor, true, false, true), false);
            DefineOwnProperty("Error", Environment.CreateDataDescriptor(Environment.ErrorConstructor, true, false, true), false);
            DefineOwnProperty("EvalError", Environment.CreateDataDescriptor(Environment.EvalErrorConstructor, true, false, true), false);
            DefineOwnProperty("RangeError", Environment.CreateDataDescriptor(Environment.RangeErrorConstructor, true, false, true), false);
            DefineOwnProperty("ReferenceError", Environment.CreateDataDescriptor(Environment.ReferenceErrorConstructor, true, false, true), false);
            DefineOwnProperty("SyntaxError", Environment.CreateDataDescriptor(Environment.SyntaxErrorConstructor, true, false, true), false);
            DefineOwnProperty("TypeError", Environment.CreateDataDescriptor(Environment.TypeErrorConstructor, true, false, true), false);
            DefineOwnProperty("URIError", Environment.CreateDataDescriptor(Environment.UriErrorConstructor, true, false, true), false);
            DefineOwnProperty("Math", Environment.CreateDataDescriptor(Environment.MathObject, true, false, true), false);
            DefineOwnProperty("JSON", Environment.CreateDataDescriptor(Environment.JsonObject, true, false, true), false);
            base.Initialize();
        }

        public override IDynamic DefaultValue(string hint)
        {
            return Environment.CreateString("Global");
        }

        [NativeFunction("eval", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Eval(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("parseInt", "string", "radix"), DataDescriptor(true, false, true)]
        internal static IDynamic ParseInt(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        //    //var inputString = (args[0].ConvertToString().Value ?? string.Empty).Trim().ToLowerInvariant();
        //    //var sign = 1;

        //    //if (inputString.Length > 0)
        //    //{
        //    //    var isNegative = inputString[0] == '-';
        //    //    if (isNegative)
        //    //    {
        //    //        sign = -1;
        //    //    }
        //    //    if (isNegative || inputString[0] == '+')
        //    //    {
        //    //        inputString = inputString.Substring(1);
        //    //    }
        //    //}

        //    //var radix = (int)args[1].ConvertToInt32().Value;
        //    //var stripPrefix = true;

        //    //if (radix == 0)
        //    //{
        //    //    radix = 10;
        //    //}
        //    //else
        //    //{
        //    //    if (radix < 2 || radix > 36)
        //    //    {
        //    //        return LNumber.NaN;
        //    //    }
        //    //    if (radix != 16)
        //    //    {
        //    //        stripPrefix = false;
        //    //    }
        //    //}

        //    //if (stripPrefix && inputString.Length > 2)
        //    //{
        //    //    if (inputString.StartsWith("0x"))
        //    //    {
        //    //        inputString = inputString.Substring(2);
        //    //    }
        //    //}

        //    //var map = RadixMap.GetHashSet(radix);
        //    //var z = new List<char>();
        //    //foreach (var c in inputString)
        //    //{
        //    //    if (!map.Contains(c))
        //    //    {
        //    //        break;
        //    //    }
        //    //    z.Add(c);
        //    //}

        //    //if (z.Count == 0)
        //    //{
        //    //    return LNumber.NaN;
        //    //}

        //    //var result = 0D;
        //    //var power = 0D;

        //    //z.Reverse();
        //    //foreach (var c in z)
        //    //{
        //    //    if (c < 'a')
        //    //    {
        //    //        result += ((c - '0') * (power > 0 ? Math.Pow(radix, power) : 1));
        //    //    }
        //    //    else
        //    //    {
        //    //        result += ((c - 'a' + 10) * Math.Pow(radix, power));
        //    //    }
        //    //    power++;
        //    //}

        //    //return (LNumber)(sign * result);
        }

        [NativeFunction("parseFloat", "string"), DataDescriptor(true, false, true)]
        internal static IDynamic ParseFloat(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        //    throw new NotImplementedException();
        //    //var inputString = (args[0].ToStringPrimitive().ToString() ?? string.Empty).Trim().ToLowerInvariant();
        //    //if (string.IsNullOrEmpty(inputString))
        //    //{
        //    //    return new NumberPrimitive(double.NaN);
        //    //}

        //    //var sign = 1D;
        //    //if (inputString[0] == '-')
        //    //{
        //    //    sign = -1D;
        //    //    inputString = inputString.Substring(1);
        //    //}
        //    //else if (inputString[0] == '+')
        //    //{
        //    //    inputString = inputString.Substring(1);
        //    //}

        //    //var traverser = new StringTraverser(inputString);
        //    //var sb = new StringBuilder();
        //    //var c = default(char?);

        //    //while ((c = traverser.Next()) != null)
        //    //{
        //    //    if ((c >= '0' && c <= '9') || c == 'e' || c == 'E' || c == '.' || c == '+' || c == '-')
        //    //    {
        //    //        sb.Append(c.Value);
        //    //        continue;
        //    //    }
        //    //    break;
        //    //}

        //    //if (sb.Length == 0)
        //    //{
        //    //    return new NumberPrimitive(double.NaN);
        //    //}

        //    //var result = 0D;
        //    //while (!double.TryParse(sb.ToString(), out result))
        //    //{
        //    //    sb.Remove(sb.Length - 1, 1);
        //    //    if (sb.Length == 0)
        //    //    {
        //    //        return new NumberPrimitive(double.NaN);
        //    //    }
        //    //}
        //    //return new NumberPrimitive(sign * result);
        }

        [NativeFunction("isNaN", "number"), DataDescriptor(true, false, true)]
        internal static IDynamic IsNan(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        //    throw new NotImplementedException();
        //    //return new BooleanPrimitive(double.IsNaN(args[0].ToNumberPrimitive()));
        }

        [NativeFunction("isFinite", "number"), DataDescriptor(true, false, true)]
        internal static IDynamic IsFinite(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        //    //var num = args[0].ToNumberPrimitive();
        //    //if (double.IsNaN(num))
        //    //{
        //    //    return new BooleanPrimitive(true);
        //    //}
        //    //if (double.IsPositiveInfinity(num))
        //    //{
        //    //    return new BooleanPrimitive(true);
        //    //}
        //    //if (double.IsNegativeInfinity(num))
        //    //{
        //    //    return new BooleanPrimitive(true);
        //    //}
        //    //else
        //    //{
        //    //    return new BooleanPrimitive(false);
        //    //}
        }

        [NativeFunction("decodeURI", "encodedURI"), DataDescriptor(true, false, true)]
        internal static IDynamic DecodeUri(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("decodeURIComponent", "encodedURIComponent"), DataDescriptor(true, false, true)]
        internal static IDynamic DecodeUriComponent(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("encodeURI", "uri"), DataDescriptor(true, false, true)]
        internal static IDynamic EncodeUri(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("encodeURIComponent", "uriComponent"), DataDescriptor(true, false, true)]
        internal static IDynamic EncodeUriComponent(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }
    }
}