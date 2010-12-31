namespace Machete.Interactive

open System

module Interactive =

    type private Text = {
        value : string
        color : ConsoleColor
        lineLength : int   
    }

    type private ReadState = {
        history:list<list<Text>>
        historyUp:list<list<Text>> 
        historyDown:list<list<Text>>
        current:list<Text>
        currentLeft:list<Text>
        currentRight:list<Text>
    }
    
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

    let rec private read (tss:list<list<Text>>) (tssUp:list<list<Text>>) (tssDown:list<list<Text>>) (ts:list<Text>) =
        let c = Console.ReadKey (true)
        match c.Modifiers, c.Key with
        | _, ConsoleKey.F5 ->
            ts::tss, ts
        | _, ConsoleKey.Backspace when not ts.IsEmpty ->
            delete ts
            read tss tssUp tssDown ts.Tail
        | _, ConsoleKey.Backspace ->
            read tss tssUp tssDown ts
        | _, ConsoleKey.Enter ->
            Console.Write newLine
            read tss tssUp tssDown ({ value = newLine; color = defaultColor; lineLength = 0 }::ts)
        | _, ConsoleKey.Tab ->
            Console.Write tab
            read tss tssUp tssDown ({ value = tab; color = defaultColor; lineLength = ts.Head.lineLength + tab.Length }::ts)
        | _, ConsoleKey.UpArrow when not tssUp.IsEmpty ->
            deleteMany ts
            writeMany (tssUp.Head |> List.rev)
            read tss tssUp.Tail (ts::tssDown) tssUp.Head
        | _, ConsoleKey.UpArrow ->
            read tss tssUp tssDown ts
        | _, ConsoleKey.DownArrow when not tssDown.IsEmpty ->
            deleteMany ts
            writeMany (tssDown.Head |> List.rev)
            read tss (ts::tssUp) tssDown.Tail tssDown.Head
        | _, ConsoleKey.DownArrow ->
            read tss tssUp tssDown ts
        | _, ConsoleKey.LeftArrow 
        | _, ConsoleKey.RightArrow ->
            read tss tssUp tssDown ts
        | _, _ ->
            Console.Write c.KeyChar
            let lineLength = if ts.IsEmpty then 1 else ts.Head.lineLength + 1
            read tss tssUp tssDown ({ value = c.KeyChar.ToString(); color = defaultColor; lineLength = lineLength }::ts)

    let private isExn (o:obj) =
        match o with
        | :? Exception -> true
        | _ -> false

    let private eval text =
        sw.Restart()
        let r = engine.Value.ExecuteScript text
        sw.Stop() 
        write lineStart
        let color = if isExn r then errorColor else outputColor
        write { value = r |> string; color = color; lineLength = 0 }
        write lineStart
        write { value = sw.Elapsed |> string; color = statusColor; lineLength = 0 } 

    let rec private loop (tss:list<list<Text>>) = 
        write lineStart
        let tss, ts =  read tss tss [] []
        if not ts.IsEmpty then
            let text = ts |> List.fold (fun r t -> t.value + r) ""
            eval text
        loop tss

    let initialize () =
        write message
        loop [] |> ignore