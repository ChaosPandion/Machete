using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Linq.Expressions;

namespace Machete.Tests
{
    public abstract class TestBase
    {
        private static readonly long _negativeZeroBits = BitConverter.DoubleToInt64Bits(-0.0);
        protected readonly Engine Engine = new Engine();

        protected void ExpectBoolean(string script, bool expected)
        {
            var result = Engine.ExecuteScript(script);
            Assert.IsType<bool>(result);
            var actual = (bool)result;
            Assert.Equal(expected, actual);
        }

        protected void ExpectDouble(string script, double expected)
        {
            var result = Engine.ExecuteScript(script);
            Assert.IsType<double>(result);
            var actual = (double)result;
            Assert.Equal(expected, actual);
        }

        protected void ExpectNaN(string script)
        {
            var result = Engine.ExecuteScript(script);
            Assert.IsType<double>(result);
            var actual = (double)result;
            Assert.True(double.IsNaN(actual), "The value NaN was expected.");
        }

        protected void ExpectPositiveZero(string script)
        {
            var result = Engine.ExecuteScript(script);
            Assert.IsType<double>(result);
            var actual = (double)result;
            Assert.True(actual == 0.0 && !IsNegativeZero(actual), "The value +0 was expected.");
        }

        protected void ExpectNegativeZero(string script)
        {
            var result = Engine.ExecuteScript(script);
            Assert.IsType<double>(result);
            var actual = (double)result;
            Assert.True(IsNegativeZero(actual), "The value -0 was expected.");
        }

        protected void ExpectPositiveInfinity(string script)
        {
            var result = Engine.ExecuteScript(script);
            Assert.IsType<double>(result);
            var actual = (double)result;
            Assert.True(double.IsPositiveInfinity(actual), "The value +∞ was expected.");
        }

        protected void ExpectNegativeInfinity(string script)
        {
            var result = Engine.ExecuteScript(script);
            Assert.IsType<double>(result);
            var actual = (double)result;
            Assert.True(double.IsNegativeInfinity(actual), "The value -∞ was expected.");
        }

        protected void ExpectString(string script, string expected)
        {
            var result = Engine.ExecuteScript(script);
            Assert.IsType<string>(result);
            var actual = (string)result;
            Assert.Equal(expected, actual);
        }

        private static bool IsNegativeZero(double x)
        {
            return BitConverter.DoubleToInt64Bits(x) == _negativeZeroBits;
        }
    }
}