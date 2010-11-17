namespace Machete.Json

module Lexer =

    open FParsec
    open FParsec.CharParsers
    open FParsec.Primitives
    open Machete.Compiler
    open Machete.Compiler.Parsers
    open Machete.Compiler.NumericLiteral
    open Machete.Compiler.StringLiteral
    

//    let complete value (state:State<unit>) =
//        { tokenValue = value; line = state.Line; column = state.Column }

    let jsonWhiteSpace =
        anyOf "\t\r\n " |>> JsonWhiteSpace

    let jsonBooleanLiteral =
        Parsers.booleanLiteral |>> JsonBooleanLiteral
        
    let evalJsonBooleanLiteral v =
        match v with
        | JsonBooleanLiteral v -> evalBooleanLiteral v

    let jsonNullLiteral =
        Parsers.nullLiteral |>> JsonNullLiteral
        
    let evalJsonNullLiteral v =
        match v with
        | JsonNullLiteral v -> evalNullLiteral v

    let jsonEscapeCharacter =
        anyOf "\"/\\bfnrt" |>> JsonEscapeCharacter
        
    let evalJsonEscapeCharacter v =
        match v with
        | JsonEscapeCharacter c -> c

    let jsonEscapeSequence =
        (jsonEscapeCharacter <|> (unicodeEscapeSequence |>> JsonInputElement)) |>> JsonEscapeSequence
        
    let evalJsonEscapeSequence v =
        match v with
        | JsonEscapeCharacter _  -> evalJsonEscapeCharacter v
        | JsonInputElement e -> evalUnicodeEscapeSequence e

    let jsonStringCharacter =
        let none = noneOf ("\"\\" + new string([|for c in '\u0000'..'\u001f' -> c|]))
        ((none |>> (fun c -> c, JsonNil)) <|> pipe2 (pchar '\\') jsonEscapeSequence (fun a b -> a, b)) |>> JsonStringCharacter
       
    let rec evalJsonStringCharacter v =
        match v with
        | JsonStringCharacter ('\\', e) -> evalJsonEscapeSequence e
        | JsonStringCharacter (c, JsonNil) -> c

    let jsonStringCharacters =
        manyFold JsonNil (fun a b -> JsonStringCharacters (a, b)) jsonStringCharacter

    let rec evalJsonStringCharacters v =
        match v with
        | JsonStringCharacters (JsonNil, c) -> 
            evalJsonStringCharacter c |> string
        | JsonStringCharacters (cs, c) -> 
            evalJsonStringCharacters cs + (evalJsonStringCharacter c |> string)

    let jsonString =
        let dq = pchar '\"'
        between dq dq (jsonStringCharacters <|> preturn JsonNil) |>> JsonString

    let evalJsonString v =
        match v with
        | JsonString v -> "\"" + evalJsonStringCharacters v +  "\""

    let jsonFraction =
        pipe2 (pchar '.') (decimalDigits) (fun a b -> JsonFraction b)

    let jsonNumber =
        let a = opt (pchar '-')
        let b = decimalIntegerLiteral
        let c = jsonFraction <|> preturn JsonNil
        let d = exponentPart <|> preturn Nil
        pipe4 a b c d (fun a b c d -> JsonNumber (a, b, c, d))

    let beginArray = 
        skipChar '[' >>. preturn BeginArray

    let endArray = 
        skipChar ']' >>. preturn EndArray

    let beginObject = 
        skipChar '{' >>. preturn BeginObject

    let endObject = 
        skipChar '}' >>. preturn EndObject
        
    let nameSeparator = 
        skipChar ':' >>. preturn NameSeparator

    let valueSeparator = 
        skipChar ',' >>. preturn ValueSeparator

    let exec = 
        choice [
            beginArray
            endArray
            beginObject
            endObject
            nameSeparator
            valueSeparator
            jsonWhiteSpace
            jsonBooleanLiteral
            jsonNullLiteral
            jsonString
            jsonNumber
            preturn JsonNil
        ] 

    let tokenize (input:string) =
        let rec tokenize i r =
            seq {
                match r with
                | Success (v, _, p) ->
                    match v with
                    | BeginArray
                    | EndArray
                    | BeginObject
                    | EndObject
                    | NameSeparator
                    | ValueSeparator
                    | JsonString _ 
                    | JsonNumber (_, _, _, _) 
                    | JsonNullLiteral _ 
                    | JsonBooleanLiteral _ ->
                        yield v
                    | _ -> ()
                    let index = int p.Index + i
                    let length = (input.Length - index)
                    if length > 0 then            
                        yield! tokenize index (runParserOnSubstring exec () "A" input index length)
                    else
                        yield End
                | Failure (m, e, s) -> failwith m
            }
        tokenize 0 (runParserOnString exec () "A" input) |> LazyList.ofSeq