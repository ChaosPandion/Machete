namespace Machete.Compiler


open System
open System.Collections
open System.Collections.Generic
open System.Text
open System.Linq.Expressions
open System.Reflection
open FParsec
open FParsec.CharParsers
open FParsec.Primitives
open Machete.Core
open InputElementParsers
    

type internal exp = System.Linq.Expressions.Expression



type Label = {
    labelExpression: LabelExpression
} with     
    interface IComparable with
        member this.CompareTo o =
            match o with
            | :? Label as label ->              
                this.labelExpression.Target.Name.CompareTo (label.labelExpression.Target.Name)
            | _ -> -1


type CompileState = {
    strict : list<bool * bool>
    labels : list<Map<string, Label>>
    functions : list<FunctionDeclaration>
    variables : list<string>
}
  
  
type CompilerService (environment:IEnvironment) as this =

    let stopwatch = new System.Diagnostics.Stopwatch()

    let equalityTestMethod = this.GetType().GetMethod ("equalityTest", BindingFlags.Static ||| BindingFlags.NonPublic)
       
    // Source Elements

    let skippable = 
        choice [|
            evalWhiteSpace
            evalLineTerminator
            evalComment |>> ignore
        |]

    let skipIgnorableTokens state =
        optional (skipMany skippable) state
        
    let skipToken value state =
        (skipIgnorableTokens >>. skipString value >>. skipIgnorableTokens) state
        
    let skipIdentifierName value state =
        (parse {
            let! v = skipIgnorableTokens >>. evalIdentifierName .>> skipIgnorableTokens
            if v = value then
                return ()
        }) state
        
    let skipStatementTerminator state =
        (parse {
            let! r = skipMany evalWhiteSpace >>. opt (lookAhead (anyOf ";}")) 
            match r with
            | Some ';' ->
                do! skipAnyChar 
                return ()
            | Some r -> return ()
            | None ->
                let! r = opt (evalLineTerminator <|> previousCharSatisfies CharSets.isLineTerminator)
                match r with
                | Some r -> return ()
                | None ->
                    let! r = opt evalMultiLineComment
                    if r.IsSome then                  
                        return ()
                    else 
                        do! eof
                        return ()
        }) state
         
    let skipEval parser state =
        (skipIgnorableTokens >>. parser) state

    let betweenParentheses parser state =
        between (skipToken "(") (skipToken ")") (skipEval parser) state

    let betweenBraces parser state =
        between (skipToken "{") (skipToken "}") (skipEval parser) state

    let betweenBrackets parser state =
        between (skipToken "[") (skipToken "]") (skipEval parser) state

    let skipComma =
        skipToken ","
           
    let rec evalPrimaryExpression state =
        (skipIgnorableTokens >>. (
            (attempt (skipIdentifierName "this") >>. preturn (Expressions.ThisBinding :> exp)) <|>
            attempt (evalIdentifierReference )<|>
            evalLiteral <|>
            evalArrayLiteral <|>
            evalObjectLiteral <|>
            evalGroupingOperator
        )) state

    and evalIdentifierReference state =
        (parse {
            let! identifier = evalIdentifier
            let! s = getUserState
            let args = [| exp.Constant (identifier) :> exp; exp.Constant (s.strict.Head |> fst) :> exp |]
            return exp.Call (Expressions.LexicalEnviroment, Reflection.ILexicalEnvironmentMemberInfo.GetIdentifierReference, args) :> exp
        }) state
    
    and evalLiteral =
        (choice [|
            attempt (skipIdentifierName "null") |>> fun () -> exp.Constant (environment.Null) :> exp
            attempt (skipIdentifierName "true") |>> fun () -> exp.Constant (environment.True) :> exp
            attempt (skipIdentifierName "false") |>> fun () -> exp.Constant (environment.False) :> exp
            evalNumericLiteral |>> fun num -> exp.Constant (environment.CreateNumber num) :> exp
            evalStringLiteral |>> fun str -> exp.Constant (environment.CreateString str) :> exp
            evalRegularExpressionLiteral |>> fun (body, flags) -> exp.Constant (environment.CreateRegExp (body, flags)) :> exp
        |] |> attempt)

    and evalGroupingOperator state =
        (skipToken "(" >>. evalExpression .>> skipToken ")") state
        
    and evalObjectLiteral state =
        (parse {
            do! skipToken "{"
            let! s = getUserState
            let objectVar = exp.Variable (typeof<IObject>, "objectVar") 
            let defineOwnProperty name value =
                let name = exp.Constant (name) :> exp
                let strict = exp.Constant (s.strict.Head |> fst) :> exp
                exp.Call (objectVar, Reflection.IObjectMemberInfo.DefineOwnProperty, [| name; value; strict |])
            let assign = Expressions.ObjectConstructor
            let assign = exp.Convert (assign, typeof<IConstructable>)
            let assign = exp.Call (assign, Reflection.IConstructableMemberInfo.Construct, [| Expressions.Environment :> exp; Expressions.EmptyArgs :> exp |]) :> exp
            let assign = exp.Assign (objectVar, assign) :> exp
            let! e = evalPropertyNameAndValueList
            match e with
            | [] ->
                do! skipToken "}"
                return exp.Block ([| objectVar |], [| assign; objectVar :> exp |]) :> exp
            | e ->
                do! optional (skipToken ",")
                do! skipToken "}"
                let rec build result items (dataSet:Set<string>) (getterSet:Set<string>) (setterSet:Set<string>) =
                    match items with
                    | [] -> result
                    | (propType, name, expression)::rest ->
                        match propType with
                        | "data" when s.strict.Head |> fst && dataSet.Contains name ->
                            raise (environment.CreateSyntaxError "")
                        | "data" ->
                            let getValue = exp.Property (expression, Reflection.IDynamicMemberInfo.Value) :> exp
                            let args = [| getValue; Expressions.NullableBoolTrue :> exp; Expressions.NullableBoolTrue :> exp; Expressions.NullableBoolTrue :> exp |]
                            let createDesc = exp.Call (Expressions.Environment, Reflection.IEnvironmentMemberInfo.CreateDataDescriptor, args) :> exp
                            let result = (defineOwnProperty name createDesc):>exp :: result
                            build result items.Tail (dataSet.Add name) getterSet setterSet
                        | "getter" when dataSet.Contains name ->
                            raise (environment.CreateSyntaxError "")
                        | "getter" when getterSet.Contains name ->
                            raise (environment.CreateSyntaxError "")
                        | "getter" ->
                            let args = [| expression; Expressions.IDynamicNull :> exp; Expressions.NullableBoolTrue :> exp; Expressions.NullableBoolTrue :> exp  |]
                            let createDesc = exp.Call (Expressions.Environment, Reflection.IEnvironmentMemberInfo.CreateAccessorDescriptor, args) :> exp
                            let result = (defineOwnProperty name createDesc):>exp :: result
                            build result items.Tail dataSet (getterSet.Add name) setterSet
                        | "setter" when dataSet.Contains name ->
                            raise (environment.CreateSyntaxError "")
                        | "setter" when setterSet.Contains name ->
                            raise (environment.CreateSyntaxError "")
                        | "setter" ->
                            let args = [| Expressions.IDynamicNull :> exp; expression; Expressions.NullableBoolTrue :> exp; Expressions.NullableBoolTrue :> exp  |]
                            let createDesc = exp.Call (Expressions.Environment, Reflection.IEnvironmentMemberInfo.CreateAccessorDescriptor, args) :> exp
                            let result = (defineOwnProperty name createDesc):>exp :: result
                            build result items.Tail dataSet getterSet (setterSet.Add name)
                let body = assign :: (objectVar :> exp :: build [] e Set.empty Set.empty Set.empty |> List.rev)
                return exp.Block ([| objectVar |], body) :> exp
        }) state

    and evalPropertyNameAndValueList state =
        (sepBy (attempt (skipEval evalPropertyAssignment)) (attempt skipComma)) state

    and evalPropertyAssignment state =
        (parse {
            let! name = opt evalIdentifierName
            match name with
            | Some "get" ->
                let! oldState = getUserState
                let! name = evalPropertyName .>> skipToken "(" .>> skipToken ")"
                let! (functionBody:Expression<Code>) = skipToken "{" >>. evalFunctionBody .>> skipToken "}"                
                let! (newState:CompileState) = getUserState
                let variableDeclarations = ReadOnlyList<string>(newState.variables |> List.rev)
                let functionDeclarations = ReadOnlyList<FunctionDeclaration>(newState.functions |> List.rev)
                let strict = newState.strict.Head |> fst
                let exC = ExecutableCode (functionBody.Compile(), variableDeclarations, functionDeclarations, strict)
                let args = [| exp.Constant(exC):>exp; exp.Constant(ReadOnlyList<string>.Empty) :> exp; Expressions.LexicalEnviroment :> exp |]
                let func = exp.Call (Expressions.Environment, Reflection.IEnvironmentMemberInfo.CreateFunction, args) :> exp
                do! setUserState oldState  
                return "getter", name, func
            | Some "set" ->
                let! oldState = getUserState
                let! name = evalPropertyName
                let! formalParameters = betweenParentheses evalPropertySetParameterList
                let! (functionBody:Expression<Code>) = skipToken "{" >>. evalFunctionBody .>> skipToken "}"               
                let! (newState:CompileState) = getUserState
                let variableDeclarations = ReadOnlyList<string>(newState.variables |> List.rev)
                let functionDeclarations = ReadOnlyList<FunctionDeclaration>(newState.functions |> List.rev)
                let strict = newState.strict.Head |> fst
                let exC = ExecutableCode (functionBody.Compile(), variableDeclarations, functionDeclarations, strict)
                let args = [| exp.Constant(exC):>exp; exp.Constant(formalParameters) :> exp; Expressions.LexicalEnviroment :> exp |]
                let func = exp.Call (Expressions.Environment, Reflection.IEnvironmentMemberInfo.CreateFunction, args) :> exp
                do! setUserState oldState  
                return "setter", name, func
            | Some name ->
                let! value = skipToken ":" >>. evalAssignmentExpression
                return "data", name, value
            | None -> ()
        }) state



//        (evalValuePropertyAssignment <|> evalGetPropertyAssignment <|> evalSetPropertyAssignment) state

    and evalValuePropertyAssignment state =
        (parse {
            let! e1, e3 = tuple2 (skipEval evalPropertyName) (skipToken ":" >>. evalAssignmentExpression)
            return "data", e1, None, Some e3, None, false
        }) state
       
    and evalGetPropertyAssignment state =
        (parse {
            let! e1 = skipToken "get" >>. evalPropertyName
            do! skipToken "(" >>. skipToken ")"
            let! e3 = skipToken "{" >>. evalFunctionBody .>> skipToken "}"
            return "getter", e1, None, None, Some e3, false
        }) state

    and evalSetPropertyAssignment state =
        (parse {
            do! skipToken "set"
            let! e1 = evalPropertyName
            let! e2 = betweenParentheses evalPropertySetParameterList
            let! e3 = betweenBraces evalFunctionBody
            return "setter", e1, Some e2, None, Some e3, false
        }) state
                  
    and evalPropertyName state =
        (parse {
            let! r = opt (skipIgnorableTokens >>. (evalIdentifierName <|> evalStringLiteral))
            match r with
            | Some r -> return r
            | None ->
                let! r = evalNumericLiteral
                let r = environment.CreateNumber r
                let r = r.ConvertToString()
                return r.BaseValue
        }) state

    and evalPropertySetParameterList state =
        (parse {
            let! r = evalIdentifier
            return ReadOnlyList<string>([| r |])
        }) state

    and evalArrayLiteral state =
        (parse {
            do! skipToken "["
            let! s = getUserState
            let arrayVar = exp.Variable (typeof<IObject>, "arrayVar")
            let getLength () =
                let name = exp.Constant ("length") :> exp
                exp.Call (arrayVar, Reflection.IObjectMemberInfo.Get, [| name |]) :> exp
            let addPad pad =
                let pad = exp.Constant (environment.CreateNumber pad) :> exp
                let pad = exp.Convert (pad, typeof<IDynamic>) :> exp
                let value = getLength ()
                let value = exp.Call (value, Reflection.IDynamicMemberInfo.Op_Addition, [| pad |]) :> exp
                exp.Call (value, Reflection.IDynamicMemberInfo.ConvertToUInt32, Array.empty) :> exp                
            let setElement (pad, e) =
                let name = exp.Constant ("length") :> exp
                let strict = exp.Constant (s.strict.Head |> fst) :> exp
                let name = addPad pad
                let name = exp.Call (name, Reflection.IDynamicMemberInfo.ConvertToString, Array.empty) :> exp
                let name = exp.Convert (name, typeof<IString>)
                let name = exp.Property (name, Reflection.IStringMemberInfo.BaseValue) :> exp
                let args = [| name; e; strict |]
                exp.Call (arrayVar, Reflection.IObjectMemberInfo.Put, args) :> exp         
            let createPutLength pad strict =
                let name = exp.Constant ("length") :> exp
                let value = addPad pad
                let strict = exp.Constant (strict) :> exp
                let args = [| name; value; strict |]
                exp.Call (arrayVar, Reflection.IObjectMemberInfo.Put, args) :> exp                
            let assign = Expressions.ArrayConstructor
            let assign = exp.Convert (assign, typeof<IConstructable>)
            let assign = exp.Call (assign, Reflection.IConstructableMemberInfo.Construct, [| Expressions.Environment :> exp; Expressions.EmptyArgs :> exp |]) :> exp
            let assign = exp.Assign (arrayVar, assign) :> exp
            let! elements = evalElementList 
            match elements with
            | [] ->
                let! pad = evalElision
                match pad with
                | 0.0 ->
                    do! skipToken "]"
                    return exp.Block ([| arrayVar |], [| assign; arrayVar :> exp |]) :> exp
                | pad ->
                    let putLength = createPutLength pad (s.strict.Head |> fst)
                    do! skipToken "]"
                    return exp.Block ([| arrayVar |], [| assign; putLength; arrayVar :> exp |]) :> exp
            | elements ->
                do! optional (skipToken ",")
                let body = elements |> List.map setElement
                let! pad = evalElision
                match pad with
                | 0.0 ->
                    let body = assign::body @ [ arrayVar :> exp ]
                    do! skipToken "]"
                    return exp.Block ([| arrayVar |], body) :> exp
                | pad ->
                    let putLength = createPutLength pad (s.strict.Head |> fst)
                    let body = assign::body @ [ putLength; arrayVar :> exp ]
                    do! skipToken "]"
                    return exp.Block ([| arrayVar |], body) :> exp
        }) state

    and evalElision state =
        (parse {
            let! r = many skipComma
            return r.Length |> double
        }) state

    and evalElementList state =
        (parse {
            let p = parse {
                let! pad = attempt evalElision <|> preturn 0.0
                let! e = attempt (skipIgnorableTokens >>. evalAssignmentExpression)
                return pad, e
            }
            let! first = opt p
            match first with
            | Some first ->
                let! r = many (attempt (skipComma >>. p)) 
                return first::r
            | None -> return []
        }) state

    and evalMemberExpression state =
        let newMemberExpression state = 
            (tuple2 (skipToken "new" >>. skipEval evalMemberExpression) (skipEval evalArguments) 
            |>> fun (e, a) -> exp.Call (e, Reflection.IDynamicMemberInfo.Op_Construct, [| a |]) :> exp) state
        (parse {
            let! e1 = newMemberExpression <|> evalFunctionExpression <|> evalPrimaryExpression 
            let e1 = exp.Convert (e1, typeof<IDynamic>) :> exp
            let! rest = many (attempt evalBracketNotation <|> attempt evalDotNotation)
            if rest.IsEmpty then
                return e1
            else
                let e1 = exp.Property (e1, Reflection.IDynamicMemberInfo.Value) :> exp
                let! s = getUserState
                let rest = 
                    (rest |> List.fold (
                        fun acc (t, e) ->
                            match t with
                            | "bracket" ->  
                                let convert = exp.Call (e, Reflection.IDynamicMemberInfo.ConvertToString, [||]) :> exp  
                                let convert = exp.Property (convert, Reflection.IStringMemberInfo.BaseValue) :> exp                     
                                let args = [| convert; exp.Convert(acc, typeof<IReferenceBase>) :> exp; exp.Constant (s.strict.Head |> fst) :> exp |]
                                let r = exp.Call (Expressions.Environment, Reflection.IEnvironmentMemberInfo.CreateReference, args) :> exp 
                                let r = exp.Convert (r, typeof<IDynamic>)
                                exp.Property (r, Reflection.IDynamicMemberInfo.Value) :> exp
                            | "dot" ->                         
                                let args = [| e; exp.Convert(acc, typeof<IReferenceBase>) :> exp; exp.Constant (s.strict.Head |> fst) :> exp |]
                                let r = exp.Call (Expressions.Environment, Reflection.IEnvironmentMemberInfo.CreateReference, args) :> exp 
                                let r = exp.Convert (r, typeof<IDynamic>)
                                exp.Property (r, Reflection.IDynamicMemberInfo.Value) :> exp        
                    ) e1)
                return (rest :?> MemberExpression).Expression                  
        }) state
        
    and evalNewExpression state =
        choice [|
            attempt (parse {
                do! skipToken "new"
                let! r = evalNewExpression   
                let args = [| exp.Constant (environment.EmptyArgs) :> exp |]
                return exp.Call (r, Reflection.IDynamicMemberInfo.Op_Construct, args) :> exp
            })
            evalMemberExpression
        |] state

    and evalCallExpression e1 state =
        (parse {
            let! e2 = evalArguments
            let first = exp.Call (e1, Reflection.IDynamicMemberInfo.Op_Call, [| e2 |]) :> exp
            let! rest = many ((evalArguments |>> fun a -> "args", a) <|> evalBracketNotation <|> evalDotNotation)
            let! s = getUserState
            return rest |> List.fold (
                fun acc (t, e) ->
                    match t with
                    | "args" -> 
                        exp.Call (acc, Reflection.IDynamicMemberInfo.Op_Call, [| e |]) :> exp
                    | "bracket" ->  
                        let convert = exp.Call (e, Reflection.IDynamicMemberInfo.ConvertToString, [||]) :> exp   
                        let convert = exp.Property (convert, Reflection.IStringMemberInfo.BaseValue) :> exp                       
                        let args = [| convert; exp.Convert(acc, typeof<IReferenceBase>) :> exp; exp.Constant (s.strict.Head |> fst) :> exp |]
                        let r = exp.Call (Expressions.Environment, Reflection.IEnvironmentMemberInfo.CreateReference, args) :> exp  
                        let r = exp.Convert (r, typeof<IDynamic>)
                        r :> exp
                    | "dot" ->                         
                        let args = [| e; exp.Convert(acc, typeof<IReferenceBase>) :> exp; exp.Constant (s.strict.Head |> fst) :> exp |]
                        let r = exp.Call (Expressions.Environment, Reflection.IEnvironmentMemberInfo.CreateReference, args) :> exp 
                        let r = exp.Convert (r, typeof<IDynamic>)
                        r :> exp           
            ) first
        }) state

    and evalArguments state =
        (skipToken "(" >>. evalArgumentList .>> skipToken ")") state

    and evalArgumentList state =
        (parse {
            let! r = sepBy (skipIgnorableTokens >>. evalAssignmentExpression) (skipToken ",")
            let r = r |> List.map (fun e -> exp.Property (e, Reflection.IDynamicMemberInfo.Value) :> exp)
            let args = [| exp.NewArrayInit (typeof<IDynamic>, r) :> exp |]
            return exp.Call (Expressions.Environment, Reflection.IEnvironmentMemberInfo.CreateArgs, args) :> exp
        }) state

    and evalBracketNotation state =
        (betweenBrackets evalExpression |>> fun a -> "bracket", a) state
        
    and evalDotNotation state =
        ((skipToken "." >>. evalIdentifierName) |>> fun e -> "dot", exp.Constant e :> exp) state

    and evalLeftHandSideExpression state =
        (
            parse {
                let! e1 = attempt evalMemberExpression
                let! e2 = opt (attempt (evalCallExpression e1))
                return if e2.IsSome then e2.Value else e1
            } <|>
            evalNewExpression
        ) state

    and evalPostfixExpression state =
        (parse {
            let! e1 = evalLeftHandSideExpression
            let! op = skipIgnorableTokens >>. opt evalPostfixOperator
            match op with
            | Some op ->
                return exp.Call (e1, op, [||]) :> exp
            | None ->
                return e1
        }) state

    and evalPostfixOperator =  
        choice [|
            skipString "++" |>> fun () -> Reflection.IDynamicMemberInfo.Op_PostfixIncrement
            skipString "--" |>> fun () -> Reflection.IDynamicMemberInfo.Op_PostfixDecrement
        |]

    and evalUnaryExpression state =
        (parse {
            let! op = opt evalUnaryOperator
            match op with
            | Some op ->
                let! r = skipIgnorableTokens >>. evalUnaryExpression
                return exp.Call (r, op, [||]) :> exp
            | None ->
                let! r = evalPostfixExpression
                return r
        }) state

    and evalUnaryOperator =  
        choice [|
            skipString "++" |>> fun () -> Reflection.IDynamicMemberInfo.Op_PrefixIncrement
            skipString "--" |>> fun () -> Reflection.IDynamicMemberInfo.Op_PrefixDecrement
            skipString "+" |>> fun () -> Reflection.IDynamicMemberInfo.Op_Plus
            skipString "-" |>> fun () -> Reflection.IDynamicMemberInfo.Op_Minus
            skipString "~" |>> fun () -> Reflection.IDynamicMemberInfo.Op_BitwiseNot
            skipString "!" |>> fun () -> Reflection.IDynamicMemberInfo.Op_LogicalNot
            attempt (skipIdentifierName "delete") |>> fun () -> Reflection.IDynamicMemberInfo.Op_Delete
            attempt (skipIdentifierName "void") |>> fun () -> Reflection.IDynamicMemberInfo.Op_Void
            attempt (skipIdentifierName "typeof") |>> fun () -> Reflection.IDynamicMemberInfo.Op_Typeof
        |]

    and evalBinaryExpression (p1:Parser<exp, CompileState>) p2 (state:FParsec.State<CompileState>) =
        (parse {
            let! e1 = p1
            let e1 = if e1.Type <> typeof<IDynamic> then exp.Convert(e1, typeof<IDynamic>) :> exp else e1
            let folder e1 (op, e2:exp) = 
                let e2 = if e2.Type <> typeof<IDynamic> then exp.Convert(e2, typeof<IDynamic>) :> exp else e2
                let args = [| e2 |]
                exp.Call (e1, op, args) :> exp
            let! r = manyFold e1 folder (attempt (tuple2 (skipIgnorableTokens >>. p2) (skipIgnorableTokens >>. p1)))
            return r
        }) state

    and evalMultiplicativeExpression state =
        evalBinaryExpression evalUnaryExpression evalMultiplicativeOperator state

    and evalMultiplicativeOperator =  
        choice [|
            skipString "*" |>> fun () -> Reflection.IDynamicMemberInfo.Op_Multiplication
            skipString "/" |>> fun () -> Reflection.IDynamicMemberInfo.Op_Division
            skipString "%" |>> fun () -> Reflection.IDynamicMemberInfo.Op_Modulus
        |]

    and evalAdditiveExpression state =
        evalBinaryExpression evalMultiplicativeExpression evalAdditiveOperator state             

    and evalAdditiveOperator = 
        choice [|
            skipString "+" |>> fun () -> Reflection.IDynamicMemberInfo.Op_Addition
            skipString "-" |>> fun () -> Reflection.IDynamicMemberInfo.Op_Subtraction
        |] 

    and evalShiftExpression state =
        evalBinaryExpression evalAdditiveExpression evalBitwiseShiftOperator state            

    and evalBitwiseShiftOperator =  
        choice [|
            skipString "<<" |>> fun () -> Reflection.IDynamicMemberInfo.Op_LeftShift
            skipString ">>>" |>> fun () -> Reflection.IDynamicMemberInfo.Op_UnsignedRightShift
            skipString ">>" |>> fun () -> Reflection.IDynamicMemberInfo.Op_SignedRightShift
        |]

    and evalRelationalExpression state =
        evalBinaryExpression evalShiftExpression evalRelationalOperator state
        
    and evalRelationalOperator =  
        choice [|
            skipString "<=" |>> fun () -> Reflection.IDynamicMemberInfo.Op_LessthanOrEqual
            skipString ">=" |>> fun () -> Reflection.IDynamicMemberInfo.Op_GreaterthanOrEqual
            skipString "<" |>> fun () -> Reflection.IDynamicMemberInfo.Op_Lessthan
            skipString ">" |>> fun () -> Reflection.IDynamicMemberInfo.Op_Greaterthan
            attempt (skipIdentifierName "instanceof") |>> fun () -> Reflection.IDynamicMemberInfo.Op_Instanceof
            attempt (skipIdentifierName "in") |>> fun () -> Reflection.IDynamicMemberInfo.Op_In
        |]
        
    and evalRelationalExpressionNoIn state =
        evalBinaryExpression evalShiftExpression evalRelationalOperatorNoIn state
        
    and evalRelationalOperatorNoIn =  
        choice [|
            skipString "<=" |>> fun () -> Reflection.IDynamicMemberInfo.Op_LessthanOrEqual
            skipString ">=" |>> fun () -> Reflection.IDynamicMemberInfo.Op_GreaterthanOrEqual
            skipString "<" |>> fun () -> Reflection.IDynamicMemberInfo.Op_Lessthan
            skipString ">" |>> fun () -> Reflection.IDynamicMemberInfo.Op_Greaterthan
            attempt (skipIdentifierName "instanceof") |>> fun () -> Reflection.IDynamicMemberInfo.Op_Instanceof
        |]

    and evalEqualityExpression state =
        evalBinaryExpression evalRelationalExpression evalEqualityOperator state

    and evalEqualityExpressionNoIn state =
        evalBinaryExpression evalRelationalExpressionNoIn evalEqualityOperator state

    and evalEqualityOperator =  
        choice [|
            skipString "===" |>> fun () -> Reflection.IDynamicMemberInfo.Op_StrictEquals
            skipString "!==" |>> fun () -> Reflection.IDynamicMemberInfo.Op_StrictDoesNotEquals
            skipString "==" |>> fun () -> Reflection.IDynamicMemberInfo.Op_Equals
            skipString "!=" |>> fun () -> Reflection.IDynamicMemberInfo.Op_DoesNotEquals
        |]

    and evalBitwiseANDExpression state =
        evalBinaryExpression evalEqualityExpression evalBitwiseANDOperator state

    and evalBitwiseANDExpressionNoIn state =
        evalBinaryExpression evalEqualityExpressionNoIn evalBitwiseANDOperator state

    and evalBitwiseANDOperator =  
        skipString "&" |>> fun () -> Reflection.IDynamicMemberInfo.Op_BitwiseAnd

    and evalBitwiseXORExpression state =
        evalBinaryExpression evalBitwiseANDExpression evalBitwiseXOROperator state

    and evalBitwiseXORExpressionNoIn state =
        evalBinaryExpression evalBitwiseANDExpressionNoIn evalBitwiseXOROperator state
        
    and evalBitwiseXOROperator =  
        skipString "^" |>> fun () -> Reflection.IDynamicMemberInfo.Op_BitwiseXor

    and evalBitwiseORExpression state =
        evalBinaryExpression evalBitwiseXORExpression evalBitwiseOROperator state

    and evalBitwiseORExpressionNoIn state =
        evalBinaryExpression evalBitwiseXORExpressionNoIn evalBitwiseOROperator state
        
    and evalBitwiseOROperator =  
        skipString "|" |>> fun () -> Reflection.IDynamicMemberInfo.Op_BitwiseOr

    and evalLogicalANDExpression state =
        evalBinaryExpression evalBitwiseORExpression evalLogicalANDOperator state

    and evalLogicalANDExpressionNoIn state =
        evalBinaryExpression evalBitwiseORExpressionNoIn evalLogicalANDOperator state
        
    and evalLogicalANDOperator =  
        skipString "&&" |>> fun () -> Reflection.IDynamicMemberInfo.Op_LogicalAnd
    
    and evalLogicalORExpression state =
        evalBinaryExpression evalLogicalANDExpression evalLogicalOROperator state

    and evalLogicalORExpressionNoIn state =
        evalBinaryExpression evalLogicalANDExpressionNoIn evalLogicalOROperator state
        
    and evalLogicalOROperator =  
        skipString "||" |>> fun () -> Reflection.IDynamicMemberInfo.Op_LogicalOr
        
    and evalConditionalExpressionCommon parser1 parser2 state =
        (parse {
            let! e1 = parser1
            let! e2 = opt (skipToken "?")
            match e2 with
            | Some _ ->
                do! skipIgnorableTokens 
                let! e2 = evalAssignmentExpression
                do! skipToken ":"
                do! skipIgnorableTokens 
                let! e3 = parser2
                let e1 = exp.Call (e1, Reflection.IDynamicMemberInfo.ConvertToBoolean, [||]) :> exp
                let e1 = exp.Property (e1, Reflection.IBooleanMemberInfo.BaseValue) :> exp
                return exp.Condition (e1, e2, e3) :> exp
            | None ->
                return e1
        }) state

    and evalConditionalExpression state =
        evalConditionalExpressionCommon evalLogicalORExpression evalAssignmentExpression state

    and evalConditionalExpressionNoIn state =
        evalConditionalExpressionCommon evalLogicalORExpressionNoIn evalAssignmentExpressionNoIn state
    
    and evalAssignmentOperator =  
        choice [|
            skipString "=" |>> fun () -> None
            skipString "*=" |>> fun () -> Some Reflection.IDynamicMemberInfo.Op_Multiplication
            skipString "/=" |>> fun () -> Some Reflection.IDynamicMemberInfo.Op_Division
            skipString "%=" |>> fun () -> Some Reflection.IDynamicMemberInfo.Op_Modulus
            skipString "+=" |>> fun () -> Some Reflection.IDynamicMemberInfo.Op_Addition
            skipString "-=" |>> fun () -> Some Reflection.IDynamicMemberInfo.Op_Subtraction
            skipString "<<=" |>> fun () -> Some Reflection.IDynamicMemberInfo.Op_LeftShift
            skipString ">>>=" |>> fun () -> Some Reflection.IDynamicMemberInfo.Op_UnsignedRightShift
            skipString ">>=" |>> fun () -> Some Reflection.IDynamicMemberInfo.Op_SignedRightShift
            skipString "&=" |>> fun () -> Some Reflection.IDynamicMemberInfo.Op_BitwiseAnd
            skipString "^=" |>> fun () -> Some Reflection.IDynamicMemberInfo.Op_BitwiseXor
            skipString "|=" |>> fun () -> Some Reflection.IDynamicMemberInfo.Op_BitwiseOr
        |]

    and evalAssignmentExpressionCommon parser1 parser2 state =
        (parse {
            let! e1 = parser1
            let! op = opt evalAssignmentOperator
            match op with
            | Some op ->
                do! skipIgnorableTokens
                match op with
                | Some op ->
                    let left = e1
                    let left = exp.Convert (e1, typeof<IDynamic>) :> exp
                    let left = exp.Property (left, Reflection.IDynamicMemberInfo.Value) :> exp
                    let! right = parser2
                    let right = exp.Convert (right, typeof<IDynamic>) :> exp
                    let right = exp.Property (right, Reflection.IDynamicMemberInfo.Value) :> exp
                    let performOp = exp.Call (left, op, [| right |]) :> exp
                    return exp.Assign (left, performOp) :> exp
                | None ->
                    let left = e1
                    let left = exp.Convert (e1, typeof<IDynamic>) :> exp
                    let left = exp.Property (left, Reflection.IDynamicMemberInfo.Value)
                    let! right = parser2
                    let right = exp.Convert (right, typeof<IDynamic>) :> exp
                    let right = exp.Property (right, Reflection.IDynamicMemberInfo.Value)
                    return exp.Assign (left, right) :> exp
            | None ->
                return e1
        }) state
                    
    and evalAssignmentExpression state =
        evalAssignmentExpressionCommon evalConditionalExpression evalAssignmentExpression state

    and evalAssignmentExpressionNoIn state =
        evalAssignmentExpressionCommon evalConditionalExpressionNoIn evalAssignmentExpressionNoIn state

    and evalExpressionCommon parser state =
        (parse {
            let! r = sepBy (attempt (skipIgnorableTokens >>. parser)) (attempt skipComma)
            if r.Length > 0 then
                let r = r |> List.map (fun e -> exp.Property (e, Reflection.IDynamicMemberInfo.Value) :> exp)
                return if r.Length = 1 then r.Head else exp.Block (r) :> exp
        }) state

    and evalExpression state =
        evalExpressionCommon evalAssignmentExpression state

    and evalExpressionNoIn state =
        evalExpressionCommon evalAssignmentExpressionNoIn state

     
    and evalStatement state =
        (parse {
            do! skipIgnorableTokens
            let! v = opt (lookAhead evalIdentifierName)
            match v with
            | Some v ->
                let! (r:exp) =
                    match v with
                    | "var" -> evalVariableStatement
                    | "if" -> evalIfStatement
                    | "do" -> evalDoWhileIterationStatement
                    | "while" -> evalWhileIterationStatement
                    | "for" -> evalForIterationStatement
                    | "continue" -> evalContinueStatement
                    | "break" -> evalBreakStatement
                    | "return" -> evalReturnStatement
                    | "with" -> evalWithStatement
                    | "switch" -> evalSwitchStatement
                    | "throw" -> evalThrowStatement
                    | "try" -> evalTryStatement
                    | "debugger" -> evalDebuggerStatement
                    | _ -> attempt evalLabelledStatement <|> evalExpressionStatement
                do! skipIgnorableTokens
                return r
            | None ->
                let! v = opt (lookAhead (anyOf "{;"))
                match v with
                | Some v ->
                    let! (r:exp) =
                        match v with
                        | '{' -> evalBlock
                        | ';' -> evalEmptyStatement
                    return r
                | None ->
                    let! r = evalExpressionStatement
                    return r               
        }) state

    and evalBlock state =
        (betweenBraces (evalStatementList <|> preturn (exp.Convert (Expressions.Undefined, typeof<IDynamic>) :> exp))) state 

    and evalStatementList state =
        (parse {
            let! r = many1 (attempt evalStatement)
            let e = r |> List.filter (fun e -> e.Type <> typeof<Void> || e.NodeType = ExpressionType.Goto || e.NodeType = ExpressionType.Conditional)
            if e.IsEmpty 
            then return exp.Convert (Expressions.Undefined, typeof<IDynamic>) :> exp
            else 
                let r = exp.Block (e)
                if r.Type <> typeof<IDynamic> then
                    return exp.Block (typeof<IDynamic>, Seq.append r.Expressions [| Expressions.Undefined :> exp |]) :> exp
                else
                    return r :> exp
        }) state

    and evalVariableStatement state =
        (skipIdentifierName "var" >>. evalVariableDeclarationList .>> skipStatementTerminator) state

    and evalVariableDeclarationListCommon parser state =
        (parse {
            let! r = sepBy (attempt parser) (attempt skipComma)
            return exp.Block (typeof<IDynamic>, r @ [ Expressions.Undefined :> exp ]) :> exp 
        }) state  

    and evalVariableDeclarationList state =
        evalVariableDeclarationListCommon evalVariableDeclaration state

    and evalVariableDeclarationListNoIn state =
        evalVariableDeclarationListCommon evalVariableDeclarationNoIn state

    and evalVariableDeclarationCommon parser state =
        (parse {            
            let! identifier = skipIgnorableTokens >>. evalIdentifier
            let! oldState = getUserState
            do! setUserState { oldState with variables = identifier::oldState.variables; labels = oldState.labels }
            let args = [| exp.Constant (identifier) :> exp; exp.Constant (oldState.strict.Head |> fst) :> exp |]
            let reference = exp.Call (Expressions.LexicalEnviroment, Reflection.ILexicalEnvironmentMemberInfo.GetIdentifierReference, args) :> exp
            let reference = exp.Convert (reference, typeof<IDynamic>) :> exp
            let reference = exp.Property (reference, Reflection.IDynamicMemberInfo.Value) :> exp
            let! e2 = skipIgnorableTokens >>. (parser <|> preturn (Expressions.Undefined :> exp))
            let e2 = exp.Convert (e2, typeof<IDynamic>) :> exp
            let e2 = exp.Property (e2, Reflection.IDynamicMemberInfo.Value) :> exp
            return exp.Assign (reference, e2) :> exp
        }) state

    and evalVariableDeclaration state =
        evalVariableDeclarationCommon evalInitialiser state

    and evalVariableDeclarationNoIn state =
        evalVariableDeclarationCommon evalInitialiserNoIn state

    and evalInitialiserCommon parser state =
        (skipToken "=" .>> skipIgnorableTokens >>. parser) state

    and evalInitialiser state =
        evalInitialiserCommon evalAssignmentExpression state

    and evalInitialiserNoIn state =
        evalInitialiserCommon evalAssignmentExpressionNoIn state

    and evalEmptyStatement state =
        (skipToken ";" |>> fun () -> exp.Empty() :> exp) state

    and evalExpressionStatement state =
        (parse {
            let check (e:exp) =
                match e with
                | :? MemberExpression as e ->
                    match e.Expression with
                    | :? UnaryExpression as e ->
                        match e.Operand with
                        | :? ConstantExpression as e ->
                            match e.Value with
                            | :? IString as e ->
                                e.BaseValue = "use strict", false
                            | _ -> false, true
                        | _ -> false, true
                    | _ -> false, true
                | _ -> false, true
            let! e, currentState = tuple2 (evalExpression .>> skipStatementTerminator) getUserState            
            let strict, endOfPrologue = currentState.strict.Head 
            if not strict && not endOfPrologue then 
                let strict, endOfPrologue = check e
                do! setUserState ({ currentState with strict = (strict, endOfPrologue)::currentState.strict.Tail  })
                return e
            else
                return e
        }) state
        
    and evalIfStatement state =
        (parse {
            let! e, s1, r = tuple3 (skipIdentifierName "if" >>. betweenParentheses evalExpression) evalStatement (opt (attempt (skipIdentifierName "else")))
            let e = exp.Call (e, Reflection.IDynamicMemberInfo.ConvertToBoolean, [||])
            let e = exp.Property (e, Reflection.IBooleanMemberInfo.BaseValue) :> exp
            match r with
            | Some _ ->
                let! s2 = evalStatement
                return exp.IfThenElse(e, s1, s2) :> exp
            | None ->
                return exp.IfThen(e, s1) :> exp
        }) state

    and evalDoWhileIterationStatement state =
        (parse {            
            let breakLabelExp = exp.Label(exp.Label(typeof<Void>, "breakLoop"))              
            let breakLabel = { labelExpression = breakLabelExp }
            let continueLabelExp = exp.Label(exp.Label(typeof<Void>, "continueLoop"))   
            let continueLabel = { labelExpression = continueLabelExp }
            let breakExpression = exp.Break (breakLabel.labelExpression.Target)
            let! currentState = getUserState
            let labels = currentState.labels.Head.Add("breakLoop", breakLabel).Add("continueLoop", continueLabel)
            do! setUserState { currentState with labels = labels::currentState.labels }
            let p1 = skipIdentifierName "do" >>. evalStatement
            let p2 = skipIdentifierName "while" >>. betweenParentheses evalExpression .>> skipStatementTerminator
            let! s, e = tuple2 p1 p2
            let e = exp.Call (e, Reflection.IDynamicMemberInfo.ConvertToBoolean, [||])
            let e = exp.Property (e, Reflection.IBooleanMemberInfo.BaseValue) :> exp
            let e = exp.Not e
            let body = exp.Block ([| s; exp.IfThen (e, breakExpression) :> exp |])
            let body = exp.Loop (body, breakLabel.labelExpression.Target, continueLabel.labelExpression.Target) :> exp
            do! setUserState currentState
            return exp.Block ([| body; Expressions.Undefined :> exp |]) :> exp
        }) state

                    
    and evalWhileIterationStatement state =
        (parse {              
            let breakLabelExp = exp.Label(exp.Label(typeof<Void>, "breakLoop"))              
            let breakLabel = { labelExpression = breakLabelExp }
            let continueLabelExp = exp.Label(exp.Label(typeof<Void>, "continueLoop"))   
            let continueLabel = { labelExpression = continueLabelExp }
            let breakExpression = exp.Break (breakLabel.labelExpression.Target)
            let! currentState = getUserState
            let labels = currentState.labels.Head.Add("breakLoop", breakLabel).Add("continueLoop", continueLabel)
            do! setUserState { currentState with labels = labels::currentState.labels }
            let! e, s = tuple2 (skipIdentifierName "while" >>. betweenParentheses evalExpression) evalStatement
            let e = exp.Call (e, Reflection.IDynamicMemberInfo.ConvertToBoolean, [||])
            let e = exp.Property (e, Reflection.IBooleanMemberInfo.BaseValue) :> exp
            let body = exp.IfThenElse (e, s, breakExpression)
            let body =  exp.Loop (body, breakLabel.labelExpression.Target, continueLabel.labelExpression.Target) :> exp
            do! setUserState currentState
            return exp.Block ([| body; Expressions.Undefined :> exp |]) :> exp
        }) state 

    and evalForIterationStatement state =
        (parse {             
            let breakLabelExp = exp.Label(exp.Label(typeof<Void>, "breakLoop"))              
            let breakLabel = { labelExpression = breakLabelExp }
            let continueLabelExp = exp.Label(exp.Label(typeof<Void>, "continueLoop"))   
            let continueLabel = { labelExpression = continueLabelExp }
            let breakExpression = exp.Break (breakLabel.labelExpression.Target)
             
                             
            let evalForIterationStatements state =
                let buildInit (e:option<exp>) =
                    match e with
                    | Some e -> e
                    | None -> Expressions.Undefined :> exp 

                let buildCondition (e:option<exp>) =
                    match e with
                    | Some e -> 
                        let e = exp.Call (e, Reflection.IDynamicMemberInfo.ConvertToBoolean, [||])
                        exp.Property (e, Reflection.IBooleanMemberInfo.BaseValue) :> exp
                    | None -> exp.Constant (true) :> exp  

                let buildFollowUp (e:option<exp>) =
                    match e with
                    | Some e -> 
                        exp.Property (e, Reflection.IDynamicMemberInfo.Value) :> exp
                    | None -> Expressions.Undefined :> exp
                               
                let evalFor state = 
                    (parse {
                        let p1 = opt evalExpressionNoIn .>> skipToken ";"
                        let p2 = opt evalExpression .>> skipToken ";"
                        let p3 = opt evalExpression .>> skipToken ")"
                        let p4 = evalStatement
                        let! e1, e2, e3, s = tuple4 p1 p2 p3 p4

                        let init = buildInit e1
                        let condition = buildCondition e2
                        let followUp = buildFollowUp e3
                        let r = exp.IfThenElse (condition, s, breakExpression) :> exp
                        let r = exp.Block ([| r; followUp |]) :> exp
                        let r = exp.Loop (r, breakLabelExp.Target, continueLabelExp.Target) :> exp
                        return exp.Block ([| init; r; Expressions.Undefined :> exp |]) :> exp
                    }) state

                let evalForWithVar state = 
                    (parse {                    
                        let p1 = evalVariableDeclarationListNoIn
                        let p2 = skipToken ";" >>. opt evalExpression
                        let p3 = skipToken ";" >>. opt evalExpression
                        let p4 = skipToken ")" >>. evalStatement
                        let! e1, e2, e3, s = tuple4 p1 p2 p3 p4

                        let init = e1
                        let condition = buildCondition e2
                        let followUp = buildFollowUp e3
                        let r = exp.IfThenElse (condition, s, breakExpression) :> exp
                        let r = exp.Block ([| r; followUp |]) :> exp
                        let r = exp.Loop (r, breakLabelExp.Target, continueLabelExp.Target) :> exp
                        return exp.Block ([| init; r; Expressions.Undefined :> exp |]) :> exp
                    }) state

                let evalForIn state = 
                    (parse {                    
                        let p1 = evalLeftHandSideExpression
                        let p2 = skipIdentifierName "in" >>. evalExpression
                        let p3 = skipToken ")" >>. evalStatement
                        let! e1, e2, s = tuple3 p1 p2 p3

                        let enumeratorVar = exp.Variable(typeof<IEnumerator<string>>, "enumerator")
                        let obj = exp.Call (e2, Reflection.IDynamicMemberInfo.ConvertToObject, Array.empty) :> exp
                        let asEnumerableString = exp.Convert(obj, typeof<IEnumerable<string>>)
                        let getEnumerator = exp.Call (asEnumerableString, typeof<IEnumerable<string>>.GetMethod "GetEnumerator", Array.empty) :> exp
                        let assignEnumeratorVar = exp.Assign (enumeratorVar, getEnumerator) :> exp
                        let asEnumeratorString = exp.Convert(enumeratorVar, typeof<IEnumerator<string>>)                 
                        let current = exp.Call (asEnumeratorString, typeof<IEnumerator<string>>.GetMethod "get_Current", Array.empty) :> exp
                        let asEnumerator = exp.Convert(enumeratorVar, typeof<IEnumerator>)
                        let moveNext = exp.Call (asEnumerator, typeof<IEnumerator>.GetMethod "MoveNext", Array.empty)
                        let asDisposable = exp.Convert(enumeratorVar, typeof<IDisposable>)
                        let dispose = exp.Call (asDisposable, typeof<IDisposable>.GetMethod "Dispose", Array.empty) :> exp
                        let createString = exp.Call (Expressions.Environment, Reflection.IEnvironmentMemberInfo.CreateString, [| current |]) :> exp
                        let putCurrent = exp.Call (e1, Reflection.IDynamicMemberInfo.Value.GetSetMethod(), [| createString |]) :> exp
                        let ifTrue = exp.Block ([| putCurrent; s |]) :> exp
                        let loopCondition = exp.IfThenElse (moveNext, ifTrue, exp.Break(breakLabelExp.Target))
                        let loop = exp.Loop (loopCondition, breakLabelExp.Target, continueLabelExp.Target)
                        let initTest = exp.Not(exp.Or (exp.TypeIs (e1, typeof<IUndefined>), exp.TypeIs (e1, typeof<INull>)))
                        let initCondition = exp.IfThen (initTest, loop) :> exp
                        return exp.Block ([| enumeratorVar |], exp.TryFinally (exp.Block ([| assignEnumeratorVar; initCondition |]) :> exp, dispose)) :> exp
                    }) state

                let evalForInWithVar state = 
                    (parse {
                        let p1 = evalVariableDeclarationNoIn
                        let p2 = skipIdentifierName "in" >>. evalExpression
                        let p3 = skipToken ")" >>. evalStatement  
                        let! e1, e2, s = tuple3 p1 p2 p3

                        let! currentState = getUserState
                        let enumeratorVar = exp.Variable(typeof<IEnumerator<string>>, "enumerator")
                        let varName = currentState.variables.Head 
                        let args = [| exp.Constant (varName) :> exp; exp.Constant (currentState.strict.Head |> fst) :> exp |]
                        let varRef = exp.Call (Expressions.LexicalEnviroment :> exp, Reflection.ILexicalEnvironmentMemberInfo.GetIdentifierReference, args) :> exp
                        let experValue = exp.Property (e2, Reflection.IDynamicMemberInfo.Value) :> exp
                        let obj = exp.Call (experValue, Reflection.IDynamicMemberInfo.ConvertToObject, Array.empty)              
                        let asEnumerableString = exp.Convert(obj, typeof<IEnumerable<string>>)
                        let getEnumerator = exp.Call (asEnumerableString, typeof<IEnumerable<string>>.GetMethod "GetEnumerator", Array.empty) :> exp
                        let assignEnumeratorVar = exp.Assign (enumeratorVar, getEnumerator) :> exp       
                        let asEnumeratorString = exp.Convert(enumeratorVar, typeof<IEnumerator<string>>)      
                        let current = exp.Call (asEnumeratorString, typeof<IEnumerator<string>>.GetMethod "get_Current", Array.empty) :> exp
                        let asEnumerator = exp.Convert(enumeratorVar, typeof<IEnumerator>)
                        let moveNext = exp.Call (asEnumerator, typeof<IEnumerator>.GetMethod "MoveNext", Array.empty)
                        let asDisposable = exp.Convert(enumeratorVar, typeof<IDisposable>)
                        let dispose = exp.Call (asDisposable, typeof<IDisposable>.GetMethod "Dispose", Array.empty) :> exp 
                        let createString = exp.Call (Expressions.Environment, Reflection.IEnvironmentMemberInfo.CreateString, [| current |]) :> exp
                        let putCurrent = exp.Call (varRef, Reflection.IDynamicMemberInfo.Value.GetSetMethod(), [| createString |]) :> exp
                        let ifTrue = exp.Block ([| putCurrent; s |]) :> exp
                        let loopCondition = exp.IfThenElse (moveNext, ifTrue, exp.Break(breakLabel.labelExpression.Target))
                        let loop = exp.Loop (loopCondition, breakLabel.labelExpression.Target, continueLabel.labelExpression.Target)
                        let initTest = exp.Not(exp.Or (exp.TypeIs (experValue, typeof<IUndefined>), exp.TypeIs (experValue, typeof<INull>)))
                        let initCondition = exp.IfThen (initTest, loop) :> exp
                        let r = exp.Block ([| assignEnumeratorVar; initCondition |]) :> exp
                        return exp.Block ([| enumeratorVar |], exp.TryFinally (r , dispose)) :> exp
                    }) state

                (parse {
                    let! r = skipIdentifierName "for" >>. skipToken "(" >>. opt (attempt (skipIdentifierName "var"))
                    let! e = 
                        match r with
                        | Some _ -> attempt evalForWithVar <|> attempt evalForInWithVar
                        | None -> attempt evalFor <|> attempt evalForIn
                    return e
                }) state   
                         
            let! currentState = getUserState
            let labels = currentState.labels.Head.Add("breakLoop", breakLabel).Add("continueLoop", continueLabel)
            do! setUserState { currentState with labels = labels::currentState.labels }
            let! r = evalForIterationStatements
            do! setUserState currentState
            return exp.Block ([| r; Expressions.Undefined :> exp |]) :> exp
        }) state

    and evalContinueStatement state =
        let identifierNotFound = "The continue statement is invalid because the label '{0}' does not exist in the surrounding scope."
        let noSurroundingLoop = "The continue statement with no identifier requires a surrounding iteration statement."
        (parse {
            let p1 = skipIdentifierName "continue" >>. skipMany evalWhiteSpace >>. opt evalIdentifier .>> skipStatementTerminator
            let! identifier, currentState = tuple2 p1 getUserState
            let labels = currentState.labels.Head
            match identifier with
            | Some identifier ->
                let label = labels.TryFind ("continue" + identifier)
                match label with
                | Some label ->
                    return exp.Continue (label.labelExpression.Target) :> exp
                | None ->
                    let msg = String.Format (identifierNotFound, identifier) 
                    let err = environment.CreateSyntaxError msg
                    raise err
            | None ->
                let label = labels.TryFind "continueLoop"
                match label with
                | Some label ->
                    return exp.Continue (label.labelExpression.Target) :> exp
                | None ->
                    let msg = String.Format (noSurroundingLoop, identifier) 
                    let err = environment.CreateSyntaxError msg
                    raise err
        }) state

    and evalBreakStatement state =
        let identifierNotFound = "The break statement is invalid because the label '{0}' does not exist in the surrounding scope."
        let noSurrounding = "The break statement with no identifier requires a surrounding iteration or switch statement."
        (parse {
            let p1 = skipIdentifierName "break" >>. skipMany evalWhiteSpace >>. opt evalIdentifier .>> skipStatementTerminator
            let! identifier, currentState = tuple2 p1 getUserState
            let labels = currentState.labels.Head
            match identifier with
            | Some identifier ->
                let label = labels.TryFind ("break" + identifier)
                match label with
                | Some label ->
                    return exp.Break (label.labelExpression.Target, Expressions.Undefined :> exp, typeof<IDynamic>) :> exp
                | None ->
                    let msg = String.Format (identifierNotFound, identifier) 
                    let err = environment.CreateSyntaxError msg
                    raise err
            | None ->
                let labelSwitch = labels.TryFind "breakSwitch"
                let labelLoop = labels.TryFind "breakLoop"
                match labelSwitch, labelLoop with
                | Some labelSwitch, None
                | Some labelSwitch, Some _ ->
                    return exp.Break (labelSwitch.labelExpression.Target, Expressions.Undefined :> exp, typeof<IDynamic>) :> exp
                | None, Some labelLoop ->
                    return exp.Break (labelLoop.labelExpression.Target, Expressions.Undefined :> exp, typeof<IDynamic>) :> exp
                | None, None ->
                    let msg = String.Format (noSurrounding, identifier) 
                    let err = environment.CreateSyntaxError msg
                    raise err
        }) state

    and evalReturnStatement state =
        (parse {
            let! e = skipIdentifierName "return" >>. skipMany evalWhiteSpace >>. opt evalExpression .>> skipStatementTerminator
            let! currentState = getUserState
            let returnLabel = currentState.labels.Head.["return"].labelExpression
            let e = match e with | Some e -> exp.Property(e, Reflection.IDynamicMemberInfo.Value) :> exp | None -> Expressions.Undefined :> exp
            return exp.Return (returnLabel.Target, e, typeof<IDynamic>) :> exp
        }) state

    and evalWithStatement state =
        (parse {
            let! e, s, currentState = tuple3 (skipIdentifierName "with" >>. betweenParentheses evalExpression) evalStatement getUserState
            match currentState.strict.Head with
            | false, _ ->              
                let oldEnvVar = exp.Variable(typeof<ILexicalEnvironment>, "oldEnv")
                let newEnvVar = exp.Variable(typeof<ILexicalEnvironment>, "newEnv")
                let variables = [| oldEnvVar; newEnvVar |]
                let obj = exp.Call (e, Reflection.IDynamicMemberInfo.ConvertToObject, Array.empty) :> exp
                let assignOldEnvVar = exp.Assign (oldEnvVar, Expressions.LexicalEnviroment :> exp) :> exp
                let newObjEnv = exp.Call (oldEnvVar, Reflection.ILexicalEnvironmentMemberInfo.NewObjectEnvironment, [| obj; exp.Constant (true) :> exp |]) :> exp
                let assignNewEnvVar = exp.Assign (newEnvVar, newObjEnv) :> exp
                let assignNewEnv = exp.Assign (Expressions.LexicalEnviroment :> exp, newEnvVar) :> exp 
                let assignOldEnv = exp.Assign (Expressions.LexicalEnviroment :> exp, oldEnvVar) :> exp  
                let tryBody = exp.Block ([| assignOldEnvVar; assignNewEnvVar; assignNewEnv; s |])
                let finallyBody = exp.Block ([| assignOldEnv |])
                return exp.Block (variables, exp.TryFinally (tryBody, finallyBody)) :> exp
            | _ ->
                raise (environment.CreateSyntaxError "The with statement is not allowed in strict mode.")
        }) state

    and evalSwitchStatement state =
        (parse {
            let! e, currentState = tuple2 (skipIdentifierName "switch" >>. betweenParentheses evalExpression) getUserState
            let breakName = "breakSwitch" 
            let breakLabelExp = exp.Label(exp.Label(typeof<IDynamic>, breakName), Expressions.Undefined :> exp)
            let breakLabel = breakName, { labelExpression = breakLabelExp }
            let labels = currentState.labels.Head.Add(breakLabel) 
            let newState = { currentState with labels = labels::currentState.labels }
            let! caseBlock = setUserState newState >>. evalCaseBlock .>> setUserState currentState
            let switch = 
                match caseBlock with
                | [], None, [] ->
                    exp.Switch (typeof<IDynamic>, e, Expressions.Undefined :> exp, equalityTestMethod, [||]) :> exp
                | [], Some defaultClause, [] ->
                    exp.Switch (typeof<IDynamic>, e, defaultClause, equalityTestMethod, [||]) :> exp
                | beforeCaseClauses, None, afterCaseClauses ->
                    exp.Switch (typeof<IDynamic>, e, Expressions.Undefined :> exp, equalityTestMethod, beforeCaseClauses @ afterCaseClauses) :> exp
                | beforeCaseClauses, Some defaultClause, afterCaseClauses ->
                    exp.Switch (typeof<IDynamic>, e, defaultClause, equalityTestMethod, beforeCaseClauses @ afterCaseClauses) :> exp
            return exp.Block ([| switch; breakLabelExp :> exp; |]) :> exp
        }) state

    and evalCaseBlock state =
        (parse {
            do! skipToken "{"
            let! beforeCaseClauses = evalCaseClauses
            let! defaultClause = opt evalDefaultClause
            let! afterCaseClauses = evalCaseClauses
            do! skipToken "}"
            return beforeCaseClauses, defaultClause, afterCaseClauses 
        }) state        

    and evalCaseClauses state =
        many (attempt evalCaseClause) state

    and evalCaseClause state =
        (parse {
            let! e1 = skipToken "case" >>. evalExpression .>> skipToken ":"
            let! e = opt evalStatementList           
            match e with
            | Some e -> return exp.SwitchCase (e, [| e1 |])
            | None -> return exp.SwitchCase (Expressions.Undefined :> exp, [| e1 |])
        }) state

    and evalDefaultClause state =
        (parse {
            do! skipToken "default" >>. skipToken ":"
            let! e = opt evalStatementList            
            match e with
            | Some e -> return e
            | None -> return exp.Convert (Expressions.Undefined :> exp, typeof<IDynamic>) :> exp
        }) state     

    and evalLabelledStatement state =
        (parse {
            let! identifier = evalIdentifier
            do! skipToken ":"
            let breakName = "break" + identifier 
            let breakLabelExp = exp.Label(exp.Label(typeof<Void>, breakName))
            let breakLabel = breakName, { labelExpression = breakLabelExp }
            let continueName = "continue" + identifier
            let continueLabelExp = exp.Label(exp.Label(typeof<Void>, continueName))
            let continueLabel = continueName, { labelExpression = continueLabelExp }
            let! currentState = getUserState 
            let labels = currentState.labels.Head.Add(breakLabel).Add(continueLabel) 
            let newState = { currentState with labels = labels::currentState.labels }
            let! s = setUserState newState >>. evalStatement .>> setUserState currentState
            return exp.Block (typeof<IDynamic>, [| continueLabelExp :> exp; s; breakLabelExp :> exp; Expressions.Undefined :> exp |]) :> exp
        }) state

    and evalThrowStatement state =
        (parse {
            do! skipIdentifierName "throw" >>. skipMany evalWhiteSpace
            let! e = evalExpression 
            return exp.Block ([| exp.Call (e, Reflection.IDynamicMemberInfo.Op_Throw, [||]) :> exp; Expressions.Undefined :> exp |]) :> exp
        }) state

    and evalTryStatement state =
        let missingCatchFinally = "The try statement requires either a catch block or a finally block."
        (parse {
            let! e1, e2, e3 = tuple3 (skipIdentifierName "try" >>. evalBlock) (opt (attempt evalCatch)) (opt (attempt evalFinally))
            match e2, e3 with
            | Some e2, None ->
                return exp.TryCatch (e1, [| e2 |]) :> exp
            | None, Some e3 ->
                return exp.TryFinally (e1, e3) :> exp
            | Some e2, Some e3 ->
                return exp.TryCatchFinally (e1, e3, [| e2 |]) :> exp
            | None, None ->
                raise (environment.CreateSyntaxError missingCatchFinally)
        }) state

    and evalCatch state =
        (parse {
            let! identifier, block = tuple2 (skipIdentifierName "catch" >>. betweenParentheses evalIdentifier) evalBlock
            let catchVar = exp.Variable(typeof<MacheteRuntimeException>, "catch")
            let oldEnvVar = exp.Variable(typeof<ILexicalEnvironment>, "oldEnv")
            let catchEnvVar = exp.Variable(typeof<ILexicalEnvironment>, "catchEnv")
            let catchRecVar = exp.Variable(typeof<IEnvironmentRecord>, "catchRec")
            let assignOldEnv = exp.Assign (oldEnvVar, Expressions.LexicalEnviroment :> exp) :> exp
            let newEnv = exp.Call (oldEnvVar, Reflection.ILexicalEnvironmentMemberInfo.NewDeclarativeEnvironment, Array.empty) :> exp 
            let assignCatchEnv = exp.Assign (catchEnvVar, newEnv) :> exp
            let getRec = exp.Call (catchEnvVar, Reflection.ILexicalEnvironmentMemberInfo.Record.GetGetMethod(), Array.empty) :> exp 
            let assignCatchRec = exp.Assign (catchRecVar, exp.Convert(getRec, typeof<IEnvironmentRecord>)) :> exp
            let identifier = exp.Constant (identifier) :> exp
            let boolFalse = exp.Constant (false) :> exp
            let createBinding = exp.Call (catchRecVar, Reflection.IEnvironmentRecordMemberInfo.CreateMutableBinding, [| identifier; boolFalse |]) :> exp 
            let getThrown = exp.Call (catchVar, typeof<MacheteRuntimeException>.GetMethod "get_Thrown", Array.empty) :> exp 
            let setBinding = exp.Call (catchRecVar, Reflection.IEnvironmentRecordMemberInfo.SetMutableBinding, [| identifier; getThrown; boolFalse |]) :> exp
            let assignEnv = exp.Assign (Expressions.LexicalEnviroment :> exp,  catchEnvVar) :> exp 
            let tryBody = exp.Block ([| assignOldEnv; assignCatchEnv; assignCatchRec; createBinding; setBinding; assignEnv; block |]) :> exp
            let assignOldEnv = exp.Assign (Expressions.LexicalEnviroment :> exp,  oldEnvVar) :> exp 
            let finallyBody = exp.Block ([| assignOldEnv |]) :> exp      
            let body = exp.Block ([| oldEnvVar; catchEnvVar; catchRecVar |], exp.TryFinally (tryBody, finallyBody)) :> exp      
            return exp.Catch (catchVar, body)
        }) state     

    and evalFinally state =
        (skipIdentifierName "finally" >>. evalBlock) state

    and evalDebuggerStatement state =
        (skipIdentifierName "debugger" >>. skipStatementTerminator >>. preturn (exp.Empty() :> exp)) state

    and evalFunctionDeclaration state =
        let missingIdentifier = "The function declared at (Ln: {0}, Col: {1}) is missing an identifier."
        let badFPList = "The function '{0}' declared at (Ln: {1}, Col: {2}) has an incomplete formal parameter list."
        (parse {
            let! (oldState:CompileState) = getUserState
            let! posStart = getPosition
            do! skipToken "function"
            do! skipIgnorableTokens
            let! identifier = evalIdentifier <?> String.Format(missingIdentifier, posStart.Line, posStart.Column)
            do! skipToken "("
            do! skipIgnorableTokens
            let! formalParameterList = evalFormalParameterList
            do! skipToken ")"
            do! skipToken "{"
            do! skipIgnorableTokens
            let! functionBody = evalFunctionBody
            do! skipToken "}"
            let! (state:CompileState) = getUserState
            let variableDeclarations = ReadOnlyList<string>(state.variables |> List.rev)
            let functionDeclarations = ReadOnlyList<FunctionDeclaration>(state.functions |> List.rev)
            let ec = ExecutableCode (functionBody.Compile(), variableDeclarations, functionDeclarations, state.strict.Head |> fst)
            let func = FunctionDeclaration(identifier, formalParameterList, ec)
            do! setUserState ({ oldState with functions = func::oldState.functions  })
            return exp.Empty() :> exp
        }) state

    and evalFunctionExpression state =
        (parse {
            let! (oldState:CompileState) = getUserState
            let! posStart = getPosition
            do! skipToken "function"
            let! identifier = opt evalIdentifier
            do! skipToken "("
            do! skipIgnorableTokens
            let! formalParameterList = evalFormalParameterList
            do! skipToken ")"
            do! skipToken "{"
            let! functionBody = evalFunctionBody
            do! skipToken "}"
            let! (newState:CompileState) = getUserState
            let variableDeclarations = ReadOnlyList<string>(newState.variables |> List.rev)
            let functionDeclarations = ReadOnlyList<FunctionDeclaration>(newState.functions |> List.rev)
            let strict = newState.strict.Head |> fst
            let exC = ExecutableCode (functionBody.Compile(), variableDeclarations, functionDeclarations, strict)
            let args = [| exp.Constant(exC):>exp; exp.Constant(formalParameterList) :> exp; Expressions.LexicalEnviroment :> exp |]
            let func = exp.Call (Expressions.Environment, Reflection.IEnvironmentMemberInfo.CreateFunction, args) :> exp
            do! setUserState oldState    
            return func
        }) state

    and evalFormalParameterList state =
        (parse {
            let! r = sepBy (attempt (skipIgnorableTokens >>. evalIdentifier)) (skipToken ",")
            return ReadOnlyList<string> r
        }) state

    and evalFunctionBody state =
        (parse {            
            let! (oldState:CompileState) = getUserState
            let returnLabel = oldState.labels.Head.["return"]
            let nextState = { strict = [oldState.strict.Head |> fst, false]; labels = [ Map.ofArray [| "return", returnLabel |] ]; functions = []; variables = [] }
            do! setUserState nextState
            let! e1 = many (attempt evalSourceElement)        
            let body = exp.Block (e1 @ [ returnLabel.labelExpression :> exp ])
            return exp.Lambda<Code>(body, [| Expressions.Environment; Expressions.Args |])
        }) state

    and evalSourceElement state =
        (attempt evalFunctionDeclaration <|> evalStatement) state

    and evalSourceElements state =
        manyTill (evalSourceElement) eof state      
         
    and evalProgram (state:FParsec.State<CompileState>) =
        let complete (expressions:list<exp>) =
            let returnLabel = state.UserState.labels.Head.["return"].labelExpression
            let expressions = expressions |> List.filter (fun e -> e.Type <> typeof<Void> || e.NodeType = ExpressionType.Goto || e.NodeType = ExpressionType.Conditional)
            match expressions.Length with
            | 0 -> exp.Block ([| returnLabel :> exp |]) :> exp 
            | c ->
                let last = (expressions |> List.rev).Head
                match last with
                | _ when last.Type = typeof<Void> ->
                    exp.Block (expressions @ [ returnLabel :> exp ]) :> exp  
                | :? GotoExpression as last when last.Target = returnLabel.Target ->
                    exp.Block (expressions @ [ returnLabel :> exp ]) :> exp  
                | _ ->
                    let last = exp.Return (returnLabel.Target, last) :> exp
                    exp.Block (Seq.append (expressions |> Seq.take (c - 1)) ([| last; returnLabel :> exp |])) :> exp            
        (parse {            
            let! (oldState:CompileState) = getUserState
            let returnLabel = oldState.labels.Head.["return"]
            let! e1 = evalSourceElements
            let body = complete e1
            return exp.Lambda<Code>(body, [| Expressions.Environment; Expressions.Args |])
        }) state

    let createInitialState (strict:bool) =
        let returnLabel = { labelExpression = exp.Label(exp.Label(typeof<IDynamic>, "return"), exp.Constant (environment.Undefined)) }
        { strict = [strict, false]; labels = [ Map.ofArray [| "return", returnLabel |] ]; functions = []; variables = [] }        

    let compile (parser:Parser<Expression<Code>, CompileState>) (input:string) (strict:bool) (streamName:string) =
        let initialState = createInitialState strict
        let input = input.Trim()
        stopwatch.Restart ()
        let result = runParserOnString parser initialState streamName input
        environment.Output.Write ("Parse Time: " + stopwatch.Elapsed.ToString())
        match result with
        | Success (expression, finalState, position) ->
            let variableDeclarations = ReadOnlyList<string>(finalState.variables |> List.rev)
            let functionDeclarations = ReadOnlyList<FunctionDeclaration>(finalState.functions |> List.rev)
            let strict = finalState.strict.Head |> fst
            stopwatch.Restart ()
            let code = expression.Compile()
            environment.Output.Write ("Compile Time: " + stopwatch.Elapsed.ToString())
            ExecutableCode(code, variableDeclarations, functionDeclarations, strict)
        | Failure (message, error, finalState) -> 
            raise (environment.CreateSyntaxError message) 
                    
    member this.CompileGlobalCode (input:string) =
        compile evalProgram input false "GlobalCode"
        
    member this.CompileEvalCode (input:string, strict:bool) =
        compile evalProgram input strict "EvalCode"

    member this.CompileFunctionCode (input:string, strict:bool) =
        compile evalFunctionBody input strict "FunctionCode"
        
    member this.CompileFormalParameterList (input:string) =  
        let initialState = createInitialState false   
        let result = runParserOnString evalFormalParameterList initialState "FormalParameterList" input
        match result with
        | Success (result, finalState, position) -> result
        | Failure (message, error, finalState) -> 
            raise (environment.CreateSyntaxError message) 

    static member private equalityTest (left:obj, right:obj) =
        let left = left :?> IDynamic
        let right = right :?> IDynamic
        left.Op_StrictEquals(right).ConvertToBoolean().BaseValue