using System;
using Xunit;

namespace Machete.Tests
{
    public class UnaryOperators : TestBase
    {
        [Fact(DisplayName = "11.4.1 The delete Operator")]
        public void Test1141()
        {
            Assert.True((bool)Engine.ExecuteScript("delete 2"));
            Assert.True((bool)Engine.ExecuteScript("var o = {}; delete o"));
        }

        [Fact(DisplayName = "11.4.2 The void Operator")]
        public void Test1142()
        {
            Assert.Equal("undefined", (string)Engine.ExecuteScript("typeof (void 1)"));
        }

        [Fact(DisplayName = "11.4.3 The typeof Operator")]
        public void Test1143()
        {
            Assert.Equal("undefined", (string)Engine.ExecuteScript("typeof undefined"));
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof null"));
            Assert.Equal("boolean", (string)Engine.ExecuteScript("typeof true"));
            Assert.Equal("number", (string)Engine.ExecuteScript("typeof 0"));
            Assert.Equal("string", (string)Engine.ExecuteScript("typeof ''"));
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof {}"));
            Assert.Equal("function", (string)Engine.ExecuteScript("typeof function(){}"));
        }

        [Fact(DisplayName = "11.4.4 Prefix Increment Operator")]
        public void Test1144()
        {
            Assert.Equal(2.0, (double)Engine.ExecuteScript("var x = 1; ++x;"));
        }

        [Fact(DisplayName = "11.4.5 Prefix Decrement Operator")]
        public void Test1145()
        {
            Assert.Equal(2.0, (double)Engine.ExecuteScript("var x = 3; --x;"));
        }

        [Fact(DisplayName = "11.4.6 Unary + Operator")]
        public void Test1146()
        {
            Assert.Equal(1.0, (double)Engine.ExecuteScript("+true"));
        }

        [Fact(DisplayName = "11.4.7 Unary - Operator")]
        public void Test1147()
        {
            Assert.Equal(-1.0, (double)Engine.ExecuteScript("-1"));
        }

        [Fact(DisplayName = "11.4.8 Bitwise NOT Operator ( ~ )")]
        public void Test1148()
        {
            Assert.Equal(~16, (double)Engine.ExecuteScript("~16"));
        }

        [Fact(DisplayName = "11.4.9 Logical NOT Operator ( ! )")]
        public void Test1149()
        {
            Assert.True((bool)Engine.ExecuteScript("!false"));
        }
    }
}