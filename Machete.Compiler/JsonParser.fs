namespace Machete.Compiler

open System
open System.Globalization
open System.Text
open FParsec.Primitives
open FParsec.CharParsers
open InputElementParsers
open Machete.Core

module JsonParser =

    type private ParseState = {
        environment:IEnvironment
    }    

    type private StringifyState = {
        environment:IEnvironment
        stack:list<IDynamic>
        indent:string
        gap:string
        propertyList:list<string>
        replacerFunction:IDynamic
        space:string
    }

    let rec private evalJsonWhiteSpace state =
        (anyOf "\t\r\n " |>> string) state

    and private evalJsonString (state:FParsec.State<ParseState>) =
        ((pchar '\"' >>. evalJsonStringCharacters .>> pchar '\"') |>> fun s -> (state.UserState.environment.CreateString s) :> IDynamic) state
        
    and private evalJsonStringCharacters state =
        manyStrings evalJsonStringCharacter state

    and private evalJsonStringCharacter state =
        ((satisfy (fun c -> c <> '\"' && c <> '\\' && c > '\u001F') |>> string) <|> (pchar '\\' >>. evalJsonEscapeSequence)) state
        
    and private evalJsonEscapeSequence state =
        (evalJsonEscapeCharacter <|> evalUnicodeEscapeSequence) state
        
    and private evalJsonEscapeCharacter state =
        (anyOf "\"/\\bfnrt" |>> convertJsonEscapeCharacter) state
        
    and private convertJsonEscapeCharacter c =        
        match c with
        | '\"' -> "\u0022"
        | '/' -> "/"
        | '\\' -> "\u005C"
        | 'b' -> "\u0008"
        | 'f' -> "\u000C"
        | 'n' -> "\u000A"
        | 'r' -> "\u000D"
        | 't' -> "\u0009"
        
    and private evalJsonNumber (state:FParsec.State<ParseState>) =
        pipe4 evalSign evalDecimalIntegerLiteral evalJsonFraction (evalExponentPart <|> preturn 1.0) (completeJsonNumber state.UserState.environment) state
            
    and private completeJsonNumber environment signPart integralPart fractionalPart exponentPart  =
        environment.CreateNumber (signPart * (integralPart + fractionalPart) * exponentPart) :> IDynamic

    and private evalJsonFraction state =
        ((pchar '.' >>. evalDecimalDigits -1.0 1.0) <|> preturn 0.0) state

    and private evalSign state =
        ((pchar '-' |>> fun _ -> -1.0) <|> preturn 1.0) state
        
    and private evalJsonNullLiteralOrBooleanLiteral (state:FParsec.State<ParseState>) =
        (attempt <| parse {
            let! s = evalIdentifierName
            match s with
            | "null" -> return state.UserState.environment.Null :> IDynamic   
            | "true" -> return state.UserState.environment.True :> IDynamic
            | "false" -> return state.UserState.environment.False :> IDynamic 
            | _ -> ()
        }) state

    let private skipEval parser state =
        (skipMany evalWhiteSpace >>. parser) state

    let rec private evalJsonText state =
        evalJsonValue state
        
    and private evalJsonValue state =
        choice [|
            evalJsonNullLiteralOrBooleanLiteral
            evalJsonObject
            evalJsonArray
            evalJsonString
            evalJsonNumber
        |] state
        
    and private evalJsonObject state = 
        ((skipEval (pchar '{') >>. (skipEval evalJsonMemberList) .>> skipEval (pchar '}')) |>> completeJsonObject state.UserState.environment) state

    and private completeJsonObject environment (elements:list<IDynamic * IDynamic>) =
        let nTrue = Nullable<bool>(true) 
        let rec completeJsonObject (elements:list<IDynamic * IDynamic>) (result:IObject) =
            match elements with
            | [] -> result :> IDynamic
            | (name, value)::elements ->
                let desc = environment.CreateDataDescriptor (value, nTrue, nTrue, nTrue)
                result.DefineOwnProperty ((name :?> IString).BaseValue, desc, false) |> ignore
                completeJsonObject elements result
        let result = environment.ObjectConstructor.Op_Construct(environment.EmptyArgs)
        completeJsonObject elements result  
    
    and private evalJsonMember state = 
        (tuple2 (skipEval evalJsonString) (skipEval (pchar ':') >>. skipEval evalJsonValue)) state
    
    and private evalJsonMemberList state = 
        sepBy (skipEval evalJsonMember) (attempt (skipEval (pchar ','))) state
    
    and private evalJsonArray state = 
        ((skipEval (pchar '[') >>. (skipEval evalJsonElementList) .>> skipEval (pchar ']')) |>> completeJsonArray state.UserState.environment) state
    
    and private completeJsonArray environment (elements:list<IDynamic>) =
        let nTrue = Nullable<bool>(true) 
        let rec completeJsonArray index elements (result:IObject) =
            match elements with
            | [] -> result :> IDynamic
            | element::elements ->
                let desc = environment.CreateDataDescriptor (element, nTrue, nTrue, nTrue)
                result.DefineOwnProperty (index.ToString(), desc, false) |> ignore
                completeJsonArray (index + 1) elements result
        let result = environment.ArrayConstructor.Op_Construct(environment.EmptyArgs)
        completeJsonArray 0 elements result

    and private evalJsonElementList state =
        sepBy (skipEval evalJsonValue) (attempt (skipEval (pchar ','))) state

    let rec private walk (environment:IEnvironment) (reviver:ICallable) (holder:IObject) (name:string) : IDynamic =
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

    let Parse (environment:IEnvironment, text:IDynamic, reviver:IDynamic) =
        let text = text.ConvertToString().BaseValue
        let (state:ParseState) = { environment = environment }
        let result = runParserOnString evalJsonText state "JSON" text
        match result with
        | Success (unfiltered, state, position) -> 
            match reviver with
            | :? ICallable as reviver ->
                let root = environment.ObjectConstructor.Op_Construct(environment.EmptyArgs)
                let nTrue = Nullable<bool>(true)
                let desc = environment.CreateDataDescriptor (unfiltered, nTrue, nTrue, nTrue)
                root.DefineOwnProperty ("", desc, false) |> ignore
                walk environment reviver root "" 
            | _ -> unfiltered
        | Failure (message, errors, state) ->
            raise (environment.CreateSyntaxError message)

    let Stringify (environment:IEnvironment, value:IDynamic, replacer:IDynamic, space:IDynamic) : IDynamic =
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