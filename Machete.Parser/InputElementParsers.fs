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

    and parseIdentifierName state =
        (parse {
            let! p = getPosition
            let! r = pipe2 parseIdentifierStart (manyStrings parseIdentifierPart) (+)
            return IdentifierName (r, { line = p.Line; column = p.Column }) 
        }) state

    and parseIdentifierStart state =
        (
            parseUnicodeLetter
            <|> (anyOf "$_" |>> string)
            <|> (skipChar '\\' >>. parseUnicodeEscapeSequence)
        ) state

    and parseIdentifierPart state =
        (
            parseIdentifierStart
            <|> parseUnicodeCombiningMark
            <|> parseUnicodeDigit
            <|> parseUnicodeConnectorPunctuation
            <|> (anyOf "\u200C\u200D" |>> string)
        ) state
    
    and parseUnicodeLetter state =
        (satisfy Productions.isUnicodeLetter |>> string) state

    and parseUnicodeCombiningMark state =
        (satisfy Productions.isUnicodeCombiningMark |>> string) state

    and parseUnicodeDigit state =
        (satisfy Productions.isUnicodeDigit |>> string) state

    and parseUnicodeConnectorPunctuation state =
        (satisfy Productions.isUnicodeConnectorPunctuation |>> string) state

    and parseSpecificIdentifierName expected state =
        (attempt <| parse {
            let! r = parseIdentifierName
            match r with
            | IdentifierName (actual, d) when actual = expected ->
                return r 
            | _ -> ()
        }) state

    and parseIdentifier state =
        (attempt <| parse {
            let! r = parseIdentifierName
            match r with
            | IdentifierName (s, _) 
                when not (Productions.reservedWordSet.Contains s)  ->
                    return r
        }) state
        
    and parseLiteral state =
        (parse {
            let! result = 
                parseNullLiteral 
                <|> parseBooleanLiteral 
                <|> parseNumericLiteral 
                <|> parseStringLiteral 
                <|> parseRegularExpressionLiteral
            return result
        }) state
        
    and parseNullLiteral state =
        (attempt <| parse {
            let! r = parseIdentifierName
            match r with
            | IdentifierName ("null", d) ->
                return NullLiteral d 
            | _ -> ()
        }) state
        
    and parseBooleanLiteral state =
        (attempt <| parse {
            let! r = parseIdentifierName
            match r with
            | IdentifierName ("true", d) ->
                return BooleanLiteral (true, d)
            | IdentifierName ("false", d) ->
                return BooleanLiteral (false, d)
            | _ -> ()
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

    and parseStringLiteral state =
        (parse {
            let! p, c = tuple2 getPosition (anyOf "\"'")
            let p = { line = p.Line; column = p.Column }
            match c with
            | '\"' ->
                let! r = parseDoubleStringCharacters .>> skipChar '\"'
                return StringLiteral (r, p) 
            | '\'' ->
                let! r = parseSingleStringCharacters .>> skipChar '\''
                return StringLiteral (r, p) 
        }) state
        
    and parseDoubleStringCharacters state =
        (manyStrings parseDoubleStringCharacter) state

    and parseSingleStringCharacters state =
        (manyStrings parseSingleStringCharacter) state
        
    and parseDoubleStringCharacter state =
        (parseStringCharacter "\"\\\u000a\u000d\u2028\u2029") state

    and parseSingleStringCharacter state =
        (parseStringCharacter "\'\\\u000a\u000d\u2028\u2029") state

    and parseStringCharacter except state =
        (parse {
            let! c = noneOf except <|> pchar '\\'
            match c with
            | '\\' ->
                let! c = parseEscapeSequence <|> (anyOf "\u000a\u000d\u2028\u2029" |>> string)
                if Productions.isLineTerminator (c.[0]) 
                then return ""
                else return string c
            | c ->
                return string c
        }) state

    and parseEscapeSequence state =
        (parseCharacterEscapeSequence <|> ((pchar '0' |>> string) .>> notFollowedBy digit) <|> parseHexEscapeSequence <|> parseUnicodeEscapeSequence) state

    and parseCharacterEscapeSequence state =
        (parseSingleEscapeCharacter <|> parseNonEscapeCharacter) state

    and parseSingleEscapeCharacter state =
        (anyOf "\'\"\\bfnrtv" |>> string) state

    and parseNonEscapeCharacter state =
        (noneOf "\'\"\\bfnrtv0123456789xu\u000a\u000d\u2028\u2029" |>> string) state

    and parseHexEscapeSequence state =
        (parse {
            let! a, b = skipChar 'x' >>. (tuple2 hex hex)
            let a, b = completeHexDigit a, completeHexDigit b
            return (16.0 * a + b) |> char |> string
        }) state

    and parseUnicodeEscapeSequence state =
        (parse {
            let! a, b, c, d = skipChar 'u' >>. (tuple4 hex hex hex hex)
            let a, b, c, d = completeHexDigit a, completeHexDigit b, completeHexDigit c, completeHexDigit d
            return (4096.0 * a + 256.0 * b + 16.0 * c + d) |> char |> string
        }) state
        
    and private completeHexDigit c =
        match c with
        | c when c >= '0' && c <= '9' -> 
            double c - 48.0 
        | c when c >= 'A' && c <= 'F' -> 
            double c - 55.0
        | c when c >= 'a' && c <= 'f' -> 
            double c - 87.0
            
    and parseRegularExpressionLiteral state =
        (parse {
            let! p, body, flags = tuple3 getPosition (skipChar '/' >>. parseRegularExpressionBody) (skipChar '/' >>. parseRegularExpressionFlags)
            return RegularExpressionLiteral (body, flags, { line = p.Line; column = p.Column }) 
        }) state

    and parseRegularExpressionBody state =
        (parse {
            let! first = parseRegularExpressionFirstChar
            let! rest = parseRegularExpressionChars 
            return first + rest
        }) state

    and parseRegularExpressionChars state =
        (manyStrings parseRegularExpressionChar) state

    and parseRegularExpressionFirstChar state =
        choice [|
            noneOf "*\\/[" |>> string
            parseRegularExpressionBackslashSequence
            parseRegularExpressionClass
        |] state

    and parseRegularExpressionChar state =
        choice [|
            noneOf "\\/[" |>> string
            parseRegularExpressionBackslashSequence
            parseRegularExpressionClass
        |] state

    and parseRegularExpressionBackslashSequence state =
        (parse {
            do! skipChar '\\'
            let! r = parseRegularExpressionNonTerminator
            return "\\" + r |> string
        }) state

    and parseRegularExpressionNonTerminator state =
        (noneOf Productions.lineTerminatorString |>> string) state

    and parseRegularExpressionClass state =
        (parse {
            let! r = skipChar '[' >>. parseRegularExpressionClassChars .>> skipChar ']'
            return "[" + r + "]"
        }) state

    and parseRegularExpressionClassChars state =
        (parse {
            let! r = manyStrings parseRegularExpressionClassChar
            return r
        }) state

    and parseRegularExpressionClassChar state =
        choice [|
            noneOf "\\]" |>> string
            parseRegularExpressionBackslashSequence
        |] state

    and parseRegularExpressionFlags state =
        (manyStrings parseIdentifierPart) state