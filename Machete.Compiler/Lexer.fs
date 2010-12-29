namespace Machete.Compiler

module Lexer =

    open FParsec.CharParsers
    open FParsec.Primitives

    type InputElement =
    | Nil
    | Chr of char
    | Str of string
    | WhiteSpace
    | LineTerminator
    | LineTerminatorSequence
    | Comment of InputElement
    | MultiLineComment of string
    | SingleLineComment of string
    | Token of InputElement
    | Identifier of InputElement
    | IdentifierName of InputElement * InputElement
    | IdentifierStart of InputElement
    | IdentifierPart of InputElement
    | UnicodeLetter of char
    | UnicodeCombiningMark of char
    | UnicodeDigit of char
    | UnicodeConnectorPunctuation of char
    | ReservedWord
    | Keyword
    | FutureReservedWord
    | Punctuator of InputElement
    | DivPunctuator of InputElement
    | Literal of InputElement
    | NullLiteral of string
    | BooleanLiteral of string
    | NumericLiteral of InputElement
    | DecimalLiteral of InputElement * InputElement * InputElement * InputElement
    | DecimalIntegerLiteral of InputElement * InputElement
    | DecimalDigits of InputElement * InputElement
    | DecimalDigit of InputElement
    | NonZeroDigit of InputElement
    | ExponentPart of InputElement * InputElement
    | ExponentIndicator of InputElement
    | SignedInteger of InputElement * InputElement
    | HexIntegerLiteral of InputElement * InputElement
    | HexDigit of InputElement
    | DecimalPoint
    | StringLiteral of InputElement
    | DoubleStringCharacters of InputElement * InputElement
    | SingleStringCharacters of InputElement * InputElement
    | DoubleStringCharacter of InputElement
    | SingleStringCharacter of InputElement
    | LineContinuation
    | EscapeSequence of InputElement
    | CharacterEscapeSequence of InputElement
    | SingleEscapeCharacter of InputElement
    | NonEscapeCharacter of InputElement
    | EscapeCharacter of InputElement
    | HexEscapeSequence of InputElement * InputElement
    | UnicodeEscapeSequence of InputElement * InputElement * InputElement * InputElement
    | RegularExpressionLiteral of InputElement * InputElement
    | RegularExpressionBody of InputElement * InputElement
    | RegularExpressionChars of InputElement * InputElement
    | RegularExpressionFirstChar of InputElement
    | RegularExpressionChar of InputElement
    | RegularExpressionBackslashSequence of InputElement
    | RegularExpressionNonTerminator of char
    | RegularExpressionClass of InputElement
    | RegularExpressionClassChars of InputElement * InputElement
    | RegularExpressionClassChar of InputElement
    | RegularExpressionFlags of InputElement * InputElement


    type LexerState = {
        previousElement : option<InputElement>
    }
    
    let nil<'a> : Parser<InputElement, 'a> = preturn Nil
    let str s = pstring s |>> Str
    let chr c = pchar c |>> Chr
    let maybe p = p <|> preturn Nil

    let identifierStart, identifierStartRef = createParserForwardedToRef<InputElement, LexerState>()

    module WhiteSpace =
        let parseWhiteSpace<'a> : Parser<InputElement, 'a> =
            satisfy CharSets.isWhiteSpace |>> (fun c -> WhiteSpace)

    module LineTerminator =        
        let parseLineTerminator<'a> : Parser<InputElement, 'a> =
            satisfy CharSets.isLineTerminator |>> (fun c -> LineTerminator)    
        let parseLineTerminatorSequence<'a> : Parser<InputElement, 'a> =
            satisfy CharSets.isLineTerminator |>> (fun c -> LineTerminatorSequence)

    module Comment =            
        let parseSingleLineComment<'a> : Parser<InputElement, 'a> =
            pipe2 (pstring "//") (manyCharsTill anyChar (lookAhead (satisfy CharSets.isLineTerminator))) (fun a b -> SingleLineComment b)
        let parseMultiLineComment<'a> : Parser<InputElement, 'a> =
            (between (pstring "/*") (pstring "*/") (manyCharsTill anyChar (lookAhead (pstring "*/")))) |>> MultiLineComment
        let parseComment<'a> : Parser<InputElement, 'a> =
            (parseMultiLineComment <|> parseSingleLineComment) |>> Comment

    module NumericLiteralParser =

        open FParsec.CharParsers
        open FParsec.Primitives

 
        let decimalPoint<'a> : Parser<InputElement, 'a> =
            pchar '.' |>> fun c -> DecimalPoint

        let hexDigit<'a> : Parser<InputElement, 'a> =
            satisfy CharSets.isHexDigit |>> Chr |>> HexDigit
    
        let hexIntegerLiteral<'a> : Parser<InputElement, 'a> =
            parse {
                do! skipChar '0'
                do! skipAnyOf "xX"
                let! r = many1Fold Nil (fun x y -> HexIntegerLiteral (x, y)) hexDigit <?> "A hex integer literal was found to be incomplete."
                return r
            }

        let nonZeroDigit<'a> : Parser<InputElement, 'a> =
            satisfy CharSets.isNonZeroDigit |>> Chr |>> NonZeroDigit

        let decimalDigit<'a> : Parser<InputElement, 'a> =
            satisfy CharSets.isDecimalDigit |>> Chr |>> DecimalDigit

        let decimalDigits<'a> : Parser<InputElement, 'a> =
            many1Fold Nil (fun x y -> DecimalDigits (x, y)) decimalDigit         
        
        let signedInteger<'a> : Parser<InputElement, 'a> =
            parse {
                let! a = (anyOf "+-" |>> Chr) <|> nil
                let! b = decimalDigits
                return SignedInteger (a, b) 
            }   
         
        let exponentIndicator<'a> : Parser<InputElement, 'a> =
            anyOf "eE" |>> Chr |>> ExponentIndicator
         
        let exponentPart<'a> : Parser<InputElement, 'a> =
            pipe2 exponentIndicator signedInteger (fun a b -> ExponentPart(a, b))
        
        let decimalIntegerLiteral<'a> : Parser<InputElement, 'a> =
            (pchar '0' |>> Chr |>> (fun a -> a, Nil)) 
            <|> (pipe2 nonZeroDigit (decimalDigits <|> preturn Nil) (fun a b -> a, b )) 
            |>> (fun a -> DecimalIntegerLiteral a)
        
        let decimalLiteral<'a> : Parser<InputElement, 'a> =
            attempt (parse {
                let! i = decimalIntegerLiteral
                do! skipChar '.'
                let! f = decimalDigits <|> nil
                let! e = exponentPart <|> nil
                return DecimalLiteral (i, DecimalPoint, f, e)
            }) <|> parse {
                do! skipChar '.'
                let! f = decimalDigits
                let! e = exponentPart <|> nil
                return DecimalLiteral (Nil, DecimalPoint, f, e)
            } <|> attempt (parse {
                let! i = decimalIntegerLiteral
                let! e = exponentPart <|> nil
                return DecimalLiteral (i, Nil, Nil, e)
            })

        let numericLiteral =
            attempt (parse {
                let! r = decimalLiteral
                do! notFollowedBy identifierStart
                do! notFollowedBy decimalDigit
                return NumericLiteral r
            }) <|> parse {
                let! r = hexIntegerLiteral
                do! notFollowedBy identifierStart
                do! notFollowedBy decimalDigit
                return NumericLiteral r
            }

        let inline evalHexDigit v =
            match v with
            | HexDigit v -> 
                match v with
                | Chr c -> 
                    match c with
                    | c when c >= '0' && c <= '9' -> int c - 48 |> double   
                    | c when c >= 'a' && c <= 'f' -> int c - 87 |> double  
                    | c when c >= 'A' && c <= 'F' -> int c - 55 |> double    
                    | _ ->  invalidOp ("Unexpected value '" + c.ToString() + "' found while evaluating HexDigit.")        
                | _ -> invalidOp "Unexpected pattern for HexDigit."     
            | _ -> invalidArg "v" "Expected HexDigit."

        let rec evalHexIntegerLiteral v =
            match v with
            | HexIntegerLiteral (l, r) -> 
                match l, r with
                | Nil, HexDigit _ ->
                    evalHexDigit r |> double 
                | HexIntegerLiteral (_, _), HexDigit _ ->
                    (16.0 * evalHexIntegerLiteral l + evalHexDigit r) |> double  
                | _ -> invalidOp "Unexpected pattern for HexIntegerLiteral."       
            | _ -> invalidArg "v" "Expected HexDigit."

        let inline evalDecimalDigit v =
            match v with
            | DecimalDigit v -> 
                match v with
                | Chr c ->
                    assert(c >= '0' && c <= '9')
                    int c - 48       
                | _ -> invalidOp "Unexpected pattern for DecimalDigit."     
            | _ -> invalidArg "v" "Expected DecimalDigit."

        let evalNonZeroDigit v =
            match v with
            | NonZeroDigit v -> 
                match v with
                | Chr c -> 
                    assert(c >= '1' && c <= '9')
                    int c - 48        
                | _ -> invalidOp "Unexpected pattern for NonZeroDigit."     
            | _ -> invalidArg "v" "Expected NonZeroDigit."
    
        let rec countDecimalDigits v n =
            match v with
            | DecimalDigits (l, r) ->
                match l, r with
                | Nil, DecimalDigit _ -> n + 1
                | DecimalDigits (_, _), DecimalDigit _ -> countDecimalDigits l (n + 1)
                | _ -> invalidOp "Invalid DecimalDigits pattern found."                   
            | _ -> invalidArg "v" "Expected DecimalDigits."

        let rec evalDecimalDigits d =
            match d with
            | DecimalDigits (l, r) ->
                match l, r with
                | Nil, DecimalDigit _ ->
                    evalDecimalDigit r
                | DecimalDigits (_, _), DecimalDigit _ ->
                    10 * evalDecimalDigits l + evalDecimalDigit r
                | _ -> invalidOp "Invalid DecimalDigits pattern found."                   
            | _ -> invalidArg "d" "Expected DecimalDigits."

        let evalDecimalIntegerLiteral d =
            match d with
            | DecimalIntegerLiteral (l, r) ->
                match l, r with
                | Chr _, Nil -> 0
                | NonZeroDigit _, Nil ->
                    evalNonZeroDigit l
                | NonZeroDigit _, DecimalDigits (_, _) ->  
                    let n = float (countDecimalDigits r 0) 
                    let n = int (10.0 ** n)
                    n * evalNonZeroDigit l + evalDecimalDigits r                    
                | _ -> invalidOp "Invalid DecimalIntegerLiteral pattern found."                    
            | _ -> invalidArg "d" "Expected DecimalIntegerLiteral."

        let evalSignedInteger v =
            match v with
            | SignedInteger (l, r) -> 
                match l, r with
                | Nil, DecimalDigits (_, _) ->
                    evalDecimalDigits r
                | Chr c, DecimalDigits (_, _) ->
                    match c with
                    | '+' -> evalDecimalDigits r
                    | '-' -> -evalDecimalDigits r
                    | _ -> invalidOp ("Unexpected value '" + c.ToString() + "' found while evaluating SignedInteger.")
                | _ -> invalidOp "Invalid SignedInteger pattern found."                  
            | _ -> invalidArg "v" "Expected SignedInteger."
        
        let evalExponentPart v =
            match v with
            | ExponentPart (l, r) -> 
                match r with
                | SignedInteger (_, _) ->
                    evalSignedInteger r
                | _ -> invalidOp "Invalid ExponentPart pattern found."                  
            | _ -> invalidArg "v" "Expected ExponentPart."
        
        let evalDecimalLiteral v =
            match v with
            | DecimalLiteral (a, b, c, d) -> 
                match a, b, c, d with
                | DecimalIntegerLiteral (_, _), Nil, Nil, Nil
                | DecimalIntegerLiteral (_, _), DecimalPoint, Nil, Nil ->
                    evalDecimalIntegerLiteral a |> double
                | DecimalIntegerLiteral (_, _), DecimalPoint, DecimalDigits (_, _), Nil -> 
                    let digitCount = countDecimalDigits c 0 |> double
                    let fValue = evalDecimalDigits c |> double
                    let iValue = evalDecimalIntegerLiteral a |> double
                    iValue + (fValue * 10.0 ** -digitCount)
                | DecimalIntegerLiteral (_, _), Nil, Nil, ExponentPart (_, _)
                | DecimalIntegerLiteral (_, _), DecimalPoint, Nil, ExponentPart (_, _) ->
                    let e = evalExponentPart d |> double
                    let i = evalDecimalIntegerLiteral a |> double
                    i * 10.0 ** e
                | DecimalIntegerLiteral (_, _), DecimalPoint, DecimalDigits (_, _), ExponentPart (_, _) -> 
                    let digitCount = countDecimalDigits c 0 |> double
                    let eValue = evalExponentPart d |> double
                    let fValue = evalDecimalDigits c |> double
                    let iValue = evalDecimalIntegerLiteral a |> double
                    (iValue + (fValue * 10.0 ** -digitCount)) * (10.0 ** eValue)
                | Nil, DecimalPoint, DecimalDigits (_, _), Nil -> 
                    let digitCount = countDecimalDigits c 0 |> double
                    let fValue = evalDecimalDigits c |> double
                    fValue * 10.0 ** -digitCount
                | Nil, DecimalPoint, DecimalDigits (_, _), ExponentPart (_, _) -> 
                    let digitCount = countDecimalDigits c 0 |> double
                    let eValue = evalExponentPart d |> double
                    let fValue = evalDecimalDigits c |> double
                    fValue * 10.0 ** (eValue - digitCount)
                | _ -> invalidOp "Invalid DecimalLiteral pattern found."                  
            | _ -> invalidArg "v" "Expected DecimalLiteral."

        let evalNumericLiteral v =
            match v with
            | NumericLiteral v ->
                match v with
                | DecimalLiteral (_, _, _, _) ->
                    evalDecimalLiteral v
                | HexIntegerLiteral (_, _) ->
                    evalHexIntegerLiteral v                

    module StringLiteralParser =

        open FParsec.CharParsers
        open FParsec.Primitives
        open NumericLiteralParser
    
        let singleQuote<'a> : Parser<char, 'a> = 
            pchar '\''

        let doubleQuote<'a> : Parser<char, 'a> = 
            pchar '\"'
        
        let singleEscapeCharacter<'a> : Parser<InputElement, 'a> =
            anyOf "\'\"\\bfnrtv" |>> Chr |>> SingleEscapeCharacter

        let escapeCharacter<'a> : Parser<InputElement, 'a> =
            let a = singleEscapeCharacter |>> EscapeCharacter
            let b = decimalDigit |>> EscapeCharacter
            let c = pchar 'x' |>> Chr |>> EscapeCharacter
            let d = pchar 'u' |>> Chr |>> EscapeCharacter
            a <|> b <|> c <|> d

        let nonEscapeCharacter<'a> : Parser<InputElement, 'a> =
            pipe2 (notFollowedBy(escapeCharacter <|> LineTerminator.parseLineTerminator)) anyChar (fun a b -> NonEscapeCharacter(Chr b))

        let characterEscapeSequence<'a> : Parser<InputElement, 'a> =
            (singleEscapeCharacter <|> nonEscapeCharacter) |>> CharacterEscapeSequence 

        let hexEscapeSequence<'a> : Parser<InputElement, 'a> =
            pipe3 (skipChar 'x') hexDigit hexDigit (fun () a b -> HexEscapeSequence (a, b))

        let unicodeEscapeSequence<'a> : Parser<InputElement, 'a> =
            pipe5 (skipChar 'u') hexDigit hexDigit hexDigit hexDigit (fun () a b c d -> UnicodeEscapeSequence (a, b, c, d))
    
        let escapeSequence<'a> : Parser<InputElement, 'a> =
            let a = characterEscapeSequence
            let b = pipe2 (pchar '0') (notFollowedBy decimalDigit) (fun a b -> Chr a)
            let c = hexEscapeSequence
            let d = unicodeEscapeSequence
            (a <|> b <|> c <|> d) |>> EscapeSequence
    
        let lineContinuation<'a> : Parser<InputElement, 'a> =
            pipe2 (pchar '\\') LineTerminator.parseLineTerminatorSequence (fun a b -> LineContinuation)

        let doubleStringCharacter<'a> : Parser<InputElement, 'a> =
            choice [
                (satisfy (fun c -> c <> '\"' && c <> '\\' && not (CharSets.isLineTerminator c)) |>> Chr)
                attempt (skipChar '\\' >>. escapeSequence)
                (lineContinuation)
            ] |>> DoubleStringCharacter

        let singleStringCharacter<'a> : Parser<InputElement, 'a> =
            choice [
                (satisfy (fun c -> c <> ''' && c <> '\\' && not (CharSets.isLineTerminator c)) |>> Chr)
                attempt (skipChar '\\' >>. escapeSequence)
                (lineContinuation)
            ] |>> SingleStringCharacter

    
        let doubleStringCharacters<'a> : Parser<InputElement, 'a> =
            many doubleStringCharacter |>> List.rev |>> List.fold (fun x y -> DoubleStringCharacters (y, x)) Nil 

        let singleStringCharacters<'a> : Parser<InputElement, 'a> =
            many singleStringCharacter |>> List.rev |>> List.fold (fun x y -> SingleStringCharacters (y, x)) Nil 

        let stringLiteral<'a> : Parser<InputElement, 'a> =
            let d = between doubleQuote doubleQuote (doubleStringCharacters <|> nil) |>> StringLiteral
            let s = between singleQuote singleQuote (singleStringCharacters <|> nil) |>> StringLiteral
            d <|> s

        
        let evalUnicodeEscapeSequence v =
            match v with
            | UnicodeEscapeSequence (a, b, c, d) -> 
                char (4096.0 * evalHexDigit a + 256.0 * evalHexDigit b + 16.0 * evalHexDigit c + evalHexDigit d)                  
            | _ -> invalidArg "v" "Expected UnicodeEscapeSequence." 

        let evalHexEscapeSequence v =
            match v with
            | HexEscapeSequence (h, l) -> 
                char (16.0 * evalHexDigit h + evalHexDigit l)                  
            | _ -> invalidArg "v" "Expected HexEscapeSequence." 

        let evalNonEscapeCharacter v =
            match v with
            | NonEscapeCharacter v -> 
                match v with
                | Chr c -> c
                | _ -> invalidOp ""                 
            | _ -> invalidArg "v" "Expected NonEscapeCharacter." 
        
        let evalCharacterEscapeSequence v =
            match v with
            | CharacterEscapeSequence v -> 
                match v with
                | NonEscapeCharacter _ -> evalNonEscapeCharacter v
                | SingleEscapeCharacter v ->
                    match v with
                    | Chr c ->
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
                | Chr '0' -> '\u0000'
                | HexEscapeSequence (_,_) -> evalHexEscapeSequence v 
                | UnicodeEscapeSequence (_,_,_,_) -> evalUnicodeEscapeSequence v 
                | _ -> invalidOp ""                 
            | _ -> invalidArg "v" "Expected EscapeSequence."
    
        let evalSingleStringCharacter v =
            match v with
            | SingleStringCharacter v -> 
                match v with
                | Chr c -> c |> string
                | EscapeSequence _ -> evalEscapeSequence v |> string
                | LineContinuation -> "" 
                | _ -> invalidOp ""                 
            | _ -> invalidArg "v" "Expected SingleStringCharacter."

        let evalDoubleStringCharacter v =
            match v with
            | DoubleStringCharacter v -> 
                match v with
                | Chr c -> c |> string
                | EscapeSequence _ -> evalEscapeSequence v |> string
                | LineContinuation -> "" 
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
                    (evalSingleStringCharacter l) + evalSingleStringCharacters r
                | _ -> invalidOp ""                 
            | _ -> invalidArg "v" "Expected SingleStringCharacters."

        let rec evalDoubleStringCharacters v =
            match v with
            | DoubleStringCharacters (l, r) -> 
                match l, r with
                | DoubleStringCharacter _, Nil ->
                    evalDoubleStringCharacter l |> string
                | DoubleStringCharacter _, DoubleStringCharacters (_, _) ->
                    (evalDoubleStringCharacter l) + evalDoubleStringCharacters r
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
                | _ -> ""                
            | _ -> invalidArg "v" "Expected StringLiteral."
            

    module IdentifierNameParser =

        open System
        open System.Globalization
        open FParsec.CharParsers
        open FParsec.Primitives
        open NumericLiteralParser
        open StringLiteralParser

        let unicodeLetter<'a> : Parser<InputElement, 'a> = 
            satisfy CharSets.isUnicodeLetter |>> UnicodeLetter

        let unicodeCombiningMark<'a> : Parser<InputElement, 'a> =
            satisfy CharSets.isUnicodeCombiningMark |>> UnicodeCombiningMark 

        let unicodeDigit<'a> : Parser<InputElement, 'a> =
            satisfy CharSets.isUnicodeDigit |>> UnicodeDigit

        let unicodeConnectorPunctuation<'a> : Parser<InputElement, 'a> = 
            satisfy CharSets.isUnicodeConnectorPunctuation |>> UnicodeConnectorPunctuation

        let identifierStart<'a> : Parser<InputElement, 'a> =
            let a = unicodeLetter |>> IdentifierStart
            let b = pchar '$' |>> Chr |>> IdentifierStart
            let c = pchar '_' |>> Chr |>> IdentifierStart
            let d = pipe2 (pchar '\\') unicodeEscapeSequence (fun a b -> IdentifierStart b)
            a <|> b <|> c <|> d

        let identifierPart<'a> : Parser<InputElement, 'a> =
            let a = identifierStart |>> IdentifierPart
            let b = unicodeCombiningMark |>> IdentifierPart
            let c = unicodeDigit |>> IdentifierPart
            let d = unicodeConnectorPunctuation |>> IdentifierPart
            let e = pchar '\u200C' |>> Chr |>> IdentifierPart
            let f = pchar '\u200D' |>> Chr |>> IdentifierPart
            a <|> b <|> c <|> d <|> e <|> f

        let identifierName<'a> : Parser<InputElement, 'a> =
            pipe2 (identifierStart |>> fun a -> IdentifierName (a, Nil)) (many identifierPart) (fun a b -> b |> List.fold (fun a b -> IdentifierName(a, b)) a)

        do identifierStartRef := identifierStart

        let evalIdentifierStart v =
            match v with
            | IdentifierStart v -> 
                match v with
                | UnicodeLetter c -> c
                | Chr c -> c
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
                | Chr c -> c
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

    module Punctuator =
        let punctuatorMap<'a> : Map<char, Parser<InputElement, 'a>> =
             Map.ofList [
                ('!', choice [str "!=="; str "!="; str "!"])
                ('%', choice [str "%="; str "%"])
                ('&', choice [str "&&"; str "&="; str "&"])
                ('(', choice [str "("])
                (')', choice [str ")"])
                ('*', choice [str "*="; str "*"])
                ('+', choice [str "++"; str "+="; str "+"])
                (',', choice [str ","])
                ('-', choice [str "-="; str "--"; str "-"])
                ('.', choice [str "."])
                (';', choice [str ";"])
                ('<', choice [str "<<="; str "<<"; str "<="; str "<"])
                ('=', choice [str "==="; str "=="; str "="])
                ('>', choice [str ">>>="; str ">>>"; str ">>="; str ">>"; str ">="; str ">"])
                ('?', choice [str "?"])
                (':', choice [str ":"])
                ('[', choice [str "["])
                (']', choice [str "]"])
                ('^', choice [str "^="; str "^"])
                ('{', choice [str "{"])
                ('|', choice [str "||"; str "|="; str "|"])
                ('}', choice [str "}"])
                ('~', choice [str "~"])
            ]
        let parsePunctuator<'a> : Parser<InputElement, 'a> =
            parse {
                let! c = lookAhead anyChar
                match punctuatorMap.TryFind c with
                | Some p -> return p
                | None -> ()
            } >>= fun p -> p |>> Punctuator
            
    module DivPunctuator =
        let parseDivPunctuator<'a> : Parser<InputElement, 'a> =
            (str "/=" <|> str "/") |>> DivPunctuator |> attempt

    module RegularExpressionLiteralParser =
        let parseRegularExpressionFlags<'a> : Parser<InputElement, 'a> =
            manyFold (RegularExpressionFlags (Nil, Nil)) (fun x y -> RegularExpressionFlags (x, y)) IdentifierNameParser.identifierPart

        let parseRegularExpressionNonTerminator<'a> : Parser<InputElement, 'a> =
            satisfy (fun c -> not (CharSets.isLineTerminator c)) |>> RegularExpressionNonTerminator

        let parseRegularExpressionBackslashSequence<'a> : Parser<InputElement, 'a> =
            parse {
                do! skipChar '\\'
                let! e = parseRegularExpressionNonTerminator
                return RegularExpressionBackslashSequence (e)
            }

        let parseRegularExpressionClassChar<'a> : Parser<InputElement, 'a> =
            choice [
                parseRegularExpressionBackslashSequence                
                attempt (parseRegularExpressionNonTerminator 
                            >>= fun r ->
                                match r with
                                | RegularExpressionNonTerminator (']') -> pzero
                                | _ -> preturn r)
            ] |>> RegularExpressionClassChar

        let parseRegularExpressionClassChars<'a> : Parser<InputElement, 'a> =
            manyFold (RegularExpressionClassChars (Nil, Nil)) (fun x y -> RegularExpressionClassChars (x, y)) parseRegularExpressionClassChar

        let parseRegularExpressionClass<'a> : Parser<InputElement, 'a> =
            parse {
                do! skipChar '['
                let! e = parseRegularExpressionClassChars
                do! skipChar ']'
                return RegularExpressionClass (e)
            }

        let parseRegularExpressionChar<'a> : Parser<InputElement, 'a> =
            choice [
                parseRegularExpressionBackslashSequence
                parseRegularExpressionClass
                attempt (parseRegularExpressionNonTerminator 
                            >>= fun r ->
                                match r with
                                | RegularExpressionNonTerminator ('/') -> pzero
                                | _ -> preturn r)
            ] |>> RegularExpressionChar

        let parseRegularExpressionFirstChar<'a> : Parser<InputElement, 'a> =
            choice [
                parseRegularExpressionBackslashSequence
                parseRegularExpressionClass
                attempt (parseRegularExpressionNonTerminator 
                            >>= fun r ->
                                match r with
                                | RegularExpressionNonTerminator ('*')
                                | RegularExpressionNonTerminator ('/') -> pzero
                                | _ -> preturn r)
            ] |>> RegularExpressionFirstChar

        let parseRegularExpressionChars<'a> : Parser<InputElement, 'a> =
            manyFold (RegularExpressionChars (Nil, Nil)) (fun x y -> RegularExpressionChars (x, y)) parseRegularExpressionChar

        let parseRegularExpressionBody<'a> : Parser<InputElement, 'a> =
            parse {
                let! first = parseRegularExpressionFirstChar
                let! chars = parseRegularExpressionChars
                return RegularExpressionBody (first, chars)
            }

        let parseRegularExpressionLiteral<'a> : Parser<InputElement, 'a> =
            parse {
                do! skipChar '/'
                let! body = parseRegularExpressionBody
                do! skipChar '/'
                let! flags = parseRegularExpressionFlags
                return RegularExpressionLiteral (body, flags)
            } |> attempt
            
        let rec evalRegularExpressionFlags v =
            match v with
            | RegularExpressionFlags (Nil, Nil) -> ""
            | RegularExpressionFlags (flags, flag) ->
                let flags = evalRegularExpressionFlags flags
                let flag = IdentifierNameParser.evalIdentifierPart flag
                flags + flag.ToString() 
        
        let evalRegularExpressionNonTerminator v =
            match v with
            | RegularExpressionNonTerminator v ->
                v |> string

        let evalRegularExpressionBackslashSequence v =
            match v with
            | RegularExpressionBackslashSequence v ->
                evalRegularExpressionNonTerminator v

        let evalRegularExpressionClassChar v =
            match v with
            | RegularExpressionChar v ->
                match v with
                | RegularExpressionNonTerminator (_) ->
                    evalRegularExpressionNonTerminator v    
                | RegularExpressionBackslashSequence (_) ->
                    evalRegularExpressionBackslashSequence v

        let rec evalRegularExpressionClassChars v =
            match v with
            | RegularExpressionClassChars (Nil, Nil) -> ""
            | RegularExpressionClassChars (classChars, classChar) ->
                let classChars = evalRegularExpressionClassChars classChars
                let classChar = evalRegularExpressionClassChar classChar
                classChars + classChar 
        
        let evalRegularExpressionClass v =
            match v with
            | RegularExpressionClass classChars ->
                evalRegularExpressionClassChars classChars     

        let evalRegularExpressionChar v =
            match v with
            | RegularExpressionChar v ->
                match v with
                | RegularExpressionNonTerminator (_) ->
                    evalRegularExpressionNonTerminator v    
                | RegularExpressionBackslashSequence (_) ->
                    evalRegularExpressionBackslashSequence v
                | RegularExpressionClass (_) ->
                    evalRegularExpressionClass v

        let rec evalRegularExpressionChars v =
            match v with
            | RegularExpressionChars (Nil, Nil) -> ""
            | RegularExpressionChars (chars, char) ->
                let chars = evalRegularExpressionChars chars
                let char = evalRegularExpressionChar char
                chars + char 

        let evalRegularExpressionFirstChar v =
            match v with
            | RegularExpressionFirstChar v ->
                match v with
                | RegularExpressionNonTerminator (_) ->
                    evalRegularExpressionNonTerminator v    
                | RegularExpressionBackslashSequence (_) ->
                    evalRegularExpressionBackslashSequence v
                | RegularExpressionClass (_) ->
                    evalRegularExpressionClass v

        let evalRegularExpressionBody v =
            match v with
            | RegularExpressionBody (first, chars) ->
                let first = evalRegularExpressionFirstChar first
                let chars = evalRegularExpressionChars chars
                first + chars

        let evalRegularExpressionLiteral v =
            match v with
            | RegularExpressionLiteral (body, flags) ->
                let body = evalRegularExpressionBody body
                let flags = evalRegularExpressionFlags flags
                body, flags
            
    let private divChoice : Parser<InputElement, LexerState> =
        getUserState >>=
            fun s ->
                match s.previousElement with
                | Some element ->
                    match element with
                    | IdentifierName (_, _) ->
                        match IdentifierNameParser.evalIdentifierName element with
                        | "true" | "false" | "null" | "this" -> DivPunctuator.parseDivPunctuator <|> RegularExpressionLiteralParser.parseRegularExpressionLiteral
                        | _ -> RegularExpressionLiteralParser.parseRegularExpressionLiteral <|> DivPunctuator.parseDivPunctuator   
                    | Punctuator (Str "]")
                    | Punctuator (Str ")")
                    | NumericLiteral _ 
                    | StringLiteral _ -> DivPunctuator.parseDivPunctuator <|> RegularExpressionLiteralParser.parseRegularExpressionLiteral
                    | _ -> RegularExpressionLiteralParser.parseRegularExpressionLiteral <|> DivPunctuator.parseDivPunctuator 
                | None -> RegularExpressionLiteralParser.parseRegularExpressionLiteral <|> DivPunctuator.parseDivPunctuator 

    let private exec =
        choice [
            WhiteSpace.parseWhiteSpace
            LineTerminator.parseLineTerminator
            Comment.parseComment
            IdentifierNameParser.identifierName
            NumericLiteralParser.numericLiteral
            Punctuator.parsePunctuator
            StringLiteralParser.stringLiteral
            divChoice  
        ]

    let tokenize (input:string) =        
        let rec tokenize i r =
            seq {
                match r with
                | Success (v, s, p) ->
                    match v with
                    | WhiteSpace
                    | Comment (SingleLineComment _) -> ()
                    | Comment (MultiLineComment ct) ->
                        if ct |> Seq.exists CharSets.isLineTerminator then
                            yield LineTerminator
                    | _ -> 
                        yield v
                    let index = int p.Index + i
                    let length = (input.Length - index)
                    if length > 0 then   
                        let state =
                            match v with
                            | WhiteSpace
                            | Comment _ -> s
                            | _ -> { previousElement = Some v }          
                        yield! tokenize index (runParserOnSubstring exec state "" input index length)
                | Failure (m, e, s) -> failwith m
            }
        let state = { previousElement = None }
        tokenize 0 (runParserOnString exec state "" input) |> LazyList.ofSeq