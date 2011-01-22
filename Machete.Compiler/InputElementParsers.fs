namespace Machete.Compiler

open System
open FParsec.Primitives
open FParsec.CharParsers

module InputElementParsers =

    // White Space
    let rec evalWhiteSpace state =
        skipSatisfy CharSets.isWhiteSpace state     

    // Line Terminators
    and evalLineTerminator state =
        skipSatisfy CharSets.isLineTerminator state           
    and evalLineTerminatorSequence state =
        (newline |>> ignore <|> (skipSatisfy CharSets.isLineTerminator)) state       

    // Comments
    and evalComment state =
        (evalMultiLineComment <|> evalSingleLineComment) state         
    and evalMultiLineComment state =
        (skipString "/*" >>. manyCharsTill anyChar (lookAhead (pstring "*/")) .>> skipString "*/") state           
    and evalSingleLineComment state =
        (skipString "//" >>. manyCharsTill anyChar (lookAhead (satisfy CharSets.isLineTerminator))) state

    
    let inline evalUnicodeLetter state =
        (satisfy CharSets.isUnicodeLetter |>> string) state
    let inline evalUnicodeCombiningMark state =
        (satisfy CharSets.isUnicodeCombiningMark |>> string) state
    let inline evalUnicodeDigit state =
        (satisfy CharSets.isUnicodeDigit |>> string) state
    let inline evalUnicodeConnectorPunctuation state =
        (satisfy CharSets.isUnicodeConnectorPunctuation |>> string) state

    // Identifier Names    
    let rec evalIdentifier state =
        (attempt <| parse {
            let! r = evalIdentifierName
            if not (CharSets.reservedWordSet.Contains r) then
                return r
        }) state
    and evalIdentifierName state =
        pipe2 evalIdentifierStart (manyStrings evalIdentifierPart) (+) state
    and evalIdentifierStart state =
        (
            evalUnicodeLetter <|>
            pstring "$" <|>
            pstring "_" <|>
            (skipChar '\\' >>. evalUnicodeEscapeSequence)
        ) state
    and evalIdentifierPart state =
        (
            evalIdentifierStart <|>
            evalUnicodeCombiningMark <|>
            evalUnicodeDigit <|>
            evalUnicodeConnectorPunctuation <|>
            pstring "\u200C" <|>
            pstring "\u200D"
        ) state
        
    // Numeric Literal    
    and evalNumericLiteral state =
        (attempt evalHexIntegerLiteral <|> evalDecimalLiteral) state
    and evalDecimalLiteral state =
        choice [|
            parse {
                let! e1 = evalDecimalIntegerLiteral
                let! e2 = opt (pchar '.')
                match e2 with
                | Some _ ->
                    let! e2 = opt (evalDecimalDigits -1.0 1.0)
                    let! e3 = opt evalExponentPart
                    match e2, e3 with
                    | Some e2, Some e3 ->
                        return (e1 + e2) * e3
                    | Some e2, None ->
                        return e1 + e2
                    | None, Some e3 ->
                        return e1 * e3
                    | None, None ->
                        return e1
                | None ->
                    let! e2 = opt evalExponentPart
                    match e2 with
                    | Some e2 ->
                        return e1 * e2
                    | None ->
                        return e1 
            }
            parse {
                do! skipChar '.'
                let! e1 = evalDecimalDigits -1.0 1.0
                let! e2 = opt evalExponentPart
                match e2 with
                | Some e2 ->
                    return e1 * e2
                | None ->
                    return e1
            }
        |] state
    and evalDecimalIntegerLiteral state =
        (parse {
            let! r = opt (pchar '0')
            match r with
            | Some r -> return 0.0
            | None -> 
                let! r = evalDecimalDigits 1.0 0.0
                return r
        }) state
    and evalDecimalDigits sign modifier state =
        (parse {
            let p = if modifier = 0.0 then many1Rev evalDecimalDigit else many1 evalDecimalDigit
            let! e = p
            let e = e |> List.mapi (fun i n -> n * (10.0 ** (sign * (double i + modifier))))
            return e |> List.reduce (+)
        }) state 
    and evalDecimalDigit state =
        (parse {
            let! c = anyOf "0123456789"
            return double c - 48.0
        }) state
    and evalNonZeroDigit state =
        (parse {
            let! c = anyOf "123456789"
            return double c - 48.0
        }) state
    and evalExponentPart state =
        (parse {
            do! evalExponentIndicator
            let! r = evalSignedInteger
            return 10.0 ** r
        }) state
    and evalExponentIndicator state =
        (skipAnyOf "eE") state
    and evalSignedInteger state =
        (parse {
            let! r = opt (anyOf "+-")
            match r with
            | Some '-' ->  
                let! r = evalDecimalDigits 1.0 0.0
                return -r
            | _ -> 
                let! r = evalDecimalDigits 1.0 0.0
                return r
        }) state  

    and evalHexIntegerLiteral state =
        (parse {
            do! skipString "0x" <|> skipString "0X"
            let! e = many1Rev evalHexDigit
            let e = e |> List.mapi (fun i n -> n * (16.0 ** (double i)))
            return e |> List.reduce (+)
        }) state

    and evalHexDigit state =
        (anyOf "0123456789ABCDEFabcdef" |>> completeHexDigit) state

    and completeHexDigit c =
        match c with
        | c when c >= '0' && c <= '9' -> 
            double c - 48.0 
        | c when c >= 'A' && c <= 'F' -> 
            double c - 55.0
        | c when c >= 'a' && c <= 'f' -> 
            double c - 87.0

    // String Literals
    and evalStringLiteral state =
        (
            (skipChar '\"' >>. evalDoubleStringCharacters .>> skipChar '\"') <|>  
            (skipChar '\'' >>. evalSingleStringCharacters .>> skipChar '\'') 
        ) state
//        choice [|
//            parse {
//                do! skipChar '\"'
//                let! e = opt evalDoubleStringCharacters
//                do! skipChar '\"'
//                match e with
//                | Some e ->
//                    return e
//                | None ->
//                    return ""
//            }
//            parse {
//                do! skipChar '''
//                let! e = opt evalSingleStringCharacters
//                do! skipChar '''
//                match e with
//                | Some e ->
//                    return e
//                | None ->
//                    return ""
//            }
//        |] state    
    and evalDoubleStringCharacters state =
        manyStrings evalDoubleStringCharacter state
    and evalSingleStringCharacters state =
        manyStrings evalSingleStringCharacter state        
    and evalDoubleStringCharacter state =
        evalStringCharacter '\"' state
    and evalSingleStringCharacter state =
        evalStringCharacter '\'' state
    and evalStringCharacter quoteChar state =
        choice [|
            satisfy (satisfyStringCharacter quoteChar) |>> fun c -> c.ToString()
            parse {
                do! skipChar '\\'
                let! r = evalEscapeSequence <|> evalLineContinuation
                return r
            }
        |] state
    and satisfyStringCharacter quoteChar c =
        (c <> quoteChar && c <> '\\' && not (CharSets.isLineTerminator c))     
    and evalLineContinuation state =
        (parse {
            let! r = evalLineTerminatorSequence 
            return ""
        }) state 
    and evalEscapeSequence state =
        choice [|
            parse {
                do! skipChar '0'
                do! notFollowedBy digit
                return "\u0000"
            }
            evalHexEscapeSequence
            evalUnicodeEscapeSequence
            evalCharacterEscapeSequence
        |] state
    and evalCharacterEscapeSequence state =
        choice [|
            evalSingleEscapeCharacter
            evalNonEscapeCharacter
        |] state
    and evalSingleEscapeCharacter state =
        (parse {
            let! c = anyOf "\'\"\\bfnrtv"
            let c = 
                match c with
                | 'b' -> '\u0008'
                | 't' -> '\u0009'
                | 'n' -> '\u000A'
                | 'v' -> '\u000B'
                | 'f' -> '\u000C'
                | 'r' -> '\u000D'
                | '\"' -> '\u0022'
                | '\'' -> '\u0027'
                | '\\' -> '\u005C'
            return c.ToString()
        }) state
    and evalNonEscapeCharacter state =
        (parse {
            do! notFollowedBy evalSingleEscapeCharacter
            do! notFollowedBy evalLineTerminator
            let! c = anyChar
            return c.ToString()
        }) state
    and evalEscapeCharacter state =
        choice [|
            evalSingleEscapeCharacter
            digit |>> string
            pchar 'x' |>> string
            pchar 'u' |>> string
        |] state
    and evalHexEscapeSequence state =
        (parse {
            do! skipChar 'x'
            let! first = evalHexDigit
            let! second = evalHexDigit
            return (16.0 * first + second) |> char |> string
        }) state
    and evalUnicodeEscapeSequence state =
        (parse {
            do! skipChar 'u'
            let! first = evalHexDigit
            let! second = evalHexDigit
            let! third = evalHexDigit
            let! fourth = evalHexDigit
            return (4096.0 * first + 256.0 * second + 16.0 * third + fourth) |> char |> string
        }) state

    // Regular Expression Literal
    and evalRegularExpressionLiteral state =
        (parse {
            do! skipChar '/'
            let! body = evalRegularExpressionBody
            do! skipChar '/'
            let! flags = evalRegularExpressionFlags
            return body, flags
        }) state
    and evalRegularExpressionBody state =
        (parse {
            let! first = evalRegularExpressionFirstChar
            let! rest = evalRegularExpressionChars 
            return first + rest
        }) state
    and evalRegularExpressionChars state =
        (parse {
            let! r = manyStrings evalRegularExpressionChar
            return r
        }) state
    and evalRegularExpressionFirstChar state =
        choice [|
            satisfy (fun c -> c <> '*' && c <> '\\' && c <> '/' && c <> ']') |>> string
            evalRegularExpressionBackslashSequence
            evalRegularExpressionClass
        |] state
    and evalRegularExpressionChar state =
        choice [|
            satisfy (fun c -> c <> '\\' && c <> '/' && c <> ']') |>> string
            evalRegularExpressionBackslashSequence
            evalRegularExpressionClass
        |] state
    and evalRegularExpressionBackslashSequence state =
        (parse {
            do! skipChar '\\'
            let! r = evalRegularExpressionNonTerminator
            return r |> string
        }) state
    and evalRegularExpressionNonTerminator state =
        (parse {
            do! notFollowedBy evalLineTerminator
            let! c = anyChar
            return c |> string
        }) state
    and evalRegularExpressionClass state =
        (parse {
            do! skipChar '['
            let! r = evalRegularExpressionClassChars
            do! skipChar ']'
            return r
        }) state
    and evalRegularExpressionClassChars state =
        (parse {
            let! r = manyStrings evalRegularExpressionClassChar
            return r
        }) state
    and evalRegularExpressionClassChar state =
        choice [|
            satisfy (fun c -> c <> '\\' && c <> ']') |>> string
            evalRegularExpressionBackslashSequence
        |] state
    and evalRegularExpressionFlags state =
        (parse {
            let! r = manyStrings evalIdentifierPart
            return r
        }) state
