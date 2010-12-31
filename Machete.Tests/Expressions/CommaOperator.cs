using System;
using Xunit;

namespace Machete.Tests
{
    public class CommaOperator : TestBase
    {
        [Fact(DisplayName = "11.14 Comma Operator ( , )")]
        public void Test1114()
        {
            Assert.True((bool)Engine.ExecuteScript("var x = false; x = true, false; x;"));
        }
    }
}