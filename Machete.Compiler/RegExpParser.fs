namespace Machete.Compiler

open System
open FParsec.Primitives
open FParsec.CharParsers
open Machete.Core

module RegExpParser = 

    type private ParseState = {
        environment:IEnvironment
        globalFlag:bool
        ignoreCaseFlag:bool
        multilineFlag:bool
        totalCaptures:int
    }

    type private Max =
    | Bounded of int
    | Unbounded

    type private Quantifier = {
        min:int
        max:Max
        greedy:bool
    }
    
    type MatchState = {
        input : string
        endIndex : int
        captures : string[]
        ignoreCase : bool
        multiline : bool
    } with 
        member this.EndOfInput =
            this.endIndex = this.input.Length
        member this.CurrentChar =
            this.input.[this.endIndex]
        member this.Move () =
            { this with endIndex = this.endIndex + 1; captures = Array.copy this.captures }

    type MatchResult = {
        matchState:MatchState
        success:bool
    } with
        static member Failure matchState =
            { matchState = matchState; success = false }

    type RegExpMatcher = delegate of string * int -> MatchResult

    type private Continuation = MatchState -> MatchResult

    type private Matcher = MatchState -> Continuation -> MatchResult 

    type private AssertionTester = MatchState -> bool
    
    let private characterSetMatcher (a:string) (invert:bool) (x:MatchState) (c:Continuation) =
        if x.EndOfInput 
        then MatchResult.Failure x
        else
            let cc = x.input.[x.endIndex] |> string
            let comp = 
                if x.ignoreCase 
                then StringComparison.InvariantCultureIgnoreCase 
                else StringComparison.InvariantCulture
            let found = a.IndexOf (x.CurrentChar |> string, comp) > -1            
            let success = invert || (found && not (invert && found))
            if success 
            then c (x.Move()) 
            else MatchResult.Failure x
            
    let private characterRange (a:string) (b:string) (parseState:ParseState) =
        let notOneCharFormat = "The value '{0}' cannot {1} a character class range."
        if a.Length > 1 then
            let msg = String.Format (notOneCharFormat, a, "start")
            raise (parseState.environment.CreateSyntaxError msg)
        elif b.Length > 1 then
            let msg = String.Format (notOneCharFormat, a, "end")
            raise (parseState.environment.CreateSyntaxError msg)
        let a = a.[0]
        let b = b.[0]
        if a > b then
            let rangeErrorFormat = "The right side '{0}' is greater then the left side '{1}' in the character class range."
            let msg = String.Format (rangeErrorFormat, a, b)
            raise (parseState.environment.CreateSyntaxError msg)
        String ([|a..b|])
        
    let rec private repeatMatcher (m:Matcher) min max greedy x (c:Continuation) parenIndex parenCount =
        match max with
        | Bounded n when n = 0 -> c x
        | _ ->
            let d y : MatchResult =
                match min, y.endIndex, x.endIndex with
                | 0, ye, xe when ye = xe -> MatchResult.Failure x
                | _ ->
                    let min2 = if min = 0 
                               then 0 
                               else min - 1
                    let max2 = match max with 
                               | Bounded n -> Bounded (n - 1) 
                               | Unbounded -> Unbounded
                    repeatMatcher m min2 max2 greedy y c parenIndex parenCount
            let limit = parenIndex + parenCount
            let cap = [| 
                for i in 0..x.captures.Length - 1 do 
                    yield if i > parenIndex && i <= limit then null else x.captures.[i] 
            |]
            let xr = { x with captures = cap }
            if min <> 0 then
                m xr d
            elif not greedy then
                let z = c x
                if z.success
                then z
                else m xr d
            else
                let z = m xr d 
                if z.success
                then z
                else c x

    let private isWordChar e s =
        e <> -1 && e <> s.input.Length && CharSets.wordCharSet.Contains (s.input.[e])

    let rec private evalPattern state = 
        (parse {
            let! m = evalDisjunction
            let! parseState = getUserState
            let result input index =
                let state = {
                    input = input
                    endIndex = index  
                    captures = Array.zeroCreate parseState.totalCaptures  
                    ignoreCase = parseState.ignoreCaseFlag
                    multiline = parseState.multilineFlag
                }
                let c state = { matchState = state; success = true }
                m state c
            return RegExpMatcher result
        }) state
        
    and private evalDisjunction state = 
        (parse {
            let reduce (m1:Matcher) (m2:Matcher) : Matcher =
                fun x c -> 
                    let r = m1 x c; 
                    if r.success 
                    then r 
                    else m2 x c
            let! ms = sepBy evalAlternative (pchar '|') 
            let result = ms |> List.reduce reduce
            return result
        }) state
        
    and private evalAlternative state =
        (parse {
            let reduce (m1:Matcher) (m2:Matcher) : Matcher =
                fun x c -> 
                    let d y = m2 y c
                    m1 x d
            let! ms = many (attempt evalTerm)
            let result = ms |> List.reduce reduce
            return result
        }) state
        
    and private evalTerm state = 
        (evalAssertionTerm <|> evalAtomTerm) state

    and private evalAssertionTerm state = 
        (parse {
            let! t = evalAssertion
            let result x c =
                if t x 
                then c x
                else { matchState = x; success = false }
            return result
        }) state

    and private evalAtomTerm state = 
        (parse {
            let! s1 = getUserState
            let! a = evalAtom
            let! s2 = getUserState
            let! b = opt evalQuantifier
            match b with
            | Some b ->
                match b.max with
                | Bounded max when max < b.min ->
                    let msg = "The limit {1} of {{{0},{1}}} cannot be greater than {0}."
                    let msg = String.Format (msg, b.min, max)
                    raise (state.UserState.environment.CreateSyntaxError msg)
                | _ ->
                    let parenIndex = s1.totalCaptures
                    let parenCount = s2.totalCaptures - s1.totalCaptures
                    let matcher x c = repeatMatcher a b.min b.max b.greedy x c parenIndex parenCount
                    return matcher
            | None -> return a
        }) state
        
    and private evalAssertion state =     
        let startOfInput state =
            state.endIndex = 0 || (state.multiline && CharSets.isLineTerminator (state.input.[state.endIndex - 1]))
        let endOfInput state =
            state.endIndex = state.input.Length || (state.multiline && CharSets.isLineTerminator (state.input.[state.endIndex])) 
        let wordBoundary state =        
            let a = if isWordChar (state.endIndex - 1) state then 1 else 0
            let b = if isWordChar state.endIndex state then 1 else 0
            a ^^^ b = 1        
        let nonWordBoundary state =
            let a = if isWordChar (state.endIndex - 1) state then 1 else 0
            let b = if isWordChar state.endIndex state then 1 else 0
            a ^^^ b = 0
        (parse {
            let! c = opt (anyOf "^$")
            match c with
            | Some '^' -> return startOfInput
            | Some '$' -> return endOfInput 
            | None ->
                let! s = pstring "\\b" <|> pstring "\\B"
                match s with
                | "\\b" -> return wordBoundary
                | "\\B" -> return nonWordBoundary 
        }) state
        
    and private evalQuantifier state = 
        (parse {
            let! min, max = evalQuantifierPrefix
            let! c = opt (pchar '?')
            match c with
            | Some _ -> return { min = min; max = max; greedy = false }
            | None -> return { min = min; max = max; greedy = true }
        }) state
        
    and private evalQuantifierPrefix state = 
        (parse {
            let! c = anyOf "*+?{"
            match c with
            | '*' -> return 0, Unbounded
            | '+' -> return 1, Unbounded
            | '?' -> return 0, Bounded 1
            | '{' ->
                let! min = InputElementParsers.evalDecimalDigits 1.0 0.0
                let min = min |> int
                let! c = anyOf ",}"
                match c with
                | '}' -> return min, Unbounded
                | ',' ->
                    let! c = opt (pchar '}')
                    match c with
                    | Some _ -> return min, Unbounded
                    | None -> 
                        let! max = InputElementParsers.evalDecimalDigits 1.0 0.0
                        let max = max |> int
                        do! skipChar '}'
                        return min, Bounded max
        }) state
        
    and private evalAtom state = 
        let dotAtom x c =
            if CharSets.isLineTerminator (x.input.[x.endIndex]) 
            then { matchState = x; success = false }
            else { matchState = { x with endIndex = x.endIndex + 1 }; success = true }
        (parse {
            let! c = opt (anyOf ".\\(")
            match c with
            | Some '.' -> return dotAtom
            | Some '\\' ->
                let! r = evalAtomEscape
                return r
            | Some '(' ->
                let! q = opt (pchar '?') 
                match q with
                | Some _ ->
                    let! r = anyOf ":=!"
                    let! m = evalDisjunction
                    do! skipChar ')'
                    match r with
                    | ':' -> return m    
                    | '=' -> 
                        let matcher (x:MatchState) (c:Continuation) =
                            let d y = { matchState = y; success = true }
                            let r = m x d
                            match r with
                            | { matchState = _; success = false } as x -> x
                            | { matchState = y; success = true } ->
                                c { y with endIndex = x.endIndex }
                        return matcher                           
                    | '!' -> 
                        let matcher (x:MatchState) (c:Continuation) =
                            let d y = { matchState = y; success = true }
                            let r = m x d
                            match r with
                            | { matchState = _; success = false } -> c x
                            | { matchState = y; success = true } -> MatchResult.Failure x
                        return matcher 
                | None ->   
                    let! s1 = getUserState
                    do! setUserState { s1 with totalCaptures = s1.totalCaptures + 1 }
                    let! m = evalDisjunction
                    do! skipChar ')'   
                    let p = s1.totalCaptures 
                    let matcher (x:MatchState) (c:Continuation) =
                        let d y =
                            let cap = Array.copy y.captures
                            let xe = x.endIndex
                            let ye = y.endIndex
                            let s = x.input.[xe..ye - 1]
                            cap.[p] <- s
                            let z = { y with captures = cap }
                            c z
                        m x d
                    return matcher                             
            | _ ->
                let! r = opt evalPatternCharacter
                match r with
                | Some c ->
                    return characterSetMatcher (c.ToString()) false
                | None ->
                    let! chars, invert = evalCharacterClass
                    return characterSetMatcher chars invert
        }) state
        
    and private evalPatternCharacter state =  
        (noneOf "^$\\.*+?()[]{}|") state
        
    and private evalAtomEscape state = 
        let outOfRangeFormat = "The number {0} is not a valid capturing group{1}."
        let rangeFormat = " (1..{0})"
        let decimalEscape state =             
            let run n x c =
                let cap = x.captures
                let s = cap.[n]
                if s = null 
                then c x
                else
                    let e = x.endIndex
                    let len = s.Length
                    let f = e + len
                    if f > x.input.Length
                    then MatchResult.Failure x
                    else                                 
                        let compare =                                             
                            if x.ignoreCase 
                            then fun c1 c2 -> Char.ToUpperInvariant c1 = Char.ToUpperInvariant c2
                            else fun c1 c2 -> c1 = c2
                        let result = [0..len - 1] |> List.forall (fun i -> compare (s.[i]) (x.input.[e + i])) 
                        if result 
                        then c { x with endIndex = f; captures = Array.copy x.captures }
                        else MatchResult.Failure x
            (parse {
                let! r = evalDecimalEscape
                match r with
                | 0.0 -> return characterSetMatcher "\u0000" false
                | num ->
                    let! s = getUserState
                    let n = num |> int
                    if n = 0 || n > s.totalCaptures then
                        let range = 
                            if s.totalCaptures > 0 
                            then String.Format (rangeFormat, s.totalCaptures)
                            else ""
                        let msg = String.Format (outOfRangeFormat, n, range)
                        let! _ = failFatally msg
                        ()
                    else return run n
            }) state
        let characterEscape state =
            (evalCharacterEscape |>> fun x -> characterSetMatcher x false) state  
        let characterClassEscape state =
            (evalCharacterClassEscape |>> fun x -> characterSetMatcher x false) state  
        (decimalEscape <|> characterEscape <|> characterClassEscape) state
        
    and private evalCharacterEscape state = 
        (evalControlEscape <|> 
         (skipChar 'c' >>. evalControlEscape) <|> 
         (InputElementParsers.evalHexEscapeSequence) <|> 
         (InputElementParsers.evalUnicodeEscapeSequence) <|> 
         evalIdentityEscape) state
        
    and private evalControlEscape state = 
        (parse {
            let! c = anyOf "fnrtv"
            match c with
            | 'f' -> return "\u000C"
            | 'n' -> return "\u000A"
            | 'r' -> return "\u000D"
            | 't' -> return "\u0009"
            | 'v' -> return "\u000B"
        }) state
        
    and private evalControlLetter state = 
        (parse {
            let! c = anyOf "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
            return (c |> int) % 32 |> char |> string
        }) state
        
    and private evalIdentityEscape state = 
        (parse {
            let! r = opt InputElementParsers.evalIdentifierPart
            match r with
            | None ->
                let! r = anyChar
                return r |> string
            | Some "\u200C" -> return "\u200C"
            | Some "\u200D" -> return "\u200D"
            | _ -> ()
        }) state
        
    and private evalDecimalEscape state = 
        (InputElementParsers.evalDecimalIntegerLiteral .>> notFollowedBy InputElementParsers.evalDecimalDigit) state
        
    and private evalCharacterClassEscape state =
        (parse {
            let! c = anyOf "dDsSwW"
            match c with
            | 'd' -> return CharSets.decimalDigitCharsRegExp
            | 'D' -> return CharSets.nonDecimalDigitCharsRegExp
            | 's' -> return CharSets.whiteSpaceCharsRegExp
            | 'S' -> return CharSets.nonWhiteSpaceCharsRegExp
            | 'w' -> return CharSets.wordCharsRegExp
            | 'W' -> return CharSets.nonWordCharsRegExp
        }) state
        
    and private evalCharacterClass state = 
        (parse {
            do! skipChar '['
            let! invert = opt (skipChar '^')
            let! chars = evalClassRanges
            do! skipChar ']'
            return chars, invert.IsSome 
        }) state
        
    and private evalClassRanges state = 
        (parse {
            let! r = opt evalNonemptyClassRanges
            match r with
            | Some r -> return r
            | None -> return ""
        }) state
        
    and private evalNonemptyClassRanges state = 
        (parse {
            let! a = evalClassAtom
            let! dash = opt (pchar '-')
            match dash with
            | Some _ ->
                let! b = evalClassAtom
                let! c = evalClassRanges
                let d = characterRange a b state.UserState
                return c + d
            | None ->
                let! b = opt evalNonemptyClassRangesNoDash
                match b with
                | Some b -> return a + b                    
                | None -> return a            
        }) state
        
    and private evalNonemptyClassRangesNoDash state = 
        (parse {
            let! a = opt evalClassAtomNoDash
            match a with
            | Some a ->
                let! dash = opt (pchar '-')
                match dash with
                | Some _ ->
                    let! b = evalClassAtom
                    let! c = evalClassRanges
                    let d = characterRange a b state.UserState
                    return c + d
                | None ->
                    let! b = opt evalNonemptyClassRangesNoDash
                    match b with
                    | Some b -> return a + b                    
                    | None -> return a 
            | None ->
                let! a = evalClassAtom
                return a         
        }) state
        
    and private evalClassAtom state = 
        (pstring "-" <|> evalClassAtomNoDash) state
        
    and private evalClassAtomNoDash state = 
        ((noneOf "\\]-" |>> string) <|> (skipChar '\\' >>. evalClassEscape)) state
        
    and private evalClassEscape state =
        let decimalEscape state =
            (parse {
                let! r = evalDecimalEscape
                match r with
                | 0.0 -> return "\u0000"
                | n -> 
                    let msg = "The decimal escape '\\{0}' is not allowed in a character class."
                    let msg = String.Format (msg, n)
                    let! _ = failFatally msg
                    ()
            }) state 
        (decimalEscape <|> pstring "b" <|> evalCharacterEscape <|> evalCharacterClassEscape) state

    let private evalFlags (environment:IEnvironment) (flags:string) =
        let tooManyCharacters = "There cannot be more than 3 characters in the flags parameter: '{0}'"
        let multipleFlagFormat = "The '{0}' flag cannot appear more than once: '{1}'"
        if flags.Length > 3 then raise (environment.CreateSyntaxError (String.Format (tooManyCharacters, flags)))
        let result = flags |> Seq.countBy id |> Map.ofSeq
        let checkFlag c =
            let r = result.TryFind c
            match r with
            | None -> false
            | Some 1 -> true
            | _ -> raise (environment.CreateSyntaxError (String.Format (tooManyCharacters, c, flags)))
        let globalFlag = checkFlag 'g'
        let ignoreCaseFlag = checkFlag 'i'
        let multilineFlag = checkFlag 'm'
        globalFlag, ignoreCaseFlag, multilineFlag

    let Parse (environment:IEnvironment, body:string, flags:string) =
        if environment = null then nullArg "environment"
        if body = null then nullArg "body"
        if flags = null then nullArg "flags"
        let globalFlag, ignoreCaseFlag, multilineFlag = evalFlags environment flags
        let state = { environment = environment; globalFlag = globalFlag; ignoreCaseFlag = ignoreCaseFlag; multilineFlag = multilineFlag; totalCaptures = 0 }
        let result = runParserOnString evalPattern state "RegExp" body
        match result with
        | ParserResult.Success (value, state, position) -> value
        | ParserResult.Failure (message, errors, state) ->
            raise (environment.CreateSyntaxError message)