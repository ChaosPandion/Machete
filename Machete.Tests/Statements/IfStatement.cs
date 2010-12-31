using System;
using Xunit;

namespace Machete.Tests
{
    public class IfStatement : TestBase
    {
        [Fact(DisplayName = "12.5 The if Statement -> if ( Expression ) Statement  else Statement")]
        public void Test125A()
        {
            var script = @"
                if (true) {
                    return true;
                } else {
                    return false;
                }
            ";
            Assert.True((bool)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.5 The if Statement -> if ( Expression ) Statement")]
        public void Test125B()
        {
            var script = @"
                var x = 999;
                if (false) {
                    x += 1;
                }
                return x;
            ";
            Assert.Equal(999.0, (double)Engine.ExecuteScript(script));
        }
    }
}