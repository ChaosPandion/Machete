namespace Machete.Json

module Parser =

    open Machete.Compiler.Tools

//    open FParsec.CharParsers
//    open FParsec.Primitives
//    open Machete.Compiler
//    open Machete.Compiler.NumericLiteral


    let jsonValue, jsonValueRef : Parser<Token, unit, Node> * Parser<Token, unit, Node> ref = createParserRef()
    
    let jsonElementList = zero
        //manySepFold jsonValue (satisfy (fun v -> v = ValueSeparator)) JsonNil (fun a b -> JsonElementList (a, b))
    
    let jsonArray =
        between (satisfy (fun v -> v = BeginArray)) (satisfy (fun v -> v = EndArray)) (jsonElementList <|> result JsonNil) |>> JsonArray
    
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
            let! _ = (satisfy (fun v -> v = NameSeparator)) //like NameSeparator //<?> "Expecting ':' following member name in member list of object."
            let! value = jsonValue //<?> "Expecting a value following member name separator ':' in member list of object."
            return JsonMember (name, value)
        }

    let jsonMemberList =
        manySepFold jsonMember (satisfy (fun v -> v = ValueSeparator)) JsonMemberList JsonNil //(fun a b -> JsonMemberList (a, b))

    let jsonObject =
        parse {
            let! _ = (satisfy (fun v -> v = BeginObject))//like BeginObject
            let! v = jsonMemberList <|> result JsonNil
            let! _ = (satisfy (fun v -> v = EndObject))//like EndObject //<?> "Missing closing brace for object."
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
                    ()
        } |> Seq.toArray