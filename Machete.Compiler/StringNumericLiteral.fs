namespace Machete.Compiler

module StringNumericLiteral =

    open FParsec.CharParsers
    open FParsec.Primitives

    type Node =
    | StringNumericLiteral of Node * Node * Node
    | StrWhiteSpace of Node * Node  
    | StrWhiteSpaceChar of InputElement
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
        (satisfy CharSets.hexDigitCharSet.Contains) |>> HexDigit
        
    let parseHexIntegerLiteral<'a> =
        parse {
            let! a = tuple2 ((pstring "0x" <|> pstring "0X") |>> String) parseHexDigit |>> HexIntegerLiteral
            let! b = manyFold a (fun a b -> HexIntegerLiteral (a, b)) parseHexDigit
            return b
        }

    let parseDecimalDigit<'a> =
        (satisfy CharSets.decimalDigitCharSet.Contains) |>> DecimalDigit
        
    let parseDecimalDigits<'a> =
        many1Fold Nil (fun a b -> DecimalDigits (a, b)) parseDecimalDigit

    let parseStrWhiteSpaceChar<'a> =
        (Parsers.whiteSpace <|> Parsers.lineTerminator) |>> StrWhiteSpaceChar
        
    let parseSignedInteger<'a> =
        tuple2 ((anyOf "+-" |>> Char) <|> preturn Nil) parseDecimalDigits |>> SignedInteger
        
    let parseExponentIndicator<'a> =
        skipAnyOf "eE" |>> fun () -> ExponentIndicator
        
    let parseExponentPart<'a> =
        tuple2 parseExponentIndicator parseSignedInteger |>> ExponentPart
        
    let parseStrUnsignedDecimalLiteral<'a> =
        tuple5 ((pstring "Infinity" |>> String) <|> preturn Nil) 
            parseDecimalDigits 
            ((pchar '.' |>> Char) <|> preturn Nil) 
            (parseDecimalDigits <|> preturn Nil)
            (parseExponentPart <|> preturn Nil) 
        |>> StrUnsignedDecimalLiteral
        
    let parseStrDecimalLiteral<'a> =
        tuple2 ((anyOf "+-" |>> Char) <|> preturn Nil) parseStrUnsignedDecimalLiteral |>> StrDecimalLiteral
        
    let parseStrNumericLiteral<'a> =
        (parseStrDecimalLiteral <|> parseHexIntegerLiteral) |>> StrNumericLiteral

    let parseStrWhiteSpace<'a> =
        many1Fold Nil (fun a b -> StrWhiteSpace (a, b)) parseStrWhiteSpaceChar

    let parseStringNumericLiteral<'a> =
        let ws = parseStrWhiteSpace <|> preturn Nil
        tuple3 ws parseStrNumericLiteral ws |>> StringNumericLiteral

    let parse input =
        match runParserOnString parseStringNumericLiteral () input "" with
        | Success(a, b, c) -> a
        | Failure(a, b, c) -> failwith a 

    
    let rec countDecimalDigits v c =
        match v with
        | DecimalDigits (Nil, d) -> c
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
        | DecimalDigits (dd, d) -> 10.0 + evalDecimalDigits dd + evalDecimalDigit d
        
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
                evalDecimalDigits a +  evalDecimalDigits b * 10.0 ** (float (-countDecimalDigits b 0)) 
        | StrUnsignedDecimalLiteral (Nil, DecimalDigits (_, _), Char '.', DecimalDigits (_, _), ExponentPart (_, _)) -> 
            match v with 
            | StrUnsignedDecimalLiteral (_, a, _, b, c) -> 
                evalDecimalDigits a +  evalDecimalDigits b * 10.0 ** (evalExponentPart c - float (-countDecimalDigits b 0)) 
        | StrUnsignedDecimalLiteral (Nil, d, Nil, Nil, e) -> 
            evalDecimalDigits d * 10.0 ** (float (evalExponentPart e)) 
        | StrUnsignedDecimalLiteral (Nil, Nil, Char '.', dl, Nil) -> 
            evalDecimalDigits dl * 10.0 ** (float (-countDecimalDigits dl 0)) 
        | StrUnsignedDecimalLiteral (Nil, Nil, Char '.', dl, e) -> 
            evalDecimalDigits dl * 10.0 ** (evalExponentPart e - float (-countDecimalDigits dl 0)) 
        | StrUnsignedDecimalLiteral (String "Infinity", Nil, Nil, Nil, Nil) -> 
            infinity

    let evalStrDecimalLiteral v =
        match v with
        | StrDecimalLiteral (Nil, a)
        | StrDecimalLiteral (Char '+', a) -> evalStrUnsignedDecimalLiteral v
        | StrDecimalLiteral (Char '-', a) -> -evalStrUnsignedDecimalLiteral v
         
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
        | StringNumericLiteral (_, Nil, _) -> 0.0
        | StringNumericLiteral (_, a, _) -> evalStrDecimalLiteral a

    let eval input =
        evalStringNumericLiteral (parse input)    

