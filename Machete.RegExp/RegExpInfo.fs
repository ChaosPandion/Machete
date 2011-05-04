namespace Machete.RegExp

type RegExpInfo (patternText:string, flags:RegExpFlags) =

    do
        if patternText = null then 
            nullArg "patternText"
    
    new (patternText:string, flags:string) = 
        if flags = null then 
            nullArg "flags"
        let flags = 
            match flags.Length with
            | 0 -> 
                RegExpFlags.None
            | l when l > 3 -> 
                invalidArg "flags" "Length cannot be greater than 3."
            | _ ->
                if flags.Contains("g") then RegExpFlags.Global else RegExpFlags.None 
                ||| if flags.Contains("i") then RegExpFlags.IgnoreCase else RegExpFlags.None 
                ||| if flags.Contains("m") then RegExpFlags.Multiline else RegExpFlags.None
        RegExpInfo (patternText, flags)

    member x.PatternText = patternText
    member x.Flags = flags
    member x.Global = flags ||| RegExpFlags.Global = RegExpFlags.Global
    member x.IgnoreCase = flags ||| RegExpFlags.IgnoreCase = RegExpFlags.IgnoreCase
    member x.Multiline = flags ||| RegExpFlags.Multiline = RegExpFlags.Multiline
