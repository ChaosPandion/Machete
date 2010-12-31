using System;
using Xunit;

namespace Machete.Tests
{
    public class PrimaryExpressions : TestBase
    {
        [Fact(DisplayName = "11.1.1 The this Keyword")]
        public void Test1111()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof this"));
        }

        [Fact(DisplayName = "11.1.2 Identifier Reference")]
        public void Test1112()
        {
            Assert.IsAssignableFrom<Exception>(Engine.ExecuteScript("invalidIdentifier"));
        }

        [Fact(DisplayName = "11.1.3 Literal Reference")]
        public void Test1113()
        {
            Assert.Equal("test", (string)Engine.ExecuteScript("'test'"));
        }

        [Fact(DisplayName = "11.1.4 Array Initialiser")]
        public void Test1114()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof []"));
            Assert.Equal(0.0, (double)Engine.ExecuteScript("[].length"));
        }

        [Fact(DisplayName = "11.1.5 Object Initialiser")]
        public void Test1115()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof {}"));
            Assert.True((bool)Engine.ExecuteScript("{} instanceof Object"));
        }

        [Fact(DisplayName = "11.1.6 The Grouping Operator")]
        public void Test1116()
        {
            Assert.Equal(0.5, (double)Engine.ExecuteScript("1 / (1 + 1)"));
        }
    }
}