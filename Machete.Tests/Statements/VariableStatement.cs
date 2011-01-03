using System;
using Xunit;

namespace Machete.Tests
{
    public class VariableStatement : TestBase
    {
        [Fact(DisplayName = "12.2 Variable Statement -> Single declaration with no initializer.")]
        public void Test122A()
        {
            Assert.Equal("undefined", (string)Engine.ExecuteScript("var x;"));
            Assert.Equal("undefined", (string)Engine.ExecuteScript("typeof x"));
            Assert.True((bool)Engine.ExecuteScript("delete x"));
        }

        [Fact(DisplayName = "12.2 Variable Statement -> Single declaration with initializer.")]
        public void Test122B()
        {
            Assert.Equal("undefined", (string)Engine.ExecuteScript("var x = 1;"));
            Assert.Equal(1.0, (double)Engine.ExecuteScript("x"));
            Assert.True((bool)Engine.ExecuteScript("delete x"));
        }

        [Fact(DisplayName = "12.2 Variable Statement -> Many declarations with no initializers.")]
        public void Test122C()
        {
            Assert.Equal("undefined", (string)Engine.ExecuteScript("var x, y, z;"));
            Assert.Equal("undefined", (string)Engine.ExecuteScript("typeof x"));
            Assert.Equal("undefined", (string)Engine.ExecuteScript("typeof y"));
            Assert.Equal("undefined", (string)Engine.ExecuteScript("typeof z"));
            Assert.True((bool)Engine.ExecuteScript("delete x"));
            Assert.True((bool)Engine.ExecuteScript("delete y"));
            Assert.True((bool)Engine.ExecuteScript("delete z"));
        }

        [Fact(DisplayName = "12.2 Variable Statement -> Many declarations with initializers.")]
        public void Test122D()
        {
            Assert.Equal("undefined", (string)Engine.ExecuteScript("var x = 1, y = 2, z = 3;"));
            Assert.Equal(1.0, (double)Engine.ExecuteScript("x"));
            Assert.Equal(2.0, (double)Engine.ExecuteScript("y"));
            Assert.Equal(3.0, (double)Engine.ExecuteScript("z"));
            Assert.True((bool)Engine.ExecuteScript("delete x"));
            Assert.True((bool)Engine.ExecuteScript("delete y"));
            Assert.True((bool)Engine.ExecuteScript("delete z"));
        }

        [Fact(DisplayName = "12.2 Variable Statement -> Many declarations with some having initializers.")]
        public void Test122E()
        {
            Assert.Equal("undefined", (string)Engine.ExecuteScript("var x = 1, y, z = 3;"));
            Assert.Equal(1.0, (double)Engine.ExecuteScript("x"));
            Assert.Equal("undefined", (string)Engine.ExecuteScript("typeof y"));
            Assert.Equal(3.0, (double)Engine.ExecuteScript("z"));
            Assert.True((bool)Engine.ExecuteScript("delete x"));
            Assert.True((bool)Engine.ExecuteScript("delete y"));
            Assert.True((bool)Engine.ExecuteScript("delete z"));
        }

        [Fact(DisplayName = "12.2.1 Strict Mode Restrictions")]
        public void Test1221()
        {
            Assert.IsAssignableFrom<Exception>(Engine.ExecuteScript("'use strict'; var eval = 1;"));
            Assert.IsAssignableFrom<Exception>(Engine.ExecuteScript("'use strict'; var arguments = 1;"));
        }
    }
}