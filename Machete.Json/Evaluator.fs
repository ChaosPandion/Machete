namespace Machete.Json

open System
open System.Globalization
open System.Text
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
        | JsonNumber (None, e, Token.JsonNil, InputElement.Nil)
        | JsonNumber (Some '+', e, Token.JsonNil, InputElement.Nil) ->
            environment.CreateNumber (Lexer.NumericLiteralParser.evalDecimalIntegerLiteral e |> double)
        | JsonNumber (Some '-', e, Token.JsonNil, InputElement.Nil) ->
            environment.CreateNumber (-Lexer.NumericLiteralParser.evalDecimalIntegerLiteral e |> double)
        | JsonNumber (None, e1, e2, InputElement.Nil) 
        | JsonNumber (Some '+', e1, e2, InputElement.Nil) ->
            let integral = Lexer.NumericLiteralParser.evalDecimalIntegerLiteral e1 |> double
            let fractional = evalJSONFraction environment e2
            environment.CreateNumber (integral + fractional)
        | JsonNumber (Some '-', e1, e2, InputElement.Nil) ->
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


    type private StringifyState = {
        environment:IEnvironment
        stack:list<IDynamic>
        indent:string
        gap:string
        propertyList:list<string>
        replacerFunction:IDynamic
        space:string
    }

    let rec private ja (state:StringifyState) (value:IObject) : string =
        if not state.stack.IsEmpty && (state.stack.Head = (value :> IDynamic)) then 
            raise (state.environment.CreateTypeError("Cyclical objects cannot be serialized."))
        let stepback = state.indent
        let state = { state with stack = value:>IDynamic::state.stack; indent = state.indent + state.gap }
        let partial = [
            let len = (value.Get "length" :?> INumber).BaseValue |> uint32
            if len > 0u then
                for index in 0u..len - 1u do
                    let p = index.ToString()
                    let strP = str state p value
                    if strP.TypeCode <> LanguageTypeCode.Undefined 
                    then yield (strP :?> IString).BaseValue
                    else yield "null"
        ]
        if partial.IsEmpty 
        then "[]" 
        else 
            let head, separator, tail = 
                if state.gap <> ""
                then "[\n" + state.indent, ",\n" + state.indent, "\n" + stepback + "]"
                else "[", ",", "]"
            let sb = StringBuilder()
            sb.Append head |> ignore
            for s in partial do
                if sb.Length > head.Length then
                    sb.Append separator |> ignore
                sb.Append s |> ignore
            sb.Append tail |> ignore
            sb.ToString ()

    and private jo (state:StringifyState) (value:IObject) : string =
        if not state.stack.IsEmpty && (state.stack.Head = (value :> IDynamic)) then 
            raise (state.environment.CreateTypeError("Cyclical objects cannot be serialized."))
        let stepback = state.indent
        let state = { state with stack = value:>IDynamic::state.stack; indent = state.indent + state.gap }
        let k = match state.propertyList with | [] -> value |> Seq.toList | _ -> state.propertyList
        let partial = [
            for p in k do
                let strP = str state p value
                if strP.TypeCode <> LanguageTypeCode.Undefined then
                    yield quote p + ":" + (if state.gap <> "" then " "  else "") + (strP :?> IString).BaseValue                    
        ]
        if partial.IsEmpty 
        then "{}" 
        else 
            let head, separator, tail = 
                if state.gap <> ""
                then "{\n" + state.indent, ",\n" + state.indent, "\n" + stepback + "}"
                else "{", ",", "}"
            let sb = StringBuilder()
            sb.Append head |> ignore
            for s in partial do
                if sb.Length > head.Length then
                    sb.Append separator |> ignore
                sb.Append s |> ignore
            sb.Append tail |> ignore
            sb.ToString ()

    and private quote (value:string) : string =
        let sb = StringBuilder()
        sb.Append ('\"') |> ignore
        for c in value do
            match c with
            | '\"' -> sb.Append ("\\\"") |> ignore
            | '\b' -> sb.Append ("\\b") |> ignore
            | '\f' -> sb.Append ("\\f") |> ignore
            | '\n' -> sb.Append ("\\n") |> ignore
            | '\r' -> sb.Append ("\\r") |> ignore
            | '\t' -> sb.Append ("\\t") |> ignore
            | _ ->
                match Char.GetUnicodeCategory c with
                | UnicodeCategory.Control when c < ' ' ->
                     sb.Append ("\\u") |> ignore
                     sb.Append ((c |> int).ToString("x4")) |> ignore
                | _ -> sb.Append (c) |> ignore             
        sb.Append ('\"') |> ignore
        sb.ToString ()

    and private str (state:StringifyState) (key:string) (holder:IObject) : IDynamic =
        let fourth (value:IDynamic) =
            match value.TypeCode with
            | LanguageTypeCode.Null -> 
                state.environment.CreateString "null" :> IDynamic
            | LanguageTypeCode.Boolean -> 
                let v = value :?> IBoolean
                state.environment.CreateString (if v.BaseValue then "true" else "false") :> IDynamic
            | LanguageTypeCode.String -> 
                let v = value :?> IString
                state.environment.CreateString (quote v.BaseValue) :> IDynamic
            | LanguageTypeCode.Number -> 
                let v = value :?> INumber
                if not (Double.IsInfinity(v.BaseValue)) 
                then v.ConvertToString() :> IDynamic 
                else state.environment.CreateString "null" :> IDynamic
            | LanguageTypeCode.Object -> 
                let v = value :?> IObject
                let v = if v.Class = "Array" then ja state v else jo state v 
                state.environment.CreateString v :> IDynamic
            | _ -> 
                state.environment.Undefined :> IDynamic
        let third (value:IDynamic) =
            match value.TypeCode with
            | LanguageTypeCode.Object ->
                let v = value :?> IObject
                match v.Class with
                | "Number" -> 
                    fourth (v.ConvertToNumber())
                | "String" ->
                    fourth (v.ConvertToString())
                | "Boolean" ->
                    fourth (v.ConvertToBoolean())
                | _ -> 
                    fourth value
            | _ -> 
                fourth value
        let second (value:IDynamic) =
            match state.replacerFunction with
            | :? ICallable as f ->
                let args = state.environment.CreateArgs ([| state.environment.CreateString key :> IDynamic; value |])
                let value = f.Call (state.environment, holder :> IDynamic, args)
                third value
            | _ -> 
                third value 
        let first (value:IDynamic) =
            match value with
            | :? IObject as v ->
                let toJSON = v.Get "toJSON"
                match toJSON with
                | :? ICallable as f ->
                    let args = state.environment.CreateArgs ([| state.environment.CreateString key :> IDynamic |])
                    let value = f.Call (state.environment, value, args) 
                    second value
                | _ -> 
                    second value
            | _ -> 
                second value
        first (holder.Get key)

    let ParseForEnvironment (environment:IEnvironment, text:IDynamic, reviver:IDynamic) : IDynamic =
        let jText = text.ConvertToString().BaseValue
        let jsonText = Machete.Json.Parser.parse jText 
        match jsonText with
        |  JsonText _ ->
            let unfiltered = evalJSONText environment jsonText//(jsonText.[0])    
            match reviver with
            | :? IFunction as reviver ->
                let root = environment.ObjectConstructor.Op_Construct(environment.EmptyArgs)
                let nTrue = Nullable<bool>(true)
                let desc = environment.CreateDataDescriptor (unfiltered, nTrue, nTrue, nTrue)
                root.DefineOwnProperty ("", desc, false) |> ignore
                walk environment reviver root "" 
            | _ -> unfiltered  
        | _ -> raise (environment.CreateSyntaxError "The text supplied could not be parsed as JSON.")

    let StringifyForEnvironment (environment:IEnvironment, value:IDynamic, replacer:IDynamic, space:IDynamic) : IDynamic =
        let space =            
            match space with
            | :? INumber as v ->
                String.replicate (min 10 ((v.ConvertToNumber ()).BaseValue |> int)) " "
            | :? IString as v -> 
                let s = v.BaseValue
                if s.Length > 10 then s.Substring (0, 10) else s
            | :? IObject as v when v.Class = "Number" -> 
                String.replicate (min 10 ((v.ConvertToNumber ()).BaseValue |> int)) " "
            | :? IObject as v when v.Class = "String" -> 
                let s = (v.ConvertToString ()).BaseValue
                if s.Length > 10 then s.Substring (0, 10) else s
            | _ -> ""
        let state = {
            environment = environment
            stack = []
            indent = ""
            space = space 
            gap = space

            replacerFunction =                 
                match replacer with
                | :? ICallable -> replacer
                | _ -> environment.Undefined :> IDynamic


            propertyList =
                match replacer with
                | :? IObject as o when o.Class = "Array" ->
                    [ 
                        let n = ref 0u
                        for name in o do
                            if UInt32.TryParse(name, n) then
                                let v = o.Get name
                                match v with
                                | :? IString as v -> yield v.BaseValue
                                | :? INumber as v -> yield (v.ConvertToString ()).BaseValue
                                | :? IObject as v when v.Class = "String" || v.Class = "Number" -> 
                                    yield (v.ConvertToString ()).BaseValue
                    ] 
                | _ -> List.empty<string> 
        }   
        let wrapper = environment.ObjectConstructor.Op_Construct(environment.EmptyArgs)
        let nTrue = Nullable<bool>(true)
        let desc = environment.CreateDataDescriptor (value, nTrue, nTrue, nTrue)
        wrapper.DefineOwnProperty ("", desc, false) |> ignore               
        str state "" wrapper  