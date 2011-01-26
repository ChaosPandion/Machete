using System;
using Xunit;

namespace Machete.Tests
{
    public class RegExpObjects : TestBase
    {
        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: AAA")]
        public void Test1()
        {
            ExpectString("/AAA/.exec('AAA')", "AAA");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: AAA+")]
        public void Test2()
        {
            ExpectString("/AAA+/.exec('AAAAAA')", "AAAAAA");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: AAA|BBB")]
        public void Test3()
        {
            ExpectString("/AAA|BBB/.exec('BBB')", "BBB");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: (AAA|BBB)*")]
        public void Test4()
        {
            ExpectString("/(AAA|BBB)*/.exec('AAABBB')", "AAABBB,BBB");
        }

    }
}