using System;
using Xunit;

namespace Machete.Tests
{
    public class LeftHandSideExpressions : TestBase
    {
        [Fact(DisplayName = "11.2.1 Property Accessors -> Dot Notation")]
        public void Test1121A()
        {
            Assert.Equal(1.0, (double)Engine.ExecuteScript("this.Object.length"));
        }

        [Fact(DisplayName = "11.2.1 Property Accessors -> Bracket Notation")]
        public void Test1121B()
        {
            Assert.Equal(1.0, (double)Engine.ExecuteScript("this['Object']['length']"));
        }

        [Fact(DisplayName = "11.2.1 Property Accessors -> Mixed Notation")]
        public void Test1121C()
        {
            Assert.Equal(1.0, (double)Engine.ExecuteScript("this['Object'].length"));
        }

        [Fact(DisplayName = "11.2.2 The new Operator")]
        public void Test1122()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof new Object()"));
        }

        [Fact(DisplayName = "11.2.3 Function Calls")]
        public void Test1123()
        {
            Assert.Equal(100.0, (double)Engine.ExecuteScript("parseInt('100', 10);"));
        }

        [Fact(DisplayName = "11.2.4 Argument Lists")]
        public void Test1124()
        {
            Assert.Equal("1,2,3,4,5", (string)Engine.ExecuteScript("new Array(1, 2, 3, 4, 5)"));
            Assert.Equal("1,2,3,4,5", (string)Engine.ExecuteScript("new Array ( 1, 2, 3, 4, 5 )  "));
        }

        [Fact(DisplayName = "11.2.5 Function Expressions")]
        public void Test1125()
        {
            Assert.Equal("Param = 1", (string)Engine.ExecuteScript("(function(a) { return 'Param = ' + a; })(1);"));
        }
    }
}