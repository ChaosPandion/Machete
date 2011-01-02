using System;
using Xunit;

namespace Machete.Tests
{
    public class SwitchStatement : TestBase
    {
        [Fact(DisplayName = "12.11 The switch Statement")]
        public void Test129A()
        {
            var script = @"
                var n = 1;
                switch (n) {
                    case 1: 
                        return true;
                    default:
                        return false;
                }
            ";
            Assert.True((bool)Engine.ExecuteScript(script));
            Assert.True((bool)Engine.ExecuteScript("delete n;"));
        }
    }
}