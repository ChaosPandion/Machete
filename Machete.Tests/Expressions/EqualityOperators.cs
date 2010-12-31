using System;
using Xunit;

namespace Machete.Tests
{
    public class EqualityOperators : TestBase
    {
        [Fact(DisplayName = "11.9.1 The Equals Operator ( == )")]
        public void Test1191()
        {
            Assert.True((bool)Engine.ExecuteScript("1 == '1'"));
        }

        [Fact(DisplayName = "11.9.2 The Does-not-equals Operator ( != )")]
        public void Test1192()
        {
            Assert.True((bool)Engine.ExecuteScript("2 != '1'"));
        }

        [Fact(DisplayName = "11.9.4 The Strict Equals Operator ( === )")]
        public void Test1194()
        {
            Assert.True((bool)Engine.ExecuteScript("1 === 1"));
        }

        [Fact(DisplayName = "11.9.5 The Strict Does-not-equal Operator ( !== )")]
        public void Test1195()
        {
            Assert.True((bool)Engine.ExecuteScript("2 !== 1"));
        }
    }
}