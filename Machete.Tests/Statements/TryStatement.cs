using System;
using Xunit;

namespace Machete.Tests
{
    public class TryStatement : TestBase
    {
        [Fact(DisplayName = "12.14 The try Statement -> Try-Catch")]
        public void Test1214A()
        {
            const string script = @"
                var s = '';
                try {
                    s = 'A';
                    throw 'B';
                } catch (e) {
                    s = e;
                }
                return s;
            ";
            Assert.Equal("B", (string)Engine.ExecuteScript(script));
            Assert.True((bool)Engine.ExecuteScript("delete s"));
        }

        [Fact(DisplayName = "12.14 The try Statement -> Try-Finally")]
        public void Test1214B()
        {
            const string script = @"
                var s = 'A';
                try {
                    throw 'Fail!';
                } finally {
                    s = 'B';
                }
            ";
            Assert.IsAssignableFrom<Exception>(Engine.ExecuteScript(script));
            Assert.Equal("B", (string)Engine.ExecuteScript("s"));
            Assert.True((bool)Engine.ExecuteScript("delete s"));
        }

        [Fact(DisplayName = "12.14 The try Statement -> Try-Catch-Finally")]
        public void Test1214C()
        {
            const string script = @"
                var s = 'A';
                try {
                    throw 'B';
                } catch (e) {
                    s += e;
                } finally {
                    s += 'C';
                }
                return s;
            ";
            Assert.Equal("ABC", (string)Engine.ExecuteScript(script));
            Assert.True((bool)Engine.ExecuteScript("delete s"));
        }
    }
}