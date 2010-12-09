namespace Machete.Compiler

module Program =
    open FParsec.CharParsers
    open FParsec.Primitives

    let main () =
        let str = "2 + 2;3+3;"
        let r = Parser.parse str
        let r = Parser.parse str
        let r = Parser.parse str
        printfn "%s" (r.ToString())
        ()
    main()