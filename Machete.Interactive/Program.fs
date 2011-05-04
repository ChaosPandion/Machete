namespace Machete.Interactive

open System
open FParsec
open FParsec.Primitives
open FParsec.CharParsers
open Machete.Parser.InputElementParsers
open Machete.RegExp

module Program =

    let main () =
        Interactive.initialize()


    main()