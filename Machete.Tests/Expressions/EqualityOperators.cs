using System;
using Xunit;

namespace Machete.Tests
{
    public class EqualityOperators : TestBase
    {
        [Fact(DisplayName = "11.9.1 The Equals Operator ( == ): Same Type")]
        public void Test1191A()
        {
            ExpectTrue("1 == 1");
            ExpectTrue("'' == ''");
        }

        [Fact(DisplayName = "11.9.1 The Equals Operator ( == ): null == undefined")]
        public void Test1191B()
        {
            ExpectTrue("null == undefined");
        }

        [Fact(DisplayName = "11.9.1 The Equals Operator ( == ): undefined == null")]
        public void Test1191C()
        {
            ExpectTrue("undefined == null");
        }

        [Fact(DisplayName = "11.9.1 The Equals Operator ( == ): Number == String")]
        public void Test1191D()
        {
            ExpectTrue("2 == '2'");
            ExpectFalse("3 == '2'");
        }

        [Fact(DisplayName = "11.9.1 The Equals Operator ( == ): String == Number")]
        public void Test1191E()
        {
            ExpectTrue("'2' == 2");
            ExpectFalse("'3' == 2");
        }

        [Fact(DisplayName = "11.9.2 The Does-not-equals Operator ( != )")]
        public void Test1192()
        {
            Assert.True((bool)Engine.ExecuteScript("2 != '1'"));
        }

        [Fact(DisplayName = "11.9.4 The Strict Equals Operator ( === )")]
        public void Test1194()
        {
            Assert.True((bool)Engine.ExecuteScript("1 === 1"));
        }

        [Fact(DisplayName = "11.9.5 The Strict Does-not-equal Operator ( !== )")]
        public void Test1195()
        {
            Assert.True((bool)Engine.ExecuteScript("2 !== 1"));
        }
    }
}