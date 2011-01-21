using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System;
using Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects;
using System.Globalization;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CDate : BConstructor
    {
        private const DateTimeStyles _styles = DateTimeStyles.AdjustToUniversal;
        
        private static readonly DateTime _utcStart = new DateTime(1970, 1, 1);

        private static readonly IFormatProvider _format = CultureInfo.CurrentCulture.DateTimeFormat;

        private static readonly string[] _startingDateFormats = {
            "yyyy-MM-ddTHH:mm:ss.FFFK",
            "yyyy-MM-ddTHH:mm:ss.FFF",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssK",
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-ddTHH:mmK",
            "yyyy-MM-dd",
            "yyyy-MMTHH:mm:ss.FFFK",
            "yyyy-MMTHH:mm:ss.FFF",
            "yyyy-MMTHH:mm:ss",
            "yyyy-MMTHH:mm:ssK",
            "yyyy-MMTHH:mm",
            "yyyy-MMTHH:mmK",
            "yyyy-MM",
            "yyyyTHH:mm:ss.FFFK",
            "yyyyTHH:mm:ss.FFF",
            "yyyyTHH:mm:ss",
            "yyyyTHH:mm:ssK",
            "yyyyTHH:mm",
            "yyyyTHH:mmK",
            "yyyy"
        };

        public CDate(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(7.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.DatePrototype, false, false, false), false);
        }

        protected override IDynamic Call(IEnvironment environment, IArgs args)
        {
            return environment.CreateString(DateTime.UtcNow.ToString());
        }

        public override IObject Construct(IEnvironment environment, IArgs args)
        {
            var r = new NDate(environment);
            r.Class = "Date";
            r.Extensible = true;
            r.Prototype = environment.DatePrototype;

            var argCount = args.Count;
            double timeValue = 0.0;
            if (argCount == 0)
            {
                timeValue = (DateTime.UtcNow - _utcStart).TotalMilliseconds;
            }
            else if (argCount == 1)
            {
                var value = args[0].ConvertToPrimitive(null);
                if (value.TypeCode == LanguageTypeCode.String)
                {
                    timeValue = Parse(environment, args).ConvertToNumber().BaseValue;
                }
                else
                {
                    timeValue = value.ConvertToNumber().BaseValue;
                }
            }
            else
            {
                var year = args[0].ConvertToNumber().ConvertToInteger().BaseValue;
                var month = args[1].ConvertToNumber().BaseValue;
                var date = argCount > 2 ? args[2].ConvertToNumber().BaseValue : 1.0;
                var hours = argCount > 3 ? args[3].ConvertToNumber().BaseValue : 0.0;
                var minutes = argCount > 4 ? args[4].ConvertToNumber().BaseValue : 0.0;
                var seconds = argCount > 5 ? args[5].ConvertToNumber().BaseValue : 0.0;
                var ms = argCount > 6 ? args[6].ConvertToNumber().BaseValue : 0.0;

                if (!double.IsNaN(year) && year >= 0.0 && year <= 99)
                {
                    year = 1900.0 + year;
                }

                var dayPortion = PDate.MakeDay(year, month, date);
                var timePortion = PDate.MakeTime(hours, minutes, seconds, ms);
                var dateValue = PDate.MakeDate(dayPortion, timePortion);

                timeValue = PDate.TimeClip(PDate.UTC(dateValue));
            }

            r.PrimitiveValue = environment.CreateNumber(PDate.TimeClip(timeValue));
            return r;
        }

        [BuiltinFunction("parse", "string"), DataDescriptor(true, false, true)]
        internal static IDynamic Parse(IEnvironment environment, IArgs args)
        {
            var s = args[0].ConvertToString().BaseValue;

            DateTime result;
            if (!DateTime.TryParseExact(s, _startingDateFormats, _format, _styles, out result))
            {
                if (!DateTime.TryParse(s, _format, _styles, out result))
                {
                    return environment.CreateNumber(double.NaN);
                }
            }
            return environment.CreateNumber((result.ToUniversalTime() - _utcStart).TotalMilliseconds);
        }

        [BuiltinFunction("UTC", "year", "month", "date", "hours", "minutes", "seconds", "ms"), DataDescriptor(true, false, true)]
        internal static IDynamic Utc(IEnvironment environment, IArgs args)
        {
            var argCount = args.Count;
            var year = args[0].ConvertToNumber().ConvertToInteger().BaseValue;
            var month = args[1].ConvertToNumber().BaseValue;
            var date = argCount > 2 ? args[2].ConvertToNumber().BaseValue : 1.0;
            var hours = argCount > 3 ? args[3].ConvertToNumber().BaseValue : 0.0;
            var minutes = argCount > 4 ? args[4].ConvertToNumber().BaseValue : 0.0;
            var seconds = argCount > 5 ? args[5].ConvertToNumber().BaseValue : 0.0;
            var ms = argCount > 6 ? args[6].ConvertToNumber().BaseValue : 0.0;

            if (!double.IsNaN(year) && year >= 0.0 && year <= 99)
            {
                year = 1900.0 + year;
            }

            var dayPortion = PDate.MakeDay(year, month, date);
            var timePortion = PDate.MakeTime(hours, minutes, seconds, ms);
            var dateValue = PDate.MakeDate(dayPortion, timePortion);

            return environment.CreateNumber(PDate.TimeClip(dateValue));
        }

        [BuiltinFunction("now"), DataDescriptor(true, false, true)]
        internal static IDynamic Now(IEnvironment environment, IArgs args)
        {
            var value = environment.CreateNumber((DateTime.UtcNow - _utcStart).TotalMilliseconds);
            var constructArgs = environment.CreateArgs(new IDynamic[] { value });
            return ((IConstructable)environment.DateConstructor).Construct(environment, constructArgs);
        }
    }
}