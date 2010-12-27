namespace Machete

module JsonTests =

    open Xunit

    let engine = Engine()

    [<Fact(DisplayName = "JSON: Empty Array")>]
    let test1 () =
        Assert.IsNotType<exn>(engine.ExecuteScript "var array = JSON.parse('[]');")
        Assert.True(engine.ExecuteScript "typeof array === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "array instanceof Array" :?> bool)
        Assert.True(engine.ExecuteScript "array.length === 0" :?> bool)
        Assert.True(engine.ExecuteScript "delete array" :?> bool)
