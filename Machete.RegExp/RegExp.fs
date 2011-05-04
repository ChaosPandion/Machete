namespace Machete.RegExp

type RegExp (info:RegExpInfo) =
    let matcher = Parser.parse info

    new (patternText:string, flags:RegExpFlags) = 
        RegExp (RegExpInfo (patternText, flags))
    new (patternText:string, flags:string) = 
        RegExp (RegExpInfo (patternText, flags))

    member x.Info = info
    member x.Match (input:string, index:int) = matcher input index
