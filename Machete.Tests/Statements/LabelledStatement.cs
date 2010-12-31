using System;
using Xunit;

namespace Machete.Tests
{
    public class LabelledStatement : TestBase
    {
        [Fact(DisplayName = "12.12 Labelled Statements  -> break Label;")]
        public void Test1212A()
        {
            var script = @"
                var i = 0, b = true;
                Test: while (b) {
                    while (++i < 10) {
                        break Test;
                    }
                    b = false;
                }
                return i + b;
            ";
            Assert.Equal(2.0, (double)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.12 Labelled Statements  -> continue Label;")]
        public void Test1212B()
        {
            var script = @"
                var i = 0;
                Test: while (++i < 10) {
                    while (true) {
                        continue Test;
                    }
                }
                return i;
            ";
            Assert.Equal(10.0, (double)Engine.ExecuteScript(script));
        }
    }
}