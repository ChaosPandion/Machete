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
    | RegularExpressionLiteral of InputElement * InputElement * InputElement * InputElement
    | RegularExpressionBody of InputElement * InputElement
    | RegularExpressionChars of InputElement * InputElement
    | RegularExpressionFirstChar of InputElement
    | RegularExpressionChar of InputElement
    | RegularExpressionBackslashSequence of InputElement * InputElement
    | RegularExpressionNonTerminator of InputElement
    | RegularExpressionClass of InputElement * InputElement * InputElement
    | RegularExpressionClassChars of InputElement * InputElement
    | RegularExpressionClassChar of InputElement
    | RegularExpressionFlags of InputElement * InputElement
//    | Null
//    | True
//    | False
//    | Break 
//    | Case 
//    | Catch 
//    | Continue 
//    | Debugger 
//    | Default 
//    | Delete 
//    | Do
//    | Else 
//    | Finally 
//    | For 
//    | Function
//    | If 
//    | In 
//    | Instanceof 
//    | New
//    | Return
//    | Switch
//    | This 
//    | Throw 
//    | Try 
//    | Typeof
//    | Var 
//    | Void
//    | While 
//    | With
//    | Class
//    | Const
//    | Enum
//    | Export
//    | Extends
//    | Implements
//    | Import
//    | Interface
//    | Let
//    | Package
//    | Private
//    | Protected
//    | Public
//    | Static
//    | Super
//    | Yield
//    | LeftCurlyBracket
//    | RightCurlyBracket
//    | LeftParenthesis
//    | RightParenthesis
//    | LeftSquareBracket
//    | RightSquareBracket
//    | FullStop
//    | Comma
//    | LessThan
//    | GreaterThan
//    | LessThanOrEqual
//    | GreaterThanOrEqual
//    | Equal
//    | DoesNotEqual
//    | StrictEqual
//    | StrictDoesNotEqual
//    | Plus
//    | Minus
//    | Multiply
//    | Divide
//    | Modulus
//    | Increment
//    | Decrement
//    | LeftShift
//    | SignedRightShift
//    | UnsignedRightShift
//    | BitwiseAnd
//    | BitwiseOr
//    | BitwiseXor
//    | LogicalNot
//    | BitwiseNot
//    | LogicalAnd
//    | LogicalOr
//    | QuestionMark
//    | Colon
//    | Assign
//    | PlusAssign
//    | MinusAssign
//    | MultiplyAssign
//    | DivideAssign
//    | ModulusAssign
//    | LeftShiftAssign
//    | SignedRightShiftAssign
//    | UnsignedRightShiftAssign
//    | BitwiseAndAssign
//    | BitwiseOrAssign
//    | BitwiseXorAssign


    type LexerState = {
        previousElement : option<InputElement>
    }
    
    let nil<'a> : Parser<InputElement, 'a> = preturn Nil
    let str s = pstring s |>> Str
    let chr c = pchar c |>> Chr
    let maybe p = p <|> preturn Nil

    module WhiteSpace =
        let parseWhiteSpace<'a> : Parser<InputElement, 'a> =
            satisfy CharSets.whiteSpaceCharSet.Contains |>> (fun c -> WhiteSpace)

    module LineTerminator =        
        let parseLineTerminator<'a> : Parser<InputElement, 'a> =
            satisfy CharSets.lineTerminatorCharSet.Contains |>> (fun c -> LineTerminator)    
        let parseLineTerminatorSequence<'a> : Parser<InputElement, 'a> =
            satisfy CharSets.lineTerminatorCharSet.Contains |>> (fun c -> LineTerminatorSequence)

    module Comment =            
        let parseSingleLineComment<'a> : Parser<InputElement, 'a> =
            pipe2 (pstring "//") (manyCharsTill anyChar (lookAhead (satisfy CharSets.lineTerminatorCharSet.Contains))) (fun a b -> SingleLineComment b)
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
            satisfy CharSets.hexDigitCharSet.Contains |>> Chr |>> HexDigit
    
        let hexIntegerLiteral<'a> : Parser<InputElement, 'a> =
            parse {
                let! a = anyOf "0"
                let! b = anyOf "xX"
                let! c = many1Fold Nil (fun x y -> HexIntegerLiteral (x, y)) hexDigit 
                return c 
            }

        let nonZeroDigit<'a> : Parser<InputElement, 'a> =
            satisfy CharSets.nonZeroDigitCharSet.Contains |>> Chr |>> NonZeroDigit

        let decimalDigit<'a> : Parser<InputElement, 'a> =
            satisfy CharSets.decimalDigitCharSet.Contains |>> Chr |>> DecimalDigit

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
            let optExp = exponentPart <|> nil
            let top = pipe3 decimalPoint (decimalDigits <|> nil) optExp (fun a b c -> a, b, c)
            let middle = pipe3 decimalPoint decimalDigits optExp (fun a b c -> a, b, c)
            let bottom = optExp |>> (fun a -> Nil, Nil, a)
            let completeTopOrBottom a (b, c, d) = DecimalLiteral (a, b, c, d)
            let completeMiddle a b c = DecimalLiteral (Nil, a, b, c)
            let left = pipe2 decimalIntegerLiteral (top <|> bottom) completeTopOrBottom
            let right = pipe3 decimalPoint decimalDigits optExp completeMiddle
            left <|> right

        let numericLiteral<'a> : Parser<InputElement, 'a> =
            (decimalLiteral <|> hexIntegerLiteral) |>> NumericLiteral


        let evalHexDigit v =
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

        let evalDecimalDigit v =
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
                    let n = int (10.0 ** -float (countDecimalDigits c 0)) 
                    evalDecimalIntegerLiteral a + (n * evalDecimalDigits c) |> double
                | DecimalIntegerLiteral (_, _), Nil, Nil, ExponentPart (_, _)
                | DecimalIntegerLiteral (_, _), DecimalPoint, Nil, ExponentPart (_, _) ->
                    let e = int (10.0 ** -float (evalExponentPart d))
                    evalDecimalIntegerLiteral a * e |> double
                | DecimalIntegerLiteral (_, _), DecimalPoint, DecimalDigits (_, _), ExponentPart (_, _) -> 
                    let n = int (10.0 ** -float (countDecimalDigits c 0))
                    let e = int (10.0 ** -float (evalExponentPart d))
                    evalDecimalIntegerLiteral a + (n * evalDecimalDigits c) * e |> double
                | Nil, DecimalPoint, DecimalDigits (_, _), Nil -> 
                    let n = int (10.0 ** -float (countDecimalDigits c 0))
                    n * evalDecimalDigits c |> double
                | Nil, DecimalPoint, DecimalDigits (_, _), ExponentPart (_, _) -> 
                    let n = countDecimalDigits c 0
                    let e = evalExponentPart d
                    evalDecimalDigits c * int (10.0 ** float (e - n)) |> double
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
            pipe2 hexDigit hexDigit (fun a b -> HexEscapeSequence (a, b))

        let unicodeEscapeSequence<'a> : Parser<InputElement, 'a> =
            pipe4 hexDigit hexDigit hexDigit hexDigit (fun a b c d -> UnicodeEscapeSequence (a, b, c, d))
    
        let escapeSequence<'a> : Parser<InputElement, 'a> =
            let a = characterEscapeSequence
            let b = pipe2 (pchar '0') (notFollowedBy decimalDigit) (fun a b -> Chr a)
            let c = hexEscapeSequence
            let d = unicodeEscapeSequence
            (a <|> b <|> c <|> d) |>> EscapeSequence
    
        let lineContinuation<'a> : Parser<InputElement, 'a> =
            pipe2 (pchar '\\') LineTerminator.parseLineTerminatorSequence (fun a b -> LineContinuation)

        let doubleStringCharacter<'a> : Parser<InputElement, 'a> =
            satisfy (fun c -> c <> '\"' && c <> '\\' && not (CharSets.lineTerminatorCharSet.Contains c)) |>> Chr |>> DoubleStringCharacter

        let singleStringCharacter<'a> : Parser<InputElement, 'a> =
            satisfy (fun c -> c <> '\'' && c <> '\\' && not (CharSets.lineTerminatorCharSet.Contains c)) |>> Chr |>> SingleStringCharacter
    
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
                | Chr c -> c
                | EscapeSequence _ -> evalEscapeSequence v
                | _ -> invalidOp ""                 
            | _ -> invalidArg "v" "Expected SingleStringCharacter."

        let evalDoubleStringCharacter v =
            match v with
            | DoubleStringCharacter v -> 
                match v with
                | Chr c -> c
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

    module IdentifierNameParser =

        open FParsec.CharParsers
        open FParsec.Primitives
        open NumericLiteralParser
        open StringLiteralParser

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
                ('-', choice [str "-="; str "-"])
                ('.', choice [str "."])
                (';', choice [str ";"])
                ('<', choice [str "<<"; str "<"])
                ('=', choice [str "==="; str "=="; str "="])
                ('>', choice [str ">>>="; str ">>>"; str ">>="; str ">>"; str ">="; str ">"])
                ('?', choice [str "?"])
                ('[', choice [str "["])
                ('^', choice [str "^="])
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
            (str "/=" <|> str "/") |>> DivPunctuator

    module RegularExpressionLiteral =
        let parseRegularExpressionFlags<'a> : Parser<InputElement, 'a> =
            manyFold (RegularExpressionFlags (Nil, Nil)) (fun x y -> RegularExpressionFlags (x, y)) IdentifierNameParser.identifierPart
        let parseRegularExpressionNonTerminator<'a> : Parser<InputElement, 'a> =
            satisfy (fun c -> not (CharSets.lineTerminatorCharSet.Contains c)) |>> Chr |>> RegularExpressionNonTerminator
        let parseRegularExpressionBackslashSequence<'a> : Parser<InputElement, 'a> =
            tuple2 (chr '\\') parseRegularExpressionNonTerminator |>> RegularExpressionBackslashSequence
        let parseRegularExpressionClassChar<'a> : Parser<InputElement, 'a> =
            choice [
                parseRegularExpressionBackslashSequence                
                attempt (parseRegularExpressionNonTerminator 
                            >>= fun r ->
                                match r with
                                | RegularExpressionNonTerminator (Chr ']') -> pzero
                                | _ -> preturn r)
            ] |>> RegularExpressionClassChar
        let parseRegularExpressionClassChars<'a> : Parser<InputElement, 'a> =
            manyFold (RegularExpressionClassChars (Nil, Nil)) (fun x y -> RegularExpressionClassChars (x, y)) parseRegularExpressionClassChar
        let parseRegularExpressionClass<'a> : Parser<InputElement, 'a> =
            tuple3 (chr '[') parseRegularExpressionClassChars (chr ']') |>> RegularExpressionClass
        let parseRegularExpressionChar<'a> : Parser<InputElement, 'a> =
            choice [
                parseRegularExpressionBackslashSequence
                parseRegularExpressionClass
                attempt (parseRegularExpressionNonTerminator 
                            >>= fun r ->
                                match r with
                                | RegularExpressionNonTerminator (Chr '/') -> pzero
                                | _ -> preturn r)
            ] |>> RegularExpressionClassChar
        let parseRegularExpressionFirstChar<'a> : Parser<InputElement, 'a> =
            choice [
                parseRegularExpressionBackslashSequence
                parseRegularExpressionClass
                attempt (parseRegularExpressionNonTerminator 
                            >>= fun r ->
                                match r with
                                | RegularExpressionNonTerminator (Chr '*')
                                | RegularExpressionNonTerminator (Chr '/') -> pzero
                                | _ -> preturn r)
            ] |>> RegularExpressionClassChar
        let parseRegularExpressionChars<'a> : Parser<InputElement, 'a> =
            manyFold (RegularExpressionChars (Nil, Nil)) (fun x y -> RegularExpressionChars (x, y)) parseRegularExpressionChar
        let parseRegularExpressionBody<'a> : Parser<InputElement, 'a> =
            tuple2 parseRegularExpressionFirstChar parseRegularExpressionChars |>> RegularExpressionBody
        let parseRegularExpressionLiteral<'a> : Parser<InputElement, 'a> =
            tuple4 (chr '/') parseRegularExpressionBody (chr '/') parseRegularExpressionFlags |>> RegularExpressionLiteral
            
    let private divChoice : Parser<InputElement, LexerState> =
        getUserState >>=
            fun s ->
                match s.previousElement with
                | Some element ->
                    match element with
                    | Token element ->
                        match element with
                        | IdentifierName (_, _) ->
                            match IdentifierNameParser.evalIdentifierName element with
                            | "true" | "false" | "null" | "this" -> DivPunctuator.parseDivPunctuator
                            | _ -> RegularExpressionLiteral.parseRegularExpressionLiteral   
                        | Punctuator (Str "]")
                        | Punctuator (Str ")")
                        | NumericLiteral _ 
                        | StringLiteral _ -> DivPunctuator.parseDivPunctuator
                        | _ -> RegularExpressionLiteral.parseRegularExpressionLiteral  
                    | _ -> RegularExpressionLiteral.parseRegularExpressionLiteral
                | None -> RegularExpressionLiteral.parseRegularExpressionLiteral 

    let private exec =
        choice [
            WhiteSpace.parseWhiteSpace
            LineTerminator.parseLineTerminator
            Comment.parseComment
            IdentifierNameParser.identifierName
            Punctuator.parsePunctuator
            NumericLiteralParser.numericLiteral
            StringLiteralParser.stringLiteral
            divChoice  
        ]

//    let reservedWordMap =
//        Map.ofList [
//            // Keyword
//            ("break", Break)
//            ("case", Case)
//            ("catch", Catch)
//            ("continue", Continue)
//            ("debugger", Debugger)
//            ("default", Default)
//            ("delete", Delete)
//            ("do", Do)
//            ("else", Else)
//            ("finally", Finally); 
//            ("for", For)
//            ("function", Function)
//            ("if", If)
//            ("in", In)
//            ("instanceof", Instanceof)
//            ("new", New)
//            ("return", Return)
//            ("switch", Switch)
//            ("this", This)
//            ("throw", Throw)
//            ("try", Try)
//            ("typeof", Typeof)
//            ("var", Var)
//            ("void", Void)
//            ("while", While)
//            ("with", With)
//            // FutureReservedWord
//            ("class", Class); 
//            ("const", Const)
//            ("enum", Enum); 
//            ("export", Export)
//            ("extends", Extends)
//            ("implements", Implements)
//            ("import", Import)
//            ("interface", Interface)
//            ("let", Let)
//            ("package", Package)
//            ("private", Private)
//            ("protected", Protected)
//            ("public", Public)
//            ("static", Static)
//            ("super", Super)
//            ("yield", Yield)
//            // NullLiteral
//            ("null", Null)
//            // BooleanLiteral
//            ("true", True)
//            ("false", False)
//        ]

    let tokenize (input:string) =        
        let rec tokenize i r =
            seq {
                match r with
                | Success (v, s, p) ->
                    match v with
                    | WhiteSpace
                    | Comment (SingleLineComment _) -> ()
                    | Comment (MultiLineComment ct) ->
                        if ct |> Seq.exists CharSets.lineTerminatorCharSet.Contains then
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