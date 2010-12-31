using System;
using Xunit;

namespace Machete.Tests
{
    public class PostfixExpressions : TestBase
    {
        [Fact(DisplayName = "11.3.1 Postfix Increment Operator")]
        public void Test1131()
        {
            Assert.Equal(3.0, (double)Engine.ExecuteScript("var x = 1; x++ + x;"));
        }

        [Fact(DisplayName = "11.3.2 Postfix Decrement Operator")]
        public void Test1132()
        {
            Assert.Equal(5.0, (double)Engine.ExecuteScript("var x = 3; x-- + x;"));
        }
    }
}