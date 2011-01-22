using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Machete.Tests
{
    public abstract class TestBase
    {
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

        protected void ExpectString(string script, string expected)
        {
            var result = Engine.ExecuteScript(script);
            Assert.IsType<string>(result);
            var actual = (string)result;
            Assert.Equal(expected, actual);
        }
    }
}