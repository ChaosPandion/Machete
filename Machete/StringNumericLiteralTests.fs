namespace Machete

module StringNumericLiteralTests =
            
    open Xunit

    let engine = Engine()
    
    let makeNumber input =
        engine.ExecuteScript input :?> double

    [<Fact(DisplayName = "StringNumericLiteral: Empty string converts to zero.")>]
    let test1 () =
        Assert.Equal<double> (0.0, makeNumber "+''")

    [<Fact(DisplayName = "StringNumericLiteral: Infinity")>]
    let test2 () =
        Assert.True (System.Double.IsInfinity (makeNumber "+'Infinity'"))

    [<Fact(DisplayName = "StringNumericLiteral: DecimalDigits")>]
    let test3 () =
        Assert.Equal<double> (999.0, makeNumber "+'999'")

    [<Fact(DisplayName = "StringNumericLiteral: DecimalDigits, decimal point")>]
    let test4 () =
        Assert.Equal<double> (999.0, makeNumber "+'999.'")
        
    [<Fact(DisplayName = "StringNumericLiteral: DecimalDigits, decimal point, DecimalDigits")>]
    let test5 () =
        Assert.Equal<double> (999.999, makeNumber "+'999.999'")
        
    [<Fact(DisplayName = "StringNumericLiteral: DecimalDigits, decimal point, DecimalDigits, ExponentPart")>]
    let test6 () =
        Assert.Equal<double> (999.2e2, makeNumber "+'999.2E2'")
        Assert.Equal<double> (999.2e+2, makeNumber "+'999.2e+2'")
        Assert.Equal<double> (999.2e-2, makeNumber "+'999.2e-2'")
        
    [<Fact(DisplayName = "StringNumericLiteral: DecimalDigits, ExponentPart")>]
    let test7 () =
        Assert.Equal<double> (999999E2, makeNumber "+'999999E2'")
        Assert.Equal<double> (999999e+2, makeNumber "+'999999e+2'")
        Assert.Equal<double> (999999e-2, makeNumber "+'999999e-2'")

    [<Fact(DisplayName = "StringNumericLiteral: decimal point, DecimalDigits")>]
    let test8 () =
        Assert.Equal<double> (0.999, makeNumber "+'.999'")
        
    [<Fact(DisplayName = "StringNumericLiteral: decimal point, DecimalDigits, ExponentPart")>]
    let test9 () =
        Assert.Equal<double> (0.29992e2, makeNumber "+'.29992E2'")
        Assert.Equal<double> (0.29992e+2, makeNumber "+'.29992e+2'")
        Assert.Equal<double> (0.29992e-2, makeNumber "+'.29992e-2'")
        
    [<Fact(DisplayName = "StringNumericLiteral: HexIntegerLiteral")>]
    let test10 () =
        Assert.Equal<double> (15.0, makeNumber "+'0xF'")
        Assert.Equal<double> (15.0, makeNumber "+'0XF'")
        
    [<Fact(DisplayName = "StringNumericLiteral: Allow leading white space.")>]
    let test11 () =
        Assert.Equal<double> (100.0, makeNumber "+'   100'")
        Assert.Equal<double> (15.0, makeNumber "+'    0XF'")

    [<Fact(DisplayName = "StringNumericLiteral: Allow trailing white space.")>]
    let test12 () =
        Assert.Equal<double> (100.0, makeNumber "+'100    '")
        Assert.Equal<double> (15.0, makeNumber "+'0XF    '")

    [<Fact(DisplayName = "StringNumericLiteral: Allow leading and trailing white space.")>]
    let test13 () =
        Assert.Equal<double> (100.0, makeNumber "+'   100    '")
        Assert.Equal<double> (15.0, makeNumber "+'   0XF    '")
        
    [<Fact(DisplayName = "StringNumericLiteral: Return NaN for failures.")>]
    let test14 () =
        Assert.True (System.Double.IsNaN (makeNumber "+'z'"))
        Assert.True (System.Double.IsNaN (makeNumber "+'q100'"))
