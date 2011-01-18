using System;
using Xunit;
using Machete.Core;

namespace Machete.Tests
{
    public sealed class NumericLiteral : TestBase
    {
        [Fact(DisplayName = "7.8.3 Numeric Literals: DecimalLiteral :: DecimalIntegerLiteral")]
        public void Test783A()
        {
            Assert.Equal(999.0, (double)Engine.ExecuteScript("999"));
        }

        [Fact(DisplayName = "7.8.3 Numeric Literals: DecimalLiteral :: DecimalIntegerLiteral .")]
        public void Test783B()
        {
            Assert.Equal(999.0, (double)Engine.ExecuteScript("999."));
        }

        [Fact(DisplayName = "7.8.3 Numeric Literals: DecimalLiteral :: DecimalIntegerLiteral . DecimalDigits")]
        public void Test783C()
        {
            Assert.Equal(999.222, (double)Engine.ExecuteScript("999.222"));
        }

        [Fact(DisplayName = "7.8.3 Numeric Literals: DecimalLiteral :: DecimalIntegerLiteral . ExponentPart")]
        public void Test783D()
        {
            Assert.Equal(999.0e2, (double)Engine.ExecuteScript("999.e2"));
        }

        [Fact(DisplayName = "7.8.3 Numeric Literals: DecimalLiteral :: DecimalIntegerLiteral . DecimalDigits ExponentPart")]
        public void Test783E()
        {
            Assert.Equal(999.222e2, (double)Engine.ExecuteScript("999.222e2"));
        }

        [Fact(DisplayName = "7.8.3 Numeric Literals: DecimalLiteral :: . DecimalDigits")]
        public void Test783F()
        {
            Assert.Equal(.222, (double)Engine.ExecuteScript(".222"));
        }

        [Fact(DisplayName = "7.8.3 Numeric Literals: DecimalLiteral :: . DecimalDigits ExponentPart")]
        public void Test783G()
        {
            Assert.Equal(.222e2, (double)Engine.ExecuteScript(".222e2"));
        }

        [Fact(DisplayName = "7.8.3 Numeric Literals: DecimalLiteral :: DecimalIntegerLiteral ExponentPart")]
        public void Test783H()
        {
            Assert.Equal(999e2, (double)Engine.ExecuteScript("999e2"));
        }

        [Fact(DisplayName = "7.8.3 Numeric Literals: HexIntegerLiteral")]
        public void Test783I()
        {
            Assert.Equal(0xFFF, (double)Engine.ExecuteScript("0xFFF"));
            Assert.Equal(0xFFF, (double)Engine.ExecuteScript("0XFFF"));
            Assert.Equal(0x12A, (double)Engine.ExecuteScript("0x12A"));
        }

        [Fact(DisplayName = "7.8.3 Numeric Literals: No Trailing IdentifierStart Or DecimalDigit")]
        public void Test783J()
        {
            Assert.IsAssignableFrom<MacheteRuntimeException>(Engine.ExecuteScript("3in"));
        }
    }
}