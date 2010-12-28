namespace Machete

module JsonTests =

    open Xunit

    let engine = Engine()
    
    [<Fact(DisplayName = "JSON: String")>]
    let test1 () =
        Assert.IsNotType<exn>(engine.ExecuteScript "var r = JSON.parse('\"Test\"');")
        Assert.True(engine.ExecuteScript "typeof r === 'string'" :?> bool)
        Assert.True(engine.ExecuteScript "r === 'Test'" :?> bool)

    [<Fact(DisplayName = "JSON: Number")>]
    let test2 () =
        Assert.IsNotType<exn>(engine.ExecuteScript "var r = JSON.parse('22');")
        Assert.True(engine.ExecuteScript "typeof r === 'number'" :?> bool)
        Assert.True(engine.ExecuteScript "r === 22" :?> bool)

    [<Fact(DisplayName = "JSON: Boolean")>]
    let test3 () =
        Assert.IsNotType<exn>(engine.ExecuteScript "var r = JSON.parse('true');")
        Assert.True(engine.ExecuteScript "typeof r === 'boolean'" :?> bool)
        Assert.True(engine.ExecuteScript "r" :?> bool)
        Assert.IsNotType<exn>(engine.ExecuteScript "var r = JSON.parse('false');")
        Assert.True(engine.ExecuteScript "typeof r === 'boolean'" :?> bool)
        Assert.True(engine.ExecuteScript "!r" :?> bool)

    [<Fact(DisplayName = "JSON: Null")>]
    let test4 () =
        Assert.IsNotType<exn>(engine.ExecuteScript "var r = JSON.parse('null');")
        Assert.True(engine.ExecuteScript "typeof r === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "r === null" :?> bool)

    [<Fact(DisplayName = "JSON: Empty Array")>]
    let test5 () =
        Assert.IsNotType<exn>(engine.ExecuteScript "var array = JSON.parse('[]');")
        Assert.True(engine.ExecuteScript "typeof array === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "array instanceof Array" :?> bool)
        Assert.True(engine.ExecuteScript "array.length === 0" :?> bool)
        Assert.True(engine.ExecuteScript "delete array" :?> bool)

    [<Fact(DisplayName = "JSON: Array with primitive values")>]
    let test6 () =
        Assert.IsNotType<exn>(engine.ExecuteScript "var array = JSON.parse('[1, \"\", true]');")
        Assert.True(engine.ExecuteScript "typeof array === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "array instanceof Array" :?> bool)
        Assert.True(engine.ExecuteScript "array.length === 3" :?> bool)
        Assert.True(engine.ExecuteScript "array[0] === 1" :?> bool)
        Assert.True(engine.ExecuteScript "array[1] === ''" :?> bool)
        Assert.True(engine.ExecuteScript "array[2] === true" :?> bool)
        Assert.True(engine.ExecuteScript "delete array" :?> bool)
        
    [<Fact(DisplayName = "JSON: Stringify empty array")>]
    let test7 () =
        Assert.True(engine.ExecuteScript "JSON.stringify([]) === '[]';" :?> bool)
        
    [<Fact(DisplayName = "JSON: Stringify array with primitive values")>]
    let test8 () =
        Assert.True(engine.ExecuteScript "JSON.stringify([1, 2, 3, 4, 5]) === '[1,2,3,4,5]';" :?> bool)

    [<Fact(DisplayName = "JSON: Stringify nested array")>]
    let test9 () =
        Assert.True(engine.ExecuteScript "JSON.stringify([1, [1, 2, 3]]) === '[1,[1,2,3]]';" :?> bool)