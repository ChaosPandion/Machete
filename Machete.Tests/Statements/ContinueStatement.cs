using System;
using Xunit;

namespace Machete.Tests
{
    public class ContinueStatement : TestBase
    {
        [Fact(DisplayName = "12.7 The continue Statement")]
        public void Test127A()
        {
            var script = @"
                var i = 0, run = true;
                while (run) {
                    i++;
                    if (i === 1)
                        continue;
                    run = false;
                }
                return i;
            ";
            Assert.Equal(2.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.7 The continue Statement -> continue; with no loop or switch")]
        public void Test127B()
        {
            var script = @"
                continue;
            ";
            var r = Engine.ExecuteScript(script);
            Assert.IsAssignableFrom<Exception>(r);
        }

        [Fact(DisplayName = "12.7 The continue Statement -> continue Identifier; missing label")]
        public void Test127C()
        {
            var script = @"
                continue Identifier;
            ";
            var r = Engine.ExecuteScript(script);
            Assert.IsAssignableFrom<Exception>(r);
        }
    }
}