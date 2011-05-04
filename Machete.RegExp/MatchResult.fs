namespace Machete.RegExp

type MatchResult (success:bool, matchState:MatchState) =
    member x.Success = success
    member x.MatchState = matchState
