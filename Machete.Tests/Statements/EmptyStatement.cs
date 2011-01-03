using System;
using Xunit;

namespace Machete.Tests
{
    public class EmptyStatement : TestBase
    {
        [Fact(DisplayName = "12.3 Empty Statement")]
        public void Test123()
        {
            const string script = @";";
            Assert.Equal("undefined", (string)Engine.ExecuteScript(script));
        }
    }
}