namespace Machete.Tools

module BacktrackingPrimitives =

    open System
    open System.Diagnostics
    open LazyList 

    [<Struct;DebuggerStepThrough>]
    type State<'a, 'b> (input:LazyList<'a>, data:'b) =
        member this.Input = input
        member this.Data = data

    type Result<'a, 'b, 'c> =
    | Success of 'c * State<'a, 'b>
    | Failure of string * State<'a, 'b>


    type Parser<'a, 'b, 'c> = State<'a, 'b> -> LazyList<Result<'a, 'b, 'c>>

    
    [<DebuggerStepThrough>]
    let item (state:State<'a, 'b>) =
        match state.Input with
        | Cons (head, tail) ->
            ofArray [| Success(head, State (tail, state.Data)) |]
        | Nil -> empty
    
    [<DebuggerStepThrough>]
    let result value state =
        ofArray [| Success (value, state) |]
    
    [<DebuggerStepThrough>]
    let zero state =
        empty


    let (>>=) left right state =
        let rec readLeft results =
            match results with
            | Cons (head, tail) ->
                match head with
                | Success (result, state) ->
                    let rec readRight results =
                        match results with
                        | Cons (head, tail) ->
                            match head with
                            | Success (_, _) as result -> ofArray [| result |]                     
                            | Failure (_, _) as result -> cons result  (readRight tail)
                        | Nil -> empty<Result<'a, 'b, 'd>> 
                    match readRight (right result state) with 
                    | Cons (head, _) ->
                        match head with
                        | Success (_, _) as result -> ofArray [| result |]                     
                        | Failure (_, _) as result -> cons result  (readLeft tail)
                    | Nil -> readLeft tail   
                | Failure (message, state) ->
                    let head = Result<'a, 'b, 'd>.Failure (message, state) 
                    cons head (readLeft tail)
            | Nil -> empty<Result<'a, 'b, 'd>>
        readLeft (left state)

    let (<|>) left right state =
        let rec check results =
            match results with
            | Cons (head, tail) ->
                match head with
                | Success (_, _) -> cons head (delayed (fun () -> check tail))
                | Failure (_, _) -> cons head (check tail)
            | Nil ->
                let rec check results =
                    match results with
                    | Cons (head, tail) ->
                        match head with
                        | Success (_, _) -> ofArray [| head |]
                        | Failure (_, _) -> cons head (check tail)
                    | Nil ->  empty
                check (right state)          
        check (left state)
            
    let (|>>) parser transform state =
        let rec read results = 
            match results with
            | Cons (head, tail) -> 
                match head with
                | Success (result, state) ->
                    let head = Success (transform result, state) 
                    let tail = delayed (fun () -> read tail)
                    cons head tail
                | Failure (message, _) ->
                    let head = Failure (message, state)
                    let tail = read tail
                    cons head tail    
            | Nil -> empty
        read (parser state)

    let (<?>) parser errorMessage state =
        let rec read results = 
            match results with
            | Cons (head, tail) -> 
                match head with
                | Success (_, _) -> cons head (delayed (fun () -> read tail))
                | Failure (message, _) -> cons (Failure (message, state)) (read tail)    
            | Nil -> ofArray [| Failure (errorMessage, state) |]
        read (parser state)                

    type ParseMonad() =        
        member this.Bind (f:Parser<'a, 'b, 'c>, g:'c -> Parser<'a, 'b, 'd>) = f >>= g      
        member this.Combine (f, g) = f <|> g                
        member this.Delay (f:unit -> Parser<_,_,_>) = f()
        member this.Return x = result x
        member this.ReturnFrom p = p
        member this.Zero () = zero
    
    let parse = ParseMonad() 
    
    [<DebuggerStepThrough>]
    let satisfy predicate = parse {
        let! value = item
        if predicate value then
            return value 
    } 
    
    let maybe parser = 
        let some = parse {
            let! v = parser
            return Some v
        }
        some <|> result None
        
    let many parser state =
        let rec many (state:State<_,_>) result =
            match parser state with
            | Cons (x, cs) ->
                match x with
                | Success(x, state) -> many state (result @ [x])
                | _ -> LazyList.ofList [Success(result, state)]
            | Nil -> LazyList.empty
        many state [] 

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

    let like value =
        satisfy (fun v -> v = value)

    let skipWhenLike value state =
        match item state with
        | Cons (head, tail) ->
            match head with
            | Success(result, state) 
                when result = value -> 
                    ofArray [| Success ((), state) |]
            | _ -> empty
        | Nil -> empty
    
    let manySeparatedFold parser separator start (f:_ -> _ -> _) =
        let rec go acc = 
            parse {
                let! v = parser
                return! parse {
                    let! _ = separator
                    return! go (v::acc)
                }
                return (v::acc) |> List.rev |> List.fold f start
            }
        go []

    let manyFold parser start (f:_ -> _ -> _) =
        let rec go acc = 
            parse {
                let! v = parser
                return! parse {
                    return! go (v::acc)
                }
                return (v::acc) |> List.rev |> List.fold f start
            }
        go []

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

    let run p i d =
        p (State(i, d))  

    
    let createParserRef<'a, 'b, 'c> () =
        let dummyParser = fun state -> failwith "a parser was not initialized"
        let r = ref dummyParser
        (fun state -> !r state), r : Parser<'a, 'b, 'c> * Parser<'a, 'b, 'c> ref
