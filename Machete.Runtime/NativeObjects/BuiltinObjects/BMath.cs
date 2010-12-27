using System;
using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects
{
    public sealed class BMath : LObject
    {
        public BMath(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Math";
            Extensible = true;
            Prototype = Environment.ObjectPrototype;
            DefineOwnProperty("E", Environment.CreateDataDescriptor(Environment.CreateNumber(Math.E), false, false, false), false);
            DefineOwnProperty("LN10", Environment.CreateDataDescriptor(Environment.CreateNumber(Math.Log(10.0)), false, false, false), false);
            DefineOwnProperty("LN2", Environment.CreateDataDescriptor(Environment.CreateNumber(Math.Log(2.0)), false, false, false), false);
            DefineOwnProperty("LOG2E", Environment.CreateDataDescriptor(Environment.CreateNumber(Math.Log(Math.E, 2.0)), false, false, false), false);
            DefineOwnProperty("LOG10E", Environment.CreateDataDescriptor(Environment.CreateNumber(Math.Log10(Math.E)), false, false, false), false);
            DefineOwnProperty("PI", Environment.CreateDataDescriptor(Environment.CreateNumber(Math.PI), false, false, false), false);
            DefineOwnProperty("SQRT1_2", Environment.CreateDataDescriptor(Environment.CreateNumber(Math.Sqrt(1.0 / 2.0)), false, false, false), false);
            DefineOwnProperty("SQRT2", Environment.CreateDataDescriptor(Environment.CreateNumber(Math.Sqrt(2.0)), false, false, false), false);
            base.Initialize();
        }

        [NativeFunction("abs", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Abs(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("acos", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Acos(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("asin", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Asin(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("atan", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Atan(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("atan2", "y", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Atan2(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("ceil", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Ceil(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("cos", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Cos(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("exp", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Exp(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("floor", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Floor(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("log", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Log(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("max", "value1", "value2"), DataDescriptor(true, false, true)]
        internal static IDynamic Max(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("min", "value1", "value2"), DataDescriptor(true, false, true)]
        internal static IDynamic Min(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("pow", "x", "y"), DataDescriptor(true, false, true)]
        internal static IDynamic Pow(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("random"), DataDescriptor(true, false, true)]
        internal static IDynamic Random(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("round", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Round(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("sin", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Sin(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("sqrt", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Sqrt(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("tan", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Tan(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        [NativeFunction("fact", "n"), DataDescriptor(true, false, true)]
        internal static IDynamic Fact(IEnvironment environment, IArgs args)
        {
            double r = 1.0, n = Math.Truncate(args[0].ConvertToNumber().BaseValue);
            for (double i = n; i > 1; --i)
            {
                r *= i;
            }
            return environment.CreateNumber(r);
        }
    }
}