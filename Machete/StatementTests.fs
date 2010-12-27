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

    [<Fact(DisplayName = "VariableStatement ")>]
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
        
    [<Fact(DisplayName = "IfStatement: if ( Expression ) Statement")>]
    let test6 () =
        Assert.True(engine.ExecuteScript "if (true) return true;" :?> bool)
        
    [<Fact(DisplayName = "IfStatement: if ( Expression ) Statement  else Statement ")>]
    let test7 () =
        Assert.True(engine.ExecuteScript "if (false) return false; else return true;" :?> bool)

    [<Fact(DisplayName = "IterationStatement: The do-while Statement", Timeout = 5000)>]
    let test8 () =
        let s = "
            var n = 1;
            do {
                n++;
            } while (n < 100);
            return n;
        "        
        Assert.Equal<double>(100.0, engine.ExecuteScript s :?> double)
        
    [<Fact(DisplayName = "IterationStatement: The while Statement", Timeout = 5000)>]
    let test9 () =
        let s = "
            var n = 1;
            while (n < 100) {
                n++;
            }
            return n;
        "  
        Assert.Equal<double>(100.0, engine.ExecuteScript s :?> double)

    [<Fact(DisplayName = "IterationStatement: The for Statement with no optional expressions.")>]
    let test10 () =
        let s = "
            var n = 1;
            for (;;) {
                n *= 2;
                if (n > 1000) {
                    break;
                }
            }
            return n;
        "        
        Assert.True(engine.ExecuteScript s :?> double > 1000.0)

    [<Fact(DisplayName = "IterationStatement: The for Statement with break condition expression.")>]
    let test11 () =        
        let s = "
            var n = 1;
            for (; n < 1000;) {
                n *= 2;
            }
            return n;
        "        
        Assert.True(engine.ExecuteScript s :?> double > 1000.0)

    [<Fact(DisplayName = "IterationStatement: The for Statement with tail expression.")>]
    let test12 () =        
        let s = "
            var n = 1;
            for (;; n *= 2) {
                if (n > 1000) {
                    break;
                }
            }
            return n;
        "        
        Assert.True(engine.ExecuteScript s :?> double > 1000.0)

    [<Fact(DisplayName = "IterationStatement: The for Statement with break condition and tail expression")>]
    let test13 () =        
        let s = "
            var n = 1;
            for (; n < 1000; n *= 2) {

            }
            return n;
        "        
        Assert.True(engine.ExecuteScript s :?> double > 1000.0)

    [<Fact(DisplayName = "IterationStatement: The for Statement with variables only")>]
    let test14 () =
        let s = "
            var n = 1;
            for (var i = 5;;) {
                if (i <= 1) {
                    break;
                }
                n *= i;
                --i;
            }
            return n;
        "        
        Assert.Equal<double>(120.0, engine.ExecuteScript s :?> double)

    [<Fact(DisplayName = "IterationStatement: The for Statement with variables and break condition.")>]
    let test15 () =
        let s = "
            var n = 1;
            for (var i = 5; i > 1;) {
                n *= i--;
            }
            return n;
        "        
        Assert.Equal<double>(120.0, engine.ExecuteScript s :?> double)

    [<Fact(DisplayName = "IterationStatement: The for Statement")>]
    let test16 () =
        let s = "
            var n = 1;
            for (var i = 5; i > 1; --i) {
                n *= i;
            }
            return n;
        "        
        Assert.Equal<double>(120.0, engine.ExecuteScript s :?> double)

    [<Fact(DisplayName = "IterationStatement: The for-in Statement with LeftHandSideExpression")>]
    let test17 () =
        let s = "
            var i = 0, o = { a:1, b:2, c:3 };
            for ({} in o) {
                i++;
            }
            return i;
        "        
        Assert.Equal<double>(3.0, engine.ExecuteScript s :?> double)

    [<Fact(DisplayName = "IterationStatement: The for-in Statement with variable")>]
    let test18 () =
        let s = "
            var r = '', o = { a:1, b:2, c:3 };
            for (var n in o) {
                r += n;
            }
            return r;
        "        
        Assert.Equal<string>("abc", engine.ExecuteScript s :?> string)

    [<Fact(DisplayName = "ContinueStatement: No identifier")>]
    let test19 () =
        let s = "
            var i = 0, run = true;
            while (run) {
                i++;
                if (i === 1)
                    continue;
                run = false;
            }
            return i;
        "        
        Assert.Equal<double>(2.0, engine.ExecuteScript s :?> double)
        

    [<Fact(DisplayName = "BreakStatement: No identifier")>]
    let test20 () =
        let s = "
            var i = 0;
            while (true) {
                i++;
                if (i === 10)
                    break;
            }
            return i;
        "        
        Assert.Equal<double>(10.0, engine.ExecuteScript s :?> double)
        

    [<Fact(DisplayName = "ReturnStatement: No expression")>]
    let test21 () =
        let s = "return;"        
        Assert.Equal<string>("undefined", engine.ExecuteScript s :?> string)        

    [<Fact(DisplayName = "WithStatement")>]
    let test22 () =
        let s = "
            var n = 'C', r = 'A';
            with ({ n: 'B' }) {
                r += n;
            }
            return r + n;
        "        
        Assert.Equal<string>("ABC", engine.ExecuteScript s :?> string)
       
    [<Fact(DisplayName = "SwitchStatement")>]
    let test23 () =
        let s = "
            var n = 1;
            switch (n) {
                case 1: 
                    return true;
                default:
                    return false;
            }
        "        
        Assert.True(engine.ExecuteScript s :?> bool)
         
    [<Fact(DisplayName = "ThrowStatement")>]
    let test24 () =
        let s = "throw 'Error';"
        Assert.IsAssignableFrom<exn>(engine.ExecuteScript s) |> ignore  

    [<Fact(DisplayName = "TryStatement: Try, Catch")>]
    let test25 () =
        let s = "
            var s = '';
            try {
                s = 'A';
                throw 'B';
            } catch (e) {
                s = e;
            }
            return s;
        "        
        Assert.Equal<string>("B", engine.ExecuteScript s :?> string)

    [<Fact(DisplayName = "TryStatement: Try, Finally")>]
    let test26 () =
        let s = "
            var s = 'A';
            try {
                throw 'Fail!';
            } finally {
                s = 'B';
            }
        "
        Assert.IsAssignableFrom<exn>(engine.ExecuteScript s) |> ignore        
        Assert.Equal<string>("B", engine.ExecuteScript "s;" :?> string)

    [<Fact(DisplayName = "TryStatement: Try, Catch, Finally")>]
    let test27 () =
        let s = "
            var s = 'A';
            try {
                throw 'B';
            } catch (e) {
                s += e;
            } finally {
                s += 'C';
            }
            return s;
        "      
        Assert.Equal<string>("ABC", engine.ExecuteScript s :?> string)

    [<Fact(DisplayName = "LabelledStatement: BreakStatement")>]
    let test28 () =
        let s = "
            var i = 0, b = true;
            Test: while (b) {
                while (++i < 10) {
                    break Test;
                }
                b = false;
            }
            return i + b;
        "        
        Assert.Equal<double>(2.0, engine.ExecuteScript s :?> double)

    [<Fact(DisplayName = "LabelledStatement: ContinueStatement")>]
    let test29 () =
        let s = "
            var i = 0;
            Test: while (++i < 10) {
                while (true) {
                    continue Test;
                }
            }
            return i;
        "        
        Assert.Equal<double>(10.0, engine.ExecuteScript s :?> double)
