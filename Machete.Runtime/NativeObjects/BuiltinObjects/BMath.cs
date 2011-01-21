using System;
using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Collections;

namespace Machete.Runtime.NativeObjects.BuiltinObjects
{
    public sealed class BMath : LObject
    {
        private static readonly long _negativeZeroBits = BitConverter.DoubleToInt64Bits(-0.0);
        private static readonly Random _random = new Random();


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

        [BuiltinFunction("abs", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Abs(IEnvironment environment, IArgs args)
        {
            double x = args[0].ConvertToNumber().BaseValue;
            if (double.IsNaN(x))
                return environment.CreateNumber(double.NaN);
            if (IsNegativeZero(x))
                return environment.CreateNumber(0.0);
            if (double.IsNegativeInfinity(x))
                return environment.CreateNumber(double.PositiveInfinity);
            return environment.CreateNumber(Math.Abs(x));
        }

        [BuiltinFunction("acos", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Acos(IEnvironment environment, IArgs args)
        {
            double x = args[0].ConvertToNumber().BaseValue;
            if (double.IsNaN(x))
                return environment.CreateNumber(double.NaN);
            if (x > 1.0)
                return environment.CreateNumber(double.NaN);
            if (x < -1.0)
                return environment.CreateNumber(double.NaN);
            if (x == 1.0)
                return environment.CreateNumber(0.0);
            return environment.CreateNumber(Math.Acos(x));
        }

        [BuiltinFunction("asin", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Asin(IEnvironment environment, IArgs args)
        {
            double x = args[0].ConvertToNumber().BaseValue;
            if (double.IsNaN(x))
                return environment.CreateNumber(double.NaN);
            if (x > 1.0 || x < -1.0)
                return environment.CreateNumber(double.NaN);
            if (x == 0.0)
                return environment.CreateNumber(x);
            return environment.CreateNumber(Math.Asin(x));
        }

        [BuiltinFunction("atan", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Atan(IEnvironment environment, IArgs args)
        {
            double x = args[0].ConvertToNumber().BaseValue;
            if (double.IsNaN(x))
                return environment.CreateNumber(double.NaN);
            if (x == 0.0)
                return environment.CreateNumber(x);
            if (double.IsPositiveInfinity(x))
                return environment.CreateNumber(Math.PI / 2.0);
            if (double.IsNegativeInfinity(x))
                return environment.CreateNumber(-Math.PI / 2.0);
            return environment.CreateNumber(Math.Atan(x));
        }

        [BuiltinFunction("atan2", "y", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Atan2(IEnvironment environment, IArgs args)
        {
            double y = args[0].ConvertToNumber().BaseValue;
            double x = args[1].ConvertToNumber().BaseValue;
            if (double.IsNaN(y) || double.IsNaN(x))
                return environment.CreateNumber(double.NaN);
            if (y > 0.0 && x == 0.0)
                return environment.CreateNumber(Math.PI / 2.0);
            if (y == 0.0)
            {
                bool yIsNegZero = IsNegativeZero(y);
                bool xIsNegZero = IsNegativeZero(x);
                if (!yIsNegZero)
                {
                    if (x > 0.0 || !xIsNegZero)
                        return environment.CreateNumber(0.0);
                    if (x < 0.0 || xIsNegZero)
                        return environment.CreateNumber(Math.PI);
                }
                else
                {
                    if (x > 0.0 || !xIsNegZero)
                        return environment.CreateNumber(-0.0);
                    if (x < 0.0 || xIsNegZero)
                        return environment.CreateNumber(-Math.PI);
                }
            }
            else if (y < 0.0)
            {
                if (x == 0)
                    return environment.CreateNumber(-Math.PI / 2.0);
                if (!double.IsInfinity(y))
                {
                    if (double.IsPositiveInfinity(x))
                        return environment.CreateNumber(-0.0);
                    if (double.IsNegativeInfinity(x))
                        return environment.CreateNumber(-Math.PI);
                }
            }
            else if (y > 0.0)
            {
                if (!double.IsInfinity(y))
                {
                    if (double.IsPositiveInfinity(x))
                        return environment.CreateNumber(0.0);
                    if (double.IsNegativeInfinity(x))
                        return environment.CreateNumber(Math.PI);
                }
            }
            else if (double.IsPositiveInfinity(y))
            {
                if (double.IsPositiveInfinity(x))
                    return environment.CreateNumber(Math.PI / 4.0);
                if (double.IsNegativeInfinity(x))
                    return environment.CreateNumber(3.0 * Math.PI / 4.0);
                return environment.CreateNumber(Math.PI / 2.0);
            }
            else if (double.IsNegativeInfinity(y))
            {
                if (double.IsPositiveInfinity(x))
                    return environment.CreateNumber(-Math.PI / 4.0);
                if (double.IsNegativeInfinity(x))
                    return environment.CreateNumber(-3.0 * Math.PI / 4.0);
                return environment.CreateNumber(-Math.PI / 2.0);
            }
            return environment.CreateNumber(Math.Atan2(y, x));
        }

        [BuiltinFunction("ceil", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Ceil(IEnvironment environment, IArgs args)
        {
            double x = args[0].ConvertToNumber().BaseValue;
            if (double.IsNaN(x))
                return environment.CreateNumber(double.NaN);
            if (x == 0.0)
                return environment.CreateNumber(x);
            if (double.IsInfinity(x))
                return environment.CreateNumber(x);
            if (x < 0.0 && x > -1.0)
                return environment.CreateNumber(-0.0);
            return environment.CreateNumber(Math.Ceiling(x));
        }

        [BuiltinFunction("cos", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Cos(IEnvironment environment, IArgs args)
        {
            double x = args[0].ConvertToNumber().BaseValue;
            if (double.IsNaN(x))
                return environment.CreateNumber(double.NaN);
            if (x == 0.0)
                return environment.CreateNumber(1.0);
            if (double.IsInfinity(x))
                return environment.CreateNumber(double.NaN);
            return environment.CreateNumber(Math.Cos(x));
        }

        [BuiltinFunction("exp", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Exp(IEnvironment environment, IArgs args)
        {
            double x = args[0].ConvertToNumber().BaseValue;
            if (double.IsNaN(x))
                return environment.CreateNumber(double.NaN);
            if (x == 0.0)
                return environment.CreateNumber(1.0);
            if (double.IsPositiveInfinity(x))
                return environment.CreateNumber(double.PositiveInfinity);
            if (double.IsNegativeInfinity(x))
                return environment.CreateNumber(0.0);
            return environment.CreateNumber(Math.Exp(x));
        }

        [BuiltinFunction("floor", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Floor(IEnvironment environment, IArgs args)
        {
            double x = args[0].ConvertToNumber().BaseValue;
            if (double.IsNaN(x))
                return environment.CreateNumber(double.NaN);
            if (x == 0.0)
                return environment.CreateNumber(x);
            if (double.IsInfinity(x))
                return environment.CreateNumber(x);
            if (x > 0.0 && x < 1.0)
                return environment.CreateNumber(0.0);
            return environment.CreateNumber(Math.Floor(x));
        }

        [BuiltinFunction("log", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Log(IEnvironment environment, IArgs args)
        {
            double x = args[0].ConvertToNumber().BaseValue;
            if (double.IsNaN(x) || x < 0.0)
                return environment.CreateNumber(double.NaN);
            if (x == 0.0)
                return environment.CreateNumber(double.NegativeInfinity);
            if (x == 1.0)
                return environment.CreateNumber(0.0);
            if (double.IsPositiveInfinity(x))
                return environment.CreateNumber(double.PositiveInfinity);
            return environment.CreateNumber(Math.Log(x));
        }

        [BuiltinFunction("max", "value1", "value2"), DataDescriptor(true, false, true)]
        internal static IDynamic Max(IEnvironment environment, IArgs args)
        {
            if (args.IsEmpty)
                return environment.CreateNumber(double.NegativeInfinity);
            if (args.Count == 1)
                return args[0].ConvertToNumber();
            
            var numbers = new INumber[args.Count];
            for (int i = 0; i < numbers.Length; i++)
            {
                var number = args[i].ConvertToNumber();
                if (double.IsNaN(number.BaseValue))
                    return environment.CreateNumber(double.NaN);
                numbers[i] = number;
            }

            INumber r = null;
            foreach (var n in numbers)
            {
                if (r == null || r.Op_Lessthan(n).ConvertToBoolean().BaseValue)
                    r = n;
            }
            return r;
        }

        [BuiltinFunction("min", "value1", "value2"), DataDescriptor(true, false, true)]
        internal static IDynamic Min(IEnvironment environment, IArgs args)
        {
            if (args.IsEmpty)
                return environment.CreateNumber(double.PositiveInfinity);
            if (args.Count == 1)
                return args[0].ConvertToNumber();

            var numbers = new INumber[args.Count];
            for (int i = 0; i < numbers.Length; i++)
            {
                var number = args[i].ConvertToNumber();
                if (double.IsNaN(number.BaseValue))
                    return environment.CreateNumber(double.NaN);
                numbers[i] = number;
            }

            INumber r = null;
            foreach (var n in numbers)
            {
                if (r == null || r.Op_Greaterthan(n).ConvertToBoolean().BaseValue)
                    r = n;
            }
            return r;
        }

        [BuiltinFunction("pow", "x", "y"), DataDescriptor(true, false, true)]
        internal static IDynamic Pow(IEnvironment environment, IArgs args)
        {
            double x = args[0].ConvertToNumber().BaseValue;
            double y = args[1].ConvertToNumber().BaseValue;

            if (double.IsNaN(y))
                return environment.CreateNumber(double.NaN);
            if (y == 0.0)
                return environment.CreateNumber(1.0);
            if (double.IsNaN(x) && y != 0.0)
                return environment.CreateNumber(double.NaN);

            double absX = Math.Abs(x);

            if (absX > 1.0)
            {
                if (double.IsPositiveInfinity(y))
                    return environment.CreateNumber(double.PositiveInfinity);
                if (double.IsNegativeInfinity(y))
                    return environment.CreateNumber(0.0);
            }
            else if (absX == 1.0)
            {
                if (double.IsInfinity(y))
                    return environment.CreateNumber(double.NaN);
            }
            else if (absX < 1.0)
            {
                if (double.IsPositiveInfinity(y))
                    return environment.CreateNumber(0.0);
                if (double.IsNegativeInfinity(y))
                    return environment.CreateNumber(double.PositiveInfinity);
            }

            if (double.IsPositiveInfinity(x))
            {
                if (y > 0.0)
                    return environment.CreateNumber(double.PositiveInfinity);
                if (y < 0.0)
                    return environment.CreateNumber(0.0);
            }
            else if (double.IsNegativeInfinity(x))
            {
                if (y > 0.0)
                {
                    if (y % 2.0 != 0.0)
                        return environment.CreateNumber(double.NegativeInfinity);
                    return environment.CreateNumber(double.PositiveInfinity);
                }
                else if (y < 0.0)
                {
                    if (y % 2.0 != 0.0)
                        return environment.CreateNumber(-0.0);
                    return environment.CreateNumber(0.0);
                }
            }

            if (IsNegativeZero(x))
            {
                if (y < 0.0)
                {
                    if (y % 2.0 != 0.0)
                        return environment.CreateNumber(double.NegativeInfinity);
                    return environment.CreateNumber(double.PositiveInfinity);
                }
                else if (y > 0.0)
                {
                    if (y % 2.0 != 0.0)
                        return environment.CreateNumber(-0.0);
                    return environment.CreateNumber(0.0);
                }
            }
            else
            {
                if (y > 0.0)
                    return environment.CreateNumber(0.0);
                if (y > 0.0)
                    return environment.CreateNumber(double.PositiveInfinity);
            }

            if (x < 0 && !double.IsInfinity(x) && !double.IsNaN(x) && Math.Truncate(y) != y)
                return environment.CreateNumber(double.NaN);

            return environment.CreateNumber(Math.Pow(x, y));
        }

        [BuiltinFunction("random"), DataDescriptor(true, false, true)]
        internal static IDynamic Random(IEnvironment environment, IArgs args)
        {
            return environment.CreateNumber(_random.NextDouble());
        }

        [BuiltinFunction("round", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Round(IEnvironment environment, IArgs args)
        {
            double x = args[0].ConvertToNumber().BaseValue;

            if (x == 0.0 || double.IsNaN(x) || double.IsInfinity(x))
                return environment.CreateNumber(x);
            if (x > 0 && x < 0.5)
                return environment.CreateNumber(0.0);
            if (x < 0 && x > -0.5)
                return environment.CreateNumber(-0.0);

            double integralPortion = Math.Truncate(x);
            double fractionalPortion = Math.Abs(x - integralPortion);

            if (x < 0)
            {
                if (fractionalPortion <= 0.5)
                    return environment.CreateNumber(integralPortion);
                return environment.CreateNumber(integralPortion - 1.0);
            }
            else
            {
                if (fractionalPortion >= 0.5)
                    return environment.CreateNumber(integralPortion + 1.0);
                return environment.CreateNumber(integralPortion);
            }
        }

        [BuiltinFunction("sin", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Sin(IEnvironment environment, IArgs args)
        {
            double x = args[0].ConvertToNumber().BaseValue;
            if (double.IsNaN(x) || double.IsInfinity(x))
                return environment.CreateNumber(double.NaN);
            if (x == 0.0)
                return environment.CreateNumber(x);
            return environment.CreateNumber(Math.Sin(x));
        }

        [BuiltinFunction("sqrt", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Sqrt(IEnvironment environment, IArgs args)
        {
            double x = args[0].ConvertToNumber().BaseValue;
            if (double.IsNaN(x) || x < 0.0)
                return environment.CreateNumber(double.NaN);
            if (x == 0.0 || double.IsPositiveInfinity(x))
                return environment.CreateNumber(x);
            return environment.CreateNumber(Math.Sqrt(x));
        }

        [BuiltinFunction("tan", "x"), DataDescriptor(true, false, true)]
        internal static IDynamic Tan(IEnvironment environment, IArgs args)
        {
            double x = args[0].ConvertToNumber().BaseValue;
            if (double.IsNaN(x) || double.IsInfinity(x))
                return environment.CreateNumber(double.NaN);
            if (x == 0.0)
                return environment.CreateNumber(x);
            return environment.CreateNumber(Math.Tan(x));
        }

        [BuiltinFunction("fact", "n"), DataDescriptor(true, false, true)]
        internal static IDynamic Fact(IEnvironment environment, IArgs args)
        {
            double r = 1.0, n = Math.Truncate(args[0].ConvertToNumber().BaseValue);
            for (double i = n; i > 1; --i)
            {
                r *= i;
            }
            return environment.CreateNumber(r);
        }

        public static bool IsNegativeZero(double x)
        {
            return BitConverter.DoubleToInt64Bits(x) == _negativeZeroBits;
        }
    }
}