namespace Machete

module TypeConversionTests =

    open Xunit

    let engine = Engine()
    
    [<Fact(DisplayName = "Type Conversion: x -> Boolean")>]
    let test1 () =
        Assert.False(engine.ExecuteScript "!!undefined" :?> bool)
        Assert.False(engine.ExecuteScript "!!null" :?> bool)
        Assert.False(engine.ExecuteScript "!!+0" :?> bool)
        Assert.False(engine.ExecuteScript "!!-0" :?> bool)
        Assert.False(engine.ExecuteScript "!!NaN" :?> bool)
        Assert.False(engine.ExecuteScript "!!''" :?> bool)
        Assert.True(engine.ExecuteScript "!!'A'" :?> bool)
        Assert.True(engine.ExecuteScript "!!{}" :?> bool)

    [<Fact(DisplayName = "Type Conversion: x -> Number")>]
    let test2 () =
        Assert.False(System.Double.IsNaN(engine.ExecuteScript "+undefined" :?> double))
        Assert.True(engine.ExecuteScript "+null === 0" :?> bool)
        Assert.True(engine.ExecuteScript "+true === 1" :?> bool)
        Assert.True(engine.ExecuteScript "+false === 0" :?> bool)
        Assert.True(engine.ExecuteScript "+'1' === 1" :?> bool)
        Assert.True(engine.ExecuteScript "+{ valueOf: function() { return 100; }} === 100" :?> bool)
                
    [<Fact(DisplayName = "Type Conversion: x -> String")>]
    let test3 () =
        Assert.True(engine.ExecuteScript "'' + undefined === 'undefined'" :?> bool)
        Assert.True(engine.ExecuteScript "'' + null === 'null'" :?> bool)
        Assert.True(engine.ExecuteScript "'' + true === 'true'" :?> bool)
        Assert.True(engine.ExecuteScript "'' + false === 'false'" :?> bool)
        Assert.True(engine.ExecuteScript "'' + 1 === '1'" :?> bool)
        Assert.True(engine.ExecuteScript "+'1' === 1" :?> bool)
        Assert.True(engine.ExecuteScript "'' + { toString: function() { return 'Test'; }} === 'Test'" :?> bool)




