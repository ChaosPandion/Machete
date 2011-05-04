namespace Machete.RegExp

open System
open System.Globalization

module Characters =

    let private whiteSpaceChars = [
        '\u0009'
        '\u000b'
        '\u000c'
        '\u0020'
        '\u00a0'
        '\u1680'
        '\u180e'
        '\u2000'
        '\u2001'
        '\u2002'
        '\u2003'
        '\u2004'
        '\u2005'
        '\u2006'
        '\u2007'
        '\u2008'
        '\u2009'
        '\u200a'
        '\u202f'
        '\u205f'
        '\u3000'
        '\ufeff'
    ]

    let private lineTerminatorChars = [
        '\u000a'
        '\u000d'
        '\u2028'
        '\u2029'
    ]

    let trimCharacters = whiteSpaceChars @ lineTerminatorChars |> List.toArray

    let isWhiteSpace c =  
        match c with
        | '\u0009'
        | '\u000b'
        | '\u000c'
        | '\u0020'
        | '\u00a0'
        | '\u1680'
        | '\u180e'
        | '\u2000'
        | '\u2001'
        | '\u2002'
        | '\u2003'
        | '\u2004'
        | '\u2005'
        | '\u2006'
        | '\u2007'
        | '\u2008'
        | '\u2009'
        | '\u200a'
        | '\u202f'
        | '\u205f'
        | '\u3000'
        | '\ufeff' -> true
        | _ -> false

    let isLineTerminator c = 
        match c with
        | '\u000a'
        | '\u000d'
        | '\u2028'
        | '\u2029' -> true
        | _ -> false

    let isUnicodeLetter c =
        match Char.GetUnicodeCategory(c) with
        | UnicodeCategory.UppercaseLetter
        | UnicodeCategory.LowercaseLetter 
        | UnicodeCategory.TitlecaseLetter 
        | UnicodeCategory.ModifierLetter
        | UnicodeCategory.OtherLetter
        | UnicodeCategory.LetterNumber -> true
        | _ -> false

    let isUnicodeCombiningMark c =
        match Char.GetUnicodeCategory c with
        | UnicodeCategory.NonSpacingMark
        | UnicodeCategory.SpacingCombiningMark -> true
        | _ -> false  

    let isUnicodeDigit c =
        match Char.GetUnicodeCategory c with
        | UnicodeCategory.DecimalDigitNumber -> true
        | _ -> false 

    let isUnicodeConnectorPunctuation c =
        match Char.GetUnicodeCategory c with
        | UnicodeCategory.ConnectorPunctuation -> true
        | _ -> false 

    let isNotLineTerminator c = 
        not (isLineTerminator c)

    let isDecimalDigit c = 
        match c with
        | '0' | '1' | '2'
        | '3' | '4' | '5'
        | '6' | '7' | '8'
        | '9' -> true
        | _ -> false

    let isNonDecimalDigit c = 
        not (isDecimalDigit c)

    let isNonZeroDigit c =  
        match c with
        | '1' | '2' | '3' 
        | '4' | '5' | '6' 
        | '7' | '8' | '9' -> true
        | _ -> false

    let isHexDigit c = 
        match c with
        | '0' | '1' | '2' | '3' 
        | '4' | '5' | '6' 
        | '7' | '8' | '9' 
        | 'a' | 'b' | 'c'
        | 'd' | 'e' | 'f'
        | 'A' | 'B' | 'C'
        | 'D' | 'E' | 'F' -> true
        | _ -> false   

    let keyWordSet =
        set [|
            "break"; 
            "case"; "catch"; "continue";
            "debugger"; "default"; "delete"; "do";  
            "else"; 
            "finally"; "for"; "function"; 
            "if"; "in"; "instanceof";
            "new";
            "return";
            "switch";
            "this"; "throw"; "try"; "typeof";
            "var"; "void";
            "while"; "with";
        |]

    let futureReservedWordSet = 
        set [|
            "class"; "const"; 
            "enum"; "export"; "extends";
            "implements"; "import"; "interface";
            "let";
            "package"; "private"; "protected"; "public";
            "static"; "super";
            "yield";
        |]

    let booleanLiteralSet = 
        set [|
            "true"; "false"
        |]

    let nullLiteralSet = 
        set [|
            "null"
        |]

    let reservedWordSet =
        Set.union (Set.union keyWordSet futureReservedWordSet) (Set.union booleanLiteralSet nullLiteralSet)


    
    let wordCharSet = 
        set "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_"

    let decimalDigitCharsRegExp =
        "0123456789"

    let nonDecimalDigitCharsRegExp =
        String([| Char.MinValue..Char.MaxValue |] |> Array.filter (fun c -> not(isDecimalDigit c)))

    let wordCharsRegExp = 
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_"

    let nonWordCharsRegExp = 
        String([| Char.MinValue..Char.MaxValue |] |> Array.filter (fun c -> not(wordCharSet.Contains c)))

    let whiteSpaceCharsRegExp = 
        String (whiteSpaceChars @ lineTerminatorChars |> List.toArray)

    let nonWhiteSpaceCharsRegExp = 
        String ([| Char.MinValue..Char.MaxValue |] |> Array.filter (fun c -> not(isWhiteSpace c) && not(isLineTerminator c)))
        