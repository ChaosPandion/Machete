namespace Machete.RegExp

open System
open FParsec.Primitives
open FParsec.CharParsers

module Replacer =

    type private State = {
        replaceValue:string
        m:Match
    }

    let private escapeDollarSign reply =
        (parse {
            do! skipChar '$'
            return "$"
        }) reply
    
    let private replaceMatchedSubString reply =
        (parse {
            do! skipChar '&'
            let! state = getUserState
            return state.m.[0]
        }) reply
        
    let private replacePrecedingSubString reply =
        (parse {
            do! skipChar '‘'
            let! state = getUserState
            return state.m.Input.Substring (0, state.m.Index)
        }) reply
        
    let private replaceFollowingSubString reply =
        (parse {
            do! skipChar '’'
            let! state = getUserState
            return state.m.Input.Substring (state.m.[0].Length)
        }) reply

    let private replaceCapture reply =
        (attempt <| parse {
            let! d1 = digit
            let! d2 = opt digit
            let n = 
                match d2 with
                | Some c -> (10 * ((int d1) - 48)) + ((int c) - 48)
                | None -> (int d1) - 48
            let! state = getUserState
            let caps = state.m
            return if n < 1 || n > caps.Length then "" else caps.[n]
        }) reply

    let private choices =
        [| 
            escapeDollarSign
            replaceMatchedSubString
            replacePrecedingSubString
            replaceFollowingSubString
            replaceCapture
            preturn "$" 
        |]
        
    let private collect reply =
        (parse {
            let! preceding = manyCharsTill anyChar (pchar '$')
            let! replace = choice choices
            return preceding + replace
        }) reply
        
    let private run reply =
        (parse {
            let! results = many collect
            return results |> List.reduce (+)
        }) reply

    let Replace (value:string) (m:Match) (replaceValue:string) =
        let state = { m = m; replaceValue = replaceValue }
        let replacement = runParserOnString run state "" replaceValue
        match replacement with
        | Success (middlePortion, _, _) ->
            let startPortion = value.Substring (0, m.Index)
            let endPortion = value.Substring (m.Index + m.[0].Length) 
            startPortion + middlePortion + endPortion
        | Failure (m, _, _) -> failwith m

