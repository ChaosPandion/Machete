using System;
using Xunit;

namespace Machete.Tests
{
    public class AdditiveOperators : TestBase
    {
        [Fact(DisplayName = "11.6.1 The Addition operator ( + )")]
        public void Test1161()
        {
            Assert.Equal("1A", (string)Engine.ExecuteScript("1 + 'A'"));
            Assert.Equal(2.0, (double)Engine.ExecuteScript("1 + 1"));
        }

        [Fact(DisplayName = "11.6.2 The Subtraction Operator ( - )")]
        public void Test1162()
        {
            Assert.Equal(0.0, (double)Engine.ExecuteScript("1 - 1"));
        }
    }
}