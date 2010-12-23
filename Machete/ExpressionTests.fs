namespace Machete

module ExpressionTests =

    open Xunit

    let engine = Engine()

    [<Fact(DisplayName = "Expression: All side effects are evaluated when using the comma operator.")>]
    let expression1 () =
        Assert.Equal<bool>(true, engine.ExecuteScript "var x = false; x = true, false; x;" :?> bool)
    
    [<Fact(DisplayName = "Expression: Simple Assignment")>]
    let assignment1 () =
        Assert.Equal<double>(2.0, engine.ExecuteScript "var x = 1; x = 2; x;" :?> double)
        
    [<Fact(DisplayName = "Expression: Compound Assignment")>]
    let assignment2 () =
        Assert.Equal<double>(2.0, engine.ExecuteScript "var x = 1; x *= 2; x;" :?> double)
        Assert.Equal<double>(0.5, engine.ExecuteScript "var x = 1; x /= 2; x;" :?> double)
        Assert.Equal<double>(0.0, engine.ExecuteScript "var x = 10; x %= 2; x;" :?> double)
        Assert.Equal<double>(3.0, engine.ExecuteScript "var x = 1; x += 2; x;" :?> double)
        Assert.Equal<double>(-1.0, engine.ExecuteScript "var x = 1; x -= 2; x;" :?> double)
        Assert.Equal<double>(2.0, engine.ExecuteScript "var x = 1; x <<= 1; x;" :?> double)
        Assert.Equal<double>(1.0, engine.ExecuteScript "var x = 2; x >>= 1; x;" :?> double)
        Assert.Equal<double>(1.0, engine.ExecuteScript "var x = 2; x >>>= 1; x;" :?> double)
        Assert.Equal<double>(1.0, engine.ExecuteScript "var x = 1; x &= 1; x;" :?> double)
        Assert.Equal<double>(3.0, engine.ExecuteScript "var x = 2; x ^= 1; x;" :?> double)
        Assert.Equal<double>(3.0, engine.ExecuteScript "var x = 2; x |= 1; x;" :?> double)

    [<Fact(DisplayName = "Expression: Condition")>]
    let condition () =
        Assert.Equal<bool>(true, engine.ExecuteScript "1 === 1 ? true : false" :?> bool)

    [<Fact(DisplayName = "Expression: Logical OR")>]
    let logicalORExpression () =
        Assert.Equal<bool>(true, engine.ExecuteScript "true || true" :?> bool)
        Assert.Equal<bool>(true, engine.ExecuteScript "false || true" :?> bool)
        Assert.Equal<bool>(true, engine.ExecuteScript "0 || true" :?> bool)
        Assert.Equal<bool>(true, engine.ExecuteScript "'' || true" :?> bool)
        Assert.Equal<bool>(true, engine.ExecuteScript "\"\" || true" :?> bool)
        Assert.Equal<bool>(true, engine.ExecuteScript "null || true" :?> bool)
        Assert.Equal<bool>(true, engine.ExecuteScript "undefined || true" :?> bool)    
    
    [<Fact(DisplayName = "Expression: Logical AND")>]
    let logicalANDExpression () =
        Assert.Equal<bool>(true, engine.ExecuteScript "true && true" :?> bool)
        Assert.Equal<bool>(false, engine.ExecuteScript "false && true" :?> bool)
        Assert.Equal<double>(0.0, engine.ExecuteScript "0 && true" :?> double)
        Assert.Equal<string>("", engine.ExecuteScript "'' && true" :?> string)
        Assert.Equal<string>("", engine.ExecuteScript "\"\" && true" :?> string)
        Assert.Equal<obj>(null, engine.ExecuteScript "null && true")
        Assert.Equal<string>("undefined", engine.ExecuteScript "undefined && true" :?> string)

    [<Fact(DisplayName = "Expression: Bitwise OR")>]
    let bitwiseORExpression () =
        Assert.Equal<double>(3.0, engine.ExecuteScript "1 | 2" :?> double)
        Assert.Equal<double>(2.0, engine.ExecuteScript "'' | 2" :?> double)
        Assert.Equal<double>(2.0, engine.ExecuteScript "\"\" | 2" :?> double)
        Assert.Equal<double>(2.0, engine.ExecuteScript "null | 2" :?> double)
        Assert.Equal<double>(7.0, engine.ExecuteScript "3.2 | 4" :?> double)

    [<Fact(DisplayName = "Expression: Bitwise XOR")>]
    let bitwiseXORExpression () =
        Assert.Equal<double>(2.0, engine.ExecuteScript "0 ^ 2" :?> double)
        Assert.Equal<double>(2.0, engine.ExecuteScript "'' ^ 2" :?> double)
        Assert.Equal<double>(2.0, engine.ExecuteScript "\"\" ^ 2" :?> double)
        Assert.Equal<double>(5.0, engine.ExecuteScript "null ^ 5" :?> double)
        Assert.Equal<double>(7.0, engine.ExecuteScript "3.2 ^ 4" :?> double)
        Assert.Equal<double>(0.0, engine.ExecuteScript "3.2 ^ 3" :?> double)

    [<Fact(DisplayName = "Expression: Bitwise AND")>]
    let bitwiseANDExpression () =
        Assert.Equal<double>(0.0, engine.ExecuteScript "0 & 2" :?> double)
        Assert.Equal<double>(0.0, engine.ExecuteScript "1 & 2" :?> double)
        Assert.Equal<double>(0.0, engine.ExecuteScript "null & 2" :?> double)
        Assert.Equal<double>(0.0, engine.ExecuteScript "'test' & 3" :?> double)
        
    [<Fact(DisplayName = "Expression: Equals")>]
    let equals () =
        Assert.True(engine.ExecuteScript "1 == 1" :?> bool)
        Assert.False(engine.ExecuteScript "1 == 2" :?> bool)
        Assert.True(engine.ExecuteScript "'' == 0" :?> bool)
        Assert.False(engine.ExecuteScript "'left' == 'right'" :?> bool)
        
    [<Fact(DisplayName = "Expression: Does Not Equals")>]
    let doesNotEquals () =
        Assert.False(engine.ExecuteScript "1 != 1" :?> bool)
        Assert.True(engine.ExecuteScript "1 != 2" :?> bool)
        Assert.False(engine.ExecuteScript "'' != 0" :?> bool)
        Assert.True(engine.ExecuteScript "'left' != 'right'" :?> bool)

    [<Fact(DisplayName = "Expression: Strict Equals")>]
    let strictEquals () =
        Assert.True(engine.ExecuteScript "1 === 1" :?> bool)
        Assert.False(engine.ExecuteScript "1 === 2" :?> bool)
        Assert.True(engine.ExecuteScript "'' === ''" :?> bool)
        Assert.False(engine.ExecuteScript "'left' === 'right'" :?> bool)

    [<Fact(DisplayName = "Expression: Strict Does Not Equals")>]
    let strictDoesNotEquals () =
        Assert.False(engine.ExecuteScript "1 !== 1" :?> bool)
        Assert.True(engine.ExecuteScript "1 !== 2" :?> bool)
        Assert.False(engine.ExecuteScript "'' !== ''" :?> bool)
        Assert.True(engine.ExecuteScript "'left' !== 'right'" :?> bool)
        
    [<Fact(DisplayName = "Expression: Less Than")>]
    let lessThan () =
        Assert.False(engine.ExecuteScript "1 < 1" :?> bool)
        
    [<Fact(DisplayName = "Expression: Greater Than")>]
    let greaterThan () =
        Assert.False(engine.ExecuteScript "1 > 1" :?> bool)
        
    [<Fact(DisplayName = "Expression: Less Than or Equal")>]
    let lessThanOrEqual () =
        Assert.True(engine.ExecuteScript "1 <= 1" :?> bool)
        
    [<Fact(DisplayName = "Expression: Greater Than or Equal")>]
    let greaterThanOrEqual () =
        Assert.True(engine.ExecuteScript "1 >= 1" :?> bool)
        
    [<Fact(DisplayName = "Expression: Left Shift")>]
    let leftShift () =
        Assert.Equal<double>(2.0, engine.ExecuteScript "1 << 1" :?> double)
        
    [<Fact(DisplayName = "Expression: Signed Right Shift")>]
    let signedRightShift () =
        Assert.Equal<double>(1.0, engine.ExecuteScript "2 >> 1" :?> double)
        
    [<Fact(DisplayName = "Expression: Unsigned Right Shift")>]
    let unsignedRightShift () =
        Assert.Equal<double>(1.0, engine.ExecuteScript "2 >>> 1" :?> double)
        
    [<Fact(DisplayName = "Expression: Addition")>]
    let addition () =
        Assert.Equal<double>(1.0, engine.ExecuteScript "0 + 1" :?> double)
        
    [<Fact(DisplayName = "Expression: Subtraction")>]
    let subtraction () =
        Assert.Equal<double>(1.0, engine.ExecuteScript "2 - 1" :?> double)
        
    [<Fact(DisplayName = "Expression: Multiplication")>]
    let multiplication () =
        Assert.Equal<double>(4.0, engine.ExecuteScript "2 * 2" :?> double)
        
    [<Fact(DisplayName = "Expression: Division")>]
    let division () =
        Assert.Equal<double>(0.5, engine.ExecuteScript "1 / 2" :?> double)
        
    [<Fact(DisplayName = "Expression: Modulus")>]
    let modulus () =
        Assert.Equal<double>(0.0, engine.ExecuteScript "10 % 2" :?> double)

    [<Fact(DisplayName = "Expression: Delete")>]
    let delete () =
        Assert.True(engine.ExecuteScript "delete 1" :?> bool)
        engine.ExecuteScript "var o = {};" |> ignore
        Assert.True(engine.ExecuteScript "delete o" :?> bool)
        
    [<Fact(DisplayName = "Expression: Void")>]
    let _void () =
        Assert.Equal<string>("undefined", engine.ExecuteScript "void 1" :?> string)

    [<Fact(DisplayName = "Expression: Typeof")>]
    let typeof () =
        Assert.Equal<string>("undefined", engine.ExecuteScript "typeof undefined" :?> string)
        Assert.Equal<string>("object", engine.ExecuteScript "typeof null" :?> string)
        Assert.Equal<string>("boolean", engine.ExecuteScript "typeof true" :?> string)
        Assert.Equal<string>("number", engine.ExecuteScript "typeof 0" :?> string)
        Assert.Equal<string>("string", engine.ExecuteScript "typeof ''" :?> string)
        Assert.Equal<string>("object", engine.ExecuteScript "typeof {}" :?> string)
        Assert.Equal<string>("function", engine.ExecuteScript "typeof function(){}" :?> string)
        
    [<Fact(DisplayName = "Expression: Prefix Increment")>]
    let prefixIncrement () =
        Assert.Equal<double>(2.0, engine.ExecuteScript "var x = 1; ++x;" :?> double)
        
    [<Fact(DisplayName = "Expression: Prefix Decrement")>]
    let prefixDecrement () =
        Assert.Equal<double>(0.0, engine.ExecuteScript "var x = 1; --x;" :?> double)
        
    [<Fact(DisplayName = "Expression: Unary Plus")>]
    let unaryPlus () =
        Assert.Equal<double>(0.0, engine.ExecuteScript "+false" :?> double)
        
    [<Fact(DisplayName = "Expression: Unary Minus")>]
    let unaryMinus () =
        Assert.Equal<double>(-1.0, engine.ExecuteScript "-1" :?> double)
        
    [<Fact(DisplayName = "Expression: Bitwise Not")>]
    let bitwiseNot () =
        Assert.Equal<double>((~~~10)|>double, engine.ExecuteScript "~10" :?> double)

    [<Fact(DisplayName = "Expression: Logical Not")>]
    let logicalNot () =
        Assert.Equal<bool>(true, engine.ExecuteScript "!false" :?> bool)
        
    [<Fact(DisplayName = "Expression: Postfix Increment")>]
    let postfixIncrement () =
        Assert.Equal<double>(1.0, engine.ExecuteScript "var x = 1; x++;" :?> double)
        Assert.Equal<double>(2.0, engine.ExecuteScript "x" :?> double)
        
    [<Fact(DisplayName = "Expression: Postfix Decrement")>]
    let postfixDecrement () =
        Assert.Equal<double>(1.0, engine.ExecuteScript "var x = 1; x--;" :?> double)
        Assert.Equal<double>(0.0, engine.ExecuteScript "x" :?> double)

    [<Fact(DisplayName = "Expression: Empty ObjectLiteral")>]
    let objectLiteral1 () =
        engine.ExecuteScript "var o = {};" |> ignore
        Assert.True(engine.ExecuteScript "typeof o === 'object'" :?> bool)
        
    [<Fact(DisplayName = "Expression: ObjectLiteral with only data properties.")>]
    let objectLiteral2 () =
        engine.ExecuteScript "var o = { age : 10, name : 'Billy', active : true };" |> ignore
        Assert.True(engine.ExecuteScript "typeof o === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "o.age === 10" :?> bool)
        Assert.True(engine.ExecuteScript "o.name === 'Billy'" :?> bool)
        Assert.True(engine.ExecuteScript "o.active" :?> bool)
        
    [<Fact(DisplayName = "Expression: ObjectLiteral with getter and setter.")>]
    let objectLiteral3 () =
        engine.ExecuteScript "var n = 'Susan', o = { get age () { return 2; }, get name () { return n; }, set name (v) { n = v; } };" |> ignore         
        Assert.True(engine.ExecuteScript "typeof o === 'object'" :?> bool)
        Assert.True(engine.ExecuteScript "o.age === 2" :?> bool)
        Assert.True(engine.ExecuteScript "o.name === 'Susan'" :?> bool)
        engine.ExecuteScript "o.name = 'Timmy'" |> ignore
        Assert.True(engine.ExecuteScript "o.name === 'Timmy'" :?> bool)


