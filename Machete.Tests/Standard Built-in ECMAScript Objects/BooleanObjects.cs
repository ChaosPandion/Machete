using System;
using Xunit;

namespace Machete.Tests
{
    public class BooleanObjects : TestBase
    {
        [Fact(DisplayName = "15.6.1.1 Boolean (value)")]
        public void Test15611()
        {
            Assert.Equal("boolean", (string)Engine.ExecuteScript("typeof Boolean(true)"));
        }

        [Fact(DisplayName = "15.6.2.1 new Boolean (value)")]
        public void Test15621()
        {
            Assert.IsNotType<Exception>(Engine.ExecuteScript("var o = new Boolean(true);"));
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof o"));
            Assert.True((bool)Engine.ExecuteScript("!!o"));
            Assert.True((bool)Engine.ExecuteScript("delete o"));
        }

        [Fact(DisplayName = "15.6.3.1 Boolean.prototype")]
        public void Test15631()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof Boolean.prototype"));
        }

        [Fact(DisplayName = "15.6.4.1 Boolean.prototype.constructor")]
        public void Test15641()
        {
            Assert.True((bool)Engine.ExecuteScript("Boolean.prototype.constructor === Boolean"));
        }

        [Fact(DisplayName = "15.6.4.2 Boolean.prototype.toString ( )")]
        public void Test15642()
        {
            Assert.Equal("true", (string)Engine.ExecuteScript("new Boolean(true).toString()"));
            Assert.Equal("false", (string)Engine.ExecuteScript("new Boolean(false).toString()"));
        }

        [Fact(DisplayName = "15.6.4.3 Boolean.prototype.valueOf ( )")]
        public void Test15643()
        {
            Assert.True((bool)Engine.ExecuteScript("new Boolean(true).valueOf()"));
            Assert.False((bool)Engine.ExecuteScript("new Boolean(false).valueOf()"));
        }
    }
}