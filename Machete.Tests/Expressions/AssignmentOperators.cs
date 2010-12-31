using System;
using Xunit;

namespace Machete.Tests
{
    public class AssignmentOperators : TestBase
    {
        [Fact(DisplayName = "11.13 Assignment Operators -> Simple")]
        public void Test1113A()
        {
            Assert.Equal(2.0, (double)Engine.ExecuteScript("var x = 1; x = 2;"));
            Assert.True((bool)Engine.ExecuteScript("delete x;"));
        }

        [Fact(DisplayName = "11.13 Assignment Operators -> Compound Multiplication")]
        public void Test1113B()
        {
            Assert.Equal(4.0, (double)Engine.ExecuteScript("var x = 2; x *= 2;"));
            Assert.True((bool)Engine.ExecuteScript("delete x;"));
        }

        [Fact(DisplayName = "11.13 Assignment Operators -> Compound Division")]
        public void Test1113C()
        {
            Assert.Equal(0.5, (double)Engine.ExecuteScript("var x = 1; x /= 2;"));
            Assert.True((bool)Engine.ExecuteScript("delete x;"));
        }

        [Fact(DisplayName = "11.13 Assignment Operators -> Compound Modulus")]
        public void Test1113D()
        {
            Assert.Equal(0.0, (double)Engine.ExecuteScript("var x = 10; x %= 2;"));
            Assert.True((bool)Engine.ExecuteScript("delete x;"));
        }

        [Fact(DisplayName = "11.13 Assignment Operators -> Compound Addition")]
        public void Test1113E()
        {
            Assert.Equal(12.0, (double)Engine.ExecuteScript("var x = 10; x += 2;"));
            Assert.True((bool)Engine.ExecuteScript("delete x;"));
        }

        [Fact(DisplayName = "11.13 Assignment Operators -> Compound Subtraction")]
        public void Test1113F()
        {
            Assert.Equal(8.0, (double)Engine.ExecuteScript("var x = 10; x -= 2;"));
            Assert.True((bool)Engine.ExecuteScript("delete x;"));
        }

        [Fact(DisplayName = "11.13 Assignment Operators -> Compound Left Shift")]
        public void Test1113G()
        {
            Assert.Equal(16.0, (double)Engine.ExecuteScript("var x = 4; x <<= 2;"));
            Assert.True((bool)Engine.ExecuteScript("delete x;"));
        }

        [Fact(DisplayName = "11.13 Assignment Operators -> Compound Signed Right Shift")]
        public void Test1113H()
        {
            Assert.Equal(-536870912.0, (double)Engine.ExecuteScript("var x = 2147483648; x >>= 2;"));
            Assert.True((bool)Engine.ExecuteScript("delete x;"));
        }

        [Fact(DisplayName = "11.13 Assignment Operators -> Compound Unsigned Right Shift")]
        public void Test1113I()
        {
            Assert.Equal(536870912.0, (double)Engine.ExecuteScript("var x = 2147483648; x >>>= 2;"));
            Assert.True((bool)Engine.ExecuteScript("delete x;"));
        }

        [Fact(DisplayName = "11.13 Assignment Operators -> Compound Bitwise AND")]
        public void Test1113J()
        {
            Assert.Equal(2.0, (double)Engine.ExecuteScript("var x = 3; x &= 2;"));
            Assert.True((bool)Engine.ExecuteScript("delete x;"));
        }

        [Fact(DisplayName = "11.13 Assignment Operators -> Compound Bitwise XOR")]
        public void Test1113K()
        {
            Assert.Equal(1 ^ 2, (double)Engine.ExecuteScript("var x = 1; x ^= 2;"));
            Assert.True((bool)Engine.ExecuteScript("delete x;"));
        }

        [Fact(DisplayName = "11.13 Assignment Operators -> Compound Bitwise OR")]
        public void Test1113L()
        {
            Assert.Equal(3.0, (double)Engine.ExecuteScript("var x = 1; x |= 2;"));
            Assert.True((bool)Engine.ExecuteScript("delete x;"));
        }
    }
}