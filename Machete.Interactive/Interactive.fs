namespace Machete.Interactive

open System
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
    
    let private commandStartRegex = new Regex ("^\\s*#", RegexOptions.Compiled)
    let private sw = System.Diagnostics.Stopwatch()
    let private engine = lazy(new Machete.Engine())
    let private newLine = "\n"  
    let private message = { value = "Machete Interactive 1.0.0.0" + newLine; color = ConsoleColor.Blue; lineLength = 0 } 
    let private lineStartVal = "> " 
    let private lineStart = { value = newLine + lineStartVal; color = ConsoleColor.DarkRed; lineLength = 0 }
    let private backSpace = "\u0008 \u0008"
    let private tab = "    "
    let private defaultColor = ConsoleColor.Gray
    let private outputColor = ConsoleColor.DarkCyan
    let private statusColor = ConsoleColor.DarkGray
    let private errorColor = ConsoleColor.Red

    let private write (t:Text) =
        let oldColor = Console.ForegroundColor
        Console.ForegroundColor <- t.color
        Console.Write t.value
        Console.ForegroundColor <- oldColor
               
    let rec private delete (ts:list<Text>) =
        if not ts.IsEmpty then
            Console.Write (backSpace |> String.replicate ts.Head.value.Length)
            if ts.Head.value = newLine then
                Console.CursorTop <- Console.CursorTop - 1
                if not ts.Tail.IsEmpty then
                    Console.CursorLeft <- ts.Tail.Head.lineLength + (if Console.CursorLeft = 0 then lineStartVal.Length  else 0)

    let rec private deleteMany (ts:list<Text>) =
        if not ts.IsEmpty then
            Console.Write (backSpace |> String.replicate ts.Head.value.Length)
            if ts.Head.value = newLine then
                Console.CursorTop <- Console.CursorTop - 1
                if not ts.Tail.IsEmpty then
                    Console.CursorLeft <- ts.Tail.Head.lineLength + (if Console.CursorLeft = 0 then lineStartVal.Length  else 0) 
            deleteMany ts.Tail

    let rec private writeMany (ts:list<Text>) =
        if not ts.IsEmpty then
            write ts.Head
            writeMany ts.Tail

    let rec private read (state:State) =
        let c = Console.ReadKey (true)
        match c.Modifiers, c.Key with
        | _, ConsoleKey.F5 ->
            { state with history = ((state.currentRight |> List.rev) @ state.currentLeft)::state.history }
        | _, ConsoleKey.Backspace when not state.currentLeft.IsEmpty ->
            delete state.currentLeft
            read { state with currentLeft = state.currentLeft.Tail }
        | _, ConsoleKey.Backspace ->
            read state
        | _, ConsoleKey.Enter ->
            Console.Write newLine
            let text = { value = newLine; color = defaultColor; lineLength = 0 }
            let state = { state with currentLeft = text::state.currentLeft }
            read state
        | _, ConsoleKey.Tab ->
            Console.Write tab
            let text = { value = tab; color = defaultColor; lineLength = state.currentLeft.Head.lineLength + tab.Length }
            let state = { state with currentLeft = text::state.currentLeft }
            read state
        | _, ConsoleKey.UpArrow when not state.historyUp.IsEmpty ->
            deleteMany state.currentLeft
            writeMany (state.historyUp.Head |> List.rev)
            let state = { state with currentLeft = state.historyUp.Head; historyUp = state.historyUp.Tail; historyDown = state.currentLeft::state.historyDown }
            read state
        | _, ConsoleKey.UpArrow ->
            read state
        | _, ConsoleKey.DownArrow when not state.historyDown.IsEmpty ->
            deleteMany state.currentLeft
            writeMany (state.historyDown.Head |> List.rev)
            let state = { state with currentLeft = state.historyDown.Head; historyUp = state.currentLeft::state.historyUp; historyDown = state.historyDown.Tail }
            read state
        | _, ConsoleKey.DownArrow ->
            read state
        | _, ConsoleKey.LeftArrow when not state.currentLeft.IsEmpty -> 
            Console.CursorLeft <- Console.CursorLeft - state.currentLeft.Head.value.Length 
            let state = { state with currentLeft = state.currentLeft.Tail; currentRight = state.currentLeft.Head::state.currentRight }
            read state
        | _, ConsoleKey.RightArrow when not state.currentRight.IsEmpty -> 
            Console.CursorLeft <- Console.CursorLeft + state.currentRight.Head.value.Length 
            let state = { state with currentLeft = state.currentRight.Head::state.currentLeft; currentRight = state.currentRight.Tail }
            read state
        | _, ConsoleKey.LeftArrow 
        | _, ConsoleKey.RightArrow ->
            read state
        | _, _ when not state.currentRight.IsEmpty ->
            let pos = Console.CursorLeft
            Console.Write (state.currentRight |> List.fold (fun s t -> s + " " |> String.replicate t.value.Length) "")
            Console.CursorLeft <- pos
            Console.Write c.KeyChar
            let lineLength = if state.currentLeft.IsEmpty then 1 else state.currentLeft.Head.lineLength + 1
            let text = { value = c.KeyChar.ToString(); color = defaultColor; lineLength = lineLength }
            writeMany state.currentRight
            Console.CursorLeft <- pos + 1
            let state = { state with currentLeft = text::state.currentLeft }
            read state
        | _, _ ->
            Console.Write c.KeyChar
            let lineLength = if state.currentLeft.IsEmpty then 1 else state.currentLeft.Head.lineLength + 1
            let text = { value = c.KeyChar.ToString(); color = defaultColor; lineLength = lineLength }
            let state = { state with currentLeft = text::state.currentLeft }
            read state

    let private isExn (o:obj) =
        match o with
        | :? Exception -> true
        | _ -> false

    let private evalScript text =
        sw.Restart()
        let r = engine.Value.ExecuteScript text
        sw.Stop() 
        write lineStart
        let color = if isExn r then errorColor else outputColor
        write { value = r |> string; color = color; lineLength = 0 }
        write lineStart
        write { value = "Execution Time: " + sw.Elapsed.ToString(); color = statusColor; lineLength = 0 } 
        
    let private evalCommand (cmd:Command) = ()

    let rec private loop (state:State) = 
        write lineStart
        let state =  read { state with historyUp = state.history; historyDown = []; currentLeft = []; currentRight = []  }
        if not state.history.Head.IsEmpty then
            let text = state.history.Head |> List.fold (fun r t -> t.value + r) ""
            try 
                if not (commandStartRegex.IsMatch text) 
                then evalScript text
                else 
                    let result = run CommandParser.parse text
                    match result with
                    | Success (cmd, _, _) ->
                        evalCommand cmd
                    | Failure (msg, a, b) ->
                        write { value = msg |> string; color = errorColor; lineLength = 0 }
            with 
            | e ->
                write { value = e.Message |> string; color = errorColor; lineLength = 0 }
        loop state

    let initialize () =
        write message
        let state = { history = []; historyUp = []; historyDown = []; currentLeft = []; currentRight = [] }
        loop state |> ignore