namespace Machete.Tools

module StandardPrimitives =

    open System
    open LazyList 

    type State<'a, 'b> (input:LazyList<'a>, data:'b) =
        member this.Input = input
        member this.Data = data

    type Result<'a, 'b, 'c> =
    | Success of 'c * State<'a, 'b>
    | Failure of string * State<'a, 'b>

    type Parser<'a,'b, 'c> = State<'a, 'b> -> Result<'a, 'b, 'c>

    let (>>=) left right state =
        match left state with
        | Success (result, state) -> (right result) state
        | Failure (message, _) -> Result<'a, 'b, 'd>.Failure (message, state)

    let (<|>) left right state =
        match left state with
        | Success (_, _) as result -> result
        | Failure (_, _) -> right state

    let (|>>) parser transform state =
        match parser state with
        | Success (result, state) -> Success (transform result, state)
        | Failure (message, _) -> Failure (message, state)

    let (<?>) parser errorMessage state =
        match parser state with
        | Success (_, _) as result -> result
        | Failure (_, _) -> Failure (errorMessage, state)  
        
    let item (state:State<_,_>) =
        match state.Input with
        | Cons (head, tail) -> Success(head, State(tail, state.Data))
        | Nil -> Failure(String.Empty, state)  
        
    let result value state =
        Success(value, state)
        
    let zero state =
        Failure(String.Empty, state)                 

    type ParseMonad() =
        member this.Bind (f, g) = f >>= g  
        member this.Combine (f, g) = f <|> g                         
        member this.Delay (f:unit -> Parser<_, _, _>) = f()
        member this.Return x = result x
        member this.ReturnFrom parser = parser
        member this.Zero () = zero

    let parse = ParseMonad()

    let satisfy predicate state =
        match item state with
        | Success (v, _) as r when predicate v -> r
        | _ -> Failure (String.Empty, state)  

    let like value state =
        match item state with
        | Success (v, _) as r when v = value -> r
        | _ -> Failure (String.Empty, state)  

    let maybe parser state = 
        match parser state with
        | Success (v, s) -> Success(Some v, s)
        | Failure (_, _) -> Success(None, state)
        
    let many parser state =
        let rec many state result =
            match parser state with
            | Success (value, state) -> many state (value::result)
            | _ -> Success (List.rev result, state)
        many state [] 
        
    let between left right parser state =
        match left state with
        | Success (_, leftState) ->
            match parser leftState with
            | Success (value, middleState) ->
                match right middleState with
                | Success (_, rightState) ->
                    Success (value, rightState)
                | Failure (m, _) -> Failure (m, state)
            | Failure (m, _) -> Failure (m, state)
        | Failure (m, _) -> Failure (m, state)    

    let manySeparated parser separator state =
        let rec manySeparated state result =
            match parser state with
            | Success (value, leftState) ->
                match separator leftState with
                | Success (_, middleState) ->
                    manySeparated middleState (value::result)
                | _ -> Success (List.rev result, state)
            | _ -> Success (List.rev result, state)
        manySeparated state []
    
    let manySeparatedFold parser separator start (f:_ -> _ -> _) state =
        let complete result state = Success (List.rev result |> List.fold f start, state) 
        let rec manySeparated state result =
            match parser state with
            | Success (value, leftState) ->
                match separator leftState with
                | Success (_, middleState) ->
                    manySeparated middleState (value::result)
                | _ -> 
                    complete (value::result) leftState
            | _ -> 
                complete result state
        manySeparated state []

    let lookAhead parser state =
        match parser state with
        | Success (_, _) -> Success ((), state)
        | Failure (m, _) -> Failure (m, state)

    let run p i d =
        p (State(i, d)) 

    
    let createParserRef() =
        let dummyParser = fun state -> failwith "a parser was not initialized"
        let r = ref dummyParser
        (fun state -> !r state), r : Parser<_, _, 'u> * Parser<_, _, 'u> ref