using System;
using Xunit;
using Machete.Core;

namespace Machete.Tests
{
    public class RegExpObjects : TestBase
    {
        [Fact(DisplayName = "15.10.3.1  RegExp(pattern, flags)")]
        public void Test151031()
        {
            ExpectString("typeof RegExp('AAA', '')", "object");
            ExpectTrue("RegExp('AAA', '') instanceof RegExp");
            ExpectString("RegExp('AAA', '')", "/AAA/");
        }

        [Fact(DisplayName = "15.10.4.1  new RegExp(pattern, flags)")]
        public void Test151041()
        {
            ExpectString("typeof new RegExp('AAA', '')", "object");
            ExpectTrue("new RegExp('AAA', '') instanceof RegExp");
            ExpectString("new RegExp('AAA', '')", "/AAA/");
        }

        [Fact(DisplayName = "15.10.5.1  RegExp.prototype")]
        public void Test151051()
        {
            ExpectString("typeof RegExp.prototype", "object");
            ExpectTrue("RegExp.prototype.isPrototypeOf(new RegExp('AAA', ''))");
        }

        [Fact(DisplayName = "15.10.6.1  RegExp.prototype.constructor")]
        public void Test151061()
        {
            ExpectTrue("RegExp.prototype.constructor === RegExp");
        }

        [Fact(DisplayName = "15.10.6.2  RegExp.prototype.exec(string)")]
        public void Test151062()
        {
            Expect("typeof RegExp.prototype.exec", "function");
            Expect("typeof (new RegExp('AAA', '')).exec", "function");
            Expect("/AAA/.exec('AAA')", "AAA");
        }

        [Fact(DisplayName = "15.10.6.3  RegExp.prototype.test(string)")]
        public void Test151063()
        {
            Expect("typeof RegExp.prototype.test", "function");
            Expect("typeof (new RegExp('AAA', '')).test", "function");
            ExpectTrue("/AAA/.test('AAA')");
        }

        [Fact(DisplayName = "15.10.6.4  RegExp.prototype.toString()")]
        public void Test151064()
        {
            Expect("typeof RegExp.prototype.toString", "function");
            Expect("typeof (new RegExp('AAA', '')).toString", "function");
            Expect("/AAA/.toString()", "/AAA/");
        }

        [Fact(DisplayName = "15.10.7.1  source")]
        public void Test151071()
        {
            Expect(@"
                (function() {
                    var r = /a/;
                    if (!r.hasOwnProperty('source')) return 'The source property is missing.';
                    var d = Object.getOwnPropertyDescriptor(r, 'source');
                    if (d.writable) return 'The source property is writable.';
                    if (d.enumerable) return 'The source property is enumerable.';
                    if (d.configurable) return 'The source property is configurable.';
                    if (d.value !== 'a') return 'The source property value is incorrect. (' + d.value + ').';
                    return 'Success';
                })();
            ", "Success");
        }

        [Fact(DisplayName = "15.10.7.2  global")]
        public void Test151072()
        {
            Expect(@"
                (function() {
                    var r = /a/g;
                    if (!r.hasOwnProperty('global')) return 'The global property is missing.';
                    var d = Object.getOwnPropertyDescriptor(r, 'global');
                    if (d.writable) return 'The global property is writable.';
                    if (d.enumerable) return 'The global property is enumerable.';
                    if (d.configurable) return 'The global property is configurable.';
                    if (!d.value) return 'The global property value is incorrect. (' + d.value + ').';
                    return 'Success';
                })();
            ", "Success");
        }

        [Fact(DisplayName = "15.10.7.3  ignoreCase")]
        public void Test151073()
        {
            Expect(@"
                (function() {
                    var r = /a/i;
                    if (!r.hasOwnProperty('ignoreCase')) return 'The ignoreCase property is missing.';
                    var d = Object.getOwnPropertyDescriptor(r, 'ignoreCase');
                    if (d.writable) return 'The ignoreCase property is writable.';
                    if (d.enumerable) return 'The ignoreCase property is enumerable.';
                    if (d.configurable) return 'The ignoreCase property is configurable.';
                    if (!d.value) return 'The ignoreCase value is incorrect. (' + d.value + ').';
                    return 'Success';
                })();
            ", "Success");
        }

        [Fact(DisplayName = "15.10.7.4  multiline")]
        public void Test151074()
        {
            Expect(@"
                (function() {
                    var r = /a/m;
                    if (!r.hasOwnProperty('multiline')) return 'The multiline property is missing.';
                    var d = Object.getOwnPropertyDescriptor(r, 'multiline');
                    if (d.writable) return 'The multiline property is writable.';
                    if (d.enumerable) return 'The multiline property is enumerable.';
                    if (d.configurable) return 'The multiline property is configurable.';
                    if (!d.value) return 'The multiline value is incorrect. (' + d.value + ').';
                    return 'Success';
                })();
            ", "Success");
        }

        [Fact(DisplayName = "15.10.7.5  lastIndex")]
        public void Test151075()
        {
            Expect(@"
                (function() {
                    var r = /a/;
                    if (!r.hasOwnProperty('lastIndex')) return 'The lastIndex property is missing.';
                    var d = Object.getOwnPropertyDescriptor(r, 'lastIndex');
                    if (!d.writable) return 'The lastIndex property is not writable.';
                    if (d.enumerable) return 'The lastIndex property is enumerable.';
                    if (d.configurable) return 'The lastIndex property is configurable.';
                    if (d.value !== 0) return 'The lastIndex value is incorrect. (' + d.value + ').';
                    return 'Success';
                })();
            ", "Success");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /A/")]
        public void PatternCharacter()
        {
            ExpectString("/A/.exec('A')", "A");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /A|B|C/")]
        public void Alternatives()
        {
            ExpectString("/A|B|C/.exec('C')", "C");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /./")]
        public void DotAtom()
        {
            ExpectString("/./.exec('A')", "A");
            ExpectString("/./.exec('1')", "1");
            ExpectString("/./.exec('\t')", "\t");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /[a-z]/")]
        public void CharacterClassOneRange()
        {
            ExpectString("/[a-z]/.exec('x')", "x");
            ExpectString("/[a-z]/.exec('1')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /[^a-z]/")]
        public void CharacterClassOneRangeNegate()
        {
            ExpectString("/[^a-z]/.exec('x')", "null");
            ExpectString("/[^a-z]/.exec('1')", "1");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /[a-z0-9]/")]
        public void CharacterClassMultipleRange()
        {
            ExpectString("/[a-z0-9]/.exec('x')", "x");
            ExpectString("/[a-z0-9]/.exec('1')", "1");
            ExpectString("/[a-z0-9]/.exec('_')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /[^a-z0-9]/")]
        public void CharacterClassMultipleRangeNegate()
        {
            ExpectString("/[^a-z0-9]/.exec('x')", "null");
            ExpectString("/[^a-z0-9]/.exec('1')", "null");
            ExpectString("/[^a-z0-9]/.exec('_')", "_");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /[\\b]/")]
        public void BackSpaceClassEscape()
        {
            ExpectString("/[\\b]/.exec('\\u0008')", "\u0008");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /[\\0]/")]
        public void ClassDecimalEscapeA()
        {
            Expect("/[\\0]/.exec('\\u0000')", "\u0000");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /[\\1]/")]
        public void ClassDecimalEscapeB()
        {
            Expect<MacheteRuntimeException>("/[\\1]/.exec('test')");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /(AAA)/")]
        public void TestCapturingGroup()
        {
            ExpectString("/(AAA)/.exec('AAA')", "AAA,AAA");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /(?:AAA)/")]
        public void TestNonCapturingGroup()
        {
            ExpectString("/(?:AAA)/.exec('AAA')", "AAA");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /(AAA)\\0/")]
        public void TestDecimalEscapeA()
        {
            ExpectString("/(AAA)\\0/.exec('AAA\\u0000')", "AAA\u0000,AAA");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /(AAA)\\1/")]
        public void TestDecimalEscapeB()
        {
            ExpectString("/(AAA)\\1/.exec('AAAAAA')", "AAAAAA,AAA");
            ExpectString("/(AAA)\\1/.exec('AAABBB')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /\\t\\n\\v\\f\\r/")]
        public void TestControlEscape()
        {
            ExpectString("/\\t\\n\\v\\f\\r/.exec('\\u0009\\u000A\\u000B\\u000C\\u000D')", "\u0009\u000A\u000B\u000C\u000D");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: IdentityEscape")]
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

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /\\x0D/")]
        public void TestHexEscapeSequence()
        {
            ExpectString("/\\x0D/.exec('\\x0D')", "\x0D");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /\\u000D/")]
        public void TestUnicodeEscapeSequence()
        {
            ExpectString("/\\u000D/.exec('\\u000D')", "\u000D");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /A*/")]
        public void ZeroOrMore()
        {
            ExpectString("/A*/.exec('AAA')", "AAA");
            ExpectString("/A*/.exec('')", "");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /A+/")]
        public void OneOrMore()
        {
            ExpectString("/A+/.exec('AAA')", "AAA");
            ExpectString("/A+/.exec('')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /A?/")]
        public void ZeroOrOne()
        {
            ExpectString("/A?/.exec('A')", "A");
            ExpectString("/A?/.exec('')", "");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /A{3}/")]
        public void NOrMore()
        {
            ExpectString("/A{3}/.exec('AAAA')", "AAAA");
            ExpectString("/A{3}/.exec('AA')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /A{3,}/")]
        public void NOrMoreWithComma()
        {
            ExpectString("/A{3,}/.exec('AAAA')", "AAAA");
            ExpectString("/A{3,}/.exec('AA')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /A{2,4}/")]
        public void BetweenXAndY()
        {
            ExpectString("/A{2,4}/.exec('A')", "null");
            ExpectString("/A{2,4}/.exec('AA')", "AA");
            ExpectString("/A{2,4}/.exec('AAA')", "AAA");
            ExpectString("/A{2,4}/.exec('AAAA')", "AAAA");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /^AAA/g")]
        public void TestStartOfInputAssertion()
        {
            ExpectString("/^AAA/g.exec('AAA')", "AAA");
            ExpectString("/^AAA/g.exec('BAAA')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /AAA$/g")]
        public void TestEndOfInputAssertion()
        {
            ExpectString("/AAA$/g.exec('AAA')", "AAA");
            ExpectString("/AAA$/g.exec('BBBAAA')", "AAA");
            ExpectString("/AAA$/g.exec('AAABBB')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /(AAA)\\b/")]
        public void TestWordBoundary()
        {
            ExpectString("/(AAA)\\b/.exec('AAA BBB')", "AAA,AAA");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /(AAA)\\B/")]
        public void TestNonWordBoundary()
        {
            ExpectString("/(AAA)\\B/.exec('AAABBB')", "AAA,AAA");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /AAA(?=BBB)/")]
        public void TestFollowedByAssertion()
        {
            ExpectString("/AAA(?=BBB)/.exec('AAABBB')", "AAA");
            ExpectString("/AAA(?=BBB)/.exec('AAACCC')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /AAA(?!BBB)/")]
        public void TestNotFollowedByAssertion()
        {
            ExpectString("/AAA(?!BBB)/.exec('AAACCC')", "AAA");
            ExpectString("/AAA(?!BBB)/.exec('AAABBB')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /\\d/")]
        public void TestDigitCharacterClassEscape()
        {
            ExpectString("/\\d/.exec('1')", "1");
            ExpectString("/\\d/.exec('A')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /\\D/")]
        public void TestNonDigitCharacterClassEscape()
        {
            ExpectString("/\\D/.exec('A')", "A");
            ExpectString("/\\D/.exec('1')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /\\s/")]
        public void TestWhiteSpace()
        {
            ExpectString("/\\s/.exec('\t')", "\t");
            ExpectString("/\\s/.exec('A')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /\\S/")]
        public void TestNonWhiteSpace()
        {
            ExpectString("/\\S/.exec('A')", "A");
            ExpectString("/\\S/.exec(' ')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /\\w/")]
        public void TestWordCharacter()
        {
            ExpectString("/\\w/.exec('A')", "A");
            ExpectString("/\\w/.exec('\t')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /\\W/")]
        public void TestNonWordCharacter()
        {
            ExpectString("/\\W/.exec('\t')", "\t");
            ExpectString("/\\W/.exec('A')", "null");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /A/i")]
        public void IgnoreCase()
        {
            Expect("/A/i.exec('A')", "A");
            Expect("/A/i.exec('a')", "a");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /A/g")]
        public void Global()
        {
            Expect("/A/g.exec('123A')", "A");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /A/m")]
        public void Multiline()
        {
            Expect("/A/m.exec('\\nA')", "A");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /\\d{3}-\\d{2}-\\d{4}/")]
        public void TestSsnExample()
        {
            ExpectString("/(\\d{3})-(\\d{2})-(\\d{4})/.exec('123-45-6789')", "123-45-6789,123,45,6789");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /(\\w+)@(\\w+)\\.com")]
        public void TestBasicEmailExample()
        {
            ExpectString("/(\\w+)@(\\w+)\\.com/.exec('test@test.com')", "test@test.com,test,test");
        }

        [Fact(DisplayName = "15.10.2  Pattern Semantics: /\\\\x[0-9a-zA-Z]{2}/")]
        public void TestHexEscapeSequenceExample()
        {
            ExpectString(@"/\\x[0-9a-zA-Z]{2}/.exec('\\xAF')", "\\xAF");
        }
    }
}