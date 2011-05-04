namespace Machete.RegExp

open System
open FParsec.Primitives
open FParsec.CharParsers

module Parser =

    type private ParseState = {
        info:RegExpInfo
        totalCaptures:int
    }

    type private Term =
    | Assertion of Matcher
    | Atom of int * Matcher * int * option<int * Limit * bool>


    let private characterRange (a:string) (b:string) =
        let notOneCharFormat = "The value '{0}' cannot {1} a character class range."
        if a.Length > 1 then
            let msg = String.Format (notOneCharFormat, a, "start")
            failwith msg
        elif b.Length > 1 then
            let msg = String.Format (notOneCharFormat, a, "end")
            failwith msg
        let a = a.[0]
        let b = b.[0]
        if a > b then
            let rangeErrorFormat = "The right side '{0}' is greater then the left side '{1}' in the character class range."
            let msg = String.Format (rangeErrorFormat, a, b)
            failwith msg
        String ([|a..b|])

    let rec private parsePattern state =
        (parse {
            let! disjunction = parseDisjunction 
            let! state = getUserState
            return Matchers.createPattern state.info state.totalCaptures disjunction
        }) state

    and private parseDisjunction state =
        (parse {
            let! alternatives = sepBy parseAlternative (pchar '|')  
            return Matchers.createDisjunction alternatives 
        }) state

    and private parseAlternative state =
        (parse {
            let! terms = many1 parseTerm
            let rec exec ts acc =
                match ts with
                | t::[] ->
                    match t with
                    | Atom (before, matcher, after, Some (min, max, greedy)) ->
                        let next = None
                        let matcher = Matchers.createQuantifier matcher next min max greedy before (after - before)
                        matcher::acc
                    | Atom (_, matcher, _, _)
                    | Assertion matcher ->
                        exec [] (matcher::acc)
                | t::ts ->
                    match t with
                    | Atom (before, matcher, after, Some (min, max, greedy)) ->
                        let next = 
                            if ts.IsEmpty then 
                                None 
                            else
                                match ts.Head with
                                | Atom (_, matcher, _, _) ->
                                    Some matcher
                                | Assertion matcher ->
                                    Some matcher
                        let matcher = Matchers.createQuantifier matcher next min max greedy before (after - before)
                        exec ts (matcher::acc)
                    | Atom (_, matcher, _, _) ->
                        exec ts (matcher::acc)
                    | Assertion matcher ->
                        exec ts (matcher::acc)
                | [] -> acc
            let terms = exec terms [] |> List.rev
            return Matchers.createAlternative terms
        }) state

    and private parseTerm state =
        (parse {
            let! before, atom, after, quantifier = 
                tuple4 getUserState parseAtom getUserState (opt parseQuantifier)
            return Atom (before.totalCaptures, atom, after.totalCaptures, quantifier)
         } <|> (parseAssertion |>> Assertion)) state

    and private parseAssertion state =
        (attempt <| parse {
            let! c = anyOf "^$\\"
            match c with
            | '^' -> return Matchers.createStartOfInputAssertion ()
            | '$' -> return Matchers.createEndOfInputAssertion ()
            | '\\' ->
                let! c = anyOf "bB"
                match c with
                | 'b' -> return Matchers.createWordBoundaryAssertion ()
                | 'B' -> return Matchers.createNonWordBoundaryAssertion ()
        }) state

    and private parseQuantifier state = 
        (parse {
            let! min, max = parseQuantifierPrefix
            let! c = opt (pchar '?')
            match c with
            | Some _ -> return min, max, false
            | None -> return min, max, true
        }) state

    and private parseQuantifierPrefix state = 
        (parse {
            let! c = anyOf "*+?{"
            match c with
            | '*' -> return 0, Infinite
            | '+' -> return 1, Infinite
            | '?' -> return 0, Finite 1
            | '{' ->
                let! min = pint32
                let! c = anyOf ",}"
                match c with
                | '}' -> return min, Infinite
                | ',' ->
                    let! c = opt (pchar '}')
                    match c with
                    | Some _ -> return min, Infinite
                    | None -> 
                        let! max = pint32
                        do! skipChar '}'
                        return min, Finite max
        }) state

    and private parseAtom state =
        (parse {
            let! state = getUserState
            let! c = opt (anyOf ".\\(")
            match c with
            | Some '.' -> return Matchers.createDotAtom ()
//            | Some '\\' ->
//                let! c = parseAtomEscape
//                return c
            | Some '(' ->
                let! q = opt (pchar '?') 
                match q with
                | Some _ ->
                    let! r = anyOf ":=!"
                    let! m = parseDisjunction
                    do! skipChar ')'
                    match r with
                    | ':' -> 
                        return m    
                    | '=' -> 
                        return Matchers.createFollowedByAssertion m                          
                    | '!' -> 
                        return Matchers.createNotFollowedByAssertion m 
                | None ->  
                    do! setUserState { state with totalCaptures = state.totalCaptures + 1 }
                    let! m = parseDisjunction
                    do! skipChar ')'
                    return Matchers.createCapturingAtom state.totalCaptures m                            
            | _ ->
                let! r = opt parsePatternCharacter
                match r with
                | Some c ->
                    return Matchers.createSingleCharacterAtom state.info.IgnoreCase c
                | None -> ()
//                    let! cs, invert = parseCharacterClass
//                    return Matchers.createManyCharacterAtom state.info.IgnoreCase invert cs 
        }) state

    and private parsePatternCharacter state =
        (noneOf "^$\\.*+?()[]{}|") state

//    and private parseAtomEscape state : Reply<Matcher, ParseState> = 
//        (parseDecimalEscape <|> parseCharacterEscape <|> parseCharacterClassEscape) state

//    and private parseCharacterEscape state : Reply<char, ParseState> = 
//         (parseControlEscape <|> 
//         (skipChar 'c' >>. parseControlEscape) <|> 
//         parseHexEscapeSequence <|> 
//         parseUnicodeEscapeSequence <|> 
//         parseIdentityEscape) state
//         
//    and parseHexEscapeSequence state : Reply<char, ParseState> =
//        (parse {
//            let! state = getUserState
//            let! a, b = skipChar 'x' >>. (tuple2 hex hex)
//            let a, b = completeHexDigit a, completeHexDigit b
//            return (16.0 * a + b) |> char
//        }) state
//
//    and parseUnicodeEscapeSequence state : Reply<char, ParseState> =
//        (parse {
//            let! state = getUserState
//            let! a, b, c, d = skipChar 'u' >>. (tuple4 hex hex hex hex)
//            let a, b, c, d = completeHexDigit a, completeHexDigit b, completeHexDigit c, completeHexDigit d
//            return (4096.0 * a + 256.0 * b + 16.0 * c + d) |> char
//        }) state
//        
//    and private completeHexDigit c =
//        match c with
//        | c when c >= '0' && c <= '9' -> 
//            double c - 48.0 
//        | c when c >= 'A' && c <= 'F' -> 
//            double c - 55.0
//        | c when c >= 'a' && c <= 'f' -> 
//            double c - 87.0
//
//    and private parseControlEscape state : Reply<char, ParseState> = 
//        (parse {
//            let! c = anyOf "fnrtv"
//            let c = 
//                match c with
//                | 'f' -> '\u000C'
//                | 'n' -> '\u000A'
//                | 'r' -> '\u000D'
//                | 't' -> '\u0009'
//                | 'v' -> '\u000B'
//            return c
//        }) state
//
//    and private parseControlLetter state =
//        (parse {
//            let! state = getUserState
//            let! c = anyOf "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
//            let c = (c |> int) % 32 |> char
//            return c
//        }) state
//
//    and private parseIdentityEscape state : Reply<char, ParseState> = 
//        ((anyOf "\u200C\u200D$") <|> parse {
//            let! r = notFollowedBy parseIdentifierPart >>. anyChar
//            return r
//        }) state
//
//    and private parseIdentifierStart state =
//        (
//            parseUnicodeLetter
//            <|> (anyOf "$_" )
//            <|> (skipChar '\\' >>. parseUnicodeEscapeSequence |>> fun _ -> 'c')
//        ) state
//
//    and private parseIdentifierPart state =
//        (
//            parseIdentifierStart
//            <|> parseUnicodeCombiningMark
//            <|> parseUnicodeDigit
//            <|> parseUnicodeConnectorPunctuation
//            <|> (anyOf "\u200C\u200D")
//        ) state
//    
//    and parseUnicodeLetter state =
//        (satisfy Characters.isUnicodeLetter) state
//
//    and parseUnicodeCombiningMark state =
//        (satisfy Characters.isUnicodeCombiningMark) state
//
//    and parseUnicodeDigit state =
//        (satisfy Characters.isUnicodeDigit) state
//
//    and parseUnicodeConnectorPunctuation state =
//        (satisfy Characters.isUnicodeConnectorPunctuation)  

//    and private parseDecimalEscape state = 
//        ((pchar '0' |>> fun c -> 0) <|> pint32 .>> notFollowedBy digit |>> fun n -> Matchers.createDecimalEscapeAtom n) state
//
//    and private parseCharacterClassEscape state =
//        (parse {
//            let! c = anyOf "dDsSwW"
//            let! state = getUserState
//            match c with
//            | 'd' -> return Matchers.createManyCharacterAtom state.info.IgnoreCase false Characters.decimalDigitCharsRegExp
//            | 'D' -> return Matchers.createManyCharacterAtom state.info.IgnoreCase true Characters.decimalDigitCharsRegExp
//            | 's' -> return Matchers.createManyCharacterAtom state.info.IgnoreCase false Characters.whiteSpaceCharsRegExp
//            | 'S' -> return Matchers.createManyCharacterAtom state.info.IgnoreCase true Characters.whiteSpaceCharsRegExp
//            | 'w' -> return Matchers.createManyCharacterAtom state.info.IgnoreCase false Characters.wordCharsRegExp
//            | 'W' -> return Matchers.createManyCharacterAtom state.info.IgnoreCase true Characters.wordCharsRegExp
//        }) state
//
//    and private parseCharacterClass state =         
//        (parse {
//            do! skipChar '['
//            let! invert = opt (skipChar '^')
//            let! chars = parseClassRanges
//            do! skipChar ']'
//            return chars, invert.IsSome 
//        }) state
//
//    and private parseClassRanges state = 
//        (parse {
//            let! r = opt parseNonemptyClassRanges
//            match r with
//            | Some r -> return r
//            | None -> return ""
//        }) state
//
//    and private parseNonemptyClassRanges state = 
//        (parse {
//            let! a = parseClassAtom
//            let! dash = opt (pchar '-')
//            match dash with
//            | Some _ ->
//                let! b = parseClassAtom
//                let! c = parseClassRanges
//                let d = characterRange a b
//                return c + d
//            | None ->
//                let! b = opt parseNonemptyClassRangesNoDash
//                match b with
//                | Some b -> return a + b                    
//                | None -> return a            
//        }) state
//
//    and private parseNonemptyClassRangesNoDash state = 
//        (parse {
//            let! a = opt parseClassAtomNoDash
//            match a with
//            | Some a ->
//                let! dash = opt (pchar '-')
//                match dash with
//                | Some _ ->
//                    let! b = parseClassAtom
//                    let! c = parseClassRanges
//                    let d = characterRange a b
//                    return c + d
//                | None ->
//                    let! b = opt parseNonemptyClassRangesNoDash
//                    match b with
//                    | Some b -> return a + b                    
//                    | None -> return a 
//            | None ->
//                let! a = parseClassAtom
//                return a         
//        }) state
//
//    and private parseClassAtom state =
//        (pstring "-" <|> parseClassAtomNoDash) state
//
//    and private parseClassAtomNoDash state = 
//        ((noneOf "\\]-" |>> string) <|> (skipChar '\\' >>. parseClassEscape)) state
//
//    and private parseClassEscape state =
//        let decimalEscape state =
//            (parse {
//                let! r = parseDecimalEscape
//                match r with
//                | 0.0 -> return "\u0000"
//                | n -> 
//                    let msg = "The decimal escape '\\{0}' is not allowed in a character class."
//                    let msg = String.Format (msg, n)
//                    let! _ = failFatally msg
//                    ()
//            }) state 
//        (decimalEscape <|> (pstring "b" |>> fun _ -> "\u0008") <|> parseCharacterEscape <|> parseCharacterClassEscape) state

    let parse (info:RegExpInfo) : PatternMatcher =
        let state = { info = info; totalCaptures = 0 }
        let result = runParserOnString parsePattern state "RegExp" info.PatternText
        match result with
        | Success (result, state, position) -> result
        | Failure (message, _, _) -> failwith message

//module Parser2 =
//
//    type private ParseState = {
//        info:RegExpInfo
//        totalCaptures:int
//    }
//
//    type private Term =
//    | Assertion of Matcher
//    | Atom of int * Matcher * int * option<int * Limit * bool>
//
//
//    let private characterRange (a:string) (b:string) =
//        let notOneCharFormat = "The value '{0}' cannot {1} a character class range."
//        if a.Length > 1 then
//            let msg = String.Format (notOneCharFormat, a, "start")
//            failwith msg
//        elif b.Length > 1 then
//            let msg = String.Format (notOneCharFormat, a, "end")
//            failwith msg
//        let a = a.[0]
//        let b = b.[0]
//        if a > b then
//            let rangeErrorFormat = "The right side '{0}' is greater then the left side '{1}' in the character class range."
//            let msg = String.Format (rangeErrorFormat, a, b)
//            failwith msg
//        String ([|a..b|])
//
//    let rec private parsePattern state =
//        (parse {
//            let! disjunction = parseDisjunction 
//            let! state = getUserState
//            return Pattern (state.totalCaptures, disjunction)
//        }) state
//
//    and private parseDisjunction state =
//        (parse {
//            let! head = parseAlternative
//            let! tail = opt parseDisjunctionTail
//            return Disjunction (head, tail)
//        }) state
//
//    and private parseDisjunctionTail state =
//        (parse {
//            do! skipChar '|'
//            let! head = parseAlternative
//            let! tail = opt parseDisjunctionTail
//            return Disjunction (head, tail)
//        }) state
//
//    and private parseAlternative state =
//        (parse {
//            let! head = parseTerm
//            let! tail = opt parseAlternative
//            return Alternative (head, tail)
//        }) state
//
//    and private parseTerm state =
//        (parse {
//            let! term = parseAssertion <|> parseAtomTerm
//            return Term term
//        }) state
//
//    and private parseAtomTerm state =
//        (parse {
//            let! atom = parseAtom
//            let! quantifier = opt parseQuantifier
//            return Atom (atom, quantifier)
//         }) state
//
//    and private parseAssertion state =
//        (attempt <| parse {
//            let! c = anyOf "^$\\"
//            match c with
//            | '^' -> return StartOfInput
//            | '$' -> return EndOfInput
//            | '\\' ->
//                let! c = anyOf "bB"
//                match c with
//                | 'b' -> return WordBoundary
//                | 'B' -> return NonWordBoundary
//        }) state
//
//    and private parseQuantifier state = 
//        (parse {
//            let! min, max = parseQuantifierPrefix
//            let! c = opt (pchar '?')
//            match c with
//            | Some _ -> return { min = min; max = max; greedy = false }
//            | None -> return { min = min; max = max; greedy = true }
//        }) state
//
//    and private parseQuantifierPrefix state = 
//        (parse {
//            let! c = anyOf "*+?{"
//            match c with
//            | '*' -> return 0, Infinite
//            | '+' -> return 1, Infinite
//            | '?' -> return 0, Finite 1
//            | '{' ->
//                let! min = pint32
//                let! c = anyOf ",}"
//                match c with
//                | '}' -> return min, Infinite
//                | ',' ->
//                    let! c = opt (pchar '}')
//                    match c with
//                    | Some _ -> return min, Infinite
//                    | None -> 
//                        let! max = pint32
//                        do! skipChar '}'
//                        return min, Finite max
//        }) state
//
//    and private parseAtom state =
//        (parse {
//            let! state = getUserState
//            let! c = opt (anyOf ".\\(")
//            match c with
//            | Some '.' -> return Atom.d
////            | Some '\\' ->
////                let! c = parseAtomEscape
////                return c
//            | Some '(' ->
//                let! q = opt (pchar '?') 
//                match q with
//                | Some _ ->
//                    let! r = anyOf ":=!"
//                    let! m = parseDisjunction
//                    do! skipChar ')'
//                    match r with
//                    | ':' -> 
//                        return m    
//                    | '=' -> 
//                        return Matchers.createFollowedByAssertion m                          
//                    | '!' -> 
//                        return Matchers.createNotFollowedByAssertion m 
//                | None ->  
//                    do! setUserState { state with totalCaptures = state.totalCaptures + 1 }
//                    let! m = parseDisjunction
//                    do! skipChar ')'
//                    return Matchers.createCapturingAtom state.totalCaptures m                            
//            | _ ->
//                let! r = opt parsePatternCharacter
//                match r with
//                | Some c ->
//                    return Matchers.createSingleCharacterAtom state.info.IgnoreCase c
//                | None -> ()
////                    let! cs, invert = parseCharacterClass
////                    return Matchers.createManyCharacterAtom state.info.IgnoreCase invert cs 
//        }) state
//
//    and private parsePatternCharacter state =
//        (noneOf "^$\\.*+?()[]{}|") state
//
////    and private parseAtomEscape state : Reply<Matcher, ParseState> = 
////        (parseDecimalEscape <|> parseCharacterEscape <|> parseCharacterClassEscape) state
//
////    and private parseCharacterEscape state : Reply<char, ParseState> = 
////         (parseControlEscape <|> 
////         (skipChar 'c' >>. parseControlEscape) <|> 
////         parseHexEscapeSequence <|> 
////         parseUnicodeEscapeSequence <|> 
////         parseIdentityEscape) state
////         
////    and parseHexEscapeSequence state : Reply<char, ParseState> =
////        (parse {
////            let! state = getUserState
////            let! a, b = skipChar 'x' >>. (tuple2 hex hex)
////            let a, b = completeHexDigit a, completeHexDigit b
////            return (16.0 * a + b) |> char
////        }) state
////
////    and parseUnicodeEscapeSequence state : Reply<char, ParseState> =
////        (parse {
////            let! state = getUserState
////            let! a, b, c, d = skipChar 'u' >>. (tuple4 hex hex hex hex)
////            let a, b, c, d = completeHexDigit a, completeHexDigit b, completeHexDigit c, completeHexDigit d
////            return (4096.0 * a + 256.0 * b + 16.0 * c + d) |> char
////        }) state
////        
////    and private completeHexDigit c =
////        match c with
////        | c when c >= '0' && c <= '9' -> 
////            double c - 48.0 
////        | c when c >= 'A' && c <= 'F' -> 
////            double c - 55.0
////        | c when c >= 'a' && c <= 'f' -> 
////            double c - 87.0
////
////    and private parseControlEscape state : Reply<char, ParseState> = 
////        (parse {
////            let! c = anyOf "fnrtv"
////            let c = 
////                match c with
////                | 'f' -> '\u000C'
////                | 'n' -> '\u000A'
////                | 'r' -> '\u000D'
////                | 't' -> '\u0009'
////                | 'v' -> '\u000B'
////            return c
////        }) state
////
////    and private parseControlLetter state =
////        (parse {
////            let! state = getUserState
////            let! c = anyOf "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
////            let c = (c |> int) % 32 |> char
////            return c
////        }) state
////
////    and private parseIdentityEscape state : Reply<char, ParseState> = 
////        ((anyOf "\u200C\u200D$") <|> parse {
////            let! r = notFollowedBy parseIdentifierPart >>. anyChar
////            return r
////        }) state
////
////    and private parseIdentifierStart state =
////        (
////            parseUnicodeLetter
////            <|> (anyOf "$_" )
////            <|> (skipChar '\\' >>. parseUnicodeEscapeSequence |>> fun _ -> 'c')
////        ) state
////
////    and private parseIdentifierPart state =
////        (
////            parseIdentifierStart
////            <|> parseUnicodeCombiningMark
////            <|> parseUnicodeDigit
////            <|> parseUnicodeConnectorPunctuation
////            <|> (anyOf "\u200C\u200D")
////        ) state
////    
////    and parseUnicodeLetter state =
////        (satisfy Characters.isUnicodeLetter) state
////
////    and parseUnicodeCombiningMark state =
////        (satisfy Characters.isUnicodeCombiningMark) state
////
////    and parseUnicodeDigit state =
////        (satisfy Characters.isUnicodeDigit) state
////
////    and parseUnicodeConnectorPunctuation state =
////        (satisfy Characters.isUnicodeConnectorPunctuation)  
//
////    and private parseDecimalEscape state = 
////        ((pchar '0' |>> fun c -> 0) <|> pint32 .>> notFollowedBy digit |>> fun n -> Matchers.createDecimalEscapeAtom n) state
////
////    and private parseCharacterClassEscape state =
////        (parse {
////            let! c = anyOf "dDsSwW"
////            let! state = getUserState
////            match c with
////            | 'd' -> return Matchers.createManyCharacterAtom state.info.IgnoreCase false Characters.decimalDigitCharsRegExp
////            | 'D' -> return Matchers.createManyCharacterAtom state.info.IgnoreCase true Characters.decimalDigitCharsRegExp
////            | 's' -> return Matchers.createManyCharacterAtom state.info.IgnoreCase false Characters.whiteSpaceCharsRegExp
////            | 'S' -> return Matchers.createManyCharacterAtom state.info.IgnoreCase true Characters.whiteSpaceCharsRegExp
////            | 'w' -> return Matchers.createManyCharacterAtom state.info.IgnoreCase false Characters.wordCharsRegExp
////            | 'W' -> return Matchers.createManyCharacterAtom state.info.IgnoreCase true Characters.wordCharsRegExp
////        }) state
////
////    and private parseCharacterClass state =         
////        (parse {
////            do! skipChar '['
////            let! invert = opt (skipChar '^')
////            let! chars = parseClassRanges
////            do! skipChar ']'
////            return chars, invert.IsSome 
////        }) state
////
////    and private parseClassRanges state = 
////        (parse {
////            let! r = opt parseNonemptyClassRanges
////            match r with
////            | Some r -> return r
////            | None -> return ""
////        }) state
////
////    and private parseNonemptyClassRanges state = 
////        (parse {
////            let! a = parseClassAtom
////            let! dash = opt (pchar '-')
////            match dash with
////            | Some _ ->
////                let! b = parseClassAtom
////                let! c = parseClassRanges
////                let d = characterRange a b
////                return c + d
////            | None ->
////                let! b = opt parseNonemptyClassRangesNoDash
////                match b with
////                | Some b -> return a + b                    
////                | None -> return a            
////        }) state
////
////    and private parseNonemptyClassRangesNoDash state = 
////        (parse {
////            let! a = opt parseClassAtomNoDash
////            match a with
////            | Some a ->
////                let! dash = opt (pchar '-')
////                match dash with
////                | Some _ ->
////                    let! b = parseClassAtom
////                    let! c = parseClassRanges
////                    let d = characterRange a b
////                    return c + d
////                | None ->
////                    let! b = opt parseNonemptyClassRangesNoDash
////                    match b with
////                    | Some b -> return a + b                    
////                    | None -> return a 
////            | None ->
////                let! a = parseClassAtom
////                return a         
////        }) state
////
////    and private parseClassAtom state =
////        (pstring "-" <|> parseClassAtomNoDash) state
////
////    and private parseClassAtomNoDash state = 
////        ((noneOf "\\]-" |>> string) <|> (skipChar '\\' >>. parseClassEscape)) state
////
////    and private parseClassEscape state =
////        let decimalEscape state =
////            (parse {
////                let! r = parseDecimalEscape
////                match r with
////                | 0.0 -> return "\u0000"
////                | n -> 
////                    let msg = "The decimal escape '\\{0}' is not allowed in a character class."
////                    let msg = String.Format (msg, n)
////                    let! _ = failFatally msg
////                    ()
////            }) state 
////        (decimalEscape <|> (pstring "b" |>> fun _ -> "\u0008") <|> parseCharacterEscape <|> parseCharacterClassEscape) state
//
//    let parse (info:RegExpInfo) : PatternMatcher =
//        let state = { info = info; totalCaptures = 0 }
//        let result = runParserOnString parsePattern state "RegExp" info.PatternText
//        match result with
//        | Success (result, state, position) -> result
//        | Failure (message, _, _) -> failwith message