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
    let setCursorLeft value () = Console.CursorLeft <- value

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
        while true do
            let text = read [] ""
            if text.Length > 0 then
                if not (commandStart.IsMatch text) then
                    Console.WriteLine (engine.Value.ExecuteScript text)
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
                    ()
            ()
    
    open System.Reflection

    let main () =

        let f = "and eval{0} (state:State) =
match state.Element with
| Bitwise{0} (Nil, right) ->
    eval{0} (state.WithElement right)
| Bitwise{0} (left, right) ->
    let inst = eval{0} (state.WithElement left)
    let args = [| eval (state.WithElement right) |]
    call inst Reflection.IDynamic.op_ args"


        //repl ()
        let r =
            Reflection.FSharpType.GetUnionCases (typeof<Machete.Compiler.SourceElement>)
            |> Array.map (fun m -> String.Format(f, m.Name))
        let r = String.Join ("\r\n\r\n", r)
        System.IO.File.WriteAllText ("E:\\Temp\\result.txt", r)
        System.Diagnostics.Process.Start "E:\\Temp\\result.txt"
        |> ignore
//        let r = 
//            Assembly.GetAssembly(typeof<Machete.IDynamic>).GetTypes()
//            |> Array.toSeq
//            |> Seq.filter (fun t -> t.IsInterface)
//            |> Seq.map (
//                fun it ->
//                    let d = "module " + it.Name + " = \r\n    let t = typeof<" + it.FullName + ">\r\n"
//                    let r = 
//                        it.GetMethods()
//                        |> Array.map (fun m -> "    let " + (m.Name.[0] |> Char.ToLower |> string) + m.Name.Substring 1 + " = t.GetMethod \"" + m.Name + "\"")
//                    d + String.Join ("\r\n", r)
//            )       
//        let r = String.Join ("\r\n\r\n", r)
//        let r = 
//            typeof<Machete.Runtime.RuntimeTypes.Interfaces.IDynamic>.GetMethods()
//            |> Array.map (fun m -> "let " + (m.Name.[0] |> Char.ToLower |> string) + m.Name.Substring 1 + " = t.GetMethod \"" + m.Name + "\"")
//        let r = String.Join ("\r\n", r)
        System.IO.File.WriteAllText ("E:\\Temp\\result.txt", r)
        System.Diagnostics.Process.Start "E:\\Temp\\result.txt"
        |> ignore
        ()


    main()