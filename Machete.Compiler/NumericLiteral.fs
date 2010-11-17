namespace Machete.Compiler

module NumericLiteral =

    open FParsec.CharParsers
    open FParsec.Primitives
    open Parsers

 
    let decimalPoint<'a> : Parser<InputElement, 'a> =
        pchar '.' |>> fun c -> DecimalPoint

    let hexDigit<'a> : Parser<InputElement, 'a> =
        satisfy CharSets.hexDigitCharSet.Contains |>> Char |>> HexDigit
    
    let hexIntegerLiteral<'a> : Parser<InputElement, 'a> =
        parse {
            let! a = anyOf "0"
            let! b = anyOf "xX"
            let! c = many1Fold Nil (fun x y -> HexIntegerLiteral (x, y)) hexDigit 
            return c 
        }

    let nonZeroDigit<'a> : Parser<InputElement, 'a> =
        satisfy CharSets.nonZeroDigitCharSet.Contains |>> Char |>> NonZeroDigit

    let decimalDigit<'a> : Parser<InputElement, 'a> =
        satisfy CharSets.decimalDigitCharSet.Contains |>> Char |>> DecimalDigit

    let decimalDigits<'a> : Parser<InputElement, 'a> =
        many1Fold Nil (fun x y -> DecimalDigits (x, y)) decimalDigit         
        
    let signedInteger<'a> : Parser<InputElement, 'a> =
        parse {
            let! a = (anyOf "+-" |>> Char) <|> nil
            let! b = decimalDigits
            return SignedInteger (a, b) 
        }   
         
    let exponentIndicator<'a> : Parser<InputElement, 'a> =
        anyOf "eE" |>> Char |>> ExponentIndicator
         
    let exponentPart<'a> : Parser<InputElement, 'a> =
        pipe2 exponentIndicator signedInteger (fun a b -> ExponentPart(a, b))
        
    let decimalIntegerLiteral<'a> : Parser<InputElement, 'a> =
        (pchar '0' |>> Char |>> (fun a -> a, Nil)) 
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
            | Char c -> 
                match c with
                | c when c >= '0' && c <= '9' -> int c - 48   
                | c when c >= 'a' && c <= 'f' -> int c - 87  
                | c when c >= 'A' && c <= 'F' -> int c - 55    
                | _ ->  invalidOp ("Unexpected value '" + c.ToString() + "' found while evaluating HexDigit.")        
            | _ -> invalidOp "Unexpected pattern for HexDigit."     
        | _ -> invalidArg "v" "Expected HexDigit."

    let rec evalHexIntegerLiteral v =
        match v with
        | HexIntegerLiteral (l, r) -> 
            match l, r with
            | Nil, HexDigit _ ->
                evalHexDigit r 
            | HexIntegerLiteral (_, _), HexDigit _ ->
                16 * evalHexIntegerLiteral l + evalHexDigit r  
            | _ -> invalidOp "Unexpected pattern for HexIntegerLiteral."       
        | _ -> invalidArg "v" "Expected HexDigit."

    let evalDecimalDigit v =
        match v with
        | DecimalDigit v -> 
            match v with
            | Char c ->
                assert(c >= '0' && c <= '9')
                int c - 48       
            | _ -> invalidOp "Unexpected pattern for DecimalDigit."     
        | _ -> invalidArg "v" "Expected DecimalDigit."

    let evalNonZeroDigit v =
        match v with
        | NonZeroDigit v -> 
            match v with
            | Char c -> 
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
            | Char _, Nil -> 0
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
            | Char c, DecimalDigits (_, _) ->
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
                evalDecimalIntegerLiteral a
            | DecimalIntegerLiteral (_, _), DecimalPoint, DecimalDigits (_, _), Nil -> 
                let n = int (10.0 ** -float (countDecimalDigits c 0)) 
                evalDecimalIntegerLiteral a + (n * evalDecimalDigits c) 
            | DecimalIntegerLiteral (_, _), Nil, Nil, ExponentPart (_, _)
            | DecimalIntegerLiteral (_, _), DecimalPoint, Nil, ExponentPart (_, _) ->
                let e = int (10.0 ** -float (evalExponentPart d))
                evalDecimalIntegerLiteral a * e
            | DecimalIntegerLiteral (_, _), DecimalPoint, DecimalDigits (_, _), ExponentPart (_, _) -> 
                let n = int (10.0 ** -float (countDecimalDigits c 0))
                let e = int (10.0 ** -float (evalExponentPart d))
                evalDecimalIntegerLiteral a + (n * evalDecimalDigits c) * e
            | Nil, DecimalPoint, DecimalDigits (_, _), Nil -> 
                let n = int (10.0 ** -float (countDecimalDigits c 0))
                n * evalDecimalDigits c
            | Nil, DecimalPoint, DecimalDigits (_, _), ExponentPart (_, _) -> 
                let n = countDecimalDigits c 0
                let e = evalExponentPart d
                evalDecimalDigits c * int (10.0 ** float (e - n))
            | _ -> invalidOp "Invalid DecimalLiteral pattern found."                  
        | _ -> invalidArg "v" "Expected DecimalLiteral."