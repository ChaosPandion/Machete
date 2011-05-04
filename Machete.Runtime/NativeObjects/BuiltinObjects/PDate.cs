using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PDate : LObject
    {
        private const string _invalidDateString = "Invalid Date";
        public const double HoursPerDay = 24.0;
        public const double MinutesPerHour = 60.0;
        public const double SecondsPerMinute = 60.0;
        public const double MsPerSecond = 1000.0;
        public const double MsPerMinute = 60000.0;
        public const double MsPerHour = 3600000.0;
        public const double MsPerDay = 86400000.0;
        private const int _dayMonthRangesLength = 13;

        private static readonly double[] _dayMonthRanges = new[] { 0.0, 31.0, 59.0, 90.0, 120.0, 151.0, 181.0, 212.0, 243.0, 273.0, 304.0, 334.0, 365.0 };

        public PDate(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Date";
            Extensible = true;
            Prototype = Environment.ObjectPrototype;
            DefineOwnProperty("constructor", Environment.CreateDataDescriptor(Environment.DateConstructor, true, false, true), false);
            base.Initialize();
        }

        [BuiltinFunction("toString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            var time = LocalTime(GetTimeValue(environment));
            return environment.CreateString(
                string.Format(
                    "{0} {1} {2:0#} {3:G4} {4:0#}:{5:0#}:{6:0#}",
                    WeekDayToString(WeekDay(time)),
                    MonthToString(MonthFromTime(time)),
                    DateFromTime(time),
                    YearFromTime(time),
                    HourFromTime(time),
                    MinFromTime(time),
                    SecFromTime(time)
                )
            );
        }

        [BuiltinFunction("toDateString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToDateString(IEnvironment environment, IArgs args)
        {
            var time = LocalTime(GetTimeValue(environment));
            return environment.CreateString(
                string.Format(
                    "{0} {1} {2:0#} {3:G4}",
                    WeekDayToString(WeekDay(time)),
                    MonthToString(MonthFromTime(time)),
                    DateFromTime(time),
                    YearFromTime(time)
                )
            );
        }

        [BuiltinFunction("toTimeString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToTimeString(IEnvironment environment, IArgs args)
        {
            var time = LocalTime(GetTimeValue(environment));
            return environment.CreateString(
                string.Format(
                    "{0:0#}:{1:0#}:{2:0#}",
                    HourFromTime(time),
                    MinFromTime(time),
                    SecFromTime(time)
                )
            );
        }

        [BuiltinFunction("toLocaleString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToLocaleString(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            DateTime result;
            if (DateTime.TryParse(dt.ToString(), out result))
            {
                return environment.CreateString(result.ToString());
            }
            return environment.CreateString(_invalidDateString);
        }

        [BuiltinFunction("toLocaleDateString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToLocaleDateString(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            DateTime result;
            if (DateTime.TryParse(dt.ToString(), out result))
            {
                return environment.CreateString(result.ToLongDateString());
            }
            return environment.CreateString(_invalidDateString);
        }

        [BuiltinFunction("toLocaleTimeString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToLocaleTimeString(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            DateTime result;
            if (DateTime.TryParse(dt.ToString(), out result))
            {
                return environment.CreateString(result.ToLongTimeString());
            }
            return environment.CreateString(_invalidDateString);
        }

        [BuiltinFunction("valueOf"), DataDescriptor(true, false, true)]
        internal static IDynamic ValueOf(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            return dt.PrimitiveValue;
        }

        [BuiltinFunction("getTime"), DataDescriptor(true, false, true)]
        internal static IDynamic GetTime(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => t);
        }

        [BuiltinFunction("getFullYear"), DataDescriptor(true, false, true)]
        internal static IDynamic GetFullYear(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => YearFromTime(LocalTime(t)));
        }

        [BuiltinFunction("getUTCFullYear"), DataDescriptor(true, false, true)]
        internal static IDynamic GetUTCFullYear(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => YearFromTime(t));
        }

        [BuiltinFunction("getMonth"), DataDescriptor(true, false, true)]
        internal static IDynamic GetMonth(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => MonthFromTime(LocalTime(t)));
        }

        [BuiltinFunction("getUTCMonth"), DataDescriptor(true, false, true)]
        internal static IDynamic GetUTCMonth(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => MonthFromTime(t));
        }

        [BuiltinFunction("getDate"), DataDescriptor(true, false, true)]
        internal static IDynamic GetDate(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => DateFromTime(LocalTime(t)));
        }

        [BuiltinFunction("getUTCDate"), DataDescriptor(true, false, true)]
        internal static IDynamic GetUTCDate(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => DateFromTime(t));
        }

        [BuiltinFunction("getDay"), DataDescriptor(true, false, true)]
        internal static IDynamic GetDay(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => WeekDay(LocalTime(t)));
        }

        [BuiltinFunction("getUTCDay"), DataDescriptor(true, false, true)]
        internal static IDynamic GetUTCDay(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => WeekDay(t));
        }

        [BuiltinFunction("getHours"), DataDescriptor(true, false, true)]
        internal static IDynamic GetHours(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => HourFromTime(LocalTime(t)));
        }

        [BuiltinFunction("getUTCHours"), DataDescriptor(true, false, true)]
        internal static IDynamic GetUTCHours(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => HourFromTime(t));
        }

        [BuiltinFunction("getMinutes"), DataDescriptor(true, false, true)]
        internal static IDynamic GetMinutes(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => MinFromTime(LocalTime(t)));
        }

        [BuiltinFunction("getUTCMinutes"), DataDescriptor(true, false, true)]
        internal static IDynamic GetUTCMinutes(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => MinFromTime(t));
        }

        [BuiltinFunction("getSeconds"), DataDescriptor(true, false, true)]
        internal static IDynamic GetSeconds(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => SecFromTime(LocalTime(t)));
        }

        [BuiltinFunction("getUTCSeconds"), DataDescriptor(true, false, true)]
        internal static IDynamic GetUTCSeconds(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => SecFromTime(t));
        }

        [BuiltinFunction("getMilliseconds"), DataDescriptor(true, false, true)]
        internal static IDynamic GetMilliseconds(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => MsFromTime(LocalTime(t)));
        }

        [BuiltinFunction("getUTCMilliseconds"), DataDescriptor(true, false, true)]
        internal static IDynamic GetUTCMilliseconds(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => MsFromTime(t));
        }

        [BuiltinFunction("getTimezoneOffset"), DataDescriptor(true, false, true)]
        internal static IDynamic GetTimezoneOffset(IEnvironment environment, IArgs args)
        {
            return Execute(environment, t => (t - LocalTime(t)) / MsPerMinute);
        }

        [BuiltinFunction("setTime", "time"), DataDescriptor(true, false, true)]
        internal static IDynamic SetTime(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var v = TimeClip(args[0].ConvertToNumber().BaseValue);
            dt.PrimitiveValue = environment.CreateNumber(v);
            return dt.PrimitiveValue;
        }

        [BuiltinFunction("setMilliseconds", "ms"), DataDescriptor(true, false, true)]
        internal static IDynamic SetMilliseconds(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = LocalTime(((INumber)dt.PrimitiveValue).BaseValue);
            var ms = args[0].ConvertToNumber().BaseValue;
            var time = MakeTime(HourFromTime(t), MinFromTime(t), SecFromTime(t), ms);
            var v = TimeClip(MakeDate(Day(t), time));
            dt.PrimitiveValue = environment.CreateNumber(v);
            return dt.PrimitiveValue;
        }

        [BuiltinFunction("setUTCMilliseconds", "ms"), DataDescriptor(true, false, true)]
        internal static IDynamic SetUTCMilliseconds(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = ((INumber)dt.PrimitiveValue).BaseValue;
            var ms = args[0].ConvertToNumber().BaseValue;
            var time = MakeTime(HourFromTime(t), MinFromTime(t), SecFromTime(t), ms);
            var v = TimeClip(MakeDate(Day(t), time));
            dt.PrimitiveValue = environment.CreateNumber(v);
            return dt.PrimitiveValue;
        }

        [BuiltinFunction("setSeconds", "sec", "ms"), DataDescriptor(true, false, true)]
        internal static IDynamic SetSeconds(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = ((INumber)dt.PrimitiveValue).BaseValue;
            var s = args[0].ConvertToNumber().BaseValue;
            var milli = args.Count > 1 ? args[1].ConvertToNumber().BaseValue : MsFromTime(t);
            var date = MakeDate(Day(t), MakeTime(HourFromTime(t), MinFromTime(t), s, milli));
            var u = TimeClip(UTC(date));
            dt.PrimitiveValue = environment.CreateNumber(u);
            return dt.PrimitiveValue;
        }

        [BuiltinFunction("setUTCSeconds", "sec", "ms"), DataDescriptor(true, false, true)]
        internal static IDynamic SetUTCSeconds(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = ((INumber)dt.PrimitiveValue).BaseValue;
            var s = args[0].ConvertToNumber().BaseValue;
            var milli = args.Count > 1 ? args[1].ConvertToNumber().BaseValue : MsFromTime(t);
            var date = MakeDate(Day(t), MakeTime(HourFromTime(t), MinFromTime(t), s, milli));
            var u = TimeClip(date);
            dt.PrimitiveValue = environment.CreateNumber(u);
            return dt.PrimitiveValue;
        }

        [BuiltinFunction("setMinutes", "min", "sec", "ms"), DataDescriptor(true, false, true)]
        internal static IDynamic SetMinutes(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = ((INumber)dt.PrimitiveValue).BaseValue;
            var m = args[0].ConvertToNumber().BaseValue;
            var s = args.Count > 1 ? args[1].ConvertToNumber().BaseValue : SecFromTime(t);
            var milli = args.Count > 2 ? args[2].ConvertToNumber().BaseValue : MsFromTime(t);
            var date = MakeDate(Day(t), MakeTime(HourFromTime(t), m, s, milli));
            var u = TimeClip(UTC(date));
            dt.PrimitiveValue = environment.CreateNumber(u);
            return dt.PrimitiveValue;
        }

        [BuiltinFunction("setUTCMinutes", "min", "sec", "ms"), DataDescriptor(true, false, true)]
        internal static IDynamic SetUTCMinutes(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = ((INumber)dt.PrimitiveValue).BaseValue;
            var m = args[0].ConvertToNumber().BaseValue;
            var s = args.Count > 1 ? args[1].ConvertToNumber().BaseValue : SecFromTime(t);
            var milli = args.Count > 2 ? args[2].ConvertToNumber().BaseValue : MsFromTime(t);
            var date = MakeDate(Day(t), MakeTime(HourFromTime(t), m, s, milli));
            var u = TimeClip(date);
            dt.PrimitiveValue = environment.CreateNumber(u);
            return dt.PrimitiveValue;
        }

        [BuiltinFunction("setHours", "hour", "min", "sec", "ms"), DataDescriptor(true, false, true)]
        internal static IDynamic SetHours(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = ((INumber)dt.PrimitiveValue).BaseValue;
            var h = args[0].ConvertToNumber().BaseValue;
            var m = args.Count > 1 ? args[1].ConvertToNumber().BaseValue : MinFromTime(t);
            var s = args.Count > 2 ? args[2].ConvertToNumber().BaseValue : SecFromTime(t);
            var milli = args.Count > 3 ? args[3].ConvertToNumber().BaseValue : MsFromTime(t);
            var date = MakeDate(Day(t), MakeTime(HourFromTime(t), m, s, milli));
            var u = TimeClip(UTC(date));
            dt.PrimitiveValue = environment.CreateNumber(u);
            return dt.PrimitiveValue;
        }

        [BuiltinFunction("setUTCHours", "hour", "min", "sec", "ms"), DataDescriptor(true, false, true)]
        internal static IDynamic SetUTCHours(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = ((INumber)dt.PrimitiveValue).BaseValue;
            var h = args[0].ConvertToNumber().BaseValue;
            var m = args.Count > 1 ? args[1].ConvertToNumber().BaseValue : MinFromTime(t);
            var s = args.Count > 2 ? args[2].ConvertToNumber().BaseValue : SecFromTime(t);
            var milli = args.Count > 3 ? args[3].ConvertToNumber().BaseValue : MsFromTime(t);
            var date = MakeDate(Day(t), MakeTime(HourFromTime(t), m, s, milli));
            var u = TimeClip(date);
            dt.PrimitiveValue = environment.CreateNumber(u);
            return dt.PrimitiveValue;
        }

        [BuiltinFunction("setDate", "date"), DataDescriptor(true, false, true)]
        internal static IDynamic SetDate(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = ((INumber)dt.PrimitiveValue).BaseValue;
            var date = args[0].ConvertToNumber().BaseValue;
            var newDate = MakeDate(MakeDay(YearFromTime(t), MonthFromTime(t), date), TimeWithinDay(t));
            var v = TimeClip(UTC(newDate));
            return dt.PrimitiveValue = environment.CreateNumber(v);
        }

        [BuiltinFunction("setUTCDate", "date"), DataDescriptor(true, false, true)]
        internal static IDynamic SetUTCDate(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = ((INumber)dt.PrimitiveValue).BaseValue;
            var date = args[0].ConvertToNumber().BaseValue;
            var newDate = MakeDate(MakeDay(YearFromTime(t), MonthFromTime(t), date), TimeWithinDay(t));
            var v = TimeClip(newDate);
            return dt.PrimitiveValue = environment.CreateNumber(v);
        }

        [BuiltinFunction("setMonth", "month", "date"), DataDescriptor(true, false, true)]
        internal static IDynamic SetMonth(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = ((INumber)dt.PrimitiveValue).BaseValue;
            var m = args[0].ConvertToNumber().BaseValue;
            var date = args.Count > 1 ? args[1].ConvertToNumber().BaseValue : DateFromTime(t);
            var newDate = MakeDate(MakeDay(YearFromTime(t), m, date), TimeWithinDay(t));
            var v = TimeClip(UTC(newDate));
            return dt.PrimitiveValue = environment.CreateNumber(v);
        }

        [BuiltinFunction("setUTCMonth", "month", "date"), DataDescriptor(true, false, true)]
        internal static IDynamic SetUTCMonth(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = ((INumber)dt.PrimitiveValue).BaseValue;
            var m = args[0].ConvertToNumber().BaseValue;
            var date = args.Count > 1 ? args[1].ConvertToNumber().BaseValue : DateFromTime(t);
            var newDate = MakeDate(MakeDay(YearFromTime(t), m, date), TimeWithinDay(t));
            var v = TimeClip(newDate);
            return dt.PrimitiveValue = environment.CreateNumber(v);
        }

        [BuiltinFunction("setFullYear", "year", "month", "date"), DataDescriptor(true, false, true)]
        internal static IDynamic SetFullYear(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = ((INumber)dt.PrimitiveValue).BaseValue;
            var y = args[0].ConvertToNumber().BaseValue;
            var m = args.Count > 1 ? args[1].ConvertToNumber().BaseValue : MonthFromTime(t);
            var date = args.Count > 2 ? args[2].ConvertToNumber().BaseValue : DateFromTime(t);
            var newDate = MakeDate(MakeDay(y, m, date), TimeWithinDay(t));
            var v = TimeClip(UTC(newDate));
            return dt.PrimitiveValue = environment.CreateNumber(v);
        }

        [BuiltinFunction("setUTCFullYear", "year", "month", "date"), DataDescriptor(true, false, true)]
        internal static IDynamic SetUTCFullYear(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = ((INumber)dt.PrimitiveValue).BaseValue;
            var y = args[0].ConvertToNumber().BaseValue;
            var m = args.Count > 1 ? args[1].ConvertToNumber().BaseValue : MonthFromTime(t);
            var date = args.Count > 2 ? args[2].ConvertToNumber().BaseValue : DateFromTime(t);
            var newDate = MakeDate(MakeDay(y, m, date), TimeWithinDay(t));
            var v = TimeClip(newDate);
            return dt.PrimitiveValue = environment.CreateNumber(v);
        }

        [BuiltinFunction("toUTCString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToUTCString(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            DateTime result;
            if (DateTime.TryParse(dt.ToString(), out result))
            {
                return environment.CreateString(result.ToUniversalTime().ToString());
            }
            return environment.CreateString(_invalidDateString);
        }

        [BuiltinFunction("toISOString"), DataDescriptor(true, false, true)]
        internal static IDynamic toISOString(IEnvironment environment, IArgs args)
        {
            var dt = (NDate)environment.Context.ThisBinding;
            var t = ((INumber)dt.PrimitiveValue).BaseValue;
            if (double.IsNaN(t) || double.IsInfinity(t))
                throw environment.CreateRangeError("");
            DateTime result;
            if (DateTime.TryParse(dt.ToString(), out result))
            {
                return environment.CreateString(result.ToString("yyyy-MM-ddTHH:mm:ss.FFFZ"));
            }
            return environment.CreateString(_invalidDateString);
        }

        [BuiltinFunction("toJSON", "key"), DataDescriptor(true, false, true)]
        internal static IDynamic ToJSON(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding.ConvertToObject();
            var tv = o.ConvertToPrimitive("Number");
            if (tv.TypeCode == LanguageTypeCode.Number)
            {
                var v = ((INumber)tv).BaseValue;
                if (double.IsNaN(v) || double.IsInfinity(v))
                    return environment.Null;
            }
            var toISO = o.Get("toISOString") as ICallable;
            if (toISO == null)
                throw environment.CreateTypeError("");
            return toISO.Call(environment, o, environment.EmptyArgs);
        }

        internal static double GetTimeValue(IEnvironment environment)
        {
            var date = environment.Context.ThisBinding.Value as NDate;
            if (date == null)
            {
                throw environment.CreateTypeError("");
            }
            return date.PrimitiveValue.ConvertToNumber().BaseValue;
        }

        internal static IDynamic Execute(IEnvironment environment, Func<double, double> func)
        {
            var time = GetTimeValue(environment);
            if (double.IsNaN(time))
            {
                return environment.CreateNumber(double.NaN);
            }
            else
            {
                return environment.CreateNumber(func(time));
            }
        }

        internal static string WeekDayToString(double weekDay)
        {
            switch ((int)weekDay)
            {
                case 0:
                    return "Sun";
                case 1:
                    return "Mon";
                case 2:
                    return "Tue";
                case 3:
                    return "Wed";
                case 4:
                    return "Thu";
                case 5:
                    return "Fri";
                case 6:
                    return "Sat";
                default:
                    throw new MacheteException();
            }
        }

        internal static string MonthToString(double month)
        {
            switch ((int)month)
            {
                case 0:
                    return "Jan";
                case 1:
                    return "Feb";
                case 2:
                    return "Mar";
                case 3:
                    return "Apr";
                case 4:
                    return "May";
                case 5:
                    return "Jun";
                case 6:
                    return "Jul";
                case 7:
                    return "Aug";
                case 8:
                    return "Sep";
                case 9:
                    return "Oct";
                case 10:
                    return "Nov";
                case 11:
                    return "Dec";
                default:
                    throw new MacheteException();
            }
        }

        internal static double Day(double t)
        {
            return Math.Floor(t / MsPerDay);
        }

        internal static double TimeWithinDay(double t)
        {
            return t % MsPerDay;
        }

        internal static double DaysInYear(double y)
        {
            if (y % 4.0 != 0)
            {
                return 365.0;
            }
            else if (y % 100.0 != 0.0)
            {
                return 366.0;
            }
            else if (y % 400.0 != 0.0)
            {
                return 365.0;
            }
            else
            {
                return 366.0;
            }
        }

        internal static double DayFromYear(double y)
        {
            return 365.0 * (y - 1970.0) + Math.Floor((y - 1969.0) / 4.0) - Math.Floor((y - 1901.0) / 100.0) + Math.Floor((y - 1601.0) / 400.0);
        }

        internal static double TimeFromYear(double y)
        {
            return MsPerDay * DayFromYear(y);
        }

        internal static double YearFromTime(double t)
        {
            var y = 0.0;
            while (TimeFromYear(y) <= t)
            {
                y++;
            }
            return --y;
        }

        internal static double InLeapYear(double t)
        {
            if (DaysInYear(YearFromTime(t)) == 365.0)
            {
                return 0.0;
            }
            else
            {
                return 1.0;
            }
        }

        internal static double MonthFromTime(double t)
        {
            var dayWithinYear = DayWithinYear(t);
            if (dayWithinYear >= 0 && dayWithinYear < 31)
            {
                return 0.0;
            }
            var inLeapYear = InLeapYear(t);
            var corrected = 59 + inLeapYear;
            if (dayWithinYear >= 31 && dayWithinYear < corrected)
            {
                return 1.0;
            }
            var r = 0.0;
            for (int i = 2; i < _dayMonthRangesLength; i++)
            {
                if (dayWithinYear >= corrected && dayWithinYear < (corrected = _dayMonthRanges[i + 1] + inLeapYear))
                {
                    r = (double)i;
                    break;
                }
            }
            return r;
        }

        internal static double DayWithinYear(double t)
        {
            return Day(t) - DayFromYear(YearFromTime(t));
        }

        internal static double DateFromTime(double t)
        {
            var m = (int)MonthFromTime(t);
            switch (m)
            {
                case 0:
                    return DayWithinYear(t) + 1.0;
                case 1:
                    return DayWithinYear(t) - 30.0;
                case 2:
                    return DayWithinYear(t) - 58.0 - InLeapYear(t);
                case 3:
                    return DayWithinYear(t) - 89.0 - InLeapYear(t);
                case 4:
                    return DayWithinYear(t) - 119.0 - InLeapYear(t);
                case 5:
                    return DayWithinYear(t) - 150.0 - InLeapYear(t);
                case 6:
                    return DayWithinYear(t) - 180.0 - InLeapYear(t);
                case 7:
                    return DayWithinYear(t) - 211.0 - InLeapYear(t);
                case 8:
                    return DayWithinYear(t) - 242.0 - InLeapYear(t);
                case 9:
                    return DayWithinYear(t) - 272.0 - InLeapYear(t);
                case 10:
                    return DayWithinYear(t) - 303.0 - InLeapYear(t);
                case 11:
                    return DayWithinYear(t) - 333.0 - InLeapYear(t);
                default:
                    throw new MacheteException();
            }
        }

        internal static double WeekDay(double t)
        {
            return (Day(t) + 4.0) % 7.0;
        }

        internal static double DaylightSavingTA(double t)
        {
            if (t < long.MinValue || t > long.MaxValue)
            {
                return 0.0;
            }

            var rules = TimeZoneInfo.Local.GetAdjustmentRules();
            if (rules == null || rules.Length == 0)
            {
                return 0.0;
            }

            var rule = rules[0];
            var start = rule.DaylightTransitionStart;
            var end = rule.DaylightTransitionEnd;
            var month = (int)MonthFromTime(t);
            var day = (int)DateFromTime(t);
            var hour = (int)HourFromTime(t);
            var minute = (int)MinFromTime(t);
            var inMonth = start.Month <= month && end.Month >= month;
            var inDay = start.Day <= day && end.Day >= day;
            var inHour = start.TimeOfDay.Hour <= hour && end.TimeOfDay.Hour <= hour;
            var inMinute = start.TimeOfDay.Minute <= minute && end.TimeOfDay.Minute <= minute;
            return inMonth && inDay && inHour && inMinute ? rule.DaylightDelta.TotalMilliseconds : 0.0;
        }

        internal static double LocalTime(double t)
        {
            return t + TimeZoneInfo.Local.BaseUtcOffset.TotalMilliseconds + DaylightSavingTA(t);
        }

        internal static double UTC(double t)
        {
            var offset = TimeZoneInfo.Local.BaseUtcOffset.TotalMilliseconds;
            return t - offset - DaylightSavingTA(t - offset);
        }

        internal static double HourFromTime(double t)
        {
            return Math.Floor(t / MsPerHour) % HoursPerDay;
        }

        internal static double MinFromTime(double t)
        {
            return Math.Floor(t / MsPerMinute) % MinutesPerHour;
        }

        internal static double SecFromTime(double t)
        {
            return Math.Floor(t / MsPerSecond) % SecondsPerMinute;
        }

        internal static double MsFromTime(double t)
        {
            return t % MsPerSecond;
        }

        internal static double MakeTime(double hour, double min, double sec, double ms)
        {
            if (double.IsInfinity(hour) || double.IsInfinity(min) || double.IsInfinity(sec) || double.IsInfinity(ms))
            {
                return double.NaN;
            }
            else
            {
                return ((long)hour * MsPerHour) + ((long)min * MsPerMinute) + ((long)sec * MsPerSecond) + (long)ms;
            }
        }

        internal static double MakeDay(double year, double month, double date)
        {
            if (double.IsInfinity(year) || double.IsInfinity(month) || double.IsInfinity(date))
            {
                return double.NaN;
            }
            else
            {
                var y = (double)(long)year;
                var m = (double)(long)month;
                var dt = (double)(long)date;


                if (m < 0 || m > 11)
                {
                    return double.NaN;
                }
                else if (dt < 1 || dt > 31)
                {
                    return double.NaN;
                }

                var ym = y + Math.Floor(m / 12);
                var mn = m % 12.0;
                var t = 1.0;

                while (YearFromTime(t) != ym && MonthFromTime(t) != mn && DateFromTime(t) != 1)
                {
                    t++;
                }

                return t;
            }
        }

        internal static double MakeDate(double day, double time)
        {
            if (double.IsInfinity(day) || double.IsInfinity(time))
            {
                return double.NaN;
            }
            else
            {
                return day * MsPerDay + time;
            }
        }

        internal static double TimeClip(double time)
        {
            if (double.IsInfinity(time) || Math.Abs(time) > 8.64e15)
            {
                return double.NaN;
            }
            return (double)(long)time + (+0);
        }
    }
}
