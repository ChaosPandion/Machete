namespace Machete.RegExp

type MatchState = {
    RegExpInfo:RegExpInfo
    Captures:Captures
    Input:string
    StartIndex:int
    EndIndex:int
} with 
    member x.StartOfInput = 
        x.EndIndex = 0 
    member x.EndOfInput = 
        x.EndIndex = x.Input.Length
    member x.Current =
        x.Input.[x.EndIndex]
    member x.Next =
        { x with EndIndex = x.EndIndex + 1 }
    member x.NextIf condition =
        if condition 
        then { x with EndIndex = x.EndIndex + 1 }
        else x
