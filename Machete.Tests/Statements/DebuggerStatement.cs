using System;
using Xunit;

namespace Machete.Tests
{
    public class DebuggerStatement : TestBase
    {
        [Fact(DisplayName = "12.15 The debugger statement")]
        public void Test1215()
        {
            const string script = @"debugger;";
            Assert.Equal("undefined", (string)Engine.ExecuteScript(script));
        }
    }
}