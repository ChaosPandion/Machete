using System;
using Xunit;

namespace Machete.Tests
{
    public class BreakStatement : TestBase
    {
        [Fact(DisplayName = "12.8 The break Statement -> break [no LineTerminator here] Identifieropt ;")]
        public void Test128A()
        {
            var script = @"
                var i = 0;
                while (true) {
                    i++;
                    if (i === 10)
                        break;
                }
                return i;
            ";
            Assert.Equal(10.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.8 The break Statement -> break; with no loop or switch")]
        public void Test128B()
        {
            var script = @"
                break;
            ";
            var r = Engine.ExecuteScript(script);
            Assert.IsAssignableFrom<Exception>(r);
        }

        [Fact(DisplayName = "12.8 The break Statement -> break Identifier; missing label")]
        public void Test128C()
        {
            var script = @"
                break Identifier;
            ";
            var r = Engine.ExecuteScript(script);
            Assert.IsAssignableFrom<Exception>(r);
        }
    }
}