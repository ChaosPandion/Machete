namespace Machete.Tests

open Machete
open Xunit

module SimpleExpressions =

    let engine = Engine()


    [<Fact>]
    let logicalORExpression () =
        Assert.True (engine.ExecuteScript "true || false" :?> bool)
        Assert.True (engine.ExecuteScript "undefined || true" :?> bool)
        Assert.True (engine.ExecuteScript "null || true" :?> bool)
        Assert.True (engine.ExecuteScript "0 || true" :?> bool)
        Assert.True (engine.ExecuteScript "NaN || true" :?> bool)
        Assert.True (engine.ExecuteScript "\"\" || true" :?> bool)
     
       

    
    
        

