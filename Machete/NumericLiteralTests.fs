namespace Machete

module NumericLiteralTests =

    open Xunit
    
    let engine = Engine()

    let makeNumber input =
        engine.ExecuteScript input :?> double
                    
    [<Fact(DisplayName = "NumericLiteral: DecimalLiteral With DecimalIntegerLiteral, decimal point")>]
    let decimalLiteralWithDecimalIntegerLiteralDecimalPoint () =
        Assert.Equal<double> (999.0, makeNumber "999.")

    [<Fact(DisplayName = "NumericLiteral: DecimalLiteral With DecimalIntegerLiteral, decimal point, DecimalDigits")>]
    let decimalLiteralWithDecimalIntegerLiteralDecimalPointDecimalDigits () =
        Assert.Equal<double> (999.999, makeNumber "999.999")
                
    [<Fact(DisplayName = "NumericLiteral: DecimalLiteral With DecimalIntegerLiteral, decimal point, ExponentPart")>]
    let decimalLiteralWithDecimalIntegerLiteralDecimalPointExponentPart () =
        Assert.Equal<double> (999e2, makeNumber "999E2")
        Assert.Equal<double> (999e+2, makeNumber "999e+2")
        Assert.Equal<double> (999e-2, makeNumber "999e-2")
                
    [<Fact(DisplayName = "NumericLiteral: DecimalLiteral With DecimalIntegerLiteral, decimal point, DecimalDigits, ExponentPart")>]
    let decimalLiteralWithDecimalIntegerLiteralDecimalPointDecimalDigitsExponentPart () =
        Assert.Equal<double> (9999.999e2, makeNumber "9999.999E2")
        Assert.Equal<double> (9999.999e+2, makeNumber "9999.999e+2")
        Assert.Equal<double> (9999.999e-2, makeNumber "9999.999e-2")
                
    [<Fact(DisplayName = "NumericLiteral: DecimalLiteral With decimal point, DecimalDigits")>]
    let decimalLiteralWithDecimalPointDecimalDigits () =
        Assert.Equal<double> (0.999, makeNumber ".999")

    [<Fact(DisplayName = "NumericLiteral: DecimalLiteral With decimal point, DecimalDigits, ExponentPart")>]
    let decimalLiteralWithDecimalPointDecimalDigitsExponentPart () =
        Assert.Equal<double> (0.999e2, makeNumber ".999E2")
        Assert.Equal<double> (0.999e+2, makeNumber ".999e+2")
        Assert.Equal<double> (0.999e-2, makeNumber ".999e-2")
                
    [<Fact(DisplayName = "NumericLiteral: DecimalLiteral With DecimalIntegerLiteral")>]
    let decimalLiteralWithDecimalIntegerLiteral () =
        Assert.Equal<double> (999.0, makeNumber "999")

    [<Fact(DisplayName = "NumericLiteral: DecimalLiteral With DecimalIntegerLiteral, ExponentPart")>]
    let decimalLiteralWithDecimalIntegerLiteralExponentPart () =
        Assert.Equal<double> (999e2, makeNumber "999E2")
        Assert.Equal<double> (999e+2, makeNumber "999e+2")
        Assert.Equal<double> (999e-2, makeNumber "999e-2")

    [<Fact(DisplayName = "NumericLiteral: HexIntegerLiteral")>]
    let hexIntegerLiteral () =
        Assert.Equal<double> (0x0 |> double, makeNumber "0x0")
        Assert.Equal<double> (0xF |> double, makeNumber "0xF")
        Assert.Equal<double> (0xF |> double, makeNumber "0XF")
        Assert.Equal<double> (0xFF |> double, makeNumber "0xFF")
        Assert.Equal<double> (0xFF |> double, makeNumber "0XFF")
        Assert.Equal<double> (0x123456 |> double, makeNumber "0x123456")
        Assert.Equal<double> (0xABCDEF |> double, makeNumber "0xABCDEF")
        Assert.IsAssignableFrom<exn>(engine.ExecuteScript "0xAG") |> ignore

    [<Fact(DisplayName = "NumericLiteral: No Trailing IdentifierStart Or DecimalDigit")>]
    let noTrailingIdentifierStartOrDecimalDigit () =
        Assert.IsAssignableFrom<exn>(engine.ExecuteScript "3in") |> ignore
