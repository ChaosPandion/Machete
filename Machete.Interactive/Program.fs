namespace Machete.Interactive

open System
open FParsec
open FParsec.Primitives
open FParsec.CharParsers

module Program =

    let parseJQuery () =
        let url = "http://code.jquery.com/jquery-1.4.4.js"
        let text = (new System.Net.WebClient()).DownloadString(url)
        let engine = new Machete.Engine()
        let r = engine.ExecuteScript text
        System.Console.WriteLine (r)

    let main () =

        let x = run Machete.Parser.InputElementParsers.parseNumericLiteral ".1e1"
        match x with
        | Success (a, b, c) ->
            ()
        | Failure (a, b, c) ->
            ()
        System.Console.ReadLine() |> ignore

        //Interactive.initialize ()

    main()