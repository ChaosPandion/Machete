namespace Machete.Tests

open Machete
open Xunit

module StringTests =

    let engine = Engine()
    
    [<Fact>]
    let strings () =
        Assert.Equal<string> ("", engine.ExecuteScript "\"\"" :?> string)
        Assert.Equal<string> ("a", engine.ExecuteScript "\"a\"" :?> string)
        Assert.Equal<string> ("a\r", engine.ExecuteScript "\"a\\r\"" :?> string)
        Assert.Equal<string> ("", engine.ExecuteScript "''" :?> string)
        Assert.Equal<string> ("a", engine.ExecuteScript "'a'" :?> string)
        Assert.Equal<string> ("a\r", engine.ExecuteScript "'a\\r'" :?> string)
        
    [<Fact>]
    let hexEscapeSequence () =
        Assert.Equal<string> ("\u0001", engine.ExecuteScript "'\\x01'" :?> string)
        Assert.Equal<string> ("\u00F0", engine.ExecuteScript "\"\\xF0\"" :?> string)

    [<Fact>]
    let unicodeEscapeSequence () =
        Assert.Equal<string> ("\u0001", engine.ExecuteScript "'\\u0001'" :?> string)
        Assert.Equal<string> ("\u00F0", engine.ExecuteScript "\"\\u00F0\"" :?> string)