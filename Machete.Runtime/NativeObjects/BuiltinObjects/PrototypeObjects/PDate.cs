using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PDate : LObject
    {
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
            base.Initialize();
        }
        //private IDynamic ToString(Engine engine, Args args)
        //{
        //    var time = LocalTime(GetTimeValue(engine));
        //    return new StringPrimitive(
        //        string.Format(
        //            "{0} {1} {2:0#} {3:G4} {4:0#}:{5:0#}:{6:0#}",
        //            WeekDayToString(WeekDay(time)),
        //            MonthToString(MonthFromTime(time)),
        //            DateFromTime(time),
        //            YearFromTime(time),
        //            HourFromTime(time),
        //            MinFromTime(time),
        //            SecFromTime(time)
        //        )
        //    );
        //}

        //private IDynamic ToDateString(Engine engine, Args args)
        //{
        //    var time = LocalTime(GetTimeValue(engine));
        //    return new StringPrimitive(
        //        string.Format(
        //            "{0} {1} {2:0#} {3:G4}",
        //            WeekDayToString(WeekDay(time)),
        //            MonthToString(MonthFromTime(time)),
        //            DateFromTime(time),
        //            YearFromTime(time)
        //        )
        //    );
        //}

        //private IDynamic ToTimeString(Engine engine, Args args)
        //{
        //    var time = LocalTime(GetTimeValue(engine));
        //    return new StringPrimitive(
        //        string.Format(
        //            "{0:0#}:{1:0#}:{2:0#}",
        //            HourFromTime(time),
        //            MinFromTime(time),
        //            SecFromTime(time)
        //        )
        //    );
        //}

        //private IDynamic GetDate(Engine engine, Args args)
        //{
        //    return Execute(engine, t => DateFromTime(LocalTime(t)));
        //}

        //private IDynamic GetDay(Engine engine, Args args)
        //{
        //    return Execute(engine, t => WeekDay(LocalTime(t)));
        //}

        //private IDynamic GetFullYear(Engine engine, Args args)
        //{
        //    return Execute(engine, t => YearFromTime(LocalTime(t)));
        //}

        //private IDynamic GetHours(Engine engine, Args args)
        //{
        //    return Execute(engine, t => HourFromTime(LocalTime(t)));
        //}

        //private IDynamic GetMilliseconds(Engine engine, Args args)
        //{
        //    return Execute(engine, t => MsFromTime(LocalTime(t)));
        //}

        //private IDynamic GetMinutes(Engine engine, Args args)
        //{
        //    return Execute(engine, t => MinFromTime(LocalTime(t)));
        //}

        //private IDynamic GetMonth(Engine engine, Args args)
        //{
        //    return Execute(engine, t => MonthFromTime(LocalTime(t)));
        //}

        //private IDynamic GetSeconds(Engine engine, Args args)
        //{
        //    return Execute(engine, t => SecFromTime(LocalTime(t)));
        //}

        //private IDynamic GetTime(Engine engine, Args args)
        //{
        //    return Execute(engine, t => t);
        //}

        //private IDynamic GetTimezoneOffset(Engine engine, Args args)
        //{
        //    return Execute(engine, t => (t - LocalTime(t)) / MsPerMinute);
        //}

        //private IDynamic GetUTCDate(Engine engine, Args args)
        //{
        //    return Execute(engine, t => DateFromTime(t));
        //}

        //private IDynamic GetUTCDay(Engine engine, Args args)
        //{
        //    return Execute(engine, t => WeekDay(t));
        //}

        //private IDynamic GetUTCFullYear(Engine engine, Args args)
        //{
        //    return Execute(engine, t => YearFromTime(t));
        //}

        //private IDynamic GetUTCHours(Engine engine, Args args)
        //{
        //    return Execute(engine, t => HourFromTime(t));
        //}

        //private IDynamic GetUTCMilliseconds(Engine engine, Args args)
        //{
        //    return Execute(engine, t => MsFromTime(t));
        //}

        //private IDynamic GetUTCMinutes(Engine engine, Args args)
        //{
        //    return Execute(engine, t => MinFromTime(t));
        //}

        //private IDynamic GetUTCMonth(Engine engine, Args args)
        //{
        //    return Execute(engine, t => MonthFromTime(t));
        //}

        //private IDynamic GetUTCSeconds(Engine engine, Args args)
        //{
        //    return Execute(engine, t => SecFromTime(t));
        //}

        //private IDynamic SetDate(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic SetFullYear(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic SetHours(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic SetMilliseconds(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic SetMinutes(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic SetMonth(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic SetSeconds(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic SetTime(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic SetUTCDate(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic SetUTCFullYear(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic SetUTCHours(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic SetUTCMilliseconds(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic SetUTCMinutes(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic SetUTCMonth(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic SetUTCSeconds(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic ToJSON(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic ToLocaleString(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic ToUTCString(Engine engine, Args args)
        //{
        //    return null;
        //}

        //private IDynamic ValueOf(Engine engine, Args args)
        //{
        //    return null;
        //}

        //internal static double GetTimeValue(Engine engine)
        //{
        //    var date = engine.Context.ThisBinding as DateObject;
        //    if (date == null)
        //    {
        //        throw RuntimeError.TypeError();
        //    }
        //    return date.PrimitiveValue.ToNumberPrimitive().Value;
        //}

        //internal static IDynamic Execute(Engine engine, Func<double, double> func)
        //{
        //    var time = GetTimeValue(engine);
        //    if (double.IsNaN(time))
        //    {
        //        return new NumberPrimitive(double.NaN);
        //    }
        //    else
        //    {
        //        return new NumberPrimitive(func(time));
        //    }
        //}

        //internal static string WeekDayToString(double weekDay)
        //{
        //    switch ((int)weekDay)
        //    {
        //        case 0:
        //            return "Sun";
        //        case 1:
        //            return "Mon";
        //        case 2:
        //            return "Tue";
        //        case 3:
        //            return "Wed";
        //        case 4:
        //            return "Thu";
        //        case 5:
        //            return "Fri";
        //        case 6:
        //            return "Sat";
        //        default:
        //            throw new MacheteException();
        //    }
        //}

        //internal static string MonthToString(double month)
        //{
        //    switch ((int)month)
        //    {
        //        case 0:
        //            return "Jan";
        //        case 1:
        //            return "Feb";
        //        case 2:
        //            return "Mar";
        //        case 3:
        //            return "Apr";
        //        case 4:
        //            return "May";
        //        case 5:
        //            return "Jun";
        //        case 6:
        //            return "Jul";
        //        case 7:
        //            return "Aug";
        //        case 8:
        //            return "Sep";
        //        case 9:
        //            return "Oct";
        //        case 10:
        //            return "Nov";
        //        case 11:
        //            return "Dec";
        //        default:
        //            throw new MacheteException();
        //    }
        //}

        //internal static double Day(double t)
        //{
        //    return Math.Floor(t / MsPerDay);
        //}

        //internal static double TimeWithinDay(double t)
        //{
        //    return t % MsPerDay;
        //}

        //internal static double DaysInYear(double y)
        //{
        //    if (y % 4.0 != 0)
        //    {
        //        return 365.0;
        //    }
        //    else if (y % 100.0 != 0.0)
        //    {
        //        return 366.0;
        //    }
        //    else if (y % 400.0 != 0.0)
        //    {
        //        return 365.0;
        //    }
        //    else
        //    {
        //        return 366.0;
        //    }
        //}

        //internal static double DayFromYear(double y)
        //{
        //    return 365.0 * (y - 1970.0) + Math.Floor((y - 1969.0) / 4.0) - Math.Floor((y - 1901.0) / 100.0) + Math.Floor((y - 1601.0) / 400.0);
        //}

        //internal static double TimeFromYear(double y)
        //{
        //    return MsPerDay * DayFromYear(y);
        //}

        //internal static double YearFromTime(double t)
        //{
        //    var y = 0.0;
        //    while (TimeFromYear(y) <= t)
        //    {
        //        y++;
        //    }
        //    return --y;
        //}

        //internal static double InLeapYear(double t)
        //{
        //    if (DaysInYear(YearFromTime(t)) == 365.0)
        //    {
        //        return 0.0;
        //    }
        //    else
        //    {
        //        return 1.0;
        //    }
        //}

        //internal static double MonthFromTime(double t)
        //{
        //    var dayWithinYear = DayWithinYear(t);
        //    if (dayWithinYear >= 0 && dayWithinYear < 31)
        //    {
        //        return 0.0;
        //    }
        //    var inLeapYear = InLeapYear(t);
        //    var corrected = 59 + inLeapYear;
        //    if (dayWithinYear >= 31 && dayWithinYear < corrected)
        //    {
        //        return 1.0;
        //    }
        //    var r = 0.0;
        //    for (int i = 2; i < _dayMonthRangesLength; i++)
        //    {
        //        if (dayWithinYear >= corrected && dayWithinYear < (corrected = _dayMonthRanges[i + 1] + inLeapYear))
        //        {
        //            r = (double)i;
        //            break;
        //        }
        //    }
        //    return r;
        //}

        //internal static double DayWithinYear(double t)
        //{
        //    return Day(t) - DayFromYear(YearFromTime(t));
        //}

        //internal static double DateFromTime(double t)
        //{
        //    var m = (int)MonthFromTime(t);
        //    switch (m)
        //    {
        //        case 0:
        //            return DayWithinYear(t) + 1.0;
        //        case 1:
        //            return DayWithinYear(t) - 30.0;
        //        case 2:
        //            return DayWithinYear(t) - 58.0 - InLeapYear(t);
        //        case 3:
        //            return DayWithinYear(t) - 89.0 - InLeapYear(t);
        //        case 4:
        //            return DayWithinYear(t) - 119.0 - InLeapYear(t);
        //        case 5:
        //            return DayWithinYear(t) - 150.0 - InLeapYear(t);
        //        case 6:
        //            return DayWithinYear(t) - 180.0 - InLeapYear(t);
        //        case 7:
        //            return DayWithinYear(t) - 211.0 - InLeapYear(t);
        //        case 8:
        //            return DayWithinYear(t) - 242.0 - InLeapYear(t);
        //        case 9:
        //            return DayWithinYear(t) - 272.0 - InLeapYear(t);
        //        case 10:
        //            return DayWithinYear(t) - 303.0 - InLeapYear(t);
        //        case 11:
        //            return DayWithinYear(t) - 333.0 - InLeapYear(t);
        //        default:
        //            throw new MacheteException();
        //    }
        //}

        //internal static double WeekDay(double t)
        //{
        //    return (Day(t) + 4.0) % 7.0;
        //}

        //internal static double DaylightSavingTA(double t)
        //{
        //    if (t < long.MinValue || t > long.MaxValue)
        //    {
        //        return 0.0;
        //    }

        //    var rules = TimeZoneInfo.Local.GetAdjustmentRules();
        //    if (rules == null || rules.Length == 0)
        //    {
        //        return 0.0;
        //    }

        //    var rule = rules[0];
        //    var start = rule.DaylightTransitionStart;
        //    var end = rule.DaylightTransitionEnd;
        //    var month = (int)MonthFromTime(t);
        //    var day = (int)DateFromTime(t);
        //    var hour = (int)HourFromTime(t);
        //    var minute = (int)MinFromTime(t);
        //    var inMonth = start.Month <= month && end.Month >= month;
        //    var inDay = start.Day <= day && end.Day >= day;
        //    var inHour = start.TimeOfDay.Hour <= hour && end.TimeOfDay.Hour <= hour;
        //    var inMinute = start.TimeOfDay.Minute <= minute && end.TimeOfDay.Minute <= minute;
        //    return inMonth && inDay && inHour && inMinute ? rule.DaylightDelta.TotalMilliseconds : 0.0;
        //}

        //internal static double LocalTime(double t)
        //{
        //    return t + TimeZoneInfo.Local.BaseUtcOffset.TotalMilliseconds + DaylightSavingTA(t);
        //}

        //internal static double UTC(double t)
        //{
        //    var offset = TimeZoneInfo.Local.BaseUtcOffset.TotalMilliseconds;
        //    return t - offset - DaylightSavingTA(t - offset);
        //}

        //internal static double HourFromTime(double t)
        //{
        //    return Math.Floor(t / MsPerHour) % HoursPerDay;
        //}

        //internal static double MinFromTime(double t)
        //{
        //    return Math.Floor(t / MsPerMinute) % MinutesPerHour;
        //}

        //internal static double SecFromTime(double t)
        //{
        //    return Math.Floor(t / MsPerSecond) % SecondsPerMinute;
        //}

        //internal static double MsFromTime(double t)
        //{
        //    return t % MsPerSecond;
        //}

        //internal static double MakeTime(double hour, double min, double sec, double ms)
        //{
        //    if (double.IsInfinity(hour) || double.IsInfinity(min) || double.IsInfinity(sec) || double.IsInfinity(ms))
        //    {
        //        return double.NaN;
        //    }
        //    else
        //    {
        //        return ((long)hour * MsPerHour) + ((long)min * MsPerMinute) + ((long)sec * MsPerSecond) + (long)ms;
        //    }
        //}

        //internal static double MakeDay(double year, double month, double date)
        //{
        //    if (double.IsInfinity(year) || double.IsInfinity(month) || double.IsInfinity(date))
        //    {
        //        return double.NaN;
        //    }
        //    else
        //    {
        //        var y = (double)(long)year;
        //        var m = (double)(long)month;
        //        var dt = (double)(long)date;


        //        if (m < 0 || m > 11)
        //        {
        //            return double.NaN;
        //        }
        //        else if (dt < 1 || dt > 31)
        //        {
        //            return double.NaN;
        //        }

        //        var ym = y + Math.Floor(m / 12);
        //        var mn = m % 12.0;
        //        var t = 1.0;

        //        while (YearFromTime(t) != ym && MonthFromTime(t) != mn && DateFromTime(t) != 1)
        //        {
        //            t++;
        //        }

        //        return t;
        //    }
        //}

        //internal static double MakeDate(double day, double time)
        //{
        //    if (double.IsInfinity(day) || double.IsInfinity(time))
        //    {
        //        return double.NaN;
        //    }
        //    else
        //    {
        //        return day * MsPerDay + time;
        //    }
        //}

        //internal static double TimeClip(double time)
        //{
        //    if (double.IsInfinity(time) || Math.Abs(time) > 8.64e15)
        //    {
        //        return double.NaN;
        //    }
        //    return (double)(long)time + (+0);
        //}
    }
}
