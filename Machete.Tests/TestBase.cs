using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Linq.Expressions;
using FParsec;
using Microsoft.FSharp.Core;

namespace Machete.Tests
{
    public abstract class TestBase
    {
        private static readonly long _negativeZeroBits = BitConverter.DoubleToInt64Bits(-0.0);
        protected readonly Engine Engine = new Engine();

        protected void Expect<T>(string script)
        {
            var result = Engine.ExecuteScript(script);
            Assert.IsAssignableFrom<T>(result);
        }

        protected void Expect<T>(string script, T expected)
        {
            var result = Engine.ExecuteScript(script);
            Assert.IsAssignableFrom<T>(result);
            var actual = (T)result;
            Assert.Equal<T>(expected, actual);
        }

        protected void ExpectTrue(string script)
        {
            var result = Engine.ExecuteScript(script);
            Assert.IsType<bool>(result);
            var value = (bool)result;
            Assert.True(value);
        }

        protected void ExpectFalse(string script)
        {
            var result = Engine.ExecuteScript(script);
            Assert.IsType<bool>(result);
            var value = (bool)result;
            Assert.False(value);
        }

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

        public T RunParser<T>(Func<State<Unit>, Primitives.Reply<T, Unit>> parser, string text)
        {
            var f = FuncConvert.ToFSharpFunc<State<Unit>, Primitives.Reply<T, Unit>>(s => parser(s));
            var r = CharParsers.run<T>(f, text);
            if (r.IsSuccess)
                return ((CharParsers.ParserResult<T, Unit>.Success)r).Item1;
            return default(T);
        }

        internal static bool IsNegativeZero(double x)
        {
            return BitConverter.DoubleToInt64Bits(x) == _negativeZeroBits;
        }
    }
}