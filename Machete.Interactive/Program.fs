namespace Machete.Interactive

module Program =

    open System 
    open System.Text.RegularExpressions
    open FParsec.CharParsers
    
    
    let getForegroundColor () = Console.ForegroundColor
    let setForegroundColor value () = Console.ForegroundColor <- value
        
    let getCursorTop () = Console.CursorTop  
    let setCursorTop value () = Console.CursorTop <- value     
    let getCursorLeft () = Console.CursorLeft     
    let setCursorLeft value = Console.CursorLeft <- value

    let readChar () = Console.Read () |> char         
    let readKey () = Console.ReadKey true

    let write (value:string) = Console.Write value

    let writeColored (value:string) (color:ConsoleColor) =
        let old = Console.ForegroundColor
        Console.ForegroundColor <- color 
        Console.Write value
        Console.ForegroundColor <- old

    let writeLine (value:string) = Console.WriteLine value

    let writeLineColored (value:string) (color:ConsoleColor) =
        let old = Console.ForegroundColor
        Console.ForegroundColor <- color 
        Console.WriteLine value
        Console.ForegroundColor <- old
   
    let private engine = lazy(new Machete.Engine())
    let private writeChar (c:char) = Console.Write c
    let private writeString (str:string) = Console.Write str
    
    let private commandStart = new Regex ("^\\s*#", RegexOptions.Compiled)
    let private startingWhiteSpace = new Regex ("^\\s+", RegexOptions.Compiled)
    let private endingWhiteSpace = new Regex ("\\s+$", RegexOptions.Compiled)
    
    let newLine = "\n"
    let lineStart = ">>> "
    let tab = "    "
    let message = "Machete Interactive 1.0.0.0"
    let bsMap = Map.ofList [for i in 0..100 -> i, String.replicate i "\u0008 \u0008"]

    let writeStart () =
        writeColored lineStart ConsoleColor.DarkRed

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
        | _, ConsoleKey.LeftArrow ->
            if getCursorLeft () > 4 then
                setCursorLeft (getCursorLeft () - 1) 
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
        writeStart ()
        writeColored message ConsoleColor.Blue
        writeNewLineStart ()
        writeNewLineStart ()
        let sw = System.Diagnostics.Stopwatch()
        while true do
            let text = read [] ""
            if text.Length > 0 then
                    try
                        if not (commandStart.IsMatch text) then
                            writeNewLineStart ()
                            sw.Restart()
                            writeColored (engine.Value.ExecuteScript (text, 60000) |> string) ConsoleColor.DarkCyan
                            writeNewLineStart ()
                            writeColored ("Execution Time: " + sw.Elapsed.ToString()) ConsoleColor.DarkGray
                            writeNewLineStart ()
                        else
                            let r = run CommandParser.parse text
                            match r with
                            | Success (v, s, p) ->
                                match v with
                                | GetTimeout ->
                                    writeNewLineStart ()
                                    writeColored ("The current timeout is " + (2000).ToString() + ".") ConsoleColor.DarkCyan
                                    writeNewLineStart ()  
                                | SetTimeout timeout ->
                                    writeNewLineStart ()
                                    writeColored ("The timeout value has been changed to " + timeout.ToString() + ".") ConsoleColor.DarkCyan 
                                    writeNewLineStart ()                       
                            | Failure (m, e, s) ->
                                writeNewLineStart ()
                                writeColored ("Error:") ConsoleColor.Red 
                                writeStrings m 
                    with | e ->
                        writeNewLineStart ()
                        writeColored ("Error:") ConsoleColor.Red  
                        writeStrings (e.Message)
                        writeNewLineStart () 
                    ()
            ()
    
    open System.Reflection

    let main () =
        repl ()

    main()