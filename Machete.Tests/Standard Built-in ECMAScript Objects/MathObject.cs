using System;
using Xunit;

namespace Machete.Tests
{
    public class MathObject : TestBase
    {
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