namespace Machete.RegExp

type RegExpTree =
| TreeNode
| Pattern of int * RegExpTree
| Disjunction of RegExpTree * RegExpTree option
| Alternative of RegExpTree * RegExpTree option
| Term of Term

and Term =
| Assertion of Assertion
| Atom of Atom * Quantifier option

and Assertion =
| StartOfInput
| EndOfInput
| WordBoundary
| NonWordBoundary
| FollowedBy of RegExpTree 
| NotFollowedBy of RegExpTree 

and Quantifier = {
    min:int
    max:Limit
    greedy:bool
}

and Atom =
| Character of char
| Characters of char seq
| WildCard
| CapturingGroup of int * RegExpTree 
| NonCapturingGroup of RegExpTree 
