namespace Machete.Interactive

module CommandParser =

    open FParsec
    open FParsec.CharParsers
    open FParsec.Primitives
    

    let private parseInt message =
        numberLiteral NumberLiteralOptions.DefaultInteger message 
        |>> fun n -> int n.String

    let private parseString message =
        between (pchar '\"') (pchar '\"') (manyChars (noneOf "\"")) <?> message

    let private parseGetTimeout : Parser<Command, unit> =
        skipString "get-timeout" .>> spaces 
        |>> fun () -> GetTimeout

    let private parseSetTimeout : Parser<Command, unit> =
        skipString "set-timeout" .>> spaces
        >>. parseInt "The 'set-timeout' command requires one integer argument."
        |>> SetTimeout

    let private parseEcho : Parser<Command, unit> =
//        do! skipString "echo"
//    do! spaces
//    let! msg = parseString "The 'echo' command requires one string argument."
//    return Echo msg
        skipString "echo" .>> spaces
        >>. parseString "The echo command requires one quoted string argument."
        |>> Echo

    let parse : Parser<Command, unit> =
        spaces .>> skipChar '#' .>> spaces >>. (
            parseGetTimeout <|> parseSetTimeout <|> parseEcho
        )     

