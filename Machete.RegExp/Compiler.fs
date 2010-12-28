namespace Machete.RegExp

module internal Compiler =

    open System
    open Machete.Compiler
    open Machete.Compiler.Lexer
    open Machete.Compiler.Parsers
    open Machete.Compiler.Lexer.NumericLiteralParser
    open Machete.Compiler.Lexer.StringLiteralParser
    open Machete.Compiler.Lexer.IdentifierNameParser

    type Max =
    | Bounded of int
    | Unbounded
    
    type State = {
        input : string
        endIndex : int
        captures : string array
        ignoreCase : bool
        multiline : bool
    }

    type MatchResult =
    | Success of State 
    | Failure

    type Continuation = State -> MatchResult

    type Matcher = State -> Continuation -> MatchResult 

    type AssertionTester = State -> bool
    
    type EscapeValue = 
    | Character of char 
    | Integer of int
    
    let canonicalize c s =
        if s.ignoreCase then 
            c 
        else 
            System.Char.ToUpper c

    let characterSetMatcher (a:string) (invert:bool) (s:State) (c:Continuation) =
        if s.endIndex = s.input.Length then 
            Failure
        else
            let cc = s.input.[s.endIndex] |> string
            let comp = if s.ignoreCase then System.StringComparison.InvariantCultureIgnoreCase else System.StringComparison.InvariantCulture
            let found = a.IndexOf (cc, comp) > -1            
            let success = invert || (found && not (invert && found))
            if success then 
                c { s with endIndex = s.endIndex + 1; captures = Array.copy s.captures } 
            else 
                Failure

    let isWordChar e s =
        e <> -1 && e <> s.input.Length && CharSets.wordCharSet.Contains (s.input.[e])

    let characterRange (a:char Set) (b:char Set) =
        if Set.count a <> 1 || Set.count b <> 1 then
            failwith ""
        else
            let a = a.MinimumElement
            let b = b.MinimumElement
            if a > b then
                failwith ""
            else
                set [a..b]

    let evalDecimalEscape d =
        match d with
        | DecimalEscape d ->
            match d with
            | Element.InputElement ie ->
                let i = evalDecimalIntegerLiteral ie
                match i with
                | 0 -> Character '\u0000'
                | _ -> Integer i                   
        | _ -> invalidArg "d" "Expected DecimalEscape."

    let buildAssertionTester a s =
        match a with
        | StartOfInput ->
            s.endIndex = 0 || (s.multiline && CharSets.lineTerminatorCharSet.Contains (s.input.[s.endIndex - 1])) 
        | EndOfInput ->
            s.endIndex = s.input.Length || (s.multiline && CharSets.lineTerminatorCharSet.Contains (s.input.[s.endIndex])) 
        | WordBoundary ->
            let a = isWordChar (s.endIndex - 1) s
            let b = isWordChar s.endIndex s
            (a || b) && not(a && b) 
        | NonWordBoundary ->
            let a = isWordChar (s.endIndex - 1) s
            let b = isWordChar s.endIndex s
            (a && b) || not(a || b)                                                          
        | _ -> failwith "" 

    let rec repeatMatcher m min max greedy x c parenIndex parenCount =
        match max with
        | Bounded n when n = 0 -> c x
        | _ ->
            let d y =
                match min, y.endIndex, x.endIndex with
                | 0, ye, xe when ye = xe -> Failure
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
                match z with
                | Success s -> z
                | Failure -> m xr d
            else
                let z = m xr d 
                match z with
                | Success s -> z
                | Failure -> c x 

    let evalQuantifierPrefix qp =
        match qp with
        | QuantifierPrefix p ->
            match p with
            | ZeroOrMore -> 0, Unbounded
            | OneOrMore -> 1, Unbounded
            | ZeroOrOne -> 0, Bounded 1
            | Range (min, max) ->
                match min, max with
                | Element.InputElement a, Element.InputElement b ->
                    let min = evalDecimalDigits a
                    let max = 
                        match b with
                        | DecimalDigits _ -> 
                            let max = evalDecimalDigits b
                            if max < min then failwith ""
                            Bounded max
                        | Nil -> Unbounded
                        | _ -> failwith ""
                    min, max
            | _ -> failwith ""
        | _ -> failwith ""   
        
    let evalCharacterClassEscape v =
        match v with
        | CharacterClassEscape cce ->
            match cce with
            | 'd' -> CharSets.decimalDigitCharSet
            | 'D' -> CharSets.nonDecimalDigitCharSet
            | 's' -> CharSets.whiteSpaceLineTerminatorCharSet
            | 'S' -> CharSets.nonWhiteSpaceLineTerminatorCharSet
            | 'w' -> CharSets.wordCharSet
            | 'W' -> CharSets.nonWordCharSet
        | _ -> invalidArg "v" "Expected CharacterClassEscape."   
        
    let evalCharacterEscape v =
        match v with
        | CharacterEscape ce ->
            match ce with
            | ControlEscape e ->
                match e with
                | 't' -> '\u0009'
                | 'n' -> '\u000A'
                | 'v' -> '\u000B'
                | 'f' -> '\u000C'
                | 'r' -> '\u000D'
                | _ -> failwith "" 
            | ControlLetter e ->
                let i = int e
                let j = i % 32
                char j
            | Element.InputElement ie ->
                match ie with
                | HexEscapeSequence (_, _) ->
                    evalHexEscapeSequence ie
                | UnicodeEscapeSequence (_, _, _, _) ->
                    evalUnicodeEscapeSequence ie
            | IdentityEscape e -> e
            | _ -> failwith ""
        | _ -> invalidArg "v" "Expected CharacterEscape." 
                                             

    let compile source = 
        assert(source <> null)        
        let pattern = Machete.RegExp.Parser.parse source
        match pattern with
        | Pattern(capturingGroupCount, disjunction) ->  
            let rec build node : Matcher =
                match node with
                | Disjunction (l, r) ->
                    match l, r with
                    | Element.Nil, Alternative (_, _) ->
                        build r
                    | Disjunction (_, _), Alternative (_, _) ->
                        let m1 = build l
                        let m2 = build r
                        fun x c -> 
                            let r = m1 x c
                            match r with 
                            | Success s -> r
                            | Failure -> m2 x c 
                    | _ -> invalidOp "Invalid Disjunction pattern found."

                | Alternative(l, r) ->
                    match l, r with
                    | Element.Nil, Element.Nil -> fun x c -> c x
                    | Alternative _, Term (_, _, _, _) ->
                        let m1 = build l
                        let m2 = build r
                        fun x c ->
                            let d y = m2 y c
                            m1 x d
                    | _ -> failwith ""

                | Term (l, r, pi, pc) ->
                    match l, r with
                    | Assertion a, Element.Nil ->
                        fun x c -> 
                            let t = buildAssertionTester a 
                            let r = t x
                            if r then c x else Failure
                    | Atom a, Element.Nil -> 
                        build l    
                    | Atom a, Quantifier (p, greedy) ->
                        let m = build l
                        let min, max = evalQuantifierPrefix p                            
                        fun x c -> 
                            repeatMatcher m min max greedy x c pi pc
                    | _ -> failwith ""
                                      
                | Atom a ->
                    match a with
                    | PatternCharacter c -> 
                        characterSetMatcher (c.ToString()) false
                    | Dot ->
                        characterSetMatcher (CharSets.nonLineTerminatorCharSet |> Set.toArray |> fun cs -> new String(cs)) false
                    | AtomEscape a ->
                        match a with
                        | DecimalEscape _ ->
                            let e = evalDecimalEscape a
                            match e with
                            | Character c ->
                                characterSetMatcher (c.ToString()) false
                            | Integer n when n > 0 && n <= capturingGroupCount ->
                                fun x c ->
                                    let cap = x.captures
                                    let s = cap.[n - 1]
                                    match s with
                                    | null -> c x
                                    | _ ->
                                        let e = x.endIndex
                                        let len = s.Length
                                        let f = e + len
                                        match f with
                                        | f when f <= x.input.Length ->
                                            let exists i = 
                                                canonicalize (s.[i]) x <> canonicalize (x.input.[e + i]) x
                                            let found = [0..len - 1] |> List.exists exists
                                            match found with
                                            | true -> Failure
                                            | false -> c ({ x with endIndex = f })
                                        | _ -> Failure
                            | _ -> failwith ""
                        | CharacterEscape ce ->
                            let c = evalCharacterEscape a
                            characterSetMatcher (c.ToString()) false                           
                        | CharacterClassEscape cce ->
                            let a = evalCharacterClassEscape a
                            characterSetMatcher (a |> Set.toArray |> fun cs -> new String(cs)) false
                        | _ -> failwith ""
                    | CharacterClass (invert, classRanges) -> 
                        let rec build node = 
                            match node with
                            | ClassRanges l ->
                                build l
                            | NonemptyClassRanges(l, m, h)
                            | NonemptyClassRangesNoDash(l, m, h) ->
                                match l, m, h with
                                | ClassAtom _, Element.Nil, Element.Nil -> 
                                    build l
                                | ClassAtom _, NonemptyClassRangesNoDash _, Element.Nil
                                | ClassAtomNoDash _, NonemptyClassRangesNoDash _, Element.Nil ->
                                    let a = build l
                                    let b = build m
                                    Set.union a b
                                | ClassAtom _, ClassAtom _, ClassRanges _
                                | ClassAtomNoDash _, ClassAtom _, ClassRanges _ ->
                                    let a = build l
                                    let b = build m
                                    let c = build h
                                    let d = characterRange a b
                                    Set.union c d
                                | _ ->  failwith ""
                            | ClassAtom l 
                            | ClassAtomNoDash l ->
                                match l with
                                | Element.Char c ->
                                    set [c]
                                | ClassAtomNoDash _
                                | ClassEscape _ ->
                                    build l
                                | _ ->  failwith ""
                            | ClassEscape l ->
                                match l with
                                | DecimalEscape _ ->
                                    let e = evalDecimalEscape l
                                    match e with
                                    | Character c -> set [c]
                                    | Integer i ->  failwith ""
                                | CharacterEscape _ ->
                                    set [evalCharacterEscape l]
                                | CharacterClassEscape _ ->
                                    evalCharacterClassEscape l
                                | _ ->  failwith ""
                            | BackSpaceEscape ->
                                set ['\u0008']
                            | Element.Nil -> set []
                            | _ ->  failwith ""
                        let a = build classRanges
                        characterSetMatcher (a |> Set.toArray |> fun cs -> new String(cs)) invert.IsSome                            
                    | CapturingGroup (i, d) ->
                        let m = build d 
                        fun x c -> 
                            let d y =
                                let l = y.captures.[0..i - 2]
                                let m = [| y.input.[x.endIndex..y.endIndex - 1] |]
                                let h = y.captures.[i..y.captures.Length - 1]
                                c ({ y with captures = [l; m; h] |> Array.concat })
                            m x d
                    | NonCapturingGroup d ->
                        build d                          
                    | FollowedBy d ->
                        let m = build d
                        fun x c ->
                            let d y = Success y
                            let r = m x d
                            match r with
                            | Success y -> c ({ x with captures = y.captures })
                            | Failure -> Failure                          
                    | NotFollowedBy d ->
                        let m = build d
                        fun x c ->
                            let d y = Success y
                            let r = m x d
                            match r with
                            | Success y -> Failure                                
                            | Failure -> c x
                    | _ -> failwith ""                
                | Element.Nil -> fun x c -> c x
                | _ -> 
                    fun x c -> Failure
            let matcher = build disjunction
            fun input index ignoreCase multiline ->
                let captures = Array.zeroCreate capturingGroupCount
                let x = { input = input; endIndex = index; captures = captures; ignoreCase = ignoreCase; multiline = multiline }
                let c = fun x -> Success x
                matcher x c
        | _ ->  failwith ""