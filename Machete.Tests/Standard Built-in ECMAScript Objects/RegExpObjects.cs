using System;
using Xunit;
using Machete.Core;

namespace Machete.Tests
{
    public class RegExpObjects : TestBase
    {
        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /A/")]
        public void PatternCharacter()
        {
            ExpectString("/A/.exec('A')", "A");
        }
        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /./")]
        public void DotAtom()
        {
            ExpectString("/./.exec('A')", "A");
            ExpectString("/./.exec('1')", "1");
            ExpectString("/./.exec('\t')", "\t");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /[a-z]/")]
        public void CharacterClassOneRange()
        {
            ExpectString("/[a-z]/.exec('x')", "x");
            ExpectString("/[a-z]/.exec('1')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /[^a-z]/")]
        public void CharacterClassOneRangeNegate()
        {
            ExpectString("/[^a-z]/.exec('x')", "null");
            ExpectString("/[^a-z]/.exec('1')", "1");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /[a-z0-9]/")]
        public void CharacterClassMultipleRange()
        {
            ExpectString("/[a-z0-9]/.exec('x')", "x");
            ExpectString("/[a-z0-9]/.exec('1')", "1");
            ExpectString("/[a-z0-9]/.exec('_')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /[^a-z0-9]/")]
        public void CharacterClassMultipleRangeNegate()
        {
            ExpectString("/[^a-z0-9]/.exec('x')", "null");
            ExpectString("/[^a-z0-9]/.exec('1')", "null");
            ExpectString("/[^a-z0-9]/.exec('_')", "_");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /[\\b]/")]
        public void BackSpaceClassEscape()
        {
            ExpectString("/[\\b]/.exec('\\u0008')", "\u0008");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /[\\0]/")]
        public void ClassDecimalEscapeA()
        {
            Expect("/[\\0]/.exec('\\u0000')", "\u0000");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /[\\1]/")]
        public void ClassDecimalEscapeB()
        {
            Expect<MacheteRuntimeException>("/[\\1]/.exec('test')");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /(AAA)/")]
        public void TestCapturingGroup()
        {
            ExpectString("/(AAA)/.exec('AAA')", "AAA,AAA");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /(?:AAA)/")]
        public void TestNonCapturingGroup()
        {
            ExpectString("/(?:AAA)/.exec('AAA')", "AAA");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /(AAA)\\0/")]
        public void TestDecimalEscapeA()
        {
            ExpectString("/(AAA)\\0/.exec('AAA\\u0000')", "AAA\u0000,AAA");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /(AAA)\\1/")]
        public void TestDecimalEscapeB()
        {
            ExpectString("/(AAA)\\1/.exec('AAAAAA')", "AAAAAA,AAA");
            ExpectString("/(AAA)\\1/.exec('AAABBB')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /\\t\\n\\v\\f\\r/")]
        public void TestControlEscape()
        {
            ExpectString("/\\t\\n\\v\\f\\r/.exec('\\u0009\\u000A\\u000B\\u000C\\u000D')", "\u0009\u000A\u000B\u000C\u000D");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: IdentityEscape")]
        public void TestIdentityEscape()
        {
            ExpectString(@"/\^/.exec('^')", "^");
            ExpectString(@"/\$/.exec('$')", "$");
            ExpectString(@"/\\/.exec('\\')", "\\");
            ExpectString(@"/\./.exec('.')", ".");
            ExpectString(@"/\*/.exec('*')", "*");
            ExpectString(@"/\+/.exec('+')", "+");
            ExpectString(@"/\?/.exec('?')", "?");
            ExpectString(@"/\(/.exec('(')", "(");
            ExpectString(@"/\)/.exec(')')", ")");
            ExpectString(@"/\[/.exec('[')", "[");
            ExpectString(@"/\]/.exec(']')", "]");
            ExpectString(@"/\{/.exec('{')", "{");
            ExpectString(@"/\}/.exec('}')", "}");
            ExpectString(@"/\|/.exec('|')", "|");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /\\x0D/")]
        public void TestHexEscapeSequence()
        {
            ExpectString("/\\x0D/.exec('\\x0D')", "\x0D");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /\\u000D/")]
        public void TestUnicodeEscapeSequence()
        {
            ExpectString("/\\u000D/.exec('\\u000D')", "\u000D");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /A*/")]
        public void ZeroOrMore()
        {
            ExpectString("/A*/.exec('AAA')", "AAA");
            ExpectString("/A*/.exec('')", "");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /A+/")]
        public void OneOrMore()
        {
            ExpectString("/A+/.exec('AAA')", "AAA");
            ExpectString("/A+/.exec('')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /A?/")]
        public void ZeroOrOne()
        {
            ExpectString("/A?/.exec('A')", "A");
            ExpectString("/A?/.exec('')", "");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /A{3}/")]
        public void NOrMore()
        {
            ExpectString("/A{3}/.exec('AAAA')", "AAAA");
            ExpectString("/A{3}/.exec('AA')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /A{3,}/")]
        public void NOrMoreWithComma()
        {
            ExpectString("/A{3,}/.exec('AAAA')", "AAAA");
            ExpectString("/A{3,}/.exec('AA')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /A{2,4}/")]
        public void BetweenXAndY()
        {
            ExpectString("/A{2,4}/.exec('A')", "null");
            ExpectString("/A{2,4}/.exec('AA')", "AA");
            ExpectString("/A{2,4}/.exec('AAA')", "AAA");
            ExpectString("/A{2,4}/.exec('AAAA')", "AAAA");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /^AAA/g")]
        public void TestStartOfInputAssertion()
        {
            ExpectString("/^AAA/g.exec('AAA')", "AAA");
            ExpectString("/^AAA/g.exec('BAAA')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /AAA$/g")]
        public void TestEndOfInputAssertion()
        {
            ExpectString("/AAA$/g.exec('AAA')", "AAA");
            ExpectString("/AAA$/g.exec('BBBAAA')", "AAA");
            ExpectString("/AAA$/g.exec('AAABBB')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /(AAA)\\b/")]
        public void TestWordBoundary()
        {
            ExpectString("/(AAA)\\b/.exec('AAA BBB')", "AAA,AAA");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /(AAA)\\B/")]
        public void TestNonWordBoundary()
        {
            ExpectString("/(AAA)\\B/.exec('AAABBB')", "AAA,AAA");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /AAA(?=BBB)/")]
        public void TestFollowedByAssertion()
        {
            ExpectString("/AAA(?=BBB)/.exec('AAABBB')", "AAA");
            ExpectString("/AAA(?=BBB)/.exec('AAACCC')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /AAA(?!BBB)/")]
        public void TestNotFollowedByAssertion()
        {
            ExpectString("/AAA(?!BBB)/.exec('AAACCC')", "AAA");
            ExpectString("/AAA(?!BBB)/.exec('AAABBB')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /\\d/")]
        public void TestDigitCharacterClassEscape()
        {
            ExpectString("/\\d/.exec('1')", "1");
            ExpectString("/\\d/.exec('A')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /\\D/")]
        public void TestNonDigitCharacterClassEscape()
        {
            ExpectString("/\\D/.exec('A')", "A");
            ExpectString("/\\D/.exec('1')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /\\s/")]
        public void TestWhiteSpace()
        {
            ExpectString("/\\s/.exec('\t')", "\t");
            ExpectString("/\\s/.exec('A')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /\\S/")]
        public void TestNonWhiteSpace()
        {
            ExpectString("/\\S/.exec('A')", "A");
            ExpectString("/\\S/.exec(' ')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /\\w/")]
        public void TestWordCharacter()
        {
            ExpectString("/\\w/.exec('A')", "A");
            ExpectString("/\\w/.exec('\t')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /\\W/")]
        public void TestNonWordCharacter()
        {
            ExpectString("/\\W/.exec('\t')", "\t");
            ExpectString("/\\W/.exec('A')", "null");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /\\d{3}-\\d{2}-\\d{4}/")]
        public void TestSsnExample()
        {
            ExpectString("/(\\d{3})-(\\d{2})-(\\d{4})/.exec('123-45-6789')", "123-45-6789,123,45,6789");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /(\\w+)@(\\w+)\\.com")]
        public void TestBasicEmailExample()
        {
            ExpectString("/(\\w+)@(\\w+)\\.com/.exec('test@test.com')", "test@test.com,test,test");
        }

        [Fact(DisplayName = "15.10  RegExp (Regular Expression) Objects: /\\\\x[0-9a-zA-Z]{2}/")]
        public void TestHexEscapeSequenceExample()
        {
            ExpectString(@"/\\x[0-9a-zA-Z]{2}/.exec('\\xAF')", "\\xAF");
        }
    }
}