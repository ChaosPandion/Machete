namespace Machete.Compiler

open System

module FloatParser =

    open FParsec.CharParsers
    open FParsec.Primitives

    let private floatParser replay =
        (parse {
            do! optional StringNumericLiteral.parseStrWhiteSpace
            let! result = StringNumericLiteral.parseStrDecimalLiteral
            return result
        }) replay

    let Parse (text:string) =
        let result = run floatParser text
        match result with
        | Success (value, _, _) ->
            StringNumericLiteral.evalStrDecimalLiteral value 
        | _ -> Double.NaN 