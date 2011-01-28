using System;
using Xunit;

namespace Machete.Tests
{
    public class StringObjects : TestBase
	{
        [Fact(DisplayName = "15.5.1.1 String ( [ value ] )")]
        public void Test15511()
        {
            Assert.True((bool)Engine.ExecuteScript("String(true) === 'true'"));
        }

        [Fact(DisplayName = "15.5.2.1 new String ( [ value ] )")]
        public void Test15521()
        {
            Assert.IsNotType<Exception>(Engine.ExecuteScript("var o = new String('Test');"));
            Assert.True((bool)Engine.ExecuteScript("typeof o === 'object'"));
            Assert.True((bool)Engine.ExecuteScript("o + 's' === 'Tests'"));
            Assert.True((bool)Engine.ExecuteScript("delete o;"));
        }

        [Fact(DisplayName = "15.5.3.1 String.prototype")]
        public void Test15531()
        {
            Assert.IsNotType<Exception>(Engine.ExecuteScript("var o = String.prototype"));
            Assert.True((bool)Engine.ExecuteScript("typeof o === 'object'"));
            Assert.True((bool)Engine.ExecuteScript("delete o;"));
        }

        [Fact(DisplayName = "15.5.3.2 String.fromCharCode ( [ char0 [ , char1 [ , … ] ] ] )")]
        public void Test15532()
        {
            Assert.True((bool)Engine.ExecuteScript("String.fromCharCode(0xFF, 0xFF, 0xFF) === '\u00FF\u00FF\u00FF'"));
        }

        [Fact(DisplayName = "15.5.4.1 String.prototype.constructor")]
        public void Test15541()
        {
            Assert.True((bool)Engine.ExecuteScript("String.prototype.constructor === String"));
        }

        [Fact(DisplayName = "15.5.4.2 String.prototype.toString ( )")]
        public void Test15542()
        {
            Assert.True((bool)Engine.ExecuteScript("(new String('ABC')).toString() === 'ABC'"));
        }

        [Fact(DisplayName = "15.5.4.3 String.prototype.valueOf ( )")]
        public void Test15543()
        {
            Assert.True((bool)Engine.ExecuteScript("(new String('ABC')).valueOf() === 'ABC'"));
        }

        [Fact(DisplayName = "15.5.4.4 String.prototype.charAt (pos)")]
        public void Test15544()
        {
            Assert.True((bool)Engine.ExecuteScript("('ABC').charAt(1) === 'B'"));
        }

        [Fact(DisplayName = "15.5.4.5 String.prototype.charCodeAt (pos)")]
        public void Test15545()
        {
            Assert.True((bool)Engine.ExecuteScript("('ABC').charCodeAt(1) === 66"));
        }

        [Fact(DisplayName = "15.5.4.6 String.prototype.concat ( [ string1 [ , string2 [ , … ] ] ] )")]
        public void Test15546()
        {
            Assert.True((bool)Engine.ExecuteScript("('A').concat('B', 'C', 'D') === 'ABCD'"));
        }

        [Fact(DisplayName = "15.5.4.7 String.prototype.indexOf (searchString, position)")]
        public void Test15547()
        {
            Assert.True((bool)Engine.ExecuteScript("('ABCD').indexOf('C') === 2"));
        }

        [Fact(DisplayName = "15.5.4.8 String.prototype.lastIndexOf (searchString, position)")]
        public void Test15548()
        {
            Assert.True((bool)Engine.ExecuteScript("('ABCDC').lastIndexOf('C') === 4"));
        }

        [Fact(DisplayName = "15.5.4.9 String.prototype.localeCompare (that)")]
        public void Test15549()
        {
            Assert.True((bool)Engine.ExecuteScript("('A').localeCompare('B') === -1"));
        }

        [Fact(DisplayName = "15.5.4.10 String.prototype.match (regexp)")]
        public void Test155410()
        {
            Assert.Equal("A", (string)Engine.ExecuteScript("'AAAA'.match('A')"));
            Assert.Equal("A", (string)Engine.ExecuteScript("'AAAA'.match(/A/)"));
            Assert.Equal("A,A,A,A", (string)Engine.ExecuteScript("'AAAA'.match(/A/g)"));
        }

        [Fact(DisplayName = "15.5.4.11 String.prototype.replace (searchValue, replaceValue)")]
        public void Test155411()
        {
            Assert.Equal("XXXXXXXX", (string)Engine.ExecuteScript("'XXXX'.replace(/(X)(X)/g, '$&$&')"));
        }

        [Fact(DisplayName = "15.5.4.12 String.prototype.search (regexp)")]
        public void Test155412()
        {
            Assert.Equal(1.0, (double)Engine.ExecuteScript("'A@B@C'.search('@')"));
            Assert.Equal(2.0, (double)Engine.ExecuteScript("'A@B@C'.search(/B/)"));
        }

        [Fact(DisplayName = "15.5.4.13 String.prototype.slice (start, end)")]
        public void Test155413()
        {
            Assert.True((bool)Engine.ExecuteScript("('ABC').slice(1) === 'BC'"));
        }

        [Fact(DisplayName = "15.5.4.14 String.prototype.split (separator, limit) ")]
        public void Test155414()
        {
            Assert.Equal("A,@,B,@,C", (string)Engine.ExecuteScript("('A@B@C').split(/(@)/)"));
            Assert.Equal("A,@,B,@", (string)Engine.ExecuteScript("('A@B@C').split(/(@)/, 4)"));
            Assert.Equal("A,B,C", (string)Engine.ExecuteScript("('A@B@C').split('@')"));
            Assert.Equal("A,B", (string)Engine.ExecuteScript("('A@B@C').split('@', 2)"));
        }

        [Fact(DisplayName = "15.5.4.15 String.prototype.substring (start, end)")]
        public void Test155415()
        {
            Assert.True((bool)Engine.ExecuteScript("('ABBBA').substring(1, 4) === 'BBB'"));
        }

        [Fact(DisplayName = "15.5.4.16 String.prototype.toLowerCase ( )")]
        public void Test155416()
        {
            Assert.True((bool)Engine.ExecuteScript("('A').toLowerCase() === 'a'"));
        }

        [Fact(DisplayName = "15.5.4.17 String.prototype.toLocaleLowerCase ( )")]
        public void Test155417()
        {
            Assert.True((bool)Engine.ExecuteScript("('A').toLocaleLowerCase() === 'a'"));
        }

        [Fact(DisplayName = "15.5.4.18 String.prototype.toUpperCase ( )")]
        public void Test155418()
        {
            Assert.True((bool)Engine.ExecuteScript("('a').toUpperCase() === 'A'"));
        }

        [Fact(DisplayName = "15.5.4.19 String.prototype.toLocaleUpperCase ( )")]
        public void Test155419()
        {
            Assert.True((bool)Engine.ExecuteScript("('a').toLocaleUpperCase() === 'A'"));
        }

        [Fact(DisplayName = "15.5.4.20 String.prototype.trim ( )")]
        public void Test155420()
        {
            Assert.True((bool)Engine.ExecuteScript("('\\rA \\r').trim() === 'A'"));
        }

        [Fact(DisplayName = "15.5.5.1 length")]
        public void Test15551()
        {
            Assert.True((bool)Engine.ExecuteScript("('AAA').length === 3"));
        }

        [Fact(DisplayName = "15.5.5.2 [[GetOwnProperty]] ( P )")]
        public void Test15552()
        {
            Assert.True((bool)Engine.ExecuteScript("('ABA')[1] === 'B'"));
        }
	}
}