namespace Machete.RegExp

open System

module Matchers =    
    let private isWordChar index state =
        index <> -1 && index <> state.Input.Length && Characters.wordCharSet.Contains (state.Input.[index])

    let private nullString (index:int) : string = 
        null

    let createPattern (regExpInfo:RegExpInfo) (captureCount:int) (disjunction:Matcher) : PatternMatcher = 
        fun (input:string) (index:int) ->            
            if input = null then
                nullArg "input"
            elif index < 0 then
                invalidArg "index" "Cannot be less than zero."
            elif index >= input.Length then
                invalidArg "index" "Cannot be greater than the length of the input"
            disjunction { 
                RegExpInfo = regExpInfo
                Captures = Captures (Seq.init captureCount nullString)
                Input = input
                StartIndex = index
                EndIndex = index 
            }

    let createDisjunction (alternatives:list<Matcher>) : Matcher = 
        fun (state:MatchState) ->
            let rec runAlternatives (alternatives:list<Matcher>) =
                match alternatives with
                | alternative::alternatives ->
                    let matchResult:MatchResult = alternative state
                    if matchResult.Success
                    then matchResult
                    else runAlternatives alternatives
                | [] -> MatchResult (false, state)
            runAlternatives alternatives

    let createAlternative (terms:list<Matcher>) : Matcher = 
        fun (state:MatchState) ->
            let rec runTerms (terms:list<Matcher>) (currentState:MatchState) =
                match terms with
                | term::terms ->
                    let matchResult:MatchResult = term currentState
                    if matchResult.Success
                    then runTerms terms matchResult.MatchState
                    else MatchResult (false, state)
                | [] -> MatchResult (true, currentState)
            runTerms terms state

    let createStartOfInputAssertion () : Matcher =
        fun (state:MatchState) ->
            let success = state.StartOfInput || (state.RegExpInfo.Multiline && Characters.isLineTerminator (state.Input.[state.EndIndex - 1]))
            MatchResult (success, state)

    let createEndOfInputAssertion () : Matcher =
        fun (state:MatchState) ->
            let success = state.EndOfInput || (state.RegExpInfo.Multiline && Characters.isLineTerminator state.Current)
            MatchResult (success, state)

    let createWordBoundaryAssertion () : Matcher =
        fun (state:MatchState) ->       
            let a = if isWordChar (state.EndIndex - 1) state then 1 else 0
            let b = if isWordChar state.EndIndex state then 1 else 0
            MatchResult (a ^^^ b = 1, state)  

    let createNonWordBoundaryAssertion () : Matcher =
        fun (state:MatchState) ->       
            let a = if isWordChar (state.EndIndex - 1) state then 1 else 0
            let b = if isWordChar state.EndIndex state then 1 else 0
            MatchResult (a ^^^ b = 0, state)
            
    let createFollowedByAssertion (disjunction:Matcher) : Matcher =
        fun (state:MatchState) ->
            let result = disjunction state
            MatchResult (result.Success, state)

    let createNotFollowedByAssertion (disjunction:Matcher) : Matcher =
        fun (state:MatchState) ->
            let result = disjunction state
            MatchResult (not result.Success, state)

    let createSingleCharacterAtom (ignoreCase:bool) (c:char) : Matcher =
        let c = if ignoreCase then Char.ToLower c else c
        fun (state:MatchState) ->
            let success = c = (if ignoreCase then Char.ToLower state.Current else state.Current)
            MatchResult (success, state.NextIf success)

    let createManyCharacterAtom (ignoreCase:bool) (invert:bool) (cs:char seq) : Matcher =
        let charSet = set (if ignoreCase then cs |> Seq.map Char.ToLower else cs)
        fun (state:MatchState) ->
            let success = charSet.Contains (if ignoreCase then Char.ToLower state.Current else state.Current)
            let success = if invert then not success else success
            MatchResult (success, state.NextIf success)

    let createDotAtom () : Matcher =
        fun (state:MatchState) ->
            let success = Characters.isNotLineTerminator state.Current
            MatchResult (success, state.NextIf success)  

    let createCapturingAtom (captureIndex:int) (disjunction:Matcher) : Matcher =
        fun (state:MatchState) ->
            let result = disjunction state
            if not result.Success then
                result
            else
                let state = result.MatchState
                let capture = state.Input.[state.StartIndex..state.EndIndex - 1]
                let state = { state with Captures = state.Captures.CopyWith (captureIndex, capture) } 
                MatchResult (true, state)
                
    let createDecimalEscapeAtom (index:int) : Matcher =
        fun (state:MatchState) ->
            MatchResult (true, state)  

    let createQuantifier (atom:Matcher) (nextTerm:Matcher option) (min:int) (max:Limit) (greedy:bool) (captureIndex:int) (captureCount:int) : Matcher =
        fun (state:MatchState) ->
            let limit = captureIndex + captureCount
            let resetCapture (i:int) (c:string) =
                if i > captureIndex && i <= limit 
                then null 
                else c
            let rec repeat (matchCount:int) (currentState:MatchState) : MatchResult =
                let captures = Captures (currentState.Captures |> Seq.mapi resetCapture)
                let currentState = { currentState with Captures = captures }
                let result = atom currentState
                if result.Success then
                    let matchCount = matchCount + 1  
                    if greedy then                           
                        match max with
                        | Finite max when matchCount = max ->
                            result
                        | _ ->
                            repeat matchCount result.MatchState
                    else                           
                        match max with
                        | Finite max when matchCount = max ->
                            result
                        | _ ->
                            match nextTerm with
                            | Some nextTerm ->
                                let nextTermResult = nextTerm state
                                if nextTermResult.Success then
                                    nextTermResult
                                else
                                    repeat matchCount result.MatchState
                            | None ->           
                                result
                elif min = 0 && matchCount = 0 then           
                    MatchResult (true, state)
                else
                    match max with
                    | Finite max when matchCount >= min && matchCount <= max ->                          
                        MatchResult (true, currentState)
                    | Infinite when matchCount >= min ->                          
                        MatchResult (true, currentState)
                    | _ ->                     
                        MatchResult (false, state)
            if min = 0 && not greedy then
                match nextTerm with
                | Some nextTerm ->
                    let result = nextTerm state
                    if result.Success then
                        result
                    else
                        repeat 0 state
                | None ->           
                    MatchResult (true, state)
            else
                repeat 0 state
                    
