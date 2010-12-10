namespace Machete.Interactive

module Program =

    open System 
    open System.Text.RegularExpressions
    open FParsec.CharParsers
   
    let private engine = lazy(new Machete.Engine())
    let private readKey () = Console.ReadKey true
    let private writeChar (c:char) = Console.Write c
    let private writeString (str:string) = Console.Write str
    
    let private commandStart = new Regex ("^\\s*#", RegexOptions.Compiled)
    let private startingWhiteSpace = new Regex ("^\\s+", RegexOptions.Compiled)
    let private endingWhiteSpace = new Regex ("\\s+$", RegexOptions.Compiled)
    
    let newLine = "\n"
    let lineStart = ">>> "
    let tab = "    "
    let message = "Machete Interactive 1.0"
    let bsMap = Map.ofList [for i in 0..100 -> i, String.replicate i "\u0008 \u0008"]

    let writeStart () =
        Console.ForegroundColor <- ConsoleColor.DarkRed
        Console.Write lineStart
        Console.ForegroundColor <- ConsoleColor.Gray

    let writeNewLineStart () =
        writeString newLine
        writeStart ()
    
    let writeStringLine value =
        writeStart ()
        writeString value  

    let writeStringNewLine value =
        writeNewLineStart ()
        writeString value
          
    let writeStrings (value:string) =
        for line in value.Split '\n' do
            writeNewLineStart () 
            writeString line      
    
    let rec read (lines:list<string>) (line:string) =
        let k = readKey ()
        match k.Modifiers, k.Key with
        | ConsoleModifiers.Control, ConsoleKey.Enter -> 
            let lines = line::lines |> List.rev 
            String.Join (newLine, lines)
        | _, ConsoleKey.Backspace when line.Length > 0 -> 
            writeString (bsMap.[1]) 
            read lines (line.[0..line.Length - 2]) 
        | _, ConsoleKey.Backspace when not lines.IsEmpty ->
            writeString (bsMap.[5]) 
            Console.CursorTop <- Console.CursorTop - 1
            Console.CursorLeft <- lines.Head.Length + 4
            read lines.Tail lines.Head 
        | _, ConsoleKey.Backspace ->
            read lines line     
        | ConsoleModifiers.Shift, ConsoleKey.Tab when line.Length > 0 ->
            let r = (endingWhiteSpace.Match line).Value
            writeString (bsMap.[r.Length])
            read lines (line.[0..line.Length - r.Length - 1])        
        | ConsoleModifiers.Shift, ConsoleKey.Tab ->
            read lines line  
        | _, ConsoleKey.Tab ->
            writeString tab
            read lines (line + tab)                   
        | _, ConsoleKey.Enter ->
            writeNewLineStart ()
            let r = (startingWhiteSpace.Match line).Value
            if r.Length > 0 then writeString r 
            read (line::lines) r
        | _ -> 
            writeChar k.KeyChar
            read lines (line + k.KeyChar.ToString()) 

    let repl () =                  
        writeStringLine message
        writeNewLineStart ()
        writeNewLineStart ()
        while true do
            let text = read [] ""
            if text.Length > 0 then
                if not (commandStart.IsMatch text) then
                    Console.WriteLine (engine.Value.ExecuteScript text)
                else
                    let r = run CommandParser.parseSetTimeout text
                    match r with
                    | Success (v, s, p) ->
                        match v with
                        | SetTimeout timeout ->
                            writeStringLine ("The timeout value has been changed to " + timeout.ToString() + ".")  
                            writeNewLineStart ()                        
                    | Failure (m, e, s) ->
                        writeStrings m    
                    ()
            ()
    

    let main () =
        repl ()

    main()