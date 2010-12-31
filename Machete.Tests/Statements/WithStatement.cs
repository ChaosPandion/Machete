using System;
using Xunit;

namespace Machete.Tests
{
    public class WithStatement : TestBase
    {
        [Fact(DisplayName = "12.10 The with Statement -> with ( Expression ) Statement")]
        public void Test1210()
        {
            var script = @"
                var n = 'C', r = 'A';
                with ({ n: 'B' }) {
                    r += n;
                }
                return r + n;
            ";
            Assert.Equal("ABC", (string)Engine.ExecuteScript(script));
        }

        [Fact(DisplayName = "12.10.1 Strict Mode Restrictions")]
        public void Test12101()
        {
            var script = @"
                'use strict';
                with (Object) {

                }
            ";
            var r = Engine.ExecuteScript(script);
            Assert.IsAssignableFrom<Exception>(r);
        }
    }
}