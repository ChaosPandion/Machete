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
            Class = "Environment";
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
            DefineOwnProperty("environment", Environment.CreateDataDescriptor(this, false, false, false), false);
            base.Initialize();
        }

        public override IDynamic DefaultValue(string hint)
        {
            return Environment.CreateString("[object, Environment]");
        }

        [NativeFunction("eval", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Eval(IEnvironment environment, IArgs args)
        {
            var x = args[0];
            if (x.TypeCode != LanguageTypeCode.String)
            {
                return x;
            }
            var compiler = new Machete.Compiler.Compiler(environment);
            var code = compiler.CompileEvalCode(x.ConvertToString().BaseValue);
            var context = environment.Context;
            using (var c = environment.EnterContext())
            {
                c.ThisBinding = context.ThisBinding;
                c.LexicalEnviroment = context.LexicalEnviroment;
                c.VariableEnviroment = context.VariableEnviroment;
                return code(environment, environment.EmptyArgs);
            }
        }

        [NativeFunction("parseInt", "string", "radix"), DataDescriptor(true, false, true)]
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

        [NativeFunction("parseFloat", "string"), DataDescriptor(true, false, true)]
        internal static IDynamic ParseFloat(IEnvironment environment, IArgs args)
        {
            var text = args[0].ConvertToString().BaseValue;
            return environment.CreateNumber(Compiler.FloatParser.Parse(text));
        }

        [NativeFunction("isNaN", "number"), DataDescriptor(true, false, true)]
        internal static IDynamic IsNan(IEnvironment environment, IArgs args)
        {
            var n = args[0].ConvertToNumber().BaseValue;
            return environment.CreateBoolean(double.IsNaN(n));
        }

        [NativeFunction("isFinite", "number"), DataDescriptor(true, false, true)]
        internal static IDynamic IsFinite(IEnvironment environment, IArgs args)
        {
            var n = args[0].ConvertToNumber().BaseValue;
            return environment.CreateBoolean(!double.IsNaN(n) && !double.IsInfinity(n));
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