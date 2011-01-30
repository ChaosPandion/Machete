using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Threading;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Core;
using Machete.Runtime.HostObjects;
using Machete.Compiler;
using Machete.Runtime.HostObjects.Iterables;

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
            Class = "Core";
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

            DefineOwnProperty("core", Environment.CreateDataDescriptor(this, false, false, false), false);
            DefineOwnProperty("output", Environment.CreateDataDescriptor(new HOutput(Environment), false, false, false), false);

            base.Initialize();
        }

        public override IDynamic DefaultValue(string hint)
        {
            return Environment.CreateString("[object, Core]");
        }

        [BuiltinFunction("eval", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Eval(IEnvironment environment, IArgs args)
        {
            var x = args[0];
            if (x.TypeCode != LanguageTypeCode.String)
            {
                return x;
            }
            var compiler = new CompilerService(environment);
            var context = environment.Context;
            var e = compiler.CompileEvalCode(x.ConvertToString().BaseValue, context.Strict);
            using (var c = environment.EnterContext())
            {
                c.ThisBinding = context.ThisBinding;
                c.LexicalEnviroment = context.LexicalEnviroment;
                c.VariableEnviroment = context.VariableEnviroment;
                if (e.Strict)
                {
                    var s = context.LexicalEnviroment.NewDeclarativeEnvironment();
                    c.LexicalEnviroment = s;
                    c.VariableEnviroment = s;
                }
                return e.Code(environment, environment.EmptyArgs);
            }
        }

        [BuiltinFunction("parseInt", "string", "radix"), DataDescriptor(true, false, true)]
        internal static IDynamic ParseInt(IEnvironment environment, IArgs args)
        {
            var text = args[0].ConvertToString().BaseValue;
            var radix = 0.0;

            if (args.Count > 1)
            {
                radix = args[1].ConvertToNumber().BaseValue;
                if (double.IsNaN(radix) || double.IsInfinity(radix) || radix < 2.0 || radix > 36.0)
                {
                    radix = 0.0;
                }
            }

            return environment.CreateNumber(Compiler.IntParser.Parse(text, (int)radix));
        }

        [BuiltinFunction("parseFloat", "string"), DataDescriptor(true, false, true)]
        internal static IDynamic ParseFloat(IEnvironment environment, IArgs args)
        {
            var text = args[0].ConvertToString().BaseValue;
            return environment.CreateNumber(Compiler.FloatParser.Parse(text));
        }

        [BuiltinFunction("isNaN", "number"), DataDescriptor(true, false, true)]
        internal static IDynamic IsNan(IEnvironment environment, IArgs args)
        {
            var n = args[0].ConvertToNumber().BaseValue;
            return environment.CreateBoolean(double.IsNaN(n));
        }

        [BuiltinFunction("isFinite", "number"), DataDescriptor(true, false, true)]
        internal static IDynamic IsFinite(IEnvironment environment, IArgs args)
        {
            var n = args[0].ConvertToNumber().BaseValue;
            return environment.CreateBoolean(!double.IsNaN(n) && !double.IsInfinity(n));
        }

        [BuiltinFunction("decodeURI", "encodedURI"), DataDescriptor(true, false, true)]
        internal static IDynamic DecodeUri(IEnvironment environment, IArgs args)
        {
            const string reservedURISet = ";/?:@&=+$,#";
            var uriString = args[0].ConvertToString().BaseValue;
            return environment.CreateString(Decode(environment, uriString, reservedURISet));
        }

        [BuiltinFunction("decodeURIComponent", "encodedURIComponent"), DataDescriptor(true, false, true)]
        internal static IDynamic DecodeUriComponent(IEnvironment environment, IArgs args)
        {
            var componentString = args[0].ConvertToString().BaseValue;
            return environment.CreateString(Decode(environment, componentString, ""));
        }

        [BuiltinFunction("encodeURI", "uri"), DataDescriptor(true, false, true)]
        internal static IDynamic EncodeUri(IEnvironment environment, IArgs args)
        {
            const string unescapedURISet = ";/?:@&=+$,abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_.!~*'()#";
            var uriString = args[0].ConvertToString().BaseValue;
            return environment.CreateString(Encode(environment, uriString, unescapedURISet));
        }

        [BuiltinFunction("encodeURIComponent", "uriComponent"), DataDescriptor(true, false, true)]
        internal static IDynamic EncodeUriComponent(IEnvironment environment, IArgs args)
        {
            const string unescapedURIComponentSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_.!~*'()";
            var componentString = args[0].ConvertToString().BaseValue;
            return environment.CreateString(Encode(environment, componentString, unescapedURIComponentSet));
        }

        [BuiltinFunction("assert", "condition", "message"), DataDescriptor(false, false, false)]
        internal static IDynamic Assert(IEnvironment environment, IArgs args)
        {
            var condition = args[0].ConvertToBoolean().BaseValue;
            if (!condition)
            {
                var message = args[1].ConvertToString().BaseValue;
                throw environment.CreateError(message);
            }
            return environment.Undefined;
        }

        [BuiltinFunction("iterate", "iterable", "fn"), DataDescriptor(false, false, false)]
        internal static IDynamic Iterate(IEnvironment environment, IArgs args)
        {
            var iterable = args[0].ConvertToObject();
            var createIterator = iterable.Get("createIterator") as ICallable;
            if (createIterator == null)
                throw environment.CreateTypeError("");
            var iterator = createIterator.Call(environment, iterable, environment.EmptyArgs).ConvertToObject();
            if (!iterator.HasProperty("current"))
                throw environment.CreateTypeError("");
            var next = iterator.Get("next") as ICallable;
            if (next == null)
                throw environment.CreateTypeError("");
            var fn = args[1] as ICallable;
            if (fn == null)
                throw environment.CreateTypeError("");
            while (next.Call(environment, iterator, environment.EmptyArgs).ConvertToBoolean().BaseValue)
            {
                var callArgs = environment.CreateArgs(new[] { iterator.Get("current") });
                fn.Call(environment, environment.Undefined, callArgs);
            }
            return environment.Undefined;
        }

        [BuiltinFunction("filter", "iterable", "predicate"), DataDescriptor(false, false, false)]
        internal static IDynamic Filter(IEnvironment environment, IArgs args)
        {
            var iterable = args[0].ConvertToObject();
            var predicate = args[1].ConvertToObject() as ICallable;
            if (predicate == null)
                throw environment.CreateTypeError("");
            return new HFilterIterable(environment, iterable, predicate);
        }

        internal static string Encode(IEnvironment environment, string value, string unescapedSet)
        {
            byte[] octets;
            byte jOctet;
            char c;
            int strLen, k, v, kChar, j, l;
            string r;

            r = "";
            k = 0;
            strLen = value.Length;

            while (k < strLen)
            {
                c = value[k];
                if (unescapedSet.IndexOf(c) > -1)
                {
                    r += c;
                }
                else
                {
                    if (c > '\udc00' && c < '\udfff')
                    {
                        throw environment.CreateUriError("");
                    }
                    if (c < '\ud800' && c < '\udbff')
                    {
                        v = (int)c;
                    }
                    else
                    {
                        k++;
                        if (k == strLen)
                        {
                            throw environment.CreateUriError("");
                        }
                        kChar = (int)value[k];
                        if (kChar < 0xDC00 || kChar > 0xDFFF)
                        {
                            throw environment.CreateUriError("");
                        }
                        v = ((int)c - 0xD800) * 0x400 + (kChar - 0xDC00) + 0x10000;
                    }
                    octets = Encoding.UTF8.GetBytes(((char)v).ToString());
                    l = octets.Length;
                    j = 0;
                    while (j < l)
                    {
                        jOctet = octets[j];
                        r = r + "%" + jOctet.ToString("x2").ToUpper();
                        j++;
                    }
                }
                k++;
            }

            return r;
        }

        internal static string Decode(IEnvironment environment, string value, string reservedSet)
        {
            int b, d1v, d2v, n;
            char c, d1, d2;
            int strLen, k, start, v;
            string r, s;
            byte[] octets;

            strLen = value.Length;
            k = 0;
            r = "";

            while (k < strLen)
            {
                c = value[k];
                if (c != '%')
                {
                    s = c.ToString();
                }
                else
                {
                    start = k;
                    if (k + 2 > strLen)
                    {
                        throw environment.CreateUriError("");
                    }
                    d1 = value[++k];
                    d2 = value[++k];
                    d1v = HexValueOf(environment, d1);
                    d2v = HexValueOf(environment, d2);
                    b = (d1v * 16 + d2v);
                    if ((b & 0x80) == 0x00)
                    {
                        c = (char)b;
                        if (reservedSet.IndexOf(c) < 0)
                        {
                            s = c.ToString();
                        }
                        else
                        {
                            s = value.Substring(start, k - start);
                        }
                    }
                    else
                    {
                        for (n = 1; n < 5; n++)
                        {
                            if (((b << n) & 0x80) == 0x00)
                            {
                                break;
                            }
                        }
                        if (n == 1 || n > 4)
                        {
                            throw environment.CreateUriError("");
                        }
                        octets = new byte[n];
                        octets[0] = (byte)b;
                        if (k + (3 * (n - 1)) >= strLen)
                        {
                            throw environment.CreateUriError("");
                        }
                        for (int j = 1; j < n; j++)
                        {
                            k++;
                            if (value[k] != '%')
                            {
                                throw environment.CreateUriError("");
                            }
                            d1 = value[++k];
                            d2 = value[++k];
                            d1v = HexValueOf(environment, d1);
                            d2v = HexValueOf(environment, d2);
                            b = (d1v * 16 + d2v);
                            if ((b & 0x80) != 0x80 || (b & 0x40) != 0x00)
                            {
                                throw environment.CreateUriError("");
                            }
                            octets[j] = (byte)b;
                        }

                        try
                        {
                            v = (int)Encoding.UTF8.GetString(octets)[0];
                        }
                        catch (ArgumentException) // The byte array contains invalid Unicode code points.
                        {
                            throw environment.CreateUriError("");
                        }

                        if (v < 0x10000)
                        {
                            c = (char)v;
                            if (reservedSet.IndexOf(c) < 0)
                            {
                                s = c.ToString();
                            }
                            else
                            {
                                s = value.Substring(start, k - start);
                            }
                        }
                        else
                        {
                            if (v > 0x10FFFF)
                            {
                                throw environment.CreateUriError("");
                            }

                            int lv = ((v - 0x10000) & 0x3FF) + 0xDC00;
                            int hv = (((v - 0x10000) >> 10) & 0x3FF) + 0xD800;
                            s = ((char)hv).ToString() + ((char)lv).ToString();
                        }
                    }
                }
                r += s;
                k++;
            }

            return r;
        }

        private static int HexValueOf(IEnvironment environment, char c)
        {
            if (c >= '0' && c <= '9')
            {
                return (int)c - 48;
            }
            else if (c >= 'A' && c <= 'F')
            {
                return (int)c - 55;
            }
            else if (c >= 'a' && c <= 'z')
            {
                return (int)c - 87;
            }
            else
            {
                throw environment.CreateUriError("");
            }
        }
    }
}