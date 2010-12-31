using System;
using Xunit;

namespace Machete.Tests
{
    public class BinaryBitwiseOperators : TestBase
    {
        [Fact(DisplayName = "11.10 Binary Bitwise Operators - AND")]
        public void Test1110AND()
        {
            Assert.Equal(10 & 2, (double)Engine.ExecuteScript("10 & 2"));
        }

        [Fact(DisplayName = "11.10 Binary Bitwise Operators - XOR")]
        public void Test1110XOR()
        {
            Assert.Equal(10 ^ 2, (double)Engine.ExecuteScript("10 ^ 2"));
        }

        [Fact(DisplayName = "11.10 Binary Bitwise Operators - OR")]
        public void Test1110OR()
        {
            Assert.Equal(10 | 2, (double)Engine.ExecuteScript("10 | 2"));
        }
    }
}