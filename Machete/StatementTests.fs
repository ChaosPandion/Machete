namespace Machete

module StatementTests =

    open Xunit

    let engine = Engine()
    
    [<Fact(DisplayName = "Statement: Empty Block")>]
    let test1 () =
        Assert.Equal<string>("undefined", engine.ExecuteScript "{ }" :?> string)

    [<Fact(DisplayName = "Statement: VariableStatement with VariableDeclaration but no Initialiser.")>]
    let test2 () =
        engine.ExecuteScript "var x;" |> ignore
        Assert.True(engine.ExecuteScript "typeof x === 'undefined'" :?> bool)
        engine.ExecuteScript "delete x;" |> ignore
        
    [<Fact(DisplayName = "Statement: VariableStatement with VariableDeclaration and Initialiser.")>]
    let test3 () =
        engine.ExecuteScript "var x = 1;" |> ignore
        Assert.True(engine.ExecuteScript "x === 1" :?> bool)
        engine.ExecuteScript "delete x;" |> ignore

    [<Fact(DisplayName = "Statement: VariableStatement with many VariableDeclaration but no Initialiser.")>]
    let test4 () =
        engine.ExecuteScript "var x, y, z;" |> ignore
        Assert.True(engine.ExecuteScript "typeof x === 'undefined'" :?> bool)
        Assert.True(engine.ExecuteScript "typeof y === 'undefined'" :?> bool)
        Assert.True(engine.ExecuteScript "typeof z === 'undefined'" :?> bool)
        engine.ExecuteScript "delete x; delete y; delete z;" |> ignore

    [<Fact(DisplayName = "Statement: VariableStatement with many VariableDeclaration and Initialiser.")>]
    let test5 () =
        engine.ExecuteScript "var x = 1, y = 2, z = 3;" |> ignore
        Assert.True(engine.ExecuteScript "x === 1" :?> bool)
        Assert.True(engine.ExecuteScript "y === 2" :?> bool)
        Assert.True(engine.ExecuteScript "z === 3" :?> bool)
        engine.ExecuteScript "delete x; delete y; delete z;" |> ignore
        
    [<Fact(DisplayName = "Statement: IfStatement with no else.")>]
    let test6 () =
        Assert.True(engine.ExecuteScript "if (true) return true;" :?> bool)
        
    [<Fact(DisplayName = "Statement: IfStatement with else.")>]
    let test7 () =
        Assert.True(engine.ExecuteScript "if (false) return false; else return true;" :?> bool)
        
    [<Fact(DisplayName = "Statement: IterationStatement -- for")>]
    let test8 () =
        let s = "
            var n = 1;
            for (var i = 5; i > 1; --i) {
                n *= i;
            }
            return n;
        "        
        Assert.Equal<double>(120.0, engine.ExecuteScript s :?> double)



