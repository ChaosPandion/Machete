using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.NativeObjects;
using Machete.Core;

namespace Machete.Runtime.RuntimeTypes.LanguageTypes
{
    public sealed class LNumber : LType, INumber
    {
        public double BaseValue { get; private set; }

        public override LanguageTypeCode TypeCode
        {
            get { return LanguageTypeCode.Number; }
        }

        public override bool IsPrimitive
        {
            get { return true; }
        }

        public LNumber(IEnvironment environment, double value)
            : base(environment)
        {
            BaseValue = value;
        }

        public override IDynamic Op_Equals(IDynamic other)
        {
            switch (other.TypeCode)
            {
                case LanguageTypeCode.String:
                    return this.Op_Equals(other.ConvertToNumber());
                case LanguageTypeCode.Number:
                    var lnum = (LNumber)other;
                    return Environment.CreateBoolean(!(double.IsNaN(BaseValue) || double.IsNaN(lnum.BaseValue)) && this.BaseValue == lnum.BaseValue);
                case LanguageTypeCode.Object:
                    return this.Op_Equals(other.ConvertToPrimitive(null));
                default:
                    return Environment.False;
            }
        }

        public override IDynamic Op_StrictEquals(IDynamic other)
        {
            switch (other.TypeCode)
            {
                case LanguageTypeCode.Number:
                    var lnum = (LNumber)other;
                    return Environment.CreateBoolean(!(double.IsNaN(BaseValue) || double.IsNaN(lnum.BaseValue)) && this.BaseValue == lnum.BaseValue);
                default:
                    return Environment.False;
            }
        }

        public override IDynamic Op_Typeof()
        {
            return Environment.CreateString("number");
        }

        public override IDynamic ConvertToPrimitive(string preferredType)
        {
            return this;
        }

        public override IBoolean ConvertToBoolean()
        {
            return Environment.CreateBoolean(!double.IsNaN(BaseValue) && BaseValue != 0.0);
        }

        public override INumber ConvertToNumber()
        {
            return this;
        }

        public override IString ConvertToString()
        {
            if (double.IsNaN(BaseValue))
            {
                return Environment.CreateString("NaN");
            }
            else if (BaseValue == 0.0)
            {
                return Environment.CreateString("0");
            }
            else if (BaseValue < 0.0)
            {
                return Environment.CreateString("-" + (string)(Environment.CreateNumber(-BaseValue)).ConvertToString().BaseValue);
            }
            else if (double.IsInfinity(BaseValue))
            {
                return Environment.CreateString("Infinity");
            }

            const double epsilon = 0.0000001;

            int n = 0, k = 0, s = 0;
            int min = 0, max = 0;
            double r = 0.0, rv = 0.0, rmin = BaseValue - epsilon, rmax = BaseValue + epsilon;
            bool complete = false;

            for (int ki = 1; !complete && ki < int.MaxValue; k = ki, ki++)
            {
                min = (int)Math.Pow(10, ki - 1);
                max = (int)Math.Pow(10, ki);
                for (int si = min; !complete && si < max; s = si, si++)
                {
                    if (si % 10 == 0) continue;
                    r = Math.Log10(BaseValue / si) + ki;
                    rv = si * Math.Pow(10, (int)r - ki);
                    complete = rv > rmin && rv < rmax;
                    n = (int)r;
                }
            }

            if (k <= n && n <= 21)
            {
                return Environment.CreateString(s.ToString().Substring(0, k) + "".PadRight(n - k, '0'));
            }
            else if (n > 0 && n <= 21)
            {
                var sv = s.ToString();
                var left = sv.Substring(0, n);
                var right = sv.Substring(n, k - n);
                return Environment.CreateString(left + "." + right);
            }
            else if (n > -6 && n <= 0)
            {
                return Environment.CreateString("0.".PadRight(2 + -n, '0') + s.ToString().Substring(0, k));
            }
            else if (k == 1)
            {
                var v = n - 1;
                var sign = v < 0 ? "-" : "+";
                var sv = s.ToString();
                var nv = Math.Abs(v).ToString();
                return Environment.CreateString(sv + "e" + sign + nv);
            }
            else
            {
                var v = n - 1;
                var sign = v < 0 ? "-" : "+";
                var sv = s.ToString();
                var nv = Math.Abs(v).ToString();
                return Environment.CreateString(sv[0] + "." + sv.Substring(1) + "e" + sign + nv);
            }
        }

        public override IObject ConvertToObject()
        {
            return ((IConstructable)Environment.NumberConstructor).Construct(Environment, Environment.CreateArgs(new IDynamic[] { this }));
        }
    }
}