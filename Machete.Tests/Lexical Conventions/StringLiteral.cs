using System;
using Xunit;
using Machete.Core;

namespace Machete.Tests
{
    public sealed class StringLiteral : TestBase
    {
        [Fact(DisplayName = "07.8.4 String Literals: Simple")]
        public void Test0784A()
        {
            Assert.Equal("test", (string)Engine.ExecuteScript("\"test\""));
            Assert.Equal("test", (string)Engine.ExecuteScript("'test'"));
        }

        [Fact(DisplayName = "07.8.4 String Literals: LineContinuation")]
        public void Test0784B()
        {
            Assert.Equal("", (string)Engine.ExecuteScript("\"\\\r\""));
            Assert.Equal("", (string)Engine.ExecuteScript("'\\\r'"));
        }

        [Fact(DisplayName = "07.8.4 String Literals: '\\0'")]
        public void Test0784C()
        {
            Assert.Equal("\u0000", (string)Engine.ExecuteScript("\"\\0\""));
            Assert.Equal("\u0000", (string)Engine.ExecuteScript("'\\0'"));
        }

        [Fact(DisplayName = "07.8.4 String Literals: SingleEscapeCharacter")]
        public void Test0784D()
        {
            Assert.Equal("'\"\\\b\f\n\r\t\v", (string)Engine.ExecuteScript("\"\\'\\\"\\\\\\b\\f\\n\\r\\t\\v\""));
            Assert.Equal("'\"\\\b\f\n\r\t\v", (string)Engine.ExecuteScript("'\\'\\\"\\\\\\b\\f\\n\\r\\t\\v'"));
        }

        [Fact(DisplayName = "07.8.4 String Literals: HexEscapeSequence")]
        public void Test0784E()
        {
            Assert.Equal("\u0001", (string)Engine.ExecuteScript("\"\\x01\""));
            Assert.Equal("\u0001", (string)Engine.ExecuteScript("'\\x01'"));
            Assert.Equal("\u00AF", (string)Engine.ExecuteScript("\"\\xAF\""));
            Assert.Equal("\u00AF", (string)Engine.ExecuteScript("'\\xAF'"));
        }

        [Fact(DisplayName = "07.8.4 String Literals: UnicodeEscapeSequence")]
        public void Test0784F()
        {
            Assert.Equal("\u000A", (string)Engine.ExecuteScript("\"\\u000A\""));
            Assert.Equal("\u000A", (string)Engine.ExecuteScript("'\\u000A'"));
            Assert.Equal("\u100f", (string)Engine.ExecuteScript("\"\\u100f\""));
            Assert.Equal("\u100f", (string)Engine.ExecuteScript("'\\u100f'"));
        }

        [Fact(DisplayName = "07.8.4 String Literals: Cannot Contain LineTerminator")]
        public void Test0784G()
        {
            Assert.IsAssignableFrom<MacheteRuntimeException>(Engine.ExecuteScript("\"\n\""));
            Assert.IsAssignableFrom<MacheteRuntimeException>(Engine.ExecuteScript("'\n'"));
        }
    }
}