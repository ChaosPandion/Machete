using System;
using Xunit;

namespace Machete.Tests
{
    public class ConditionalOperator : TestBase
    {
        [Fact(DisplayName = "11.12 Conditional Operator ( ? : )")]
        public void Test1112()
        {
            Assert.True((bool)Engine.ExecuteScript("true ? true : false"));
        }
    }
}