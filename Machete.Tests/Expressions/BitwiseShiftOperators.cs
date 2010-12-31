using System;
using Xunit;

namespace Machete.Tests
{
    public class BitwiseShiftOperators : TestBase
    {
        [Fact(DisplayName = "11.7.1 The Left Shift Operator ( << )")]
        public void Test1171()
        {
            Assert.Equal(16.0, (double)Engine.ExecuteScript("4 << 2"));
        }

        [Fact(DisplayName = "11.7.2 The Signed Right Shift Operator ( >> )")]
        public void Test1172()
        {
            Assert.Equal(-536870912.0, (double)Engine.ExecuteScript("2147483648 >> 2"));
        }

        [Fact(DisplayName = "11.7.3 The Unsigned Right Shift Operator ( >>> )")]
        public void Test1173()
        {
            Assert.Equal(536870912.0, (double)Engine.ExecuteScript("2147483648 >>> 2"));
        }
    }
}