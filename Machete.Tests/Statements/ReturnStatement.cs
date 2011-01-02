using System;
using Xunit;

namespace Machete.Tests
{
    public class ReturnStatement : TestBase
    {
        [Fact(DisplayName = "12.9 The return Statement -> With Expression")]
        public void Test129A()
        {
            var script = @"
                function test() { 
                    return 2; 
                }
                return test();
            ";
            Assert.Equal(2.0, (double)Engine.ExecuteScript(script));
            Assert.True((bool)Engine.ExecuteScript("delete test;"));
        }

        [Fact(DisplayName = "12.9 The return Statement -> With No Expression")]
        public void Test129B()
        {
            var script = @"
                function test() { 
                    return; 
                }
                typeof test();
            ";
            Assert.Equal("undefined", (string)Engine.ExecuteScript(script));
            Assert.True((bool)Engine.ExecuteScript("delete test;"));
        }
    }
}