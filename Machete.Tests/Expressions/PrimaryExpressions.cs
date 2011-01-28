using System;
using Xunit;

namespace Machete.Tests
{
    public class PrimaryExpressions : TestBase
    {
        [Fact(DisplayName = "11.1.1 The this Keyword")]
        public void Test1111()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof this"));
        }

        [Fact(DisplayName = "11.1.2 Identifier Reference")]
        public void Test1112()
        {
            Assert.IsAssignableFrom<Exception>(Engine.ExecuteScript("invalidIdentifier"));
        }

        [Fact(DisplayName = "11.1.3 Literal Reference")]
        public void Test1113()
        {
            Assert.Equal("test", (string)Engine.ExecuteScript("'test'"));
        }

        [Fact(DisplayName = "11.1.4 Array Initialiser - Empty")]
        public void Test1114A()
        {
            ExpectString("[]", "");
            ExpectDouble("[].length", 0.0);
        }

        [Fact(DisplayName = "11.1.4 Array Initialiser - Populated, No Elision")]
        public void Test1114B()
        {
            ExpectString("[1,2,3,4,5]", "1,2,3,4,5");
            ExpectDouble("[1,2,3,4,5].length", 5.0);
        }

        [Fact(DisplayName = "11.1.4 Array Initialiser - Populated, Leading Elision")]
        public void Test1114C()
        {
            ExpectString("[,,1,2,3,4,5]", ",,1,2,3,4,5");
            ExpectDouble("[,,1,2,3,4,5].length", 7.0);
        }

        [Fact(DisplayName = "11.1.4 Array Initialiser - Populated, Trailing Elision")]
        public void Test1114D()
        {
            ExpectString("[1,2,3,4,5,,,]", "1,2,3,4,5,,");
            ExpectDouble("[1,2,3,4,5,,,].length", 7.0);
        }

        [Fact(DisplayName = "11.1.4 Array Initialiser - Populated, Middle Elision")]
        public void Test1114E()
        {
            ExpectString("[1,,2, 3, 4, 5]", "1,,2,3,4,5");
            ExpectDouble("[1,,2, 3, 4, 5].length", 6.0);
        }

        [Fact(DisplayName = "11.1.5 Object Initialiser - Empty")]
        public void Test1115A()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof ({})"));
            Assert.True((bool)Engine.ExecuteScript("({}) instanceof Object"));
        }

        [Fact(DisplayName = "11.1.5 Object Initialiser - Data")]
        public void Test1115B()
        {
            ExpectString(@"
                (function() {
                    var o = {
                        a: 1,
                        b: 2,
                        c: 3
                    };
                    return Object.keys(o);
                })();
            ", "a,b,c");
        }

        [Fact(DisplayName = "11.1.5 Object Initialiser - Data With Dupe")]
        public void Test1115C()
        {
            ExpectDouble(@"
                (function() {
                    var o = {
                        a: 1,
                        b: 2,
                        c: 3,
                        a: 4
                    };
                    return o.a;
                })();
            ", 4.0);
        }

        [Fact(DisplayName = "11.1.5 Object Initialiser - Getter")]
        public void Test1115D()
        {
            ExpectDouble(@"
                (function() {
                    var num = 10,
                        o = {
                            get a() {
                                return num;
                            }
                        };
                    return o.a;
                })();
            ", 10.0);
        }

        [Fact(DisplayName = "11.1.5 Object Initialiser - Setter")]
        public void Test1115E()
        {
            ExpectDouble(@"
                (function() {
                    var num = 10,
                        o = {
                            set a(v) {
                                num = v;
                            }
                        };
                    o.a = 100;
                    return num;
                })();
            ", 100.0);
        }

        [Fact(DisplayName = "11.1.5 Object Initialiser - Getter and Setter")]
        public void Test1115F()
        {
            ExpectDouble(@"
                (function() {
                    var num = 10,
                        o = {
                            get a() {
                                return num;
                            },
                            set a(v) {
                                num = v;
                            }
                        };
                    var r = o.a;
                    o.a = 40;
                    return o.a + r;
                })();
            ", 50.0);
        }

        [Fact(DisplayName = "11.1.5 Object Initialiser - Getter, Setter, Data ")]
        public void Test1115G()
        {
            ExpectDouble(@"
                (function() {
                    var o = {
                        num: 10,
                        get a() {
                            return this.num;
                        },
                        set a(v) {
                            this.num = v;
                        }
                    };
                    var r = o.a;
                    o.a = 40;
                    return o.num + o.a + r;
                })();
            ", 90.0);
        }

        [Fact(DisplayName = "11.1.6 The Grouping Operator")]
        public void Test1116()
        {
            Assert.Equal(0.5, (double)Engine.ExecuteScript("1 / (1 + 1)"));
        }
    }
}