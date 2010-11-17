namespace Machete.Json

module Parser =

    open Machete.Tools.BacktrackingPrimitives

//    open FParsec.CharParsers
//    open FParsec.Primitives
//    open Machete.Compiler
//    open Machete.Compiler.NumericLiteral


    let jsonValue, jsonValueRef : Parser<Token, unit, Node> * Parser<Token, unit, Node> ref = createParserRef()
    
    let jsonElementList =
        manySeparatedFold jsonValue (like ValueSeparator) JsonNil (fun a b -> JsonElementList (a, b))
    
    let jsonArray =
        between (like BeginArray) (like EndArray) (jsonElementList <|> result JsonNil) |>> JsonArray
    
    let jsonNumber = 
        parse {
            let! v = item
            match v with 
            | JsonNumber _ -> return JsonToken v 
            | _ -> return! zero
        }

    let jsonString = 
        parse {
            let! v = item
            match v with 
            | JsonString _ -> return JsonToken v 
            | _ -> return! zero
        }

    let jsonNullLiteral =
        parse {
            let! v = item
            match v with
            | JsonNullLiteral _ -> return JsonToken v
            | _ -> return! zero
        }

    let jsonBooleanLiteral =
        parse {
            let! v = item
            match v with
            | JsonNullLiteral _ -> return JsonToken v
            | _ -> return! zero
        }

    let jsonMember =
        parse {
            let! name = jsonString 
            let! _ = like NameSeparator <?> "Expecting ':' following member name in member list of object."
            let! value = jsonValue <?> "Expecting a value following member name separator ':' in member list of object."
            return JsonMember (name, value)
        }

    let jsonMemberList =
        manySeparatedFold jsonMember (like ValueSeparator) JsonNil (fun a b -> JsonMemberList (a, b))

    let jsonObject =
        parse {
            let! _ = like BeginObject
            let! v = jsonMemberList <|> result JsonNil
            let! _ = like EndObject <?> "Missing closing brace for object."
            return JsonObject v
        }

    do jsonValueRef := 
        jsonNullLiteral <|> jsonBooleanLiteral <|> jsonObject <|> jsonArray <|> jsonString <|> jsonNumber
    
    let jsonText =
        jsonValue |>> JsonText

    let parse input =
        seq {
            for r in run jsonText (Lexer.tokenize input) () do
                match r with
                | Success (result, state) -> 
                    yield result
                | Failure (message, state) -> 
                    failwith message
        } |> Seq.toArray

    type x =  { v : int; r : string }

    let main () =
        let m = { v = 2; r = "" }
        match m with
        | { v = 4 } -> printfn "Success"
        | { r = "" } -> printfn "Success"
        System.Console.ReadLine () |> ignore

    main ()
 
//    let ch c  = skipChar c >>. jsonWhiteSpace 
//
//        
//    let jsonFraction<'a> : Parser<float, 'a> =
//        parse {
//            do! skipChar '.'
//            let! a = decimalDigits
//            let count = countDecimalDigits a 0
//            let num = evalDecimalDigits a |> float
//            let b = num * (10.0 ** -float(count))
//            return b
//        }
//
//    let jsonNumber<'a> : Parser<Node, 'a> =
//        parse {
//            let! a = (skipChar '-' |>> fun () -> -1.0) <|> preturn 1.0
//            let! b = decimalIntegerLiteral |>> evalDecimalIntegerLiteral |>> float
//            let! c = jsonFraction <|> preturn 0.0
//            let! d = (exponentPart |>> evalExponentPart <|> preturn 0) |>> float
//            let d = 10.0 ** d
//            return JsonNumber ((a * (b + c)) * d)
//        }
//
//    let jsonString<'a> : Parser<Node, 'a> =
//        parse {
//            let dq = pchar '\"'
//            let none = noneOf ("\"\\" + new string([|for c in '\u0000'..'\u001f' -> c|]))
//            let escape = 
//                skipChar '\\' 
//                >>. anyOf "\"/\\bfnrt"
//                |>> fun c ->
//                        match c with
//                        | 'b' -> '\b'
//                        | 'f' -> '\u000C'
//                        | 'n' -> '\n'
//                        | 'r' -> '\r'
//                        | 't' -> '\t'
//                        | c -> c
//                |> attempt
//            let u = skipChar '\\' .>> skipChar 'u' >>. StringLiteral.unicodeEscapeSequence |>> StringLiteral.evalUnicodeEscapeSequence
//            let! a = (between dq dq (manyChars (none <|> escape <|>  u))) .>> jsonWhiteSpace
//            return JsonString a
//        }
//        
//    let jsonNullLiteral<'a> : Parser<Node, 'a> =
//        pstring "null" >>= fun c -> preturn JsonNullLiteral
//
//    let jsonBooleanLiteral<'a> : Parser<Node, 'a> =
//        (pstring "true" <|> pstring "false") .>> jsonWhiteSpace |>> fun s -> JsonBooleanLiteral (s = "true")
//
//    let jsonArray : Parser<Node, unit> =    
//        between (ch '[') (ch ']') (sepBy jsonValue (ch ',') ) |>> JsonArray
//        
//    let jsonMember : Parser<Node * Node, unit> =    
//        tuple2 (jsonString .>> ch ':') jsonValue
//    let jsonObject : Parser<Node, unit> =    
//        between (ch '{') (ch '}') (sepBy jsonMember (ch ',') ) |>> fun l -> JsonObject (l |> Map.ofList)
//
//    do jsonValueRef := choice [jsonNumber; jsonString; jsonNullLiteral; jsonBooleanLiteral; jsonArray; jsonObject] |>> JsonValue
//        
//    let jsonText : Parser<Node, unit> =
//         jsonWhiteSpace >>. jsonValue |>> JsonText
//
//    let parse input =
//        match runParserOnString jsonText () "" input with
//        | Success (a, b, c) -> a
//        | Failure (a, b, c) -> failwith a