namespace Machete.Interactive

open System
open System.Diagnostics
open System.Text
open System.Text.RegularExpressions
open FParsec.CharParsers

module Interactive =
    
    let private engine = new Machete.Engine()
    let private messageColor = ConsoleColor.Cyan
    let private defaultColor = ConsoleColor.Gray
    let private outputColor = ConsoleColor.DarkCyan
    let private statusColor = ConsoleColor.DarkGray
    let private errorColor = ConsoleColor.Red
    
    let write text color =
        lock 
            engine 
            (fun () ->
                let oldColor = Console.ForegroundColor
                Console.ForegroundColor <- color
                Console.Write (text:string)
                Console.ForegroundColor <- oldColor)
        

    let writeLine text color =
        lock 
            engine
            (fun () ->
                let oldColor = Console.ForegroundColor
                Console.ForegroundColor <- color
                Console.WriteLine (text:string)
                Console.ForegroundColor <- oldColor)        
          
    let rec private read () =
        let sb = StringBuilder()
        let run = ref true
        while !run do
            let line = Console.ReadLine ()
            if line.EndsWith ";;" then
                run := false
                let line = line.Substring(0, line.Length - 2)
                sb.AppendLine line |> ignore
            else
                sb.AppendLine line |> ignore
        sb.ToString().Trim()

    let private isExn (o:obj) =
        match o with
        | :? Exception -> true
        | _ -> false

    let private loop () = 
        while true do
            try        
                let text =  read ()
                let sw = Stopwatch.StartNew()
                let r = engine.ExecuteScript text
                sw.Stop ()
                let result, color = 
                    match r with
                    | :? Exception as r -> r.Message, errorColor
                    | _ -> r |> string, outputColor
                System.Threading.Thread.Sleep 100
                writeLine (result) color
                writeLine (sw.Elapsed.ToString() + "\n") statusColor
            with 
            | e ->
                writeLine e.Message errorColor 
        

    let initialize () =
        Console.OutputEncoding <- System.Text.Encoding.UTF8
        let message = "Machete Interactive " + AssemblyInfo.Version + "\n"
        engine.RegisterOutputHandler (fun s -> writeLine s outputColor)
        write ("Machete " + Machete.Core.AssemblyInfo.Version + "\n") messageColor
        write message messageColor
        write (Machete.Core.AssemblyInfo.Copyright + "\n\n") messageColor
        Console.Title <- message
        loop ()