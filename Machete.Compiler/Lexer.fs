namespace Machete.Compiler

module Lexer =

    open FParsec.CharParsers
    open FParsec.Primitives
    
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


        let hexDigit reply =
            (parse {
                let! c = satisfy CharSets.isHexDigit
                return HexDigit (Chr (c))
            }) reply
    
        let hexIntegerLiteral reply =
            (parse {
                do! skipChar '0'
                do! skipAnyOf "xX"
                let! r = many1Fold Nil (fun x y -> HexIntegerLiteral (x, y)) hexDigit <?> "A hex integer literal was found to be incomplete."
                return r
            }) reply

        let nonZeroDigit reply =
            (satisfy CharSets.isNonZeroDigit |>> Chr |>> NonZeroDigit) reply

        let decimalDigit reply =
            (satisfy CharSets.isDecimalDigit |>> Chr |>> DecimalDigit) reply

        let decimalDigits reply =
            many1Fold Nil (fun x y -> DecimalDigits (x, y)) decimalDigit reply         
        
        let sign reply =
            ((skipChar '+' |>> fun () -> Plus) <|> (skipChar '-' |>> fun () -> Minus) <|> preturn Sign.Missing) reply

        let signedInteger reply =
            (parse {
                let! a = sign
                let! b = decimalDigits
                return SignedInteger (a, b) 
            }) reply   
         
        let exponentIndicator reply =
            (anyOf "eE" |>> Chr |>> ExponentIndicator) reply
         
        let exponentPart reply =
            pipe2 exponentIndicator signedInteger (fun a b -> ExponentPart(a, b)) reply
        
        let decimalIntegerLiteral reply =
            (parse {
                do! skipChar '0'
                return DecimalIntegerLiteral (Chr '0', Nil)
             } <|> parse {
                let! first = nonZeroDigit
                let! rest = decimalDigits <|> preturn Nil    
                return DecimalIntegerLiteral (first, rest)
            }) reply
        
        let decimalLiteral reply =
            (parse {
                let! i = decimalIntegerLiteral
                let! e1, e2, e3 =
                    parse {
                        do! skipChar '.'
                        let! f = decimalDigits <|> nil
                        let! e = exponentPart <|> nil
                        return DecimalPoint, f, e
                    } <|> parse {
                        let! e = exponentPart <|> nil
                        return Nil, Nil, e
                    }
                return DecimalLiteral (i, e1, e2, e3)
             } <|> parse {
                do! skipChar '.'
                let! f = decimalDigits
                let! e = exponentPart <|> nil
                return DecimalLiteral (Nil, DecimalPoint, f, e)
            }) reply 

        let numericLiteral reply =
            (attempt (parse {
                let! r = decimalLiteral
                do! notFollowedBy identifierStart
                do! notFollowedBy decimalDigit
                return NumericLiteral r
             }) <|> parse {
                let! r = hexIntegerLiteral
                do! notFollowedBy identifierStart
                do! notFollowedBy decimalDigit
                return NumericLiteral r
            }) reply

        let evalHexDigit v =
            match v with
            | HexDigit v -> 
                match v with
                | Chr c -> 
                    match c with
                    | c when c >= '0' && c <= '9' -> int c - 48 |> double   
                    | c when c >= 'a' && c <= 'f' -> int c - 87 |> double  
                    | c when c >= 'A' && c <= 'F' -> int c - 55 |> double

        let rec evalHexIntegerLiteral v =
            match v with
            | HexIntegerLiteral (l, r) -> 
                match l, r with
                | Nil, HexDigit _ ->
                    evalHexDigit r |> double 
                | HexIntegerLiteral (_, _), HexDigit _ ->
                    (16.0 * evalHexIntegerLiteral l + evalHexDigit r) |> double

        let evalDecimalDigit v =
            match v with
            | DecimalDigit v -> 
                match v with
                | Chr c ->
                    assert(c >= '0' && c <= '9')
                    int c - 48

        let evalNonZeroDigit v =
            match v with
            | NonZeroDigit v -> 
                match v with
                | Chr c -> 
                    assert(c >= '1' && c <= '9')
                    int c - 48
    
        let rec countDecimalDigits v n =
            match v with
            | DecimalDigits (l, r) ->
                match l, r with
                | Nil, DecimalDigit _ -> n + 1
                | DecimalDigits (_, _), DecimalDigit _ -> countDecimalDigits l (n + 1)

        let rec evalDecimalDigits d =
            match d with
            | DecimalDigits (l, r) ->
                match l, r with
                | Nil, DecimalDigit _ ->
                    evalDecimalDigit r
                | DecimalDigits (_, _), DecimalDigit _ ->
                    10 * evalDecimalDigits l + evalDecimalDigit r

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

        let evalSignedInteger v =
            match v with
            | SignedInteger (l, r) -> 
                match l, r with
                | Missing, DecimalDigits (_, _)
                | Plus, DecimalDigits (_, _) ->
                    evalDecimalDigits r
                | Minus, DecimalDigits (_, _) ->
                    -evalDecimalDigits r
        
        let evalExponentPart v =
            match v with
            | ExponentPart (l, r) -> 
                match r with
                | SignedInteger (_, _) ->
                    evalSignedInteger r
        
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

        let evalHexEscapeSequence v =
            match v with
            | HexEscapeSequence (h, l) -> 
                char (16.0 * evalHexDigit h + evalHexDigit l)

        let evalNonEscapeCharacter v =
            match v with
            | NonEscapeCharacter v -> 
                match v with
                | Chr c -> c
        
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
    
        let evalEscapeSequence v =
            match v with
            | EscapeSequence v -> 
                match v with
                | CharacterEscapeSequence _ -> evalCharacterEscapeSequence v
                | Chr '0' -> '\u0000'
                | HexEscapeSequence (_,_) -> evalHexEscapeSequence v 
                | UnicodeEscapeSequence (_,_,_,_) -> evalUnicodeEscapeSequence v
    
        let evalSingleStringCharacter v =
            match v with
            | SingleStringCharacter v -> 
                match v with
                | Chr c -> c |> string
                | EscapeSequence _ -> evalEscapeSequence v |> string
                | LineContinuation -> ""

        let evalDoubleStringCharacter v =
            match v with
            | DoubleStringCharacter v -> 
                match v with
                | Chr c -> c |> string
                | EscapeSequence _ -> evalEscapeSequence v |> string
                | LineContinuation -> ""

        let evalLineContinuation v =
            match v with
            | LineContinuation -> ""

        let rec evalSingleStringCharacters v =
            match v with
            | SingleStringCharacters (l, r) -> 
                match l, r with
                | SingleStringCharacter _, Nil ->
                    evalSingleStringCharacter l |> string
                | SingleStringCharacter _, SingleStringCharacters (_, _) ->
                    (evalSingleStringCharacter l) + evalSingleStringCharacters r

        let rec evalDoubleStringCharacters v =
            match v with
            | DoubleStringCharacters (l, r) -> 
                match l, r with
                | DoubleStringCharacter _, Nil ->
                    evalDoubleStringCharacter l |> string
                | DoubleStringCharacter _, DoubleStringCharacters (_, _) ->
                    (evalDoubleStringCharacter l) + evalDoubleStringCharacters r
        
        let evalStringLiteral v =
            match v with
            | StringLiteral v -> 
                match v with
                | DoubleStringCharacters (_, _) ->
                    evalDoubleStringCharacters v
                | SingleStringCharacters (_, _) ->
                    evalSingleStringCharacters v 
                | Nil -> ""           

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
            parse {
                let! first = identifierStart
                let! rest = many identifierPart
                return rest |> List.fold (fun x y -> IdentifierName(x, y)) (IdentifierName (first, Nil))
            }

        do identifierStartRef := identifierStart

        let evalIdentifierStart v =
            match v with
            | IdentifierStart v -> 
                match v with
                | UnicodeLetter c -> c
                | Chr c -> c
                | UnicodeEscapeSequence (_, _, _, _) -> evalUnicodeEscapeSequence v
    
        let evalIdentifierPart v =
            match v with
            | IdentifierPart v -> 
                match v with
                | IdentifierStart _ -> evalIdentifierStart v
                | UnicodeCombiningMark c -> c
                | UnicodeDigit c -> c
                | UnicodeConnectorPunctuation c -> c
                | Chr c -> c   

        let rec evalIdentifierName v =
            match v with
            | IdentifierName (l, r) -> 
                match l, r with
                | IdentifierStart _, Nil ->
                    evalIdentifierStart l |> string
                | IdentifierName (_, _), IdentifierPart _ ->
                    evalIdentifierName l + (evalIdentifierPart r |> string)

    module Punctuator =

        let punctuatorChoices = 
            choice [
                str "!=="; str "!="; str "!"
                str "%="; str "%"
                str "&&"; str "&="; str "&"
                str "("
                str ")"
                str "*="; str "*"
                str "++"; str "+="; str "+"
                str ","
                str "-="; str "--"; str "-"
                str "."
                str ";"
                str "<<="; str "<<"; str "<="; str "<"
                str "==="; str "=="; str "="
                str ">>>="; str ">>>"; str ">>="; str ">>"; str ">="; str ">"
                str "?"
                str ":"
                str "["
                str "]"
                str "^="; str "^"
                str "{"
                str "||"; str "|="; str "|"
                str "}"
                str "~"
            ]

        let parsePunctuator reply =
            (punctuatorChoices |>> Punctuator) reply
            
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

    let private choices = [|
        WhiteSpace.parseWhiteSpace
        LineTerminator.parseLineTerminator
        Comment.parseComment
        IdentifierNameParser.identifierName
        NumericLiteralParser.numericLiteral
        Punctuator.parsePunctuator
        StringLiteralParser.stringLiteral
        divChoice  
    |]

    let private exec reply =
        choice choices reply
        
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