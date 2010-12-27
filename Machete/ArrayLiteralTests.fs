namespace Machete

module ArrayLiteralTests =

    open Xunit

    let engine = Engine()

    [<Fact(DisplayName = "ArrayLiteral: Empty Array")>]
    let test1 () =
        Assert.IsNotType<exn>(engine.ExecuteScript "var array = [];")
        Assert.True(engine.ExecuteScript "typeof array === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "array instanceof Array" :?> bool)
        Assert.True(engine.ExecuteScript "array.length === 0" :?> bool)
        Assert.True(engine.ExecuteScript "delete array" :?> bool)

    [<Fact(DisplayName = "ArrayLiteral: Single Primitive Element")>]
    let test2 () =
        Assert.IsNotType<exn>(engine.ExecuteScript "var array = [1];")
        Assert.True(engine.ExecuteScript "typeof array === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "array instanceof Array" :?> bool)
        Assert.True(engine.ExecuteScript "array.length === 1" :?> bool)
        Assert.True(engine.ExecuteScript "array[0] === 1" :?> bool)
        Assert.True(engine.ExecuteScript "delete array" :?> bool)
        
    [<Fact(DisplayName = "ArrayLiteral: Many Primitive Elements")>]
    let test3 () =
        Assert.IsNotType<exn>(engine.ExecuteScript "var array = [1, '', true, null];")
        Assert.True(engine.ExecuteScript "typeof array === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "array instanceof Array" :?> bool)
        Assert.True(engine.ExecuteScript "array.length === 4" :?> bool)
        Assert.True(engine.ExecuteScript "array[0] === 1" :?> bool)
        Assert.True(engine.ExecuteScript "array[1] === ''" :?> bool)
        Assert.True(engine.ExecuteScript "array[2] === true" :?> bool)
        Assert.True(engine.ExecuteScript "array[3] === null" :?> bool)
        Assert.True(engine.ExecuteScript "delete array" :?> bool)
        
    [<Fact(DisplayName = "ArrayLiteral: Single array element with many primitive elements.")>]
    let test4 () =
        Assert.IsNotType<exn>(engine.ExecuteScript "var array = [[1,2]];")
        Assert.True(engine.ExecuteScript "typeof array === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "array instanceof Array" :?> bool)
        Assert.True(engine.ExecuteScript "array.length === 1" :?> bool)
        Assert.IsNotType<exn>(engine.ExecuteScript "var inner = array[0];")
        Assert.True(engine.ExecuteScript "typeof inner === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "inner instanceof Array" :?> bool)
        Assert.True(engine.ExecuteScript "inner.length === 2" :?> bool)
        Assert.True(engine.ExecuteScript "inner[0] === 1" :?> bool)
        Assert.True(engine.ExecuteScript "inner[1] === 2" :?> bool)
        Assert.True(engine.ExecuteScript "delete array" :?> bool)

    [<Fact(DisplayName = "ArrayLiteral: Empty Array with Elision")>]
    let test5 () =
        Assert.IsNotType<exn>(engine.ExecuteScript "var array = [,,,];")
        Assert.True(engine.ExecuteScript "typeof array === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "array instanceof Array" :?> bool)
        Assert.True(engine.ExecuteScript "array.length === 2" :?> bool)
        Assert.True(engine.ExecuteScript "delete array" :?> bool)
        
    [<Fact(DisplayName = "ArrayLiteral: Single Primitive Element with trailing Elision")>]
    let test6 () =
        Assert.IsNotType<exn>(engine.ExecuteScript "var array = [1,,,];")
        Assert.True(engine.ExecuteScript "typeof array === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "array instanceof Array" :?> bool)
        Assert.Equal(3.0, engine.ExecuteScript "array.length" :?> double)
        Assert.True(engine.ExecuteScript "array[0] === 1" :?> bool)
        Assert.True(engine.ExecuteScript "delete array" :?> bool)        
        
    [<Fact(DisplayName = "ArrayLiteral: Two primitive elements with separated by Elision.")>]
    let test7 () =
        Assert.IsNotType<exn>(engine.ExecuteScript "var array = [1,,,2];")
        Assert.True(engine.ExecuteScript "typeof array === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "array instanceof Array" :?> bool)
        Assert.True(engine.ExecuteScript "array.length === 4" :?> bool)
        Assert.True(engine.ExecuteScript "array[0] === 1" :?> bool)
        Assert.True(engine.ExecuteScript "array[3] === 2" :?> bool)
        Assert.True(engine.ExecuteScript "delete array" :?> bool)
