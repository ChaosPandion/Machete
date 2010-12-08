namespace Machete.Compiler

module Tools =

    open System
    open System.Diagnostics
    open LazyList 

    [<Struct;DebuggerStepThrough>]
    type State<'a, 'b> (input:LazyList<'a>, data:'b) =
        member this.Input = input
        member this.Data = data

    type Result<'a, 'b, 'c> =
    | Success of 'c * State<'a, 'b>
    | Failure of list<string> * State<'a, 'b>    
    
    type Parser<'a, 'b, 'c> = 
        State<'a, 'b> -> seq<Result<'a, 'b, 'c>>

    let zero<'a, 'b, 'c> (state:State<'a, 'b>) =
        Seq.empty<Result<'a, 'b, 'c>>

    let item<'a, 'b> (state:State<'a, 'b>) = seq { 
        match state.Input with
        | Cons (head, tail) ->
            yield Success(head, State (tail, state.Data))
        | Nil -> ()
    } 

    let result<'a, 'b, 'c> (value:'c) (state:State<'a, 'b>) = seq  {
        yield Success (value, state)
    }
    
    let run p i d =
        p (State(i, d)) 

    let (>>=) (m:Parser<'a, 'b, 'c>) (f:'c -> Parser<'a, 'b, 'd>) (state:State<'a, 'b>) = 
        let rec run errors = seq {
            for r in m state do
                match r with
                | Success (v, s) ->
                    yield! f v s
                | Failure (ms, s) ->
                    yield! run (errors @ ms)
        }
        run []

    let (<|>) (l:Parser<'a, 'b, 'c>) (r:Parser<'a, 'b, 'c>) (state:State<'a, 'b>) =  
        let rec run p = seq {
            for result in p state do
                match result with
                | Success (_, _) ->
                    yield result
                | Failure (_, _) -> ()
        }
        Seq.append (run l) (run r)

    type ParseMonad() =        
        member this.Bind (f:Parser<'a, 'b, 'c>, g:'c -> Parser<'a, 'b, 'd>) : Parser<'a, 'b, 'd> = f >>= g     
        member this.Combine (f, g) = f <|> g      
        member this.Delay (f:unit -> Parser<'a, 'b, 'c>) (state:State<'a, 'b>) = f () state
        member this.Return x = result x
        member this.ReturnFrom p = p
        member this.Zero () = zero
    
    let parse = ParseMonad()
    
    let (|>>) (parser:Parser<'a, 'b, 'c>) (f:'c -> 'd) = parse {
        let! v = parser
        return f v   
    }

    let satisfy predicate = parse {
        let! value = item
        if predicate value then
            return value 
    }

    let maybe parser = parse {
        return! parser |>> Some <|> result None 
    }

    let choice (ps:seq<Parser<'a, 'b, 'c>>) (state:State<'a, 'b>) = seq {
        if not (LazyList.isEmpty state.Input) then
            for p in ps do
                yield! p state    
    }

    let between left right parser =
        parse {
            let! _ = left
            let! v = parser
            let! _ = right
            return v
        }

    let skip p = parse {
        let! v = p
        return ()
    }

    let many parser = 
        let rec many result = parse {
            let! v = parser
            let result = v::result
            return! many result
            return result    
        }
        many []

    let many1 parser = parse {
        let! r = many parser
        if not r.IsEmpty then
            return r
    }

    let manyFold parser start (f:_ -> _ -> _) = parse {
        let! r = many parser
        return r |> List.fold f start
    }

    let many1Fold parser start (f:_ -> _ -> _) = parse {
        let! r = many1 parser
        return r |> List.fold f start
    } 

    let manySepFold (parser:Parser<'a, 'b, 'c>) (separator:Parser<'a, 'b, 'd>) (start:'e) (f:'e -> 'c -> 'e) (state:State<'a, 'b>) = seq {
        let rec run state = seq {
            for r in parser state do 
                match r with
                | Success (v, s) ->
                    yield r
                    for r in separator s do
                        match r with
                        | Success (v, s) ->
                            yield! run s
                        | Failure (_, _) -> ()
                | Failure (ms, s) -> ()
        }
        if not (LazyList.isEmpty state.Input) then
            yield run state 
                |> Seq.fold (
                    fun (result, state) (y:Result<'a, 'b, 'c>) ->
                        match y with
                        | Success (v, s) ->
                            f result v, s
                ) (start, state)
                |> Success        
    }

    let isNotFollowedBy p =
        parse {
            let! v = maybe p
            match v with
            | Some _ -> ()
            | None -> return ()
        }

    let pipe2 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (f:'c -> 'd -> 'e) = 
        parse {
            let! v1 = p1
            let! v2 = p2
            return f v1 v2
        }

    let pipe3 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (f:'c -> 'd -> 'e -> 'f) = 
        parse {
            let! v1 = p1
            let! v2 = p2
            let! v3 = p3
            return f v1 v2 v3
        }

    let pipe4 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (f:'c -> 'd -> 'e -> 'f -> 'g) = 
        parse {
            let! v1 = p1
            let! v2 = p2
            let! v3 = p3
            let! v4 = p4
            return f v1 v2 v3 v4
        }

    let pipe5 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (p5:Parser<'a, 'b, 'g>) (f:'c -> 'd -> 'e -> 'f -> 'g -> 'h) = 
        parse {
            let! v1 = p1
            let! v2 = p2
            let! v3 = p3
            let! v4 = p4
            let! v5 = p5
            return f v1 v2 v3 v4 v5
        }

    let tuple2<'a, 'b, 'c, 'd, 'e> (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (f:'c * 'd -> 'e) = 
        parse {
            let! v1 = p1
            let! v2 = p2
            return f (v1, v2)
        }

    let tuple3 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (f:'c * 'd * 'e -> 'f) = 
        parse {
            let! v1 = p1
            let! v2 = p2
            let! v3 = p3
            return f (v1, v2, v3)
        }

    let tuple4 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (f:'c * 'd * 'e * 'f -> 'g) = 
        parse {
            let! v1 = p1
            let! v2 = p2
            let! v3 = p3
            let! v4 = p4
            return f (v1, v2, v3, v4)
        }

    let tuple5 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (p5:Parser<'a, 'b, 'g>) (f:'c * 'd * 'e * 'f * 'g -> 'h) = 
        parse {
            let! v1 = p1
            let! v2 = p2
            let! v3 = p3
            let! v4 = p4
            let! v5 = p5
            return f (v1, v2, v3, v4, v5)
        }

    let createParserRef<'a, 'b, 'c> () =
        let dummyParser = fun state -> failwith "a parser was not initialized"
        let r = ref dummyParser
        (fun state -> !r state), r : Parser<'a, 'b, 'c> * Parser<'a, 'b, 'c> ref