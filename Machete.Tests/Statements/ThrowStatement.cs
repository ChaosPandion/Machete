using System;
using Xunit;
using Machete.Core;

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
            Assert.IsAssignableFrom<MacheteRuntimeException>(Engine.ExecuteScript(script));
        }
    }
}