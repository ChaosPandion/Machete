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


module Tools1 =

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
    let zero<'a, 'b, 'c> (state:State<'a, 'b>) =
        empty<Result<'a, 'b, 'c>>

    //[<DebuggerStepThrough>]
    let item<'a, 'b> (state:State<'a, 'b>) : LazyList<Result<'a, 'b, 'a>> =
        match state.Input with
        | Cons (head, tail) ->
            ofArray<Result<'a, 'b, 'a>> [| Success(head, State (tail, state.Data)) |]
        | Nil -> 
            empty<Result<'a, 'b, 'a>>
    
    //[<DebuggerStepThrough>]
    let result<'a, 'b, 'c> (value:'c) : Parser<'a, 'b, 'c> =
        fun (state:State<'a, 'b>) ->
            ofArray [| Success (value, state) |]
    

    let rec readRight results =
        match results with
        | Cons (x1, xs1) ->
            match x1 with
            | Success (_, _) as result -> 
                ofArray [| result |]                     
            | Failure (_, _) as result -> 
                readRight xs1
        | Nil -> empty<Result<'a, 'b, 'd>> 

    let (>>=) (left:Parser<'a, 'b, 'c>) (right:'c -> Parser<'a, 'b, 'd>) : Parser<'a, 'b, 'd> =
        fun state ->
            let r1 = left state 
            let r =
                r1 |> LazyList.tryFind (
                    fun r -> 
                        match r with
                        | Success (v, s1) -> true
                        | Failure (m, s1) -> false
                ) 
            match r with
            | Some (Success (v1, s1)) ->
                let r2 = right v1 s1
                let r =
                    r2 |> LazyList.tryFind (
                        fun r -> 
                            match r with
                            | Success (v, s1) -> true
                            | Failure (m, s1) -> false
                    ) 
                match r with
                | Some r ->
                    ofArray [| r |]
                | None -> r2
            | None -> empty
                                
//            let rec readLeft results =
//                match results with
//                | Cons (head, tail) ->
//                    match head with
//                    | Success (result, state2) -> 
//                        right result                   
//    //                    match readRight (right result state2) with 
//    //                    | Cons (head, _) ->
//    //                        match head with
//    //                        | Success (_, _) as result -> ofArray [| result |]                     
//    //                        | Failure (_, _) as result -> readLeft tail
//    //                    | Nil -> readLeft tail   
//                    | Failure (message, state) ->
//                        let head = Result<'a, 'b, 'd>.Failure (message, state) 
//                        //cons head (readLeft tail)
//                        readLeft tail
//                | Nil -> zero<'a, 'b, 'd> //empty<Result<'a, 'b, 'd>>
//            readLeft (left state)

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
                    //let tail = delayed (fun () -> read tail)
                    //cons head tail
                    ofArray [| head |]
                | Failure (message, _) ->
//                    let head = Failure (message, state)
//                    let tail = read tail
//                    cons head tail
                    read tail    
            | Nil -> empty
        read (parser state)

//    let (<?>) parser errorMessage state =
//        let rec read results = 
//            match results with
//            | Cons (head, tail) -> 
//                match head with
//                | Success (_, _) -> cons head (delayed (fun () -> read tail))
//                | Failure (message, _) -> cons (Failure (message, state)) (read tail)    
//            | Nil -> ofArray [| Failure (errorMessage, state) |]
//        read (parser state)                

    type ParseMonad() =        
        member this.Bind (f:Parser<'a, 'b, 'c>, g:'c -> Parser<'a, 'b, 'd>) : Parser<'a, 'b, 'd> = f >>= g      
        member this.Combine (f, g) =  f <|> g             
        member this.Delay (f:unit -> Parser<'a, 'b, 'c>) =
            fun state ->
                f () state
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
    
    let rec choice<'a, 'b, 'c> (ps:list<Parser<'a, 'b, 'c>>) : Parser<'a, 'b, 'c> =
        parse {
            match ps with
            | p::ps ->
                let! v = maybe p
                match v with
                | Some v -> 
                    return v
                | None -> 
                    return! choice ps
            | [] -> ()
        } 
    
    and maybe parser state = 
        let rec check results =
            match parser state with
            | Cons (x, xs) ->
                match x with
                | Success (v, s) ->
                     ofArray [| Success (Some v, s) |]
                | Failure (m, s) ->
                    check xs
            | Nil -> 
                ofArray [| Success (None, state) |]
        check (parser state)

//        choice [
//            parse {
//                let! v = parser
//                return Some v
//            };
//            result None
//        ]
        
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
                let! v = maybe parser
                match v with
                | Some v ->
                    let! s = maybe separator
                    match s with
                    | Some _ ->
                        return! go (v::acc)
                    | None ->
                        return (v::acc) |> List.rev |> List.fold f start
                | None ->
                    return acc |> List.rev |> List.fold f start                 
            }
        go []

    let manyFold parser start (f:_ -> _ -> _) =
        let rec go acc = 
            parse {
                let! v = maybe parser
                match v with
                | Some v ->
                    return! go (v::acc)
                | None ->
                    return acc |> List.rev |> List.fold f start 
            }
        go []

    let many1Fold parser start (f:_ -> _ -> _) =
        let rec go acc = 
            parse {
                let! v = parser
                return! parse {
                    return! go (v::acc)
                }
                if not acc.IsEmpty then
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

//    open System
//    open System.Diagnostics
//    open LazyList 
//
//    [<Struct;DebuggerStepThrough>]
//    type State<'a, 'b> (input:LazyList<'a>, data:'b) =
//        member this.Input = input
//        member this.Data = data
//
//    type Result<'a, 'b, 'c> =
//    | Success of 'c * State<'a, 'b>
//    | Failure of string * State<'a, 'b>
//
//
//    type Parser<'a, 'b, 'c> = State<'a, 'b> -> LazyList<Result<'a, 'b, 'c>>
//
//    
//    [<DebuggerStepThrough>]
//    let item (state:State<'a, 'b>) =
//        match state.Input with
//        | Cons (head, tail) ->
//            ofArray [| Success(head, State (tail, state.Data)) |]
//        | Nil -> empty
//    
//    [<DebuggerStepThrough>]
//    let result (value:'c) (state:State<'a, 'b>) =
//        ofArray [| Success (value, state) |]
//    
//    [<DebuggerStepThrough>]
//    let zero (state:State<'a, 'b>) =
//        empty<Result<'a, 'b, 'c>>
//
//
//    let (>>=) (left:Parser<'a, 'b, 'c>) (right:'c -> Parser<'a, 'b, 'd>) state =
//        let rec readLeft results =
//            match results with
//            | Cons (head, tail) ->
//                match head with
//                | Success (result, state) -> 
//                    //right result state
//                    let rec readRight results =
//                        match results with
//                        | Cons (head, tail) ->
//                            match head with
//                            | Success (_, _) as result -> 
//                                ofArray [| result |]                     
//                            | Failure (_, _) as result -> 
//                                ofSeq (seq {
//                                    yield result
//                                    for x in readRight tail do
//                                        yield x
//                                })
//                        | Nil -> empty<Result<'a, 'b, 'd>> 
//                    match (right result state) with 
//                    | Cons (head, tail2) ->
//                        match head with
//                        | Success (_, _) as result -> 
//                            ofArray [| result |]                     
//                        | Failure (_, _) as result -> 
//                            //cons result  (delayed (fun () -> readLeft tail))
//                            ofSeq (seq {
//                                yield result
//                                for x in readLeft tail do
//                                    yield x
//                            })
//                    | Nil when not (LazyList.isEmpty tail) ->
//                        ofSeq (seq {
//                            for x in readLeft tail do
//                                yield x
//                        })
//                    | _ ->  
//                        empty<Result<'a, 'b, 'd>>  
//                | Failure (message, state) ->
//                    let head = Result<'a, 'b, 'd>.Failure (message, state) 
//                    cons head (readLeft tail)
//            | Nil -> empty<Result<'a, 'b, 'd>>
//        readLeft (left state)
//
//    let (<|>) left right state =
//        let rec check results =
//            match results with
//            | Cons (head, tail) ->
//                match head with
//                | Success (_, _) as result ->
//                    ofSeq (seq {
//                        yield head
//                        for x in check tail do
//                            yield x
//                    })
//                    //cons head (check tail)
//                | Failure (_, _) -> 
//                    ofSeq (seq {
//                        yield head
//                        for x in check tail do
//                            yield x
//                    })
//            | Nil ->
//                let rec check results =
//                    match results with
//                    | Cons (head, tail) ->
//                        match head with
//                        | Success (_, _) -> ofArray [| head |]
//                        | Failure (m, _) ->
//                            ofSeq (seq {
//                                yield head
//                                for x in check tail do
//                                    yield x
//                            })
//                    | Nil ->  empty
//                check (right state)
//        check (left state)
//            
//    let (|>>) parser transform state =
//        let rec read results = 
//            match results with
//            | Cons (head, tail) -> 
//                match head with
//                | Success (result, state) ->
//                    ofSeq (seq {
//                        yield Success (transform result, state)
//                        for x in read tail do
//                            yield x
//                    })
////                    let head = Success (transform result, state) 
////                    let tail = delayed (fun () -> read tail)
////                    cons head tail
//                | Failure (message, _) ->
//                    let head = Failure (message, state)
//                    let tail = read tail
//                    cons head tail    
//            | Nil -> empty
//        read (parser state)
//
//    let (<?>) parser errorMessage state =
//        let rec read results = 
//            match results with
//            | Cons (head, tail) -> 
//                match head with
//                | Success (_, _) -> cons head (delayed (fun () -> read tail))
//                | Failure (message, _) -> cons (Failure (message, state)) (read tail)    
//            | Nil -> ofArray [| Failure (errorMessage, state) |]
//        read (parser state)                
//
//    type ParseMonad() =        
//        member this.Bind (f:Parser<'a, 'b, 'c>, g:'c -> Parser<'a, 'b, 'd>) : Parser<'a, 'b, 'd> = f >>= g      
//        member this.Combine (f:Parser<'a, 'b, 'c>, g:Parser<'a, 'b, 'c>) = f <|> g                
//        member this.Delay (f:unit -> Parser<'a, 'b, 'c>) = f()
//        member this.Return (x:'c) : Parser<'a, 'b, 'c> = result x
//        member this.ReturnFrom (p:Parser<'a, 'b, 'c>) = p
//        member this.Zero () = zero
//    
//    let parse = ParseMonad() 
//    
//    [<DebuggerStepThrough>]
//    let satisfy predicate = parse {
//        let! value = item
//        if predicate value then
//            return value 
//    } 
//
//    let choice<'a, 'b, 'c> (ps:seq<Parser<'a, 'b, 'c>>) state =
//        seq {
//            let rec read results = 
//                match results with
//                | Cons (head, tail) -> 
//                    match head with
//                    | Success (_, _) -> ofArray [| head |]
//                    | Failure (_, _) -> read tail  
//                | Nil -> empty
//            for p in ps do
//                match read (p state) with
//                | Cons (head, tail) -> 
//                    yield head
//                | Nil -> ()                
//        } |> LazyList.ofSeq
//    
//    let maybe (parser:Parser<'a, 'b, 'c>) = 
//        let some = parse {
//            let! v = parser
//            return Some v
//        }
//        some <|> result None
//        
//    let many parser state =
//        let rec many (state:State<_,_>) result =
//            match parser state with
//            | Cons (x, cs) ->
//                match x with
//                | Success(x, state) -> 
//                    many state (result @ [x])
//                | _ -> 
//                    LazyList.ofList [Success(result, state)]
//            | Nil -> 
//                LazyList.empty
//        many state []
//          
//    let many1 parser state =
//        let rec many1 (state:State<_,_>) result =
//            match parser state with
//            | Cons (x, cs) ->
//                match x with
//                | Success(x, state) -> many1 state (result @ [x])
//                | _ -> LazyList.ofList [Success(result, state)]
//            | Nil 
//                when result.IsEmpty -> 
//                    LazyList.empty
//            | Nil -> 
//                LazyList.empty
//        many1 state []
//
//    let between left right parser =
//        parse {
//            let! _ = left
//            let! v = parser
//            let! _ = right
//            return v
//        }
//
//    let skip p = parse {
//        let! v = p
//        return ()
//    }
//
//    let like value =
//        satisfy (fun v -> v = value)
//
//    let skipWhenLike value state =
//        match item state with
//        | Cons (head, tail) ->
//            match head with
//            | Success(result, state) 
//                when result = value -> 
//                    ofArray [| Success ((), state) |]
//            | _ -> empty
//        | Nil -> empty
//    
//    let manySeparatedFold parser separator start (f:_ -> _ -> _) =
//        let rec go acc = 
//            parse {
//                let! v = parser
//                return! parse {
//                    let! _ = separator
//                    return! go (v::acc)
//                }
//                return (v::acc) |> List.rev |> List.fold f start
//            }
//        go []
//
//    let manyFold parser start (f:_ -> _ -> _) =
//        let rec go acc = 
//            parse {
//                let! v = parser
//                return! parse {
//                    return! go (v::acc)
//                }
//                return (v::acc) |> List.rev |> List.fold f start
//            }
//        go []
//
//    let many1Fold parser start (f:_ -> _ -> _) =
//        let rec go acc = 
//            parse {
//                let! v = parser
//                return! parse {
//                    return! go (v::acc)
//                }
//                if not acc.IsEmpty then
//                    return (v::acc) |> List.rev |> List.fold f start
//            }
//        go []
//
//    let isNotFollowedBy p =
//        parse {
//            let! v = maybe p
//            match v with
//            | Some _ -> ()
//            | None -> return ()
//        }
//
//    let pipe2 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (f:'c -> 'd -> 'e) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            return f v1 v2
//        }
//
//    let pipe3 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (f:'c -> 'd -> 'e -> 'f) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            return f v1 v2 v3
//        }
//
//    let pipe4 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (f:'c -> 'd -> 'e -> 'f -> 'g) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            let! v4 = p4
//            return f v1 v2 v3 v4
//        }
//
//    let pipe5 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (p5:Parser<'a, 'b, 'g>) (f:'c -> 'd -> 'e -> 'f -> 'g -> 'h) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            let! v4 = p4
//            let! v5 = p5
//            return f v1 v2 v3 v4 v5
//        }
//
//    let tuple2<'a, 'b, 'c, 'd, 'e> (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (f:'c * 'd -> 'e) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            return f (v1, v2)
//        }
//
//    let tuple3 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (f:'c * 'd * 'e -> 'f) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            return f (v1, v2, v3)
//        }
//
//    let tuple4 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (f:'c * 'd * 'e * 'f -> 'g) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            let! v4 = p4
//            return f (v1, v2, v3, v4)
//        }
//
//    let tuple5 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (p5:Parser<'a, 'b, 'g>) (f:'c * 'd * 'e * 'f * 'g -> 'h) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            let! v4 = p4
//            let! v5 = p5
//            return f (v1, v2, v3, v4, v5)
//        }
//
//    let run p i d =
//        p (State(i, d))  
//
//    
//    let createParserRef<'a, 'b, 'c> () =
//        let dummyParser = fun state -> failwith "a parser was not initialized"
//        let r = ref dummyParser
//        (fun state -> !r state), r : Parser<'a, 'b, 'c> * Parser<'a, 'b, 'c> ref
//
//
