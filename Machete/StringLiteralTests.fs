namespace Machete

module StringLiteralTests =
            
    open Xunit

    let engine = Engine()

    let makeString input =
        engine.ExecuteScript input :?> string
                    
    [<Fact(DisplayName = "StringLiteral: Basic")>]
    let basic () =
        Assert.Equal<string> ("string", makeString "\"string\"")
        Assert.Equal<string> ("string", makeString "'string'")
        
    [<Fact(DisplayName = "StringLiteral: LineContinuation")>]
    let lineContinuation () =
        Assert.Equal<string> ("", makeString "'\\\r'") 
        Assert.Equal<string> ("", makeString "\"\\\r\"")
                
    [<Fact(DisplayName = "StringLiteral: Zero Char")>]
    let zeroChar () =
        Assert.Equal<string> ("\u0000", makeString "'\\0'") 
        Assert.Equal<string> ("\u0000", makeString "\"\\0\"")
                
    [<Fact(DisplayName = "StringLiteral: SingleEscapeCharacter")>]
    let singleEscapeCharacter () =
        Assert.Equal<string> ("'\"\\\b\f\n\r\t\v", makeString "'\\'\\\"\\\\\\b\\f\\n\\r\\t\\v'")        
        Assert.Equal<string> ("'\"\\\b\f\n\r\t\v", makeString "\"\\'\\\"\\\\\\b\\f\\n\\r\\t\\v\"")
                
    [<Fact(DisplayName = "StringLiteral: HexEscapeSequence")>]
    let hexEscapeSequence () =
        Assert.Equal<string> ("\u0001", makeString "'\\x01'")
        Assert.Equal<string> ("\u00F0", makeString "\"\\xF0\"")
                
    [<Fact(DisplayName = "StringLiteral: UnicodeEscapeSequence")>]
    let unicodeEscapeSequence () =
        Assert.Equal<string> ("\u0001", makeString "'\\u0001'")
        Assert.Equal<string> ("\u00F0", makeString "\"\\u00F0\"")

    [<Fact(DisplayName = "StringLiteral: Cannot Contain LineTerminator")>]
    let noTrailingIdentifierStartOrDecimalDigit () =
        Assert.IsAssignableFrom<exn>(engine.ExecuteScript "'\r'") |> ignore
        Assert.IsAssignableFrom<exn>(engine.ExecuteScript "\"\r\"") |> ignore

