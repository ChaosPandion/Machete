namespace Machete.Tests

open Machete
open Xunit

module NumberTests =
    
    let engine = Engine()

    [<Fact>]
    let hexIntegerLiteral () =
        Assert.Equal<double> (0x0 |> double, engine.ExecuteScript "0x0" :?> double)
        Assert.Equal<double> (0xF |> double, engine.ExecuteScript "0xF" :?> double)
        Assert.Equal<double> (15.0, engine.ExecuteScript "0XF" :?> double)
        Assert.Equal<double> (255.0, engine.ExecuteScript "0xFF" :?> double)
        Assert.Equal<double> (255.0, engine.ExecuteScript "0XFF" :?> double)
    
    [<Fact>]
    let decimalLiteral () =
        Assert.Equal<double> (1.0, engine.ExecuteScript "1" :?> double)
        Assert.Equal<double> (1.2, engine.ExecuteScript "1.2" :?> double)
        Assert.Equal<double> (1.2e2, engine.ExecuteScript "1.2e2" :?> double)
        Assert.Equal<double> (120.0, engine.ExecuteScript "1.2e+2" :?> double)
        Assert.Equal<double> (0.012, engine.ExecuteScript "1.2e-2" :?> double)
        Assert.Equal<double> (0.2, engine.ExecuteScript ".2" :?> double)
        Assert.Equal<double> (0.2e2, engine.ExecuteScript ".2e2" :?> double)
        Assert.Equal<double> (0.2e+2, engine.ExecuteScript ".2e+2" :?> double)
        Assert.Equal<double> (0.2e-2, engine.ExecuteScript ".2e-2" :?> double)
        Assert.Equal<double> (1e2, engine.ExecuteScript "1e2" :?> double)

    [<Fact>]
    let noTrailingIdentifierStartOrDecimalDigit () =
        Assert.IsAssignableFrom<exn>(engine.ExecuteScript "3in")