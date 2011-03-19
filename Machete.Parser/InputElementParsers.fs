namespace Machete.Parser

open System
open FParsec
open FParsec.Primitives
open FParsec.CharParsers
open Productions

module InputElementParsers =

    let rec parseWhiteSpace state =
        (parse {
            let! c, p = tuple2 (satisfy isWhiteSpace) getPosition
            return WhiteSpace (c, { line = p.Line; column = p.Column }) 
        }) state

    and parseLineTerminator state =
        (parse {
            let! c, p = tuple2 (satisfy isLineTerminator) getPosition
            return LineTerminator (c, { line = p.Line; column = p.Column }) 
        }) state

    and parseMultiLineComment state =
        (parse {
            let! body, p = skipString "/*" >>. tuple2 (manyCharsTill anyChar (pstring "*/")) getPosition
            let hasLt = Productions.containsLineTerminator body
            return MultiLineComment (body, hasLt, { line = p.Line; column = p.Column })
        }) state

    and parseSingleLineComment state =
        (parse {
            let! body, p = skipString "//" >>. tuple2 (manyCharsTill anyChar (satisfy Productions.isLineTerminator)) getPosition
            return SingleLineComment (body, { line = p.Line; column = p.Column })
        }) state

    and parseNumericLiteral state =        
        let parseSignedInteger state =
            (parse {
                let! sign, digits = tuple2 (opt (anyOf "+-")) (many1Chars digit)
                match sign with
                | Some '-' ->
                    return "-" + digits
                | Some _ | None ->
                    return digits
            }) state
        let parseExponentPart state =
            (parse {
                let! indicator = anyOf "eE"
                let! signedInteger = parseSignedInteger
                return string indicator + signedInteger
            } <|> preturn String.Empty) state
        let completeDecimal integralPortion state =
            (parse {
                let! dot = opt (pchar '.')
                match dot with
                | Some _ ->
                    let! digits = manyChars digit
                    let digits = if digits.Length = 0 then "0" else digits
                    let! e = parseExponentPart
                    let s = integralPortion + "." + digits + e
                    return double s
                | None ->
                    let! e = parseExponentPart
                    let s = integralPortion + e
                    return double s
            }) state
        let parseInternal state =
            (parse {
                let! c = anyOf ".0123456789"
                match c with
                | '.' ->
                    let! digits = many1Chars digit
                    let! e = parseExponentPart
                    let s = "0." + digits + e 
                    return double s
                | '0' ->
                    let! x = opt (anyOf "xX")
                    match x with
                    | Some _ ->
                        let! n = many1Chars hex
                        return Int64.Parse(n, Globalization.NumberStyles.AllowHexSpecifier) |> double
                    | None ->
                        let! result = completeDecimal "0"
                        return result
                | c ->
                    let! integral = manyChars digit 
                    let integral = string c + integral
                    let! result = completeDecimal integral
                    return result
            }) state
        (parse {
            let! n = parseInternal
            let! p = getPosition
            return NumericLiteral (n, { line = p.Line; column = p.Column }) 
        }) state