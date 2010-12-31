using System;
using Xunit;

namespace Machete.Tests
{
    public class MultiplicativeOperators : TestBase
    {
        [Fact(DisplayName = "11.5.1 Applying the * Operator")]
        public void Test1151()
        {
            Assert.Equal(2.0, (double)Engine.ExecuteScript("1 * 2"));
        }

        [Fact(DisplayName = "11.5.2 Applying the / Operator")]
        public void Test1152()
        {
            Assert.Equal(0.5, (double)Engine.ExecuteScript("1 / 2"));
        }

        [Fact(DisplayName = "11.5.3 Applying the % Operator")]
        public void Test1153()
        {
            Assert.Equal(0.0, (double)Engine.ExecuteScript("10 % 2"));
        }
    }
}