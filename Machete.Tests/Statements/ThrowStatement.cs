using System;
using Xunit;

namespace Machete.Tests
{
    public class ThrowStatement : TestBase
    {
        [Fact(DisplayName = "12.13 The throw Statement")]
        public void Test1213()
        {
            var script = @"
                throw 'Error';
            ";
            var r = Engine.ExecuteScript(script);
            Assert.IsAssignableFrom<Exception>(r);
        }
    }
}