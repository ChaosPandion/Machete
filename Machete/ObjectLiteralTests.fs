namespace Machete

module ObjectLiteralTests =

    open Xunit

    let engine = Engine()

    
    [<Fact(DisplayName = "ObjectLiteral: Empty")>]
    let test1 () =
        engine.ExecuteScript "var o = {};" |> ignore
        Assert.True(engine.ExecuteScript "typeof o === 'object'" :?> bool)
        
    [<Fact(DisplayName = "ObjectLiteral: Contains only data properties.")>]
    let test2 () =
        engine.ExecuteScript "var o = { age : 10, name : 'Billy', active : true };" |> ignore
        Assert.True(engine.ExecuteScript "typeof o === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "o.age === 10" :?> bool)
        Assert.True(engine.ExecuteScript "o.name === 'Billy'" :?> bool)
        Assert.True(engine.ExecuteScript "o.active" :?> bool)
        
    [<Fact(DisplayName = "ObjectLiteral: ObjectLiteral with getter and setter.")>]
    let test3 () =
        engine.ExecuteScript "var n = 'Susan', o = { get age () { return 2; }, get name () { return n; }, set name (v) { n = v; } };" |> ignore         
        Assert.True(engine.ExecuteScript "typeof o === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "o.age === 2" :?> bool)
        Assert.True(engine.ExecuteScript "o.name === 'Susan'" :?> bool)
        engine.ExecuteScript "o.name = 'Timmy'" |> ignore
        Assert.True(engine.ExecuteScript "o.name === 'Timmy'" :?> bool)
