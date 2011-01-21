namespace Machete.Compiler

open System
open FParsec.CharParsers
open FParsec.Primitives

module DateParser =
      
    let completeDigit (index:int) (c:char) = 
        double c * (10.0 ** (double index + 1.0)) |> int
          
    let completeNumber (cs:char[]) = 
        cs |> Array.rev |> Array.mapi completeDigit |> Array.reduce (+)

    let parseYear state =
        (parray 4 digit |>> completeNumber) state

    let parseMonth state =
        (parray 2 digit |>> completeNumber) state
        
    let parseDay state =
        (parray 2 digit |>> completeNumber) state

    let parseDate state = 
        (parse {
            let! year = parseYear
            do! skipChar '-'
            let! month = parseMonth
            do! skipChar '-'
            let! day = parseDay
            return ()
        }) state

