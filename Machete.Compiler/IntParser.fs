namespace Machete.Compiler

open System

module IntParser =

    let inline private isStrWhiteSpace c = 
        CharSets.isWhiteSpace c || CharSets.isLineTerminator c

    let rec private passLeadingWhiteSpaceAndSign (text:string) (index:int) =
        if index > text.Length then -1, 1 
        elif not (isStrWhiteSpace (text.[index])) then 
            if text.[index] = '-' then index + 1, -1
            else index, 1   
        else passLeadingWhiteSpaceAndSign text (index + 1)
        
    let private hasHexPrefix (text:string) =
        text.StartsWith "0x" || text.StartsWith "0X"

    let private getActualRadixAndStripPrefix (text:string) (radix:int) = 
        if radix = 0 then
            if hasHexPrefix text 
            then 16, true
            else 10, false
        else radix, radix = 16 && hasHexPrefix text

    let inline private isRadixChar (radix:int) (c:char) =
        if c >= '0' && c <= '9' then int c - 48 < radix
        elif c >= 'A' && c <= 'Z' then int c - 55 < radix
        elif c >= 'a' && c <= 'z' then int c - 87 < radix
        else false
        
    let private getRadixValue (c:char) =
        if c >= '0' && c <= '9' then double c - 48.0
        elif c >= 'A' && c <= 'Z' then double c - 55.0
        else double c - 87.0

    let rec private calculate (radix:double) (chars:list<char>) (index:double) (result:double) =
        if chars.IsEmpty then result else 
        let result = (result + ((getRadixValue chars.Head) * (radix ** index)))
        calculate radix chars.Tail (index + 1.0) result 
        
    let Parse (text:string) (radix:int) =
        let startIndex, sign = passLeadingWhiteSpaceAndSign text 0
        let text = if startIndex > 0 then text.Substring startIndex else text     
        let actualRadix, stripPrefix = getActualRadixAndStripPrefix text radix  
        let text = if stripPrefix then text.Substring 2 else text 
        let chars = text |> Seq.takeWhile (isRadixChar actualRadix) |> Seq.toList |> List.rev
        if chars.IsEmpty 
        then Double.NaN
        else calculate (actualRadix |> double) chars 0.0 0.0