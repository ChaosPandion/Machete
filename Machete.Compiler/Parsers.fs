namespace Machete.Compiler

module Parsers =

    open Lexer
    open FParsec.CharParsers
    open FParsec.Primitives

        
    
    let whiteSpace<'a> : Parser<InputElement, 'a> =
        satisfy CharSets.isLineTerminator |>> (fun c -> WhiteSpace)

    let lineTerminator<'a> : Parser<InputElement, 'a> =
        satisfy CharSets.isLineTerminator |>> (fun c -> LineTerminator)
    
    let lineTerminatorSequence<'a> : Parser<InputElement, 'a> =
        satisfy CharSets.isLineTerminator |>> (fun c -> LineTerminatorSequence)

    let booleanLiteral<'a> : Parser<InputElement, 'a> =
        (pstring "true" <|> pstring "false") |>> BooleanLiteral

    let evalBooleanLiteral v =
        match v with
        | BooleanLiteral "true" -> true
        | BooleanLiteral "false" -> false
        
    let nullLiteral<'a> : Parser<InputElement, 'a> =
        pstring "null" |>> NullLiteral

    let evalNullLiteral v =
        match v with
        | NullLiteral "null" -> null

    
