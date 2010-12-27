namespace Machete.Compiler

module StringNumericLiteral =

    open Lexer
    open FParsec.CharParsers
    open FParsec.Primitives

    type Node =
    | StringNumericLiteral of Node * Node * Node
    | StrWhiteSpace of Node * Node  
    | StrWhiteSpaceChar of char
    | StrNumericLiteral of Node  
    | StrDecimalLiteral of Node * Node
    | StrUnsignedDecimalLiteral of Node * Node * Node * Node * Node
    | DecimalDigits of Node * Node
    | DecimalDigit of char
    | ExponentPart of Node * Node
    | ExponentIndicator
    | SignedInteger of Node * Node
    | HexIntegerLiteral of Node * Node
    | HexDigit of char
    | Char of char
    | String of string
    | Nil
    
    
    let parseHexDigit<'a> =
        (satisfy CharSets.isHexDigit) |>> HexDigit
        
    let parseHexIntegerLiteral<'a> =
        parse {
            let! a = tuple2 ((pstring "0x" <|> pstring "0X") |>> String) parseHexDigit |>> HexIntegerLiteral
            let! b = manyFold a (fun a b -> HexIntegerLiteral (a, b)) parseHexDigit
            return b
        }

    let parseDecimalDigit<'a> =
        (satisfy CharSets.isDecimalDigit) |>> DecimalDigit
        
    let parseDecimalDigits<'a> =
        parse {
            let! first = parseDecimalDigit
            let first = DecimalDigits (Nil, first) 
            let! result = manyFold first (fun a b -> DecimalDigits (a, b)) parseDecimalDigit
            return result
        }

    let parseStrWhiteSpaceChar<'a> =
        satisfy (fun c -> CharSets.isWhiteSpace c || CharSets.isLineTerminator c) |>> StrWhiteSpaceChar
        
    let parseSignedInteger<'a> =
        tuple2 ((anyOf "+-" |>> Char) <|> preturn Nil) parseDecimalDigits |>> SignedInteger
        
    let parseExponentIndicator<'a> =
        skipAnyOf "eE" |>> fun () -> ExponentIndicator
        
    let parseExponentPart<'a> =
        tuple2 parseExponentIndicator parseSignedInteger |>> ExponentPart
        
    let parseStrUnsignedDecimalLiteral<'a> =
        attempt (parse {
            let! e1 = pstring "Infinity" |>> String
            return StrUnsignedDecimalLiteral (e1, Nil, Nil, Nil, Nil)
        }) <|> parse {
            let! e1 = pchar '.' |>> Char
            let! e2 = parseDecimalDigits
            let! e3 = parseExponentPart <|> preturn Nil
            return StrUnsignedDecimalLiteral (Nil, Nil, e1, e2, e3)
        } <|>  parse {
            let! e1 = parseDecimalDigits
            let! e2 = (pchar '.' |>> Char) <|> preturn Nil 
            let! e3 = parseDecimalDigits <|> preturn Nil
            let! e4 = parseExponentPart <|> preturn Nil
            return StrUnsignedDecimalLiteral (Nil, e1, e2, e3, e4)
        }
        
    let parseStrDecimalLiteral<'a> =
        parse {
            let! e1 = anyOf "+-" |>> Char <|> preturn Nil
            let! e2 = parseStrUnsignedDecimalLiteral
            return StrDecimalLiteral (e1, e2)
        }
        
    let parseStrNumericLiteral<'a> =
        (attempt parseHexIntegerLiteral <|> parseStrDecimalLiteral) |>> StrNumericLiteral

    let parseStrWhiteSpace<'a> =
        many1Fold Nil (fun a b -> StrWhiteSpace (a, b)) parseStrWhiteSpaceChar

    let parseStringNumericLiteral<'a> =
        parse {
            let ws = attempt (parseStrWhiteSpace) <|> preturn Nil
            let! e1 = ws
            let! e2 = parseStrNumericLiteral <|> (eof |>> fun () -> Nil)
            let! e3 = ws
            return  StringNumericLiteral (e1, e2, e3)
        }

    let parse input =
        match runParserOnString parseStringNumericLiteral () "" input with
        | Success(a, b, c) -> a
        | Failure(a, b, c) -> Nil

    
    let rec countDecimalDigits v c =
        match v with
        | DecimalDigits (Nil, d) -> c + 1
        | DecimalDigits (dd, d) -> countDecimalDigits dd (c + 1)

    let evalHexDigit v =
        match v with
        | HexDigit c when c >= '0' && c <= '9' -> (int c - 48) |> double
        | HexDigit c when c >= 'a' && c <= 'f' -> (int c - 87) |> double
        | HexDigit c when c >= 'A' && c <= 'F' -> (int c - 55) |> double

    let rec evalHexIntegerLiteral v =
        match v with
        | HexIntegerLiteral (String "0x", hd) 
        | HexIntegerLiteral (String "0X", hd) -> evalHexDigit hd
        | HexIntegerLiteral (hil, hd) -> 16.0 + evalHexIntegerLiteral hil + evalHexDigit hd

    let evalDecimalDigit v =
        match v with
        | DecimalDigit c -> (int c - 48) |> double

    let rec evalDecimalDigits v =
        match v with
        | DecimalDigits (Nil, d) -> evalDecimalDigit d 
        | DecimalDigits (dd, d) -> (10.0 * evalDecimalDigits dd) + evalDecimalDigit d
        
    let evalSignedInteger v =
        match v with
        | SignedInteger (Nil, d)
        | SignedInteger (Char '+', d) -> evalDecimalDigits d
        | SignedInteger (Char '-', d) -> -evalDecimalDigits d

    let evalExponentPart v =
        match v with
        | ExponentPart (ei, si) -> evalSignedInteger si

    let evalStrUnsignedDecimalLiteral v =
        match v with
        | StrUnsignedDecimalLiteral (Nil, d, Nil, Nil, Nil)
        | StrUnsignedDecimalLiteral (Nil, d, Char '.', Nil, Nil) -> 
            evalDecimalDigits d
        | StrUnsignedDecimalLiteral (Nil, DecimalDigits (_, _), Char '.', DecimalDigits (_, _), Nil) -> 
            match v with 
            | StrUnsignedDecimalLiteral (_, a, _, b, _) -> 
                let n = countDecimalDigits b 0 |> double
                let integral = evalDecimalDigits a
                let fractional = evalDecimalDigits b
                integral +  (fractional * 10.0 ** -n) 
        | StrUnsignedDecimalLiteral (Nil, DecimalDigits (_, _), Char '.', DecimalDigits (_, _), ExponentPart (_, _)) -> 
            match v with 
            | StrUnsignedDecimalLiteral (_, a, _, b, c) ->
                let integral = evalDecimalDigits a
                let fractional = evalDecimalDigits b  
                let n = countDecimalDigits b 0 |> double
                let e = evalExponentPart c
                (integral + (fractional * (10.0 ** -n))) * (10.0 ** e)
        | StrUnsignedDecimalLiteral (Nil, d, Nil, Nil, e) ->
            let integral = evalDecimalDigits d
            let e = evalExponentPart e 
            integral * 10.0 ** e 
        | StrUnsignedDecimalLiteral (Nil, Nil, Char '.', dl, Nil) ->
            let n = countDecimalDigits dl 0 |> double 
            let fractional = evalDecimalDigits dl
            fractional * 10.0 ** -n 
        | StrUnsignedDecimalLiteral (Nil, Nil, Char '.', dl, e) ->
            let fractional = evalDecimalDigits dl  
            let n = countDecimalDigits dl 0 |> double
            let e = evalExponentPart e 
            fractional * 10.0 ** (e - n) 
        | StrUnsignedDecimalLiteral (String "Infinity", Nil, Nil, Nil, Nil) -> 
            infinity

    let evalStrDecimalLiteral v =
        match v with
        | StrDecimalLiteral (Nil, a)
        | StrDecimalLiteral (Char '+', a) -> evalStrUnsignedDecimalLiteral a
        | StrDecimalLiteral (Char '-', a) -> -evalStrUnsignedDecimalLiteral a
         
    let evalStrNumericLiteral v =
        match v with
        | StrNumericLiteral (StrDecimalLiteral _) -> 
            match v with
            | StrNumericLiteral a -> evalStrDecimalLiteral a
        | StrNumericLiteral (HexIntegerLiteral _) -> 
            match v with
            | StrNumericLiteral a -> evalHexIntegerLiteral a

    let evalStringNumericLiteral v =
        match v with
        | Nil -> nan
        | StringNumericLiteral (_, Nil, _) -> 0.0
        | StringNumericLiteral (_, a, _) -> evalStrNumericLiteral a

    let eval input =
        evalStringNumericLiteral (parse input)    

