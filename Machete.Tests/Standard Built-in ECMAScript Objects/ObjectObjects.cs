using System;
using Xunit;
using Machete.Interfaces;

namespace Machete.Tests
{
    public class ObjectObjects : TestBase
    {
        [Fact(DisplayName = "15.2.1.1 Object ( [ value ] )")]
        public void Test15211()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof Object()"));
            Assert.Equal(2.0, (double)Engine.ExecuteScript("Object(2).valueOf()"));
        }

        [Fact(DisplayName = "15.2.2.1 new Object ( [ value ] )")]
        public void Test15221()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof new Object()"));
            Assert.Equal(2.0, (double)Engine.ExecuteScript("new Object(2).valueOf()"));
        }

        [Fact(DisplayName = "15.2.3.1 Object.prototype")]
        public void Test15231()
        {
            Assert.Equal("object", (string)Engine.ExecuteScript("typeof Object.prototype"));
        }

        [Fact(DisplayName = "15.2.3.2 Object.getPrototypeOf ( O )")]
        public void Test15232()
        {
            Assert.True((bool)Engine.ExecuteScript("Object.getPrototypeOf({}) === Object.prototype"));
        }

        [Fact(DisplayName = "15.2.3.3 Object.getOwnPropertyDescriptor ( O, P )")]
        public void Test15233()
        {
            Assert.True((bool)Engine.ExecuteScript(@"
                (function() {
                    var o = Object.getOwnPropertyDescriptor(Object, 'create');
                    return o.writable && !o.enumerable && o.configurable && typeof o.value === 'function';
                })();
            "));
        }

        [Fact(DisplayName = "15.2.3.4 Object.getOwnPropertyNames ( O )")]
        public void Test15234()
        {
            Assert.Equal("a,b,c", (string)Engine.ExecuteScript("Object.getOwnPropertyNames ({ a: 1, b: 2, c: 3 })"));
        }

        [Fact(DisplayName = "15.2.3.5 Object.create ( O [, Properties] )")]
        public void Test15235()
        {
            Assert.Equal("Success", (string)Engine.ExecuteScript(@"
                (function() {
                    var proto = {
                        getNum: function() {
                            return 2;
                        }
                    };
                    var o = Object.create(proto);
                    if (!proto.isPrototypeOf(o)) return 'Wrong prototype.';
                    if (!o.getNum) return 'Missing getNum.';
                    return 'Success';
                })();
            "));
        }

        [Fact(DisplayName = "15.2.3.6 Object.defineProperty ( O, P, Attributes )")]
        public void Test15236()
        {
            Assert.Equal("Success", (string)Engine.ExecuteScript(@"
                (function() {
                    var o = {}, 
                        p = 'test', 
                        attributes = { 
                            writable: false, 
                            enumerable: false, 
                            configurable: false, 
                            value: 10 
                        };
                    Object.defineProperty(o, p, attributes);
                    if (!o.test) {
                        return 'Missing';    
                    }
                    o.test = 12;
                    if (o.test !== 10) {
                        return 'Writable';
                    } else if (delete o.test) {
                        return 'Configurable';
                    }
                    return 'Success';
                })();
            "));
        }

        [Fact(DisplayName = "15.2.3.7 Object.defineProperties ( O, Properties )")]
        public void Test15237()
        {
            Assert.Equal("Success", (string)Engine.ExecuteScript(@"
                (function() {
                    var o = {},
                        properties = { 
                            name: { 
                                writable: false, 
                                enumerable: false, 
                                configurable: false, 
                                value: 'Jim' 
                            },
                            age: { 
                                writable: true, 
                                enumerable: false, 
                                configurable: false, 
                                value: 25 
                            }
                        };
                    Object.defineProperties(o, properties);
                    if (!o.name) return 'Missing name.'; 
                    if (!o.age) return 'Missing age.'; 
                    o.name = 'Bob';
                    if (o.name === 'Bob') return 'Writable name.';
                    o.age = 35;
                    if (o.age !== 35) return 'Non-writable age.';
                    return 'Success';
                })();
            "));
        }

        [Fact(DisplayName = "15.2.3.8 Object.seal ( O )")]
        public void Test15238()
        {
            Assert.Equal("Success", (string)Engine.ExecuteScript(@"
                (function() {
                    var o = {
                        prop1: 1,
                        prop2: 2
                    };
                    if (!(delete o.prop1)) return 'Should delete prop1.';
                    Object.seal(o);
                    if ((delete o.prop2)) return 'Should not be able to delete prop2.';
                    return 'Success';
                })();
            "));
        }

        [Fact(DisplayName = "15.2.3.9 Object.freeze ( O )")]
        public void Test15239()
        {
            Assert.Equal("Success", (string)Engine.ExecuteScript(@"
                (function() {
                    var o = {
                        prop1:1
                    };
                    Object.freeze(o);
                    if (!Object.isFrozen(o)) {
                        return 'Should be frozen.';
                    }
                    return 'Success';
                })();
            "));
        }

        [Fact(DisplayName = "15.2.3.10 Object.preventExtensions ( O )")]
        public void Test152310()
        {
            Assert.Equal("Success", (string)Engine.ExecuteScript(@"
                (function() {
                    var o = {
                        prop1:1
                    };
                    Object.preventExtensions(o);
                    if (Object.isExtensible(o)) {
                        return 'Should not be extensible.';
                    }
                    return 'Success';
                })();
            "));
        }

        [Fact(DisplayName = "15.2.3.11 Object.isSealed ( O )")]
        public void Test152311()
        {
            Assert.Equal("Success", (string)Engine.ExecuteScript(@"
                (function() {
                    var o = {
                        prop1:1
                    };
                    Object.seal(o);
                    if (!Object.isSealed(o)) {
                        return 'Should be sealed.';
                    }
                    return 'Success';
                })();
            "));
        }

        [Fact(DisplayName = "15.2.3.12 Object.isFrozen ( O )")]
        public void Test152312()
        {
            Assert.False((bool)Engine.ExecuteScript("Object.isFrozen(Object)"));
        }

        [Fact(DisplayName = "15.2.3.13 Object.isExtensible ( O )")]
        public void Test152313()
        {
            Assert.True((bool)Engine.ExecuteScript("Object.isExtensible(Object)"));
        }

        [Fact(DisplayName = "15.2.3.14 Object.keys ( O )")]
        public void Test152314()
        {
            Assert.Equal("a,b", (string)Engine.ExecuteScript("Object.keys({a:1,b:2})"));
        }

        [Fact(DisplayName = "15.2.4.1 Object.prototype.constructor")]
        public void Test15241()
        {
            Assert.True((bool)Engine.ExecuteScript("new Object().constructor === Object"));
        }

        [Fact(DisplayName = "15.2.4.2 Object.prototype.toString ( )")]
        public void Test15242()
        {
            Assert.Equal("[object, Function]", (string)Engine.ExecuteScript("Object.toString()"));
            Assert.Equal("[object, Object]", (string)Engine.ExecuteScript("new Object().toString()"));
        }

        [Fact(DisplayName = "15.2.4.3 Object.prototype.toLocaleString ( )")]
        public void Test15243()
        {
            Assert.Equal("[object, Function]", (string)Engine.ExecuteScript("Object.toLocaleString()"));
            Assert.Equal("[object, Object]", (string)Engine.ExecuteScript("new Object().toLocaleString()"));
        }

        [Fact(DisplayName = "15.2.4.4 Object.prototype.valueOf ( )")]
        public void Test15244()
        {
            Assert.True((bool)Engine.ExecuteScript("Object.valueOf() === Object"));
        }

        [Fact(DisplayName = "15.2.4.5 Object.prototype.hasOwnProperty (V)")]
        public void Test15245()
        {
            Assert.True((bool)Engine.ExecuteScript("({ test : 1 }).hasOwnProperty('test')"));
            Assert.True((bool)Engine.ExecuteScript("!({ test : 1 }).hasOwnProperty('hasOwnProperty')"));
        }

        [Fact(DisplayName = "15.2.4.6 Object.prototype.isPrototypeOf (V)")]
        public void Test15246()
        {
            Assert.True((bool)Engine.ExecuteScript("Object.prototype.isPrototypeOf({ test : 1 })"));
        }

        [Fact(DisplayName = "15.2.4.7 Object.prototype.propertyIsEnumerable (V)")]
        public void Test15247()
        {
            Assert.False((bool)Engine.ExecuteScript("Object.propertyIsEnumerable('create')"));
        }
    }
}