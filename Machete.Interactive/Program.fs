namespace Machete.Interactive

open System
open FParsec
open FParsec.Primitives
open FParsec.CharParsers
open Machete.Parser.InputElementParsers

module Program =

    let parseJQuery () =
        let url = "http://code.jquery.com/jquery-1.4.4.js"
        let text = (new System.Net.WebClient()).DownloadString(url)
        let engine = new Machete.Engine()
        let r = engine.ExecuteScript text
        System.Console.WriteLine (r)
 

    open System.Text.RegularExpressions

    let thd t =
        let a, b, c, d = t
        c

    let main () =
        Interactive.initialize()
//        let pattern = "a"
//        let e = new Machete.Runtime.Environment()
//        let macheteReg = Machete.Compiler.RegExpParser.Parse (e, pattern, "")
//        let bclReg = new Regex(pattern, RegexOptions.Compiled ||| RegexOptions.ECMAScript)
//        let text = "a";
//        let macheteResult = macheteReg.Invoke(text, 0)
//        let macheteResult = macheteReg.Invoke(text, 0)
//        let macheteResult = macheteReg.Invoke(text, 0)
//        let bclResult = bclReg.Match(text)
//        let count = 100000
//        let sw = System.Diagnostics.Stopwatch.StartNew()
//
//        for i in 0..count do
//            macheteReg.Invoke(text, 0) |> ignore
//        Console.WriteLine("Machete = {0}", TimeSpan.FromTicks(sw.ElapsedTicks / (count|>int64)))
//        
//        sw.Restart()
//
//        for i in 0..count do
//            bclReg.Match(text) |> ignore
//        Console.WriteLine("BCL = {0}", TimeSpan.FromTicks(sw.ElapsedTicks / (count|>int64)))
//
//        System.Console.ReadLine() |> ignore


    main()