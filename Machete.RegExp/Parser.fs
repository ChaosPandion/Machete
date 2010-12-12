namespace Machete.RegExp

module internal Parser =

    open Machete.RegExp
    open FParsec.Primitives
    open FParsec.CharParsers
    open Machete.Compiler.Parsers
    open Machete.Compiler.Lexer.NumericLiteralParser
    open Machete.Compiler.Lexer.StringLiteralParser
    open Machete.Compiler.Lexer.IdentifierNameParser


    type State = {
        groupIndex:int
    }

    let nil =
        preturn Nil

    let backSpaceEscape = parse {
        let! _ = pchar 'b'
        return ClassEscape BackSpaceEscape
    }
    
    let rec classEscape () =
        (decimalEscape() <|> backSpaceEscape <|> characterEscape() <|> characterClassEscape()) |>> ClassEscape

    and classAtomNoDash () = 
        (noneOf "\\]-" |>> Char |>> ClassAtomNoDash) <|> (pipe2 (pchar '\\') (classEscape()) (fun a b -> ClassAtomNoDash b))

    and classAtom () = 
        ((pchar '-' |>> Char) <|> classAtomNoDash()) |>> ClassAtom

    and classRange() =
        parse {
            let! a = classAtom()
            do! skipChar '-'
            let! b = classAtom()
            return a, b
        }

    and nonemptyClassRangesNoDash () =
        attempt (parse {
            let! a, b = parse {
                let! a = classAtomNoDash()
                do! skipChar '-'
                let! b = classAtom()
                return a, b
            }
            let! c = classRanges()
            return NonemptyClassRanges (a, b, c)        
        }) <|> attempt (parse {
            let! a = classAtomNoDash()
            let! b = nonemptyClassRangesNoDash()
            return NonemptyClassRanges (a, b, Nil)  
        }) <|> attempt (parse {
            let! a = classAtom()
            return NonemptyClassRanges (a, Nil, Nil)  
        })

    and nonemptyClassRanges () =
        attempt (parse {
            let! a, b = classRange()
            let! c = classRanges()
            return NonemptyClassRanges (a, b, c)        
        }) <|> attempt (parse {
            let! a = classAtom()
            let! b = nonemptyClassRangesNoDash()
            return NonemptyClassRanges (a, b, Nil)  
        }) <|> attempt (parse {
            let! a = classAtom()
            return NonemptyClassRanges (a, Nil, Nil)  
        })

    and classRanges () = 
        parse {
            let! a = nonemptyClassRanges() <|> nil
            return ClassRanges(a)
        }

    and characterClass () = 
        parse {
            do! skipChar '['
            let! invert = opt (pchar '^')
            let! ranges = classRanges() 
            do! skipChar ']'
            return CharacterClass (invert, ranges)
        }

    and characterClassEscape () = 
        anyOf "dDsSwW" |>> CharacterClassEscape

    and decimalEscape () = 
        parse {
            let! a = decimalIntegerLiteral |>> InputElement
            do! notFollowedBy decimalDigit
            return DecimalEscape a
        }

    and identityEscape () =
        parse {
            let! c = pchar '\u200C'
            return IdentityEscape c
        } <|> parse {
            let! c = pchar '\u200D'
            return IdentityEscape c
        } <|> parse {
            do! notFollowedBy identifierPart
            let! c = anyChar
            return IdentityEscape c   
        }

    and controlLetter () = 
        satisfy CharSets.controlLetterCharSet.Contains |>> ControlLetter

    and controlEscape () = 
        anyOf "fnrtv" |>> ControlEscape

    and characterEscape () =  
        parse {
            let! a = controlEscape() <|> characterEscape2() <|> (hexEscapeSequence |>> InputElement) <|> (unicodeEscapeSequence |>> InputElement) <|> identityEscape()
            return CharacterEscape a
        }

    and characterEscape2 () = 
        parse {
            let! _ = pchar 'c'
            let! a = controlLetter()
            return CharacterEscape a   
        }

    and atomEscape () = 
        (decimalEscape() <|> characterEscape() <|> characterClassEscape()) |>> AtomEscape

    and patternCharacter () = 
        noneOf "^$\\.*+?()[]{}|" |>> PatternCharacter

    and atom () = 
        parse {
            let! a = patternCharacter()
            return Atom a          
        } <|> parse {
            let! _ = pchar '.'
            return Atom Dot           
        } <|> parse {
            let! _ = pchar '\\'
            let! a = atomEscape()
            return Atom a              
        } <|> parse {
            let! a = characterClass()
            return Atom a             
        } <|> (attempt <| parse {           
            let! a = between (pchar '(') (pchar ')') (disjunction())
            let! s = getUserState
            do! setUserState({ s with groupIndex = s.groupIndex + 1})
            return Atom (CapturingGroup(s.groupIndex, a))               
        }) <|> (attempt <| parse {            
            let! a = between (pstring "(?:") (pchar ')') (disjunction())
            return Atom (NonCapturingGroup a)           
        }) <|> (attempt <| parse {
            let! a = between (pstring "(?=") (pchar ')') (disjunction())
            return Atom (FollowedBy a)         
        }) <|> (attempt <| parse {
            let! a = between (pstring "(?!") (pchar ')') (disjunction())
            return Atom (NotFollowedBy a)         
        })

    and rangeBody() = 
        parse {
            let! a = decimalDigits |>> InputElement
            do! optional (pchar ',')
            let! b = (decimalDigits |>> InputElement <|> nil)
            return [a; b]
        }

    and quantifierPrefix () = 
        parse {
            let! _ = pchar '*'
            return QuantifierPrefix ZeroOrMore
        } <|> parse {
            let! _ = pchar '+'
            return QuantifierPrefix OneOrMore
        } <|> parse {
            let! _ = pchar '?'
            return QuantifierPrefix ZeroOrOne
        } <|>  parse {            
            let! a = between (pchar '{') (pchar '}') (rangeBody())
            let b = a.Head
            let a = a.Tail            
            return QuantifierPrefix (Range(b, (if a.IsEmpty then Nil else a.Head)))
        } 

    and quantifier () = 
        pipe2 (quantifierPrefix()) (opt (pchar '?')) (fun a b -> Quantifier(a, b.IsSome))

    and assertion () = 
        let a = pchar '^' >>. preturn StartOfInput
        let b = pchar '$' >>. preturn EndOfInput
        let c = pstring "\\b" >>. preturn WordBoundary
        let d = pstring "\\B" >>. preturn NonWordBoundary
        (a <|> b <|> c <|> d) |>> Assertion 

    and term () = 
        parse {
            let! s1 = getUserState
            let! a = assertion()
            return Term(a, Nil, s1.groupIndex,  0)
        } <|> parse {
            let! s1 = getUserState
            let! a = atom()
            let! s2 = getUserState
            let parenCount = s2.groupIndex - s1.groupIndex
            let! b = quantifier() <|> nil
            return Term(a, b, s1.groupIndex, parenCount)
        }

    and alternative () = 
        many1Fold (Alternative (Nil, Nil)) (fun a b -> Alternative(a, b)) (term()) 

    and disjunction () = 
        sepBy1Fold Nil (fun a b -> Disjunction(a, b)) (alternative()) (pchar '|' |>> Char)

    and pattern () = 
        parse {
            let! a = disjunction()
            let! s = getUserState
            return Pattern(s.groupIndex - 1, a)        
        }

    let parse source =
        assert(source <> null)        
        match runParserOnString (pattern()) ({ groupIndex = 1 }) "RegExp" source with
        | Success (a, b, c) -> a
        | Failure (a, b, c) -> failwith a
        

