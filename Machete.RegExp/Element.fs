namespace Machete.RegExp
    
type internal Element =
| Nil
| Char of char
| InputElement of Machete.Compiler.InputElement
| Pattern of int * Element
| Disjunction of Element * Element
| Alternative of Element * Element
| Term of Element * Element * int * int
| Assertion of Element
| StartOfInput
| EndOfInput
| WordBoundary
| NonWordBoundary
| FollowedBy of Element
| NotFollowedBy of Element
| Quantifier of Element * bool
| QuantifierPrefix of Element
| ZeroOrMore
| OneOrMore
| ZeroOrOne
| Range of Element * Element
| Atom of Element
| PatternCharacter of char
| Dot
| CapturingGroup of int * Element
| NonCapturingGroup of Element
| AtomEscape of Element
| CharacterEscape of Element
| ControlEscape of char
| ControlLetter of char
| IdentityEscape of char
| DecimalEscape of Element
| CharacterClassEscape of char
| CharacterClass of option<char> * Element
| ClassRanges of Element
| NonemptyClassRanges of Element * Element * Element
| NonemptyClassRangesNoDash of Element * Element * Element
| ClassAtom of Element
| ClassAtomNoDash of Element
| ClassEscape of Element
| BackSpaceEscape

and internal CharacterClassEscape =
| Digit
| NonDigit
| WhiteSpace
| NonWhiteSpace
| Word
| NonWord