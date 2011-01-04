namespace Machete.Interactive

open System
open System.Text
open System.Text.RegularExpressions
open FParsec.CharParsers

module Interactive =

    type private Text = {
        value : string
        color : ConsoleColor
        lineLength : int   
    }

    type private State = {
        history:list<list<Text>>
        historyUp:list<list<Text>> 
        historyDown:list<list<Text>>
        currentLeft:list<Text>
        currentRight:list<Text>
    }
    
    let private engine = new Machete.Engine()
    let private newLine = "\n"  
    let private message = "Machete Interactive 1.0.0.0\n\n"
    let private tab = "    "
    let private messageColor = ConsoleColor.Cyan
    let private defaultColor = ConsoleColor.Gray
    let private outputColor = ConsoleColor.DarkCyan
    let private statusColor = ConsoleColor.DarkGray
    let private errorColor = ConsoleColor.Red
          
    let rec private read () =
        let sb = StringBuilder()
        let run = ref true
        while !run do
            let line = Console.ReadLine()
            if line.EndsWith "@" then
                run := false
            let line = line.TrimEnd ([| '@' |])
            sb.AppendLine line |> ignore
        sb.ToString()

    let private isExn (o:obj) =
        match o with
        | :? Exception -> true
        | _ -> false

    let private loop () = 
        while true do
            try        
                let text =  read ()
                let r = engine.ExecuteScript text
                let result, color = if isExn r then (r:?>exn).Message, errorColor else r |> string, outputColor
                System.Threading.Thread.Sleep (250)
                ConsoleAgent.writeLine (result + "\n") color
            with 
            | e ->
                ConsoleAgent.writeLine e.Message errorColor 
        

    let initialize () =
        engine.RegisterOutputHandler (fun s -> ConsoleAgent.writeLine s outputColor)
        ConsoleAgent.write message messageColor
        loop ()