using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CDate : BuiltinConstructor
    {
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
            throw new NotImplementedException();
        }

        public override IObject Construct(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        internal static IDynamic Parse(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
            //var @string = args[0].ToStringPrimitive().ToString();
            //var parser = new DateParser(@string);
            //var result = parser.Parse();
            //return new NumberPrimitive(result);
        }

        internal static IDynamic Utc(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
            //var count = args.Count;
            //if (count < 2)
            //{
            //    return NumberPrimitive.Zero;
            //}

            //var y = args[0].ToNumberPrimitive().Value;
            //var m = args[1].ToNumberPrimitive().Value;
            //var dt = count > 3 ? args[2].ToNumberPrimitive().Value : 1D;
            //var h = count > 4 ? args[3].ToNumberPrimitive().Value : 0D;
            //var min = count > 5 ? args[4].ToNumberPrimitive().Value : 0D;
            //var s = count > 6 ? args[5].ToNumberPrimitive().Value : 0D;
            //var milli = count > 7 ? args[6].ToNumberPrimitive().Value : 0D;
            //var yr = y;

            //if (!double.IsNaN(y) && y >= 0.0 && y <= 99.0)
            //{
            //    y = 1900.0 + (double)(long)y;
            //}

            //return new NumberPrimitive(
            //    DatePrototype.TimeClip(
            //        DatePrototype.MakeDate(
            //            DatePrototype.MakeDay(yr, m, dt),
            //            DatePrototype.MakeTime(h, min, s, milli)
            //        )
            //    )
            //);
        }

        internal static IDynamic Now(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
            //return Construct(new Args(new NumberPrimitive((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds)));
        }
    }
}