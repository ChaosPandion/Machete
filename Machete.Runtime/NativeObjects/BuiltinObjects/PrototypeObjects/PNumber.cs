using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Text;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PNumber : LObject
    {
        public PNumber(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Number";
            Extensible = true;
            Prototype = Environment.ObjectPrototype;
            DefineOwnProperty("constructor", Environment.CreateDataDescriptor(Environment.NumberConstructor, true, false, true), false);
            base.Initialize();
        }


        [BuiltinFunction("valueOf"), DataDescriptor(true, false, true)]
        internal static IDynamic ValueOf(IEnvironment environment, IArgs args)
        {
            var thisBinding = environment.Context.ThisBinding;
            switch (thisBinding.TypeCode)
            {
                case LanguageTypeCode.Number:
                    return thisBinding.ConvertToNumber();
                case LanguageTypeCode.Object:
                    var number = thisBinding as NNumber;
                    if (number != null)
                    {
                        return number.PrimitiveValue.ConvertToNumber();
                    }
                    break;
            }
            throw environment.CreateTypeError("");
        }

        [BuiltinFunction("toString", "radix"), DataDescriptor(true, false, true)]
        internal static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            const string rangeErrorFormat = "The value supplied for radix ({0}) must be between 2 and 36.";

            var number = ValueOf(environment, args).ConvertToNumber();
            var value = number.BaseValue;
            var radix = args[0].ConvertToInteger().BaseValue;
            var sign = "";

            if (args[0].TypeCode == LanguageTypeCode.Undefined)
            {
                radix = 10.0;
            }
            if (radix < 2.0 || radix > 36.0)
            {
                throw environment.CreateRangeError(string.Format(rangeErrorFormat, radix));
            }
            if (radix == 10.0)
            {
                return number.ConvertToString();
            }

            if (double.IsNaN(value))
            {
                return environment.CreateString("NaN");
            }
            else if (value == 0.0)
            {
                return environment.CreateString("0");
            }
            else if (value < 0.0)
            {
                sign = "-";
                value = -value;
            }
            if (double.IsInfinity(value))
            {
                return environment.CreateString(sign + "Infinity");
            }

            var result = "";
            var integralPortion = Math.Truncate(value);
            var integralCurrent = integralPortion;
            var fractionalPortion = (double)((decimal)value - (decimal)integralPortion);
            var fractionalCurrent = fractionalPortion;
            var sb = new StringBuilder();

            do
            {
                var v = integralCurrent % radix;
                if (v < 10.0)
                {
                    sb.Append((char)(v + 48.0));
                }
                else
                {
                    sb.Append((char)(v - 10.0 + 97.0));
                }
                integralCurrent = integralCurrent / radix;
            } while (integralCurrent > 0.000001);

            var ia = sb.ToString().ToCharArray();
            Array.Reverse(ia);
            result = new string(ia).TrimStart('0');

            if (fractionalPortion == 0.0)
            {
                return environment.CreateString(sign + result);
            }

            var index = 1;
            var runningValue = 0.0;
            sb.Clear();

            do
            {
                var v = fractionalCurrent * radix;
                var iv = Math.Truncate(v);
                runningValue += iv / Math.Pow(radix, index);
                if (iv < 10.0)
                {
                    sb.Append((char)(iv + 48.0));
                }
                else
                {
                    sb.Append((char)(iv - 10.0 + 97.0));
                }
                fractionalCurrent = v - iv;
            } while (++index < 50 && fractionalCurrent - runningValue > 1.0e-200);

            return environment.CreateString(sign + result + "." + sb.ToString());
        }

        [BuiltinFunction("toLocaleString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToLocaleString(IEnvironment environment, IArgs args)
        {
            return environment.CreateString(ValueOf(environment, args).ConvertToNumber().BaseValue.ToString()); 
        }

        [BuiltinFunction("toFixed", "fractionDigits"), DataDescriptor(true, false, true)]
        internal static IDynamic ToFixed(IEnvironment environment, IArgs args)
        {
            const string rangeErrorFormat = "The value supplied for fractionDigits ({0}) must be between 0 and 20.";
            const double limit = 1000000000000000000000.0;

            INumber number; 
            double f, x, n;
            string s, m, a, b, z;
            int k, v, l;

            f = args[0].ConvertToInteger().BaseValue;
            if (f < 0.0 || f > 20.0)
            {
                throw environment.CreateRangeError(string.Format(rangeErrorFormat, f));
            }
            number = environment.Context.ThisBinding.ConvertToNumber(); 
            x = number.BaseValue;
            if (double.IsNaN(x))
            {
                return environment.CreateString("NaN");
            }
            s = "";
            if (x < 0.0)
            {
                s = "-";
                x = -x;
            }
            m = "";
            if (x > limit)
            {
                m = number.ConvertToString().BaseValue;
            }
            else
            {
                n = Math.Truncate(Math.Pow(10.0, f) * x);
                m = n.ToString();
                if (f > 0.0)
                {
                    k = m.Length;
                    if (k <= f)
                    {
                        v = (int)f + 1;
                        z = new string('0', v - k);
                        k = v;
                        m = z + m;
                    }
                    l = k - (int)f;
                    a = m.Substring(0, l);
                    b = m.Substring(l);
                    m = a + "." + b;
                }
            }
            return environment.CreateString(s + m);
        }

        [BuiltinFunction("toExponential", "fractionDigits"), DataDescriptor(true, false, true)]
        internal static IDynamic ToExponential(IEnvironment environment, IArgs args)
        {
            const string rangeErrorFormat = "The value supplied for fractionDigits ({0}) must be between 0 and 20.";

            INumber number;
            double r, min, max, nMax;
            double f = 0.0, x = 0.0, n = 0.0, e = 0.0;
            string s = "", m = "", a = "", b = "", c = "", d = "";

            number = environment.Context.ThisBinding.ConvertToNumber();
            x = number.BaseValue;
            f = args[0].ConvertToInteger().BaseValue;

            if (double.IsNaN(x))
            {
                return environment.CreateString("NaN");
            }

            if (x < 0.0)
            {
                s = "-";
                x = -x;
            }

            if (double.IsPositiveInfinity(x))
            {
                return environment.CreateString(s + "Infinity");
            }
            if (f < 0.0 || f > 20.0)
            {
                throw environment.CreateRangeError(string.Format(rangeErrorFormat, f));
            }

            if (x == 0.0)
            {
                f = 0.0;
                m = "0";
                e = 0.0;
            }
            else 
            {
                if (args.Count > 0)
                {
                    n = Math.Pow(10.0, f);
                    e = Math.Round(Math.Log10(x / n) + f);
                }
                else
                {
                    f = 0.0;
                    min = x - 0.000001;
                    max = x + 0.000001;
                    do
                    {
                        n = Math.Pow(10.0, ++f);
                        nMax = Math.Pow(10.0, f + 1); 
                        do
                        {
                            e = Math.Round(Math.Log10(x / n) + f);
                            r = n * Math.Pow(10.0, e - f);
                            if (r >= min && r <= max)
                                break;
                        } while (++n < nMax);
                    } while (r < min || r > max);
                }
                m = n.ToString();
            }

            if (f != 0.0)
            {
                a = m.Substring(0, 1);
                b = m.Substring(1);
                m = a + "." + b;
            }

            if (e == 0.0)
            {
                c = "+";
                d = "0";
            }
            else
            {
                if (e > 0.0)
                {
                    c = "+";
                }
                else
                {
                    c = "-";
                    e = -e;
                }
                d = e.ToString();
            }

            return environment.CreateString(s + m + "e" + c + d);
        }

        [BuiltinFunction("toPrecision", "precision"), DataDescriptor(true, false, true)]
        internal static IDynamic ToPrecision(IEnvironment environment, IArgs args)
        {
            const string rangeErrorFormat = "The value supplied for precision ({0}) must be between 1 and 21.";

            INumber number;
            double r, min, max, nMax;
            double p = 0.0, x = 0.0, n = 0.0, e = 0.0;
            string s = "", m = "", a = "", b = "", c = "", d = "";

            number = environment.Context.ThisBinding.ConvertToNumber();

            if (args[0].TypeCode == LanguageTypeCode.Undefined)
            {
                return number.ConvertToString();
            }

            x = number.BaseValue;
            p = args[0].ConvertToInteger().BaseValue;

            if (double.IsNaN(x))
            {
                return environment.CreateString("NaN");
            }

            if (x < 0.0)
            {
                s = "-";
                x = -x;
            }

            if (double.IsPositiveInfinity(x))
            {
                return environment.CreateString(s + "Infinity");
            }
            if (p < 1.0 || p > 21.0)
            {
                throw environment.CreateRangeError(string.Format(rangeErrorFormat, p));
            }

            if (x == 0.0)
            {
                m = new string('0', (int)p);
                e = 0.0;
            }
            else
            {
                min = x - 0.0001;
                max = x + 0.0001;
                n = Math.Pow(10.0, p - 1.0) - 1;
                nMax = Math.Pow(10.0, p);

                double rr = 0.0, nr = 0.0, er = 0.0;

                do
                {
                    n++;
                    if (n == nMax)
                        break;
                    e = Math.Round(Math.Log10(x / n) + p - 1.0);
                    r = n * Math.Pow(10.0, e - p + 1.0);
                    if (rr == 0.0 || Math.Abs(r - x) < rr - x)
                    {
                        rr = r;
                        nr = n;
                        er = e;
                    }
                    if (r >= min && r <= max)
                        break;
                } while (true);
                n = nr;
                e = er;
                m = n.ToString();
                if (e < -6.0 || e >= p)
                {
                    a = m.Substring(0, 1);
                    b = m.Substring(1, (int)p - 1);
                    m = a + "." + b;
                    if (e == 0.0)
                    {
                        c = "+";
                        d = "0";
                    }
                    else
                    {
                        if (e > 0.0)
                        {
                            c = "+";
                        }
                        else
                        {
                            c = "-";
                            e = -e;
                        }
                        d = e.ToString();
                    }
                    return environment.CreateString(s + m + "e" + c + d);
                }
            }

            if (e == p - 1.0)
            {
                return environment.CreateString(s + m);
            }
            if (e >= 0.0)
            {
                m = m.Substring(0, (int)e + 1) + "." + m.Substring((int)e + 1, (int)p - ((int)e + 1));
            }
            else
            {
                m = "0." + new string('0', (int)-(e + 1)) + m;
            }

            return environment.CreateString(s + m);
        }
    }
}