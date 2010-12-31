using System;
using Xunit;

namespace Machete.Tests
{
    public class RelationalOperators : TestBase
    {
        [Fact(DisplayName = "11.8.1 The Less-than Operator ( < )")]
        public void Test1181()
        {
            Assert.True((bool)Engine.ExecuteScript("1 < 2"));
        }

        [Fact(DisplayName = "11.8.2 The Greater-than Operator ( > )")]
        public void Test1182()
        {
            Assert.True((bool)Engine.ExecuteScript("2 > 1"));
        }

        [Fact(DisplayName = "11.8.3 The Less-than-or-equal Operator ( <= )")]
        public void Test1183()
        {
            Assert.True((bool)Engine.ExecuteScript("2 <= 2"));
        }

        [Fact(DisplayName = "11.8.4 The Greater-than-or-equal Operator ( >= )")]
        public void Test1184()
        {
            Assert.True((bool)Engine.ExecuteScript("2 >= 2"));
        }

        [Fact(DisplayName = "11.8.6 The instanceof operator")]
        public void Test1186()
        {
            Assert.True((bool)Engine.ExecuteScript("new Object() instanceof Object"));
        }

        [Fact(DisplayName = "11.8.7 The in operator")]
        public void Test1187()
        {
            Assert.True((bool)Engine.ExecuteScript("'create' in Object"));
        }
    }
}