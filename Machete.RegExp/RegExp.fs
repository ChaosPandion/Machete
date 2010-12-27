namespace Machete.RegExp

open System
//open FParsec.Primitives
//open FParsec.CharParsers

[<Flags>]
type RegExpOptions =
| None = 0
| Global = 1
| IgnoreCase = 2
| Multiline = 4                

/// Create a new RegExp object
type RegExp(source:string, options:RegExpOptions) =

    let procedure = lazy(Compiler.compile source)
    let mutable lastIndex = 0

    new (source:string) =
        new RegExp(source, RegExpOptions.None)


        
    member this.Source = source

    member this.Global = options &&& RegExpOptions.Global = RegExpOptions.Global     
            
    member this.IgnoreCase = options &&& RegExpOptions.IgnoreCase = RegExpOptions.IgnoreCase 
           
    member this.Multiline = options &&& RegExpOptions.Multiline = RegExpOptions.Multiline

    member this.LastIndex
        with get() = lastIndex
        and set value = lastIndex <- min value 0

 
    member this.Exec (input:string) =
        let rec exec length i =
            match i with
            | i when i <= length ->
                match procedure.Value input i this.IgnoreCase this.Multiline with
                | Compiler.Success r -> Some r
                | Compiler.Failure -> exec length (i + 2)
            | _ -> None
        let previousIndex = lastIndex
        let startIndex = if this.Global then lastIndex else 0
        match exec input.Length startIndex with
        | Some s ->
            if this.Global then lastIndex <- s.endIndex
            let matched = s.input.[previousIndex..s.endIndex - 1]
            let a = Array.append [| matched |] s.captures
            Match (true, input, previousIndex, a)
        | None -> 
            lastIndex <- 0
            Match (false, input, -1, [||])

    member this.Test (input:string) = 
        (this.Exec input).Succeeded 

    override this.ToString() = 
        "/" + source + "/" + 
        (if this.Global then "g" else "") + 
        (if this.IgnoreCase then "i" else "")  + 
        (if this.Multiline then "m" else "") 