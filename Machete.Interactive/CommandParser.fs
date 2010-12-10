﻿namespace Machete.Interactive

module CommandParser =

    open FParsec
    open FParsec.CharParsers
    open FParsec.Primitives


    let private parseInt message =
        numberLiteral NumberLiteralOptions.DefaultInteger message 
        |>> fun n -> int n.String

    let private parseGetTimeout : Parser<Command, unit> =
        skipString "get-timeout" .>> spaces 
        |>> fun () -> GetTimeout

    let private parseSetTimeout : Parser<Command, unit> =
        skipString "set-timeout" .>> spaces
        >>. parseInt "The 'set-timeout' command requires one integer argument."
        |>> SetTimeout

    let parse : Parser<Command, unit> =
        spaces .>> skipChar '#' .>> spaces >>. (
            parseGetTimeout <|> parseSetTimeout
        )     

