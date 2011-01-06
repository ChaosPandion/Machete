namespace Machete.Interactive

module Program =

    open System.Diagnostics
    open Machete.Runtime
    open Machete.Compiler


    let main () =
        let environment = new Environment()
        let parser = new Parser2(environment)
        let sw = Stopwatch.StartNew()
        let r = parser.Parse ("'use strict'; 3 + 3; 3 + 3; 3 + 3; 3 + 3; 3 + 3; 3 + 3; 3 + 3; 3 + 3;")
        System.Console.Write (sw.Elapsed)
        let v = r.Invoke(environment, environment.EmptyArgs)
        System.Console.Write (v.ToString())
        System.Console.ReadKey(true) |> ignore
        //Interactive.initialize ()

    main()