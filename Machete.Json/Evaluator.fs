namespace Machete.Json

open System
open Machete.Interfaces
open Machete.Compiler

module Evaluator =

    let rec private evalJSONEscapeCharacter (environment:IEnvironment) value =
        match value with
        | JsonEscapeCharacter c ->
            match c with  
            | '\"' -> '\u0022' 
            | '/' -> '/'
            | '\\' -> '\u005C'         
            | 'b' -> '\u0008'
            | 'f' -> '\u000C'
            | 'n' -> '\u000A'
            | 'r' -> '\u000D'
            | 't' -> '\u0009'

    and evalJSONEscapeSequence (environment:IEnvironment) value =
        match value with
        | JsonEscapeSequence (JsonInputElement e) ->
            Lexer.StringLiteralParser.evalUnicodeEscapeSequence e
        | JsonEscapeSequence e ->
            evalJSONEscapeCharacter environment e

    and evalJSONStringCharacter (environment:IEnvironment) value =
        match value with
        | JsonStringCharacter (c, Token.JsonNil) -> c
        | JsonStringCharacter ('\\', e) ->
            evalJSONEscapeSequence environment e

    and evalJSONStringCharacters (environment:IEnvironment) value =
        match value with
        | JsonStringCharacters (e, Token.JsonNil) ->
            (evalJSONStringCharacter environment e).ToString()
        | JsonStringCharacters (e1, e2) ->
            let head = evalJSONStringCharacter environment e1
            let tail = evalJSONStringCharacters environment e2
            head.ToString() + tail

    and evalJSONString (environment:IEnvironment) value =
        match value with
        | JsonString Token.JsonNil ->
            environment.CreateString ""
        | JsonString v ->
            environment.CreateString (evalJSONStringCharacters environment v)

    and evalJSONFraction (environment:IEnvironment) value =
        match value with
        | JsonFraction e ->
            let n = Lexer.NumericLiteralParser.countDecimalDigits e 0 |> double
            let fractional = Lexer.NumericLiteralParser.evalDecimalDigits e |> double
            fractional * (10.0 ** -n)

    and evalJSONNumber (environment:IEnvironment) value =
        match value with
        | JsonNumber (None, e, Token.JsonNil, Lexer.Nil)
        | JsonNumber (Some '+', e, Token.JsonNil, Lexer.Nil) ->
            environment.CreateNumber (Lexer.NumericLiteralParser.evalDecimalIntegerLiteral e |> double)
        | JsonNumber (Some '-', e, Token.JsonNil, Lexer.Nil) ->
            environment.CreateNumber (-Lexer.NumericLiteralParser.evalDecimalIntegerLiteral e |> double)
        | JsonNumber (None, e1, e2, Lexer.Nil) 
        | JsonNumber (Some '+', e1, e2, Lexer.Nil) ->
            let integral = Lexer.NumericLiteralParser.evalDecimalIntegerLiteral e1 |> double
            let fractional = evalJSONFraction environment e2
            environment.CreateNumber (integral + fractional)
        | JsonNumber (Some '-', e1, e2, Lexer.Nil) ->
            let integral = Lexer.NumericLiteralParser.evalDecimalIntegerLiteral e1 |> double
            let fractional = evalJSONFraction environment e2
            environment.CreateNumber (-(integral + fractional))        
        | JsonNumber (None, e1, Token.JsonNil, e2) 
        | JsonNumber (Some '+', e1, Token.JsonNil, e2) ->
            let integral = Lexer.NumericLiteralParser.evalDecimalIntegerLiteral e1 |> double
            let e = Lexer.NumericLiteralParser.evalExponentPart e2 |> double
            environment.CreateNumber (integral * (10.0 ** e))
        | JsonNumber (Some '-', e1, Token.JsonNil, e2) ->
            let integral = Lexer.NumericLiteralParser.evalDecimalIntegerLiteral e1 |> double
            let e = Lexer.NumericLiteralParser.evalExponentPart e2 |> double
            environment.CreateNumber (-(integral * (10.0 ** e)))            
        | JsonNumber (None, e1, e2, e3) 
        | JsonNumber (Some '+', e1, e2, e3) ->
            let integral = Lexer.NumericLiteralParser.evalDecimalIntegerLiteral e1 |> double
            let fractional = evalJSONFraction environment e2
            let e = Lexer.NumericLiteralParser.evalExponentPart e3 |> double
            environment.CreateNumber ((integral + fractional) * (10.0 ** e))
        | JsonNumber (Some '-', e1, e2, e3) ->
            let integral = Lexer.NumericLiteralParser.evalDecimalIntegerLiteral e1 |> double
            let fractional = evalJSONFraction environment e2
            let e = Lexer.NumericLiteralParser.evalExponentPart e3 |> double
            environment.CreateNumber (-((integral + fractional) * (10.0 ** e)))

    and evalJSONNullLiteral (environment:IEnvironment) value =
        match value with
        | JsonNullLiteral "null" ->
            environment.Null
            
    and evalJSONBooleanLiteral (environment:IEnvironment) value =
        match value with
        | JsonBooleanLiteral "true" ->
            environment.True
        | JsonBooleanLiteral "false" ->
            environment.False

    and evalJSONElementList (environment:IEnvironment) (value:Node) (index:int) (array:IObject) =
        match value with
        | JsonElementList (Node.JsonNil, e1) ->
            let v1 = evalJSONValue environment e1
            let nTrue = Nullable<bool>(true)
            let desc = environment.CreateDataDescriptor (v1, nTrue, nTrue, nTrue)
            array.DefineOwnProperty (index.ToString(), desc, false) |> ignore
            array, (index + 1)
        | JsonElementList (e1, e2) ->
            let array, index = evalJSONElementList environment e1 index array
            let v = evalJSONValue environment e2
            let nTrue = Nullable<bool>(true)
            let desc = environment.CreateDataDescriptor (v, nTrue, nTrue, nTrue)
            array.DefineOwnProperty (index.ToString(), desc, false) |> ignore
            array, (index + 1)

    and evalJSONArray (environment:IEnvironment) value =
        match value with
        | JsonArray Node.JsonNil ->
            environment.ArrayConstructor.Op_Construct(environment.EmptyArgs)
        | JsonArray e ->
            let array = environment.ArrayConstructor.Op_Construct(environment.EmptyArgs)
            let array, index = evalJSONElementList environment e 0 array
            array

    and evalJSONMember (environment:IEnvironment) value =
        match value with
        | JsonMember (JsonToken e1, e2) ->
            let name = evalJSONString environment e1
            let value = evalJSONValue environment e2
            name, value 

    and evalJSONMemberList (environment:IEnvironment) value (obj:IObject) =
        match value with
        | JsonMemberList (Node.JsonNil, e1) ->
            let name, value  = evalJSONMember environment e1
            let nTrue = Nullable<bool>(true)
            let desc = environment.CreateDataDescriptor (value, nTrue, nTrue, nTrue)
            obj.DefineOwnProperty (name.BaseValue, desc, false) |> ignore
            obj
        | JsonMemberList (e1, e2) ->
            let obj = evalJSONMemberList environment e1 obj
            let name, value  = evalJSONMember environment e2
            let nTrue = Nullable<bool>(true)
            let desc = environment.CreateDataDescriptor (value, nTrue, nTrue, nTrue)
            obj.DefineOwnProperty (name.BaseValue, desc, false) |> ignore
            obj

    and evalJSONObject (environment:IEnvironment) value =
        match value with
        | JsonObject Node.JsonNil ->
            environment.ObjectConstructor.Op_Construct(environment.EmptyArgs)
        | JsonObject e ->
            let obj = environment.ObjectConstructor.Op_Construct(environment.EmptyArgs)
            let obj = evalJSONMemberList environment e obj
            obj

    and evalJSONValue (environment:IEnvironment) value =
        match value with
        | JsonValue e ->
            match e with
            | JsonToken e -> 
                match e with
                | JsonNullLiteral _ ->
                    evalJSONNullLiteral environment e :> IDynamic 
                | JsonBooleanLiteral _ ->
                    evalJSONBooleanLiteral environment e :> IDynamic 
                | JsonString _ ->
                    evalJSONString environment e :> IDynamic 
                | JsonNumber (_, _, _, _) ->
                    evalJSONNumber environment e :> IDynamic 
            | JsonObject _ ->
                evalJSONObject environment e :> IDynamic 
            | JsonArray _ ->
                evalJSONArray environment e :> IDynamic       

    and evalJSONText (environment:IEnvironment) value : IDynamic =
        match value with
        | JsonText e ->
            evalJSONValue environment e 

    let rec private walk (environment:IEnvironment) (reviver:IFunction) (holder:IObject) (name:string) : IDynamic =
        let value = holder.Get name
        match value with
        | :? IObject as value ->
            match value.Class with
            | "Array" ->
                let len = (holder.Get "length" :?> INumber).BaseValue |> uint32
                for i in 0u..len - 1u do
                    let p = i.ToString()
                    let newElement = walk environment reviver value p
                    match newElement with
                    | :? IUndefined ->
                        value.Delete (p, false) |> ignore
                    | _ ->
                        let nTrue = Nullable<bool>(true)
                        let desc = environment.CreateDataDescriptor (newElement, nTrue, nTrue, nTrue)
                        value.DefineOwnProperty (p, desc, false) |> ignore
            | _ ->
                for p in holder do
                    let newElement = walk environment reviver value p
                    match newElement with
                    | :? IUndefined ->
                        value.Delete (p, false) |> ignore
                    | _ ->
                        let nTrue = Nullable<bool>(true)
                        let desc = environment.CreateDataDescriptor (newElement, nTrue, nTrue, nTrue)
                        value.DefineOwnProperty (p, desc, false) |> ignore
        | _ -> ()
        let name = environment.CreateString name :> IDynamic
        let args = environment.CreateArgs ([| name; value |])
        reviver.Call (environment, holder, args)

    /// <summary>
    /// 15.12.2 parse ( text [ , reviver ] ) 
    /// The parse function parses a JSON text (a JSON-formatted String) and produces an ECMAScript value. The 
    /// JSON format is a restricted form of ECMAScript literal. JSON objects are realized as ECMAScript objects. 
    /// JSON arrays are realized as ECMAScript arrays. JSON strings, numbers, booleans, and null are realized as 
    /// ECMAScript Strings, Numbers, Booleans, and null. JSON uses a more limited set of white space characters 
    /// than WhiteSpace and allows Unicode code points U+2028 and U+2029 to directly appear in JSONString literals 
    /// without using an escape sequence. The process of parsing is similar to 11.1.4 and 11.1.5 as constrained by 
    /// the JSON grammar. 
    /// </summary>
    /// <param name="environment">The environment that this object will reside in.</param>
    /// <param name="text">The text that will be parse as JSON.</param>
    /// <param name="reviver">
    /// The optional  reviver parameter is a function that takes two parameters, (key and  value). It can filter and 
    /// transform the results. It is called with each of the key/value pairs produced by the parse, and its return value is 
    /// used instead of the original value. If it returns what  it received, the structure is not modified. If it returns 
    /// undefined then the property is deleted from the result. 
    /// </param>
    let ParseForEnvironment (environment:IEnvironment, text:IDynamic, reviver:IDynamic) : IDynamic =
        let jText = text.ConvertToString().BaseValue
        let jsonText = Machete.Json.Parser.parse jText 
        match jsonText with
        | [| JsonText _ |] ->
            let unfiltered = evalJSONText environment (jsonText.[0])    
            match reviver with
            | :? IFunction as reviver ->
                let root = environment.ObjectConstructor.Op_Construct(environment.EmptyArgs)
                let nTrue = Nullable<bool>(true)
                let desc = environment.CreateDataDescriptor (unfiltered, nTrue, nTrue, nTrue)
                root.DefineOwnProperty ("", desc, false) |> ignore
                walk environment reviver root "" 
            | _ -> unfiltered  
        | _ -> raise (environment.CreateSyntaxError "The text supplied could not be parsed as JSON.")

    /// <summary>
    /// 15.12.3 stringify ( value [ , replacer [ , space ] ] ) 
    /// The stringify function returns a String in JSON format representing an ECMAScript value. It can take three 
    /// parameters. The first parameter is required. 
    /// </summary>
    /// <param name="environment">The environment that the value resides in.</param>
    /// <param name="value">The value to be converted</param>
    /// <param name="replacer">
    /// The optional replacer parameter is 
    /// either a function that alters the way objects and arrays are stringified, or an array of Strings and Numbers that 
    /// acts as a white list for selecting the object properties that will be stringified. 
    /// </param>
    /// <param name="space">
    /// The optional space parameter is a 
    /// String or Number that allows the result to have white space injected into it to improve human readability. 
    /// </param>
    let StringifyForEnvironment (environment:IEnvironment, value:IDynamic, replacer:IDynamic, space:IDynamic) : IDynamic =     
        null

