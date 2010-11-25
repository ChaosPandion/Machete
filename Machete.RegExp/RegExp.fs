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

module p = 

    let pow n v =
        let rec pow n q a = 
            if q = v then a else pow n (q + 1) (n * a)
        pow n 0 1

    let intSeq = seq {
        for i in 1..System.Int32.MaxValue do yield i
    }

    let proc () =
        let m = 12.2134
        let mutable np = 0.0
        let mutable n = 0
        let mutable s = 0
        let mutable k = 0
        let mutable ki = 1
        while ki > 0 && ki < System.Int32.MaxValue do
            let min = pow 10 (ki - 1)
            let maxi = pow 10 (ki)
            let mutable si = min
            while si < maxi do
                if si % 10 <> 0 then
                    if  (si < 10 && ki = 1) || 
                        (si < 100 && ki = 2) || 
                        (si < 1000 && ki = 3) || 
                        (si < 10000 && ki = 4) || 
                        (si < 100000 && ki = 5) || 
                        (si < 1000000 && ki = 6) || 
                        (si < 10000000 && ki = 7) || 
                        (si < 100000000 && ki = 8) || 
                        (si < 1000000000 && ki = 9) then
                        let f = (log10 (m / double si)) + double ki
                        let r = double si * (10.0 ** (double (int (f)) - double ki))
                        if (r  > m - 0.0000000001 && r < m + 0.0000000001) then
                            n <- int np
                            s <- si
                            k <- ki
                            si <- maxi
                            ki <- System.Int32.MaxValue
                        else
                            np <- f
                si <- si + 1
            ki <- ki + 1
            
        n, s, k

            


    let main () =
        let m = 0.1
        let n, s, k = proc() //seq { for k in [1.0..10.0] do for s in [10.0 ** (k - 1.0)..10.0 ** k] do if s % 10.0 <> 0.0 && (int s).ToString().Length = int k then yield (log10 (m / s)) + k, s, k } |> Seq.minBy (fun (n, s, k) -> k)
        let r = if k <= n && n < 21 then 
                    s.ToString().Substring(0, int (k)) + "".PadLeft(int (n - k), '0') 
                elif 0 < n && n <= 21 then
                    s.ToString().Substring(0, int (n)) + "." + s.ToString().Substring(n, k - n) 
                elif -6 < n && n <= 0 then
                    "0." + "".PadRight(-n, '0') + s.ToString().Substring(0, k)
                else ""
        printfn "%s" r
        ()

    main ()