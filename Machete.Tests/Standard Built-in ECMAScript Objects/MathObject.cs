using System;
using Xunit;

namespace Machete.Tests
{
    public class MathObject : TestBase
    {
        [Fact(DisplayName = "15.8.1.1  E")]
        public void Test15811()
        {
            ExpectDouble("Math.E", Math.E);
        }

        [Fact(DisplayName = "15.8.1.2  LN10")]
        public void Test15812()
        {
            ExpectDouble("Math.LN10", Math.Log(10));
        }

        [Fact(DisplayName = "15.8.1.3  LN2")]
        public void Test15813()
        {
            ExpectDouble("Math.LN2", Math.Log(2));
        }

        [Fact(DisplayName = "15.8.1.4  LOG2E")]
        public void Test15814()
        {
            ExpectDouble("Math.LOG2E", Math.Log(Math.E, 2));
        }

        [Fact(DisplayName = "15.8.1.5  LOG10E")]
        public void Test15815()
        {
            ExpectDouble("Math.LOG10E", Math.Log10(Math.E));
        }

        [Fact(DisplayName = "15.8.1.6  PI")]
        public void Test15816()
        {
            ExpectDouble("Math.PI", Math.PI);
        }

        [Fact(DisplayName = "15.8.1.7  SQRT1_2")]
        public void Test15817()
        {
            ExpectDouble("Math.SQRT1_2", Math.Sqrt(0.5));
        }

        [Fact(DisplayName = "15.8.1.7  SQRT2")]
        public void Test15818()
        {
            ExpectDouble("Math.SQRT2", Math.Sqrt(2));
        }

        [Fact(DisplayName = "15.8.2.1  abs (x)")]
        public void Test15821()
        {
            ExpectNaN("Math.abs(NaN)");
            ExpectPositiveZero("Math.abs(-0)");
            ExpectPositiveInfinity("Math.abs(-Infinity)");
        }

        [Fact(DisplayName = "15.8.2.2  acos (x)")]
        public void Test15822()
        {
            ExpectNaN("Math.acos(NaN)");
            ExpectNaN("Math.acos(2)");
            ExpectNaN("Math.acos(-2)");
            ExpectPositiveZero("Math.acos(1)");
        }

        [Fact(DisplayName = "15.8.2.3  asin (x)")]
        public void Test15823()
        {
            ExpectNaN("Math.asin(NaN)");
            ExpectNaN("Math.asin(2)");
            ExpectNaN("Math.asin(-2)");
            ExpectPositiveZero("Math.asin(0)");
            ExpectNegativeZero("Math.asin(-0)");
        }

        [Fact(DisplayName = "15.8.2.15  round (x)")]
        public void Test158215()
        {
            ExpectDouble("Math.round(1.0)", 1.0);
            ExpectDouble("Math.round(1.1)", 1.0);
            ExpectDouble("Math.round(1.25)", 1.0);
            ExpectDouble("Math.round(1.5)", 2.0);
            ExpectDouble("Math.round(1.75)", 2.0);
            ExpectDouble("Math.round(-1.0)", -1.0);
            ExpectDouble("Math.round(-1.1)", -1.0);
            ExpectDouble("Math.round(-1.25)", -1.0);
            ExpectDouble("Math.round(-1.5)", -1.0);
            ExpectDouble("Math.round(-1.75)", -2.0);
        }
    }
}