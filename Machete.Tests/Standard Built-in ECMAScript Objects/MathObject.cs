using System;
using Xunit;

namespace Machete.Tests
{
    public class MathObject : TestBase
    {
        [Fact(DisplayName = "15.08.01.01  E")]
        public void Test15080101()
        {
            ExpectDouble("Math.E", Math.E);
        }

        [Fact(DisplayName = "15.08.01.02  LN10")]
        public void Test15080102()
        {
            ExpectDouble("Math.LN10", Math.Log(10));
        }

        [Fact(DisplayName = "15.08.01.03  LN2")]
        public void Test15080103()
        {
            ExpectDouble("Math.LN2", Math.Log(2));
        }

        [Fact(DisplayName = "15.08.01.04  LOG2E")]
        public void Test15080104()
        {
            ExpectDouble("Math.LOG2E", Math.Log(Math.E, 2));
        }

        [Fact(DisplayName = "15.08.01.05  LOG10E")]
        public void Test15080105()
        {
            ExpectDouble("Math.LOG10E", Math.Log10(Math.E));
        }

        [Fact(DisplayName = "15.08.01.06  PI")]
        public void Test15080106()
        {
            ExpectDouble("Math.PI", Math.PI);
        }

        [Fact(DisplayName = "15.08.01.07  SQRT1_2")]
        public void Test15080107()
        {
            ExpectDouble("Math.SQRT1_2", Math.Sqrt(0.5));
        }

        [Fact(DisplayName = "15.08.01.08  SQRT2")]
        public void Test15080108()
        {
            ExpectDouble("Math.SQRT2", Math.Sqrt(2));
        }

        [Fact(DisplayName = "15.08.02.01  abs (x)")]
        public void Test15080201()
        {
            ExpectNaN("Math.abs(NaN)");
            ExpectPositiveZero("Math.abs(-0)");
            ExpectPositiveInfinity("Math.abs(-Infinity)");
        }

        [Fact(DisplayName = "15.08.02.02  acos (x)")]
        public void Test15080202()
        {
            ExpectNaN("Math.acos(NaN)");
            ExpectNaN("Math.acos(2)");
            ExpectNaN("Math.acos(-2)");
            ExpectPositiveZero("Math.acos(1)");
        }

        [Fact(DisplayName = "15.08.02.03  asin (x)")]
        public void Test15080203()
        {
            ExpectNaN("Math.asin(NaN)");
            ExpectNaN("Math.asin(2)");
            ExpectNaN("Math.asin(-2)");
            ExpectPositiveZero("Math.asin(0)");
            ExpectNegativeZero("Math.asin(-0)");
        }

        [Fact(DisplayName = "15.08.02.04  atan (x)")]
        public void Test15080204()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.08.02.05  atan2 (y, x)")]
        public void Test15080205()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.08.02.06  ceil (x)")]
        public void Test15080206()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.08.02.07  cos (x)")]
        public void Test15080207()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.08.02.08  exp (x)")]
        public void Test15080208()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.08.02.09  floor (x)")]
        public void Test15080209()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.08.02.10  log (x)")]
        public void Test15080210()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.08.02.11  max ( [ value1 [ , value2 [ , … ] ] ] )")]
        public void Test15080211()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.08.02.12  min ( [ value1 [ , value2 [ , … ] ] ] )")]
        public void Test15080212()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.08.02.13  pow (x, y)")]
        public void Test15080213()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.08.02.14  random ()")]
        public void Test15080214()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.08.02.15  round (x)")]
        public void Test15080215()
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

        [Fact(DisplayName = "15.08.02.16  sin (x)")]
        public void Test15080216()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.08.02.17  sqrt (x)")]
        public void Test15080217()
        {
            Assert.True(false);
        }

        [Fact(DisplayName = "15.08.02.18  tan (x)")]
        public void Test15080218()
        {
            Assert.True(false);
        }
    }
}