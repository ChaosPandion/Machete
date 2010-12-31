using System;
using Xunit;

namespace Machete.Tests
{
    public class BinaryLogicalOperators : TestBase
    {
        [Fact(DisplayName = "11.11 Binary Logical Operators  - AND")]
        public void Test1111AND()
        {
            Assert.False((bool)Engine.ExecuteScript("false && true"));
        }

        [Fact(DisplayName = "11.11 Binary Logical Operators  - OR")]
        public void Test1111OR()
        {
            Assert.True((bool)Engine.ExecuteScript("false || true"));
        }
    }
}