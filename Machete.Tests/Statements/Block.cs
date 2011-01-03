using System;
using Xunit;

namespace Machete.Tests
{
    public class Block : TestBase
    {
        [Fact(DisplayName = "12.1 Block")]
        public void Test121A()
        {
            var script = @"
                { }
            ";
            Assert.Equal("undefined", (string)Engine.ExecuteScript(script));
        }
    }
}