namespace Machete.Interactive

module Program =

    open System.Diagnostics
    open Machete.Runtime
    open Machete.Compiler


    let main () =
//        let environment = new Environment()
//        let compiler = new CompilerService(environment)
//        //let sw = Stopwatch.StartNew()
//        let r = compiler.CompileGlobalCode ("2 + 2")
//        //System.Console.Write (sw.Elapsed)
//        let v = environment.Execute (r)
//        System.Console.Write (v.ToString())
//        System.Console.ReadKey(true) |> ignore
        Interactive.initialize ()

    main()