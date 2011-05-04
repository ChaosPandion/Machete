namespace Machete.RegExp

type PatternMatcher = string -> int -> MatchResult

type Matcher = MatchState -> MatchResult