namespace Machete.Compiler

module IdentifierName =

    open FParsec.CharParsers
    open FParsec.Primitives
    open NumericLiteral
    open StringLiteral
    open Parsers

    let unicodeLetter<'a> : Parser<InputElement, 'a> = 
        satisfy CharSets.unicodeLetterCharSet.Contains |>> UnicodeLetter

    let unicodeCombiningMark<'a> : Parser<InputElement, 'a> =
        satisfy CharSets.unicodeCombiningMarkCharSet.Contains |>> UnicodeCombiningMark 

    let unicodeDigit<'a> : Parser<InputElement, 'a> =
        satisfy CharSets.unicodeDigitCharSet.Contains |>> UnicodeDigit

    let unicodeConnectorPunctuation<'a> : Parser<InputElement, 'a> = 
        satisfy CharSets.unicodeConnectorPunctuationCharSet.Contains |>> UnicodeConnectorPunctuation

    let identifierStart<'a> : Parser<InputElement, 'a> =
        let a = unicodeLetter |>> IdentifierStart
        let b = pchar '$' |>> Char |>> IdentifierStart
        let c = pchar '_' |>> Char |>> IdentifierStart
        let d = pipe2 (pchar '\\') unicodeEscapeSequence (fun a b -> IdentifierStart b)
        a <|> b <|> c <|> d

    let identifierPart<'a> : Parser<InputElement, 'a> =
        let a = identifierStart |>> IdentifierPart
        let b = unicodeCombiningMark |>> IdentifierPart
        let c = unicodeDigit |>> IdentifierPart
        let d = unicodeConnectorPunctuation |>> IdentifierPart
        let e = pchar '\u200C' |>> Char |>> IdentifierPart
        let f = pchar '\u200D' |>> Char |>> IdentifierPart
        a <|> b <|> c <|> d <|> e <|> f

    let identifierName<'a> : Parser<InputElement, 'a> =
        pipe2 identifierStart (many identifierPart) (fun a b -> b |> List.fold (fun a b -> IdentifierName(a, b)) a)

    let evalIdentifierStart v =
        match v with
        | IdentifierStart v -> 
            match v with
            | UnicodeLetter c -> c
            | Char c -> c
            | UnicodeEscapeSequence (_, _, _, _) -> evalUnicodeEscapeSequence v
            | _ -> invalidOp ""                 
        | _ -> invalidArg "v" "Expected IdentifierStart."
    
    let evalIdentifierPart v =
        match v with
        | IdentifierPart v -> 
            match v with
            | IdentifierStart _ -> evalIdentifierStart v
            | UnicodeCombiningMark c -> c
            | UnicodeDigit c -> c
            | UnicodeConnectorPunctuation c -> c
            | Char c -> c
            | _ -> invalidOp ""                 
        | _ -> invalidArg "v" "Expected IdentifierPart."    

    let rec evalIdentifierName v =
        match v with
        | IdentifierName (l, r) -> 
            match l, r with
            | IdentifierStart _, Nil ->
                evalIdentifierStart l |> string
            | IdentifierName (_, _), IdentifierPart _ ->
                evalIdentifierName l + (evalIdentifierPart r |> string)
            | _ -> invalidOp ""                 
        | _ -> invalidArg "v" "Expected IdentifierName."