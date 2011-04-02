using System;
using System.Globalization;
using Machete.Parser;
using Microsoft.FSharp.Core;
using Xunit;

namespace Machete.Tests.Specification.Tests_07_Lexical_Conventions
{
    public class Tests_07_02_WhiteSpace : TestBase
    {
        [Fact(DisplayName = "07.02 White Space: TAB")]
        public void Test0702A()
        {
            var r = RunParser(InputElementParsers.parseWhiteSpace<Unit>, "\u0009");
            Assert.True(r.IsWhiteSpace, "The TAB character was not parsed as a White Space.");
        }

        [Fact(DisplayName = "07.02 White Space: VT")]
        public void Test0702B()
        {
            var r = RunParser(InputElementParsers.parseWhiteSpace<Unit>, "\u000B");
            Assert.True(r.IsWhiteSpace, "The VT character was not parsed as a White Space.");
        }

        [Fact(DisplayName = "07.02 White Space: FF")]
        public void Test0702C()
        {
            var r = RunParser(InputElementParsers.parseWhiteSpace<Unit>, "\u000C");
            Assert.True(r.IsWhiteSpace, "The FF character was not parsed as a White Space.");
        }

        [Fact(DisplayName = "07.02 White Space: SP")]
        public void Test0702D()
        {
            var r = RunParser(InputElementParsers.parseWhiteSpace<Unit>, "\u0020");
            Assert.True(r.IsWhiteSpace, "The SP character was not parsed as a White Space.");
        }

        [Fact(DisplayName = "07.02 White Space: NBSP")]
        public void Test0702E()
        {
            var r = RunParser(InputElementParsers.parseWhiteSpace<Unit>, "\u00A0");
            Assert.True(r.IsWhiteSpace, "The NBSP character was not parsed as a White Space.");
        }

        [Fact(DisplayName = "07.02 White Space: BOM")]
        public void Test0702F()
        {
            var r = RunParser(InputElementParsers.parseWhiteSpace<Unit>, "\uFEFF");
            Assert.True(r.IsWhiteSpace, "The BOM character was not parsed as a White Space.");
        }

        [Fact(DisplayName = "07.02 White Space: USP")]
        public void Test0702G()
        {
            for (char c = char.MinValue; c < char.MaxValue; c++)
            {
                switch (char.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.SpaceSeparator:
                        var r = RunParser(InputElementParsers.parseWhiteSpace<Unit>, "\uFEFF");
                        Assert.True(r.IsWhiteSpace, "The USP character '" + c + "' was not parsed as a White Space.");
                        break;
                }
            }
        }
    }
}