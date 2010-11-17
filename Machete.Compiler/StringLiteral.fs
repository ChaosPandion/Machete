namespace Machete.Compiler

module StringLiteral =

    open FParsec.CharParsers
    open FParsec.Primitives
    open NumericLiteral
    open Parsers
    
    let singleQuote<'a> : Parser<char, 'a> = 
        pchar '\''

    let doubleQuote<'a> : Parser<char, 'a> = 
        pchar '\"'
        
    let singleEscapeCharacter<'a> : Parser<InputElement, 'a> =
        anyOf "\'\"\\bfnrtv" |>> Char |>> SingleEscapeCharacter

    let escapeCharacter<'a> : Parser<InputElement, 'a> =
        let a = singleEscapeCharacter |>> EscapeCharacter
        let b = decimalDigit |>> EscapeCharacter
        let c = pchar 'x' |>> Char |>> EscapeCharacter
        let d = pchar 'u' |>> Char |>> EscapeCharacter
        a <|> b <|> c <|> d

    let nonEscapeCharacter<'a> : Parser<InputElement, 'a> =
        pipe2 (notFollowedBy(escapeCharacter <|> Parsers.lineTerminator)) anyChar (fun a b -> NonEscapeCharacter(Char b))

    let characterEscapeSequence<'a> : Parser<InputElement, 'a> =
        (singleEscapeCharacter <|> nonEscapeCharacter) |>> CharacterEscapeSequence 

    let hexEscapeSequence<'a> : Parser<InputElement, 'a> =
        pipe2 hexDigit hexDigit (fun a b -> HexEscapeSequence (a, b))

    let unicodeEscapeSequence<'a> : Parser<InputElement, 'a> =
        pipe4 hexDigit hexDigit hexDigit hexDigit (fun a b c d -> UnicodeEscapeSequence (a, b, c, d))
    
    let escapeSequence<'a> : Parser<InputElement, 'a> =
        let a = characterEscapeSequence
        let b = pipe2 (pchar '0') (notFollowedBy decimalDigit) (fun a b -> Char a)
        let c = hexEscapeSequence
        let d = unicodeEscapeSequence
        (a <|> b <|> c <|> d) |>> EscapeSequence
    
    let lineContinuation<'a> : Parser<InputElement, 'a> =
        pipe2 (pchar '\\') Parsers.lineTerminatorSequence (fun a b -> LineContinuation)

    let doubleStringCharacter<'a> : Parser<InputElement, 'a> =
        satisfy (fun c -> c <> '\"' && c <> '\\' && not (CharSets.lineTerminatorCharSet.Contains c)) |>> Char |>> DoubleStringCharacter

    let singleStringCharacter<'a> : Parser<InputElement, 'a> =
        satisfy (fun c -> c <> '\'' && c <> '\\' && not (CharSets.lineTerminatorCharSet.Contains c)) |>> Char |>> SingleStringCharacter
    
    let doubleStringCharacters<'a> : Parser<InputElement, 'a> =
        manyFold Nil (fun x y -> DoubleStringCharacters (y, x)) doubleStringCharacter

    let singleStringCharacters<'a> : Parser<InputElement, 'a> =
        manyFold Nil (fun x y -> SingleStringCharacters (y, x)) singleStringCharacter

    let stringLiteral<'a> : Parser<InputElement, 'a> =
        let d = between doubleQuote doubleQuote (doubleStringCharacters <|> nil) |>> StringLiteral
        let s = between singleQuote singleQuote (singleStringCharacters <|> nil) |>> StringLiteral
        d <|> s

        
    let evalUnicodeEscapeSequence v =
        match v with
        | UnicodeEscapeSequence (a, b, c, d) -> 
            char (4096 * evalHexDigit a + 256 * evalHexDigit b + 16 * evalHexDigit c + evalHexDigit d)                  
        | _ -> invalidArg "v" "Expected UnicodeEscapeSequence." 

    let evalHexEscapeSequence v =
        match v with
        | HexEscapeSequence (h, l) -> 
            char (16 * evalHexDigit h + evalHexDigit l)                  
        | _ -> invalidArg "v" "Expected HexEscapeSequence." 

    let evalNonEscapeCharacter v =
        match v with
        | NonEscapeCharacter v -> 
            match v with
            | Char c -> c
            | _ -> invalidOp ""                 
        | _ -> invalidArg "v" "Expected NonEscapeCharacter." 
        
    let evalCharacterEscapeSequence v =
        match v with
        | CharacterEscapeSequence v -> 
            match v with
            | NonEscapeCharacter _ -> evalNonEscapeCharacter v
            | SingleEscapeCharacter v ->
                match v with
                | Char c ->
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
                    | _ -> invalidOp ""  
                | _ -> invalidOp ""  
            | _ -> invalidOp ""                 
        | _ -> invalidArg "v" "Expected CharacterEscapeSequence." 
    
    let evalEscapeSequence v =
        match v with
        | EscapeSequence v -> 
            match v with
            | CharacterEscapeSequence _ -> evalCharacterEscapeSequence v
            | Char '0' -> '\u0000'
            | HexEscapeSequence (_,_) -> evalHexEscapeSequence v 
            | UnicodeEscapeSequence (_,_,_,_) -> evalUnicodeEscapeSequence v 
            | _ -> invalidOp ""                 
        | _ -> invalidArg "v" "Expected EscapeSequence."
    
    let evalSingleStringCharacter v =
        match v with
        | SingleStringCharacter v -> 
            match v with
            | Char c -> c
            | EscapeSequence _ -> evalEscapeSequence v
            | _ -> invalidOp ""                 
        | _ -> invalidArg "v" "Expected SingleStringCharacter."

    let evalDoubleStringCharacter v =
        match v with
        | DoubleStringCharacter v -> 
            match v with
            | Char c -> c
            | EscapeSequence _ -> evalEscapeSequence v
            | _ -> invalidOp ""                 
        | _ -> invalidArg "v" "Expected DoubleStringCharacter."

    let evalLineContinuation v =
        match v with
        | LineContinuation -> ""                
        | _ -> invalidArg "v" "Expected LineContinuation."

    let rec evalSingleStringCharacters v =
        match v with
        | SingleStringCharacters (l, r) -> 
            match l, r with
            | SingleStringCharacter _, Nil ->
                evalSingleStringCharacter l |> string
            | SingleStringCharacter _, SingleStringCharacters (_, _) ->
                (evalSingleStringCharacter l |> string) + evalSingleStringCharacters l
            | _ -> invalidOp ""                 
        | _ -> invalidArg "v" "Expected SingleStringCharacters."

    let rec evalDoubleStringCharacters v =
        match v with
        | DoubleStringCharacters (l, r) -> 
            match l, r with
            | DoubleStringCharacter _, Nil ->
                evalDoubleStringCharacter l |> string
            | DoubleStringCharacter _, DoubleStringCharacters (_, _) ->
                (evalDoubleStringCharacter l |> string) + evalDoubleStringCharacters l
            | _ -> invalidOp ""                 
        | _ -> invalidArg "v" "Expected DoubleStringCharacter."
        
    let evalStringLiteral v =
        match v with
        | StringLiteral v -> 
            match v with
            | DoubleStringCharacters (_, _) ->
                evalDoubleStringCharacters v
            | SingleStringCharacters (_, _) ->
                evalSingleStringCharacters v
            | _ -> invalidOp ""                 
        | _ -> invalidArg "v" "Expected StringLiteral."