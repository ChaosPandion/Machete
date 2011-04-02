using System;
using Machete.Parser;
using Microsoft.FSharp.Core;
using Xunit;

namespace Machete.Tests.Specification.Tests_07_Lexical_Conventions
{
    public class Tests_07_03_LineTerminators : TestBase
    {
        [Fact(DisplayName = "07.03 Line Terminators: LF")]
        public void Test0703A()
        {
            var r = RunParser(InputElementParsers.parseLineTerminator<Unit>, "\u000A");
            Assert.True(r.IsLineTerminator, "The LF character was not parsed as a Line Terminator.");
        }

        [Fact(DisplayName = "07.03 Line Terminators: CR")]
        public void Test0703B()
        {
            var r = RunParser(InputElementParsers.parseLineTerminator<Unit>, "\u000D");
            Assert.True(r.IsLineTerminator, "The CR character was not parsed as a Line Terminator.");
        }

        [Fact(DisplayName = "07.03 Line Terminators: LS")]
        public void Test0703C()
        {
            var r = RunParser(InputElementParsers.parseLineTerminator<Unit>, "\u2028");
            Assert.True(r.IsLineTerminator, "The LS character was not parsed as a Line Terminator.");
        }

        [Fact(DisplayName = "07.03 Line Terminators: PS")]
        public void Test0703D()
        {
            var r = RunParser(InputElementParsers.parseLineTerminator<Unit>, "\u2029");
            Assert.True(r.IsLineTerminator, "The PS character was not parsed as a Line Terminator.");
        }
    }
}