namespace Machete.Interactive

module CommandParser =

    open FParsec
    open FParsec.CharParsers
    open FParsec.Primitives

    let parseSetTimeout : Parser<Command, unit> =
        spaces 
        .>> skipChar '#' 
        .>> spaces
        .>> skipString "set-timeout" 
        .>> spaces
        >>. numberLiteral NumberLiteralOptions.DefaultInteger "The 'setTimeout' command requires one integer argument."
        |>> fun n -> SetTimeout (int n.String)

