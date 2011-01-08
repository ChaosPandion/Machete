namespace Machete.Compiler

open System
open System.Text
open System.Linq.Expressions
open FParsec.CharParsers
open FParsec.Primitives
open Machete.Interfaces

type internal exp = System.Linq.Expressions.Expression

type internal Function = {
    identifier : string
    formalParameterList : ReadOnlyList<string>
    strict : bool
    functionBody : SourceElement 
}


type internal State1 = {
    strict : list<bool * bool>
    element : SourceElement
    labels : list<Map<string, LabelExpression>>
    functions : list<FunctionDeclaration>
    variables : list<string>
}

type CompilerService (environment:IEnvironment) =

    let environmentParam = exp.Parameter (Reflection.IEnvironment.t, "environment")
    let argsParam = exp.Parameter (Reflection.IArgs.t, "args")
    
    let propEmptyArgs = exp.Property(environmentParam, "EmptyArgs") :> exp
    let propContext = exp.Property (environmentParam, "Context") :> exp
    let propThisBinding = exp.Property (propContext, "ThisBinding") :> exp
    let propLexicalEnviroment = exp.Property (propContext, "LexicalEnviroment") :> exp
    let propVariableEnvironment = exp.Property (propContext, "VariableEnviroment") :> exp
    
    let nullIDynamic = exp.Constant (null, typeof<IDynamic>) :> exp
    let trueNullableBool = exp.Constant (true, typeof<Nullable<bool>>) :> exp
    let falseNullableBool = exp.Constant (false, typeof<Nullable<bool>>) :> exp
    let nullNullableBool = exp.Constant (null, typeof<Nullable<bool>>) :> exp
        
    // White Space

    let rec evalWhiteSpace state =
        satisfy CharSets.isWhiteSpace state     

    // Line Terminators

    and evalLineTerminator state =
        satisfy CharSets.isLineTerminator state
           
    and evalLineTerminatorSequence state =
        (pstring "\r\n" <|> (satisfy CharSets.isLineTerminator |>> string))        

    // Comments

    and evalComment state =
        (evalMultiLineComment <|> evalSingleLineComment) state
         
    and evalMultiLineComment state =
        (parse {
            do! skipString "/*"
            let! r = manyCharsTill anyChar (lookAhead (pstring "*/"))
            do! skipString "*/"
            return r
        }) state
           
    and evalSingleLineComment state =
        (parse {
            do! skipString "//"
            let! r = manyCharsTill anyChar (lookAhead (satisfy CharSets.isLineTerminator))
            return r
        }) state

    // Identifier Names
    
    and evalIdentifier state =
        (attempt <| parse {
            let! r = evalIdentifierName
            if not (CharSets.reservedWordSet.Contains r) then
                return r
        }) state

    and evalIdentifierName state =
        (parse {
            let! start = evalIdentifierStart
            let! rest = manyStrings evalIdentifierPart 
            return start + rest 
        }) state

    and evalIdentifierStart state =
        choice [|
            evalUnicodeLetter
            pstring "$"
            pstring "_"
            parse {
                do! skipChar '\\'
                let! r = evalUnicodeEscapeSequence
                return r
            }
        |] state

    and evalIdentifierPart state =
        choice [|
            evalIdentifierStart
            evalUnicodeCombiningMark
            evalUnicodeDigit
            evalUnicodeConnectorPunctuation
            pstring "\u200C"
            pstring "\u200D"
        |] state

    and evalUnicodeLetter state =
        (satisfy CharSets.isUnicodeLetter |>> string) state

    and evalUnicodeCombiningMark state =
        (satisfy CharSets.isUnicodeCombiningMark |>> string) state

    and evalUnicodeDigit state =
        (satisfy CharSets.isUnicodeDigit |>> string) state

    and evalUnicodeConnectorPunctuation state =
        (satisfy CharSets.isUnicodeConnectorPunctuation |>> string) state
        
    // Numeric Literal
    
    and evalNumericLiteral state =
        (evalDecimalLiteral <|> evalHexIntegerLiteral) state

    and evalDecimalLiteral state =
        choice [|
            parse {
                let! e1 = evalDecimalIntegerLiteral
                let! e2 = opt (pchar '.')
                match e2 with
                | Some _ ->
                    let! e2 = opt (evalDecimalDigits -1.0)
                    let! e3 = opt evalExponentPart
                    match e2, e3 with
                    | Some e2, Some e3 ->
                        return e1 + e2 * e3
                    | Some e2, None ->
                        return e1 + e2
                    | None, Some e3 ->
                        return e1 * e3
                    | None, None ->
                        return e1
                | None ->
                    let! e2 = opt evalExponentPart
                    match e2 with
                    | Some e2 ->
                        return e1 * e2
                    | None ->
                        return e1 
            }
            parse {
                do! skipChar '.'
                let! e1 = evalDecimalDigits -1.0
                let! e2 = opt evalExponentPart
                match e2 with
                | Some e2 ->
                    return e1 * e2
                | None ->
                    return e1
            }
        |] state

    and evalDecimalIntegerLiteral state =
        (parse {
            let! r = opt (pchar '0')
            match r with
            | Some r -> return 0.0
            | None -> 
                let! r = evalDecimalDigits 1.0
                return r
        }) state

    and evalDecimalDigits sign state =
        (parse {
            let! e = many1 evalDecimalDigit
            let e = e |> List.mapi (fun i n -> n * (10.0 ** (sign * double i)))
            return e |> List.reduce (+)
        }) state 

    and evalDecimalDigit state =
        (parse {
            let! c = anyOf "0123456789"
            return double c - 48.0
        }) state

    and evalNonZeroDigit state =
        (parse {
            let! c = anyOf "123456789"
            return double c - 48.0
        }) state

    and evalExponentPart state =
        (parse {
            do! evalExponentIndicator
            let! r = evalSignedInteger
            return r
        }) state

    and evalExponentIndicator state =
        (skipAnyOf "eE") state

    and evalSignedInteger state =
        (parse {
            let! r = opt (anyOf "+-")
            match r with
            | Some '-' ->  
                let! r = evalDecimalDigits 1.0
                return -r
            | _ -> 
                let! r = evalDecimalDigits 1.0
                return r
        }) state 
 
    and evalHexIntegerLiteral state =
        (parse {
            do! skipString "0x" <|> skipString "0X"
            let! e = many1 evalHexDigit
            let e = e |> List.mapi (fun i n -> n * (16.0 ** (double i)))
            return e |> List.reduce (+)
        }) state

    and evalHexDigit state =
        (parse {
            let! c = anyOf "0123456789ABCDEFabcdef"             
            match c with
            | c when c >= '0' && c <= '9' -> 
                return double c - 48.0 
            | c when c >= 'A' && c <= 'F' -> 
                return double c - 55.0
            | c when c >= 'a' && c <= 'f' -> 
                return double c - 87.0
        }) state

    // String Literals    

    and evalStringLiteral state =
        choice [|
            parse {
                do! skipChar '\"'
                let! e = opt evalDoubleStringCharacters
                do! skipChar '\"'
                match e with
                | Some e ->
                    return e
                | None ->
                    return ""
            }
            parse {
                do! skipChar '''
                let! e = opt evalSingleStringCharacters
                do! skipChar '''
                match e with
                | Some e ->
                    return e
                | None ->
                    return ""
            }
        |] state
    
    and evalDoubleStringCharacters state =
        many1Strings evalDoubleStringCharacter state

    and evalSingleStringCharacters state =
        many1Strings evalSingleStringCharacter state
        
    and evalDoubleStringCharacter state =
        evalStringCharacter '\"' state

    and evalSingleStringCharacter state =
        evalStringCharacter '\'' state

    and evalStringCharacter quoteChar state =
        choice [|
            satisfy (satisfyStringCharacter quoteChar) |>> fun c -> c.ToString()
            parse {
                do! skipChar '\\'
                let! r = evalEscapeSequence <|> evalLineContinuation
                return r
            }
        |] state

    and satisfyStringCharacter quoteChar c =
        (c <> quoteChar && c <> '\\' && not (CharSets.isLineTerminator c)) 
    
    and evalLineContinuation state =
        (parse {
            do! skipSatisfy CharSets.isLineTerminator
            return ""
        }) state        
    
    and evalEscapeSequence state =
        (parse {
            return ""
        }) state

    and evalCharacterEscapeSequence state =
        (parse {
            return ""
        }) state

    and evalSingleEscapeCharacter state =
        (parse {
            return ""
        }) state

    and evalNonEscapeCharacter state =
        (parse {
            return ""
        }) state

    and evalEscapeCharacter state =
        (parse {
            return ""
        }) state

    and evalHexEscapeSequence state =
        (parse {
            return ""
        }) state

    and evalUnicodeEscapeSequence state =
        (parse {
            return ""
        }) state

    // Regular Expression Literal

    and evalRegularExpressionFlags state =
        (parse {
            return ()
        }) state

    and evalRegularExpressionNonTerminator state =
        (parse {
            return ()
        }) state

    and evalRegularExpressionBackslashSequence state =
        (parse {
            return ()
        }) state

    and evalRegularExpressionClassChar state =
        (parse {
            return ()
        }) state

    and evalRegularExpressionClassChars state =
        (parse {
            return ()
        }) state

    and evalRegularExpressionClass state =
        (parse {
            return ()
        }) state

    and evalRegularExpressionChar state =
        (parse {
            return ()
        }) state

    and evalRegularExpressionFirstChar state =
        (parse {
            return ()
        }) state

    and evalRegularExpressionChars state =
        (parse {
            return ()
        }) state

    and evalRegularExpressionBody state =
        (parse {
            return ()
        }) state

    and evalRegularExpressionLiteral state =
        (parse {
            return "", ""
        }) state

    // Source Elements

    let skipIgnorableTokens state =
        optional (skipMany ((evalWhiteSpace |>> ignore) <|> (evalLineTerminator |>> ignore) <|> (evalComment |>> ignore))) state
        
    let skipToken value state =
        (parse {
            do! skipIgnorableTokens
            do! skipString value
            return ()
        }) state
        
    let skipStatementTerminator state =
        (parse {
            do! skipMany evalWhiteSpace
            let! r = opt (anyOf ";}") 
            match r with
            | Some r -> return ()
            | None ->
                let! r = opt evalLineTerminator 
                match r with
                | Some r -> return ()
                | None ->
                    let! r = opt evalMultiLineComment
                    if r.IsSome then                  
                        return ()
        } <|> eof) state
         
    let skipEval parser state =
        (parse {
            do! skipIgnorableTokens
            let! r = parser
            return r
        }) state

    let betweenParentheses parser state =
        between (skipToken "(") (skipToken ")") (skipEval parser) state

    let betweenBraces parser state =
        between (skipToken "{") (skipToken "}") (skipEval parser) state

    let betweenBrackets parser state =
        between (skipToken "[") (skipToken "]") (skipEval parser) state

    let skipComma =
        skipToken ","
    

    let evalPostfixOperator =  
        choice [|
            skipString "++" |>> fun () -> Reflection.IDynamic.op_PostfixIncrement
            skipString "--" |>> fun () -> Reflection.IDynamic.op_PostfixDecrement
        |]

    let evalUnaryOperator =  
        choice [|
            skipString "delete" |>> fun () -> Reflection.IDynamic.op_Delete
            skipString "void" |>> fun () -> Reflection.IDynamic.op_Void
            skipString "typeof" |>> fun () -> Reflection.IDynamic.op_Typeof
            skipString "++" |>> fun () -> Reflection.IDynamic.op_PrefixIncrement
            skipString "--" |>> fun () -> Reflection.IDynamic.op_PrefixDecrement
            skipString "+" |>> fun () -> Reflection.IDynamic.op_Plus
            skipString "-" |>> fun () -> Reflection.IDynamic.op_Minus
            skipString "~" |>> fun () -> Reflection.IDynamic.op_BitwiseNot
            skipString "!" |>> fun () -> Reflection.IDynamic.op_LogicalNot
        |]
 
    let evalMultiplicativeOperator =  
        choice [|
            skipString "*" |>> fun () -> Reflection.IDynamic.op_Multiplication
            skipString "/" |>> fun () -> Reflection.IDynamic.op_Division
            skipString "%" |>> fun () -> Reflection.IDynamic.op_Modulus
        |]             

    let evalAdditiveOperator = 
        choice [|
            skipString "+" |>> fun () -> Reflection.IDynamic.op_Addition
            skipString "-" |>> fun () -> Reflection.IDynamic.op_Subtraction
        |]             

    let evalBitwiseShiftOperator =  
        choice [|
            skipString "<<" |>> fun () -> Reflection.IDynamic.op_LeftShift
            skipString ">>" |>> fun () -> Reflection.IDynamic.op_SignedRightShift
            skipString ">>>" |>> fun () -> Reflection.IDynamic.op_UnsignedRightShift
        |]
        
    let evalRelationalOperator =  
        choice [|
            skipString "<" |>> fun () -> Reflection.IDynamic.op_Lessthan
            skipString ">" |>> fun () -> Reflection.IDynamic.op_Greaterthan
            skipString "<=" |>> fun () -> Reflection.IDynamic.op_LessthanOrEqual
            skipString ">=" |>> fun () -> Reflection.IDynamic.op_GreaterthanOrEqual
            skipString "instanceof" |>> fun () -> Reflection.IDynamic.op_Instanceof
            skipString "in" |>> fun () -> Reflection.IDynamic.op_In
        |]
        
    let evalRelationalOperatorNoIn =  
        choice [|
            skipString "<" |>> fun () -> Reflection.IDynamic.op_Lessthan
            skipString ">" |>> fun () -> Reflection.IDynamic.op_Greaterthan
            skipString "<=" |>> fun () -> Reflection.IDynamic.op_LessthanOrEqual
            skipString ">=" |>> fun () -> Reflection.IDynamic.op_GreaterthanOrEqual
            skipString "instanceof" |>> fun () -> Reflection.IDynamic.op_Instanceof
            skipString "in" |>> fun () -> Reflection.IDynamic.op_In
        |]

    let evalEqualityOperator =  
        choice [|
            skipString "==" |>> fun () -> Reflection.IDynamic.op_Equals
            skipString "!=" |>> fun () -> Reflection.IDynamic.op_DoesNotEquals
            skipString "===" |>> fun () -> Reflection.IDynamic.op_StrictEquals
            skipString "!==" |>> fun () -> Reflection.IDynamic.op_StrictDoesNotEquals
        |]

    let evalBitwiseANDOperator =  
        skipString "&" |>> fun () -> Reflection.IDynamic.op_BitwiseAnd
        
    let evalBitwiseXOROperator =  
        skipString "^" |>> fun () -> Reflection.IDynamic.op_BitwiseXor
        
    let evalBitwiseOROperator =  
        skipString "|" |>> fun () -> Reflection.IDynamic.op_BitwiseOr
        
    let evalLogicalANDOperator =  
        skipString "&&" |>> fun () -> Reflection.IDynamic.op_LogicalAnd
        
    let evalLogicalOROperator =  
        skipString "||" |>> fun () -> Reflection.IDynamic.op_LogicalOr

    let rec evalPrimaryExpression state =
        (parse {
            do! skipIgnorableTokens
            let! r = 
                choice [|
                    evalArrayLiteral
                    evalObjectLiteral
                    evalThis
                    evalIdentifierReference
                    evalLiteral
                    evalGroupingOperator
                |] 
            return r
        }) state

    and evalThis state =
        (parse {
            do! skipToken "this"
            return propThisBinding
        }) state

    and evalIdentifierReference state =
        (parse {
            let! identifier = evalIdentifier
            let! s = getUserState
            let args = [| exp.Constant (identifier) :> exp; exp.Constant (s.strict) :> exp |]
            return exp.Call (propLexicalEnviroment, Reflection.ILexicalEnvironment.getIdentifierReference, args) :> exp
        }) state
    
    and evalLiteral state =
        (choice [|
            skipString "null" |>> fun () -> exp.Constant (environment.Null) :> exp
            skipString "true" |>> fun () -> exp.Constant (environment.True) :> exp
            skipString "false" |>> fun () -> exp.Constant (environment.False) :> exp
            evalNumericLiteral |>> fun num -> exp.Constant (environment.CreateNumber num) :> exp
            evalStringLiteral |>> fun str -> exp.Constant (environment.CreateString str) :> exp
            evalRegularExpressionLiteral |>> fun (body, flags) -> exp.Constant (environment.CreateRegExp (body, flags)) :> exp
        |] |> attempt) state

    and evalGroupingOperator state =
        between (skipToken "(") (skipToken ")") evalExpression state
        
    and evalObjectLiteral state =
        (parse {
            do! skipToken "{"
            let! s = getUserState
            let objectVar = exp.Variable (typeof<IObject>, "objectVar") 
            let defineOwnProperty name value =
                let name = exp.Constant (name) :> exp
                let strict = exp.Constant (s.strict) :> exp
                exp.Call (objectVar, Reflection.IObject.defineOwnProperty, [| name; value; strict |])
            let assign = exp.Property (environmentParam, "ObjectConstructor")
            let assign = exp.Convert (assign, typeof<IConstructable>)
            let assign = exp.Call (assign, Reflection.IConstructable.construct, [| environmentParam :> exp; propEmptyArgs |]) :> exp
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
                    | (propType, name, formalParams:option<ReadOnlyList<string>>, expression:option<exp>, functionBody:option<Expression<Code>>, strict)::rest ->
                        match propType with
                        | "data" when s.strict.Head |> fst && dataSet.Contains name ->
                            raise (environment.CreateSyntaxError "")
                        | "data" ->
                            let getValue = exp.Call (expression.Value, Reflection.IDynamic.get_Value, [||]) :> exp
                            let args = [| getValue; trueNullableBool; trueNullableBool; trueNullableBool |]
                            let createDesc = exp.Call (environmentParam, Reflection.IEnvironment.createDataDescriptor4, args) :> exp
                            let result = (defineOwnProperty name createDesc):>exp :: result
                            build result items.Tail (dataSet.Add name) getterSet setterSet
                        | "getter" when dataSet.Contains name ->
                            raise (environment.CreateSyntaxError "")
                        | "getter" when getterSet.Contains name ->
                            raise (environment.CreateSyntaxError "")
                        | "getter" ->
                            let formalParams = exp.Constant (ReadOnlyList<string>.Empty) :> exp
                            let strict = exp.Constant (strict) :> exp
                            let code = exp.Constant (lazy(functionBody.Value.Compile)) :> exp
                            let args = [| formalParams; strict; code; propLexicalEnviroment |]  
                            let createFunction = exp.Call (environmentParam, Reflection.IEnvironment.createFunction2, args) :> exp
                            let args = [| createFunction; trueNullableBool; trueNullableBool; trueNullableBool  |]
                            let createDesc = exp.Call (environmentParam, Reflection.IEnvironment.createAccessorDescriptor3, args) :> exp
                            let result = (defineOwnProperty name createDesc):>exp :: result
                            build result items.Tail dataSet (getterSet.Add name) setterSet
                        | "setter" when dataSet.Contains name ->
                            raise (environment.CreateSyntaxError "")
                        | "setter" when setterSet.Contains name ->
                            raise (environment.CreateSyntaxError "")
                        | "setter" when strict && (formalParams.Value.[0] = "eval" || formalParams.Value.[0] = "arguments") ->
                            raise (environment.CreateSyntaxError "")
                        | "setter" ->
                            let formalParams = exp.Constant (formalParams.Value) :> exp
                            let strict = exp.Constant (strict) :> exp
                            let code = exp.Constant (lazy(functionBody.Value.Compile)) :> exp
                            let args = [| formalParams; strict; code; propLexicalEnviroment |]  
                            let createFunction = exp.Call (environmentParam, Reflection.IEnvironment.createFunction2, args) :> exp
                            let args = [| createFunction; trueNullableBool; trueNullableBool; trueNullableBool  |]
                            let createDesc = exp.Call (environmentParam, Reflection.IEnvironment.createAccessorDescriptor3, args) :> exp
                            let result = (defineOwnProperty name createDesc):>exp :: result
                            build result items.Tail dataSet getterSet (setterSet.Add name)
                let body = assign :: (objectVar :> exp :: build [] e Set.empty Set.empty Set.empty |> List.rev)
                return exp.Block ([| objectVar |], body) :> exp
        }) state

    and evalPropertyNameAndValueList state =
        (parse {
            let parser = attempt <| parse {
                do! skipIgnorableTokens
                let! e = evalPropertyAssignment
                return e
            }
            let! r = sepBy parser (attempt skipComma)
            return r
        }) state

    and evalPropertyAssignment state =
        (evalValuePropertyAssignment <|> evalGetPropertyAssignment <|> evalSetPropertyAssignment) state

    and evalValuePropertyAssignment state =
        (parse {
            let! e1 = evalPropertyName
            do! skipToken ":"
            do! skipIgnorableTokens
            let! e3 = evalAssignmentExpression
            return "data", e1, None, Some e3, None, false
        }) state
       
    and evalGetPropertyAssignment state =
        (parse {
            do! skipToken "get"
            let! e1 = evalPropertyName
            do! skipToken "("
            do! skipToken ")"
            let! e3 = betweenBraces evalFunctionBody
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
            let! r = opt (evalIdentifierName <|> evalStringLiteral)
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
                exp.Call (arrayVar, Reflection.IObject.get, [| name |]) :> exp
            let addPad pad =
                let pad = exp.Constant (environment.CreateNumber pad) :> exp
                let pad = exp.Convert (pad, typeof<IDynamic>) :> exp
                let value = getLength ()
                let value = exp.Call (value, Reflection.IDynamic.op_Addition, [| pad |]) :> exp
                exp.Call (value, Reflection.IDynamic.convertToUInt32, Array.empty) :> exp                
            let setElement (e, pad) =
                let name = exp.Constant ("length") :> exp
                let strict = exp.Constant (s.strict) :> exp
                let name = addPad pad
                let name = exp.Call (name, Reflection.IDynamic.convertToString, Array.empty) :> exp
                let name = exp.Convert (name, typeof<IString>)
                let name = exp.Property (name, "BaseValue") :> exp
                let args = [| name; e; strict |]
                exp.Call (arrayVar, Reflection.IObject.put, args) :> exp                
            let assign = exp.Property (environmentParam, "ArrayConstructor")
            let createPutLength pad strict =
                let name = exp.Constant ("length") :> exp
                let value = addPad pad
                let strict = exp.Constant (strict) :> exp
                let args = [| name; value; strict |]
                exp.Call (arrayVar, Reflection.IObject.put, args) :> exp                
            let assign = exp.Property (environmentParam, "ArrayConstructor")
            let assign = exp.Convert (assign, typeof<IConstructable>)
            let assign = exp.Call (assign, Reflection.IConstructable.construct, [| environmentParam :> exp; propEmptyArgs |]) :> exp
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
                    let putLength = createPutLength pad s.strict
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
                    let putLength = createPutLength pad s.strict
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
            let parser = attempt <| parse {
                do! skipIgnorableTokens
                let! pad = evalElision
                do! skipIgnorableTokens
                let! e = evalAssignmentExpression
                return e, pad
            }
            let! r = sepBy parser (attempt skipComma)
            return r
        }) state

    and evalMemberExpression state =
        (parse {
            do! skipToken "new"
            let! e = evalMemberExpression
            let! a = evalArguments
            return exp.Call (e, Reflection.IDynamic.op_Construct, [| environmentParam :> exp; a |]) :> exp
          } <|>  parse {
            let! e1 = evalPrimaryExpression <|> evalFunctionExpression
            let! rest = manyRev (attempt evalBracketNotation <|> attempt evalDotNotation)
            let! s = getUserState
            return rest |> List.fold (
                fun acc (t, e) ->
                    match t with
                    | "bracket" ->  
                        let convert = exp.Call (e, Reflection.IDynamic.convertToString, [||]) :> exp                       
                        let args = [| convert; acc; exp.Constant (s.strict) :> exp |]
                        exp.Call (environmentParam, Reflection.IEnvironment.createReference, args) :> exp 
                    | "dot" ->                         
                        let args = [| e; exp.Convert(acc, typeof<IReferenceBase>) :> exp; exp.Constant (s.strict) :> exp |]
                        exp.Call (environmentParam, Reflection.IEnvironment.createReference, args) :> exp            
            ) e1
        }) state
        
    and evalNewExpression state =
        (parse {
            let! r = opt evalMemberExpression
            match r with
            | Some r ->
                return r
            | None ->
                do! skipToken "new"
                let! r = evalNewExpression   
                let args = [| environmentParam :> exp; exp.Constant (environment.EmptyArgs) :> exp |]
                return exp.Call (r, Reflection.IDynamic.op_Construct, args) :> exp
        }) state

    and evalCallExpression state =
        (parse {
            let! e1 = evalMemberExpression
            let! e2 = evalArguments
            let first = exp.Call (e1, Reflection.IDynamic.op_Call, [| e2 |]) :> exp
            let! rest = manyRev ((evalArguments |>> fun a -> "args", a) <|> evalBracketNotation <|> evalDotNotation)
            let! s = getUserState
            return rest |> List.fold (
                fun acc (t, e) ->
                    match t with
                    | "args" -> 
                        exp.Call (acc, Reflection.IDynamic.op_Call, [| e |]) :> exp
                    | "bracket" ->  
                        let convert = exp.Call (e, Reflection.IDynamic.convertToString, [||]) :> exp                       
                        let args = [| convert; acc; exp.Constant (s.strict) :> exp |]
                        exp.Call (environmentParam, Reflection.IEnvironment.createReference, args) :> exp 
                    | "dot" ->                         
                        let args = [| e; acc; exp.Constant (s.strict) :> exp |]
                        exp.Call (environmentParam, Reflection.IEnvironment.createReference, args) :> exp            
            ) first
        }) state

    and evalArguments state =
        betweenParentheses evalArguments state

    and evalArgumentList state =
        (parse {
            let! r = sepByRev evalAssignmentExpression (skipToken ",")
            let args = [| exp.Constant (r) :> exp |]
            return exp.Call (environmentParam, Reflection.IEnvironment.createArgsMany, args) :> exp
        }) state

    and evalBracketNotation state =
        (betweenBrackets evalExpression |>> fun a -> "bracket", a) state
        
    and evalDotNotation state =
        (parse {
            do! skipToken "."
            let! r = evalIdentifierName
            return "dot", exp.Constant (r) :> exp
        }) state

    and evalLeftHandSideExpression state =
        (parse {
            let! e = evalNewExpression <|> evalCallExpression 
            return e
        }) state

    and evalPostfixExpression state =
        (parse {
            let! e1 = evalLeftHandSideExpression
            do! skipIgnorableTokens
            let! op = opt evalPostfixOperator
            match op with
            | Some op ->
                return exp.Call (e1, op, [||]) :> exp
            | None ->
                return e1
        }) state

    and evalUnaryExpression state =
        choice [| 
            parse {
                let! op = evalUnaryOperator
                do! skipIgnorableTokens
                let! e = evalUnaryExpression
                return exp.Call (e, op, [||]) :> exp
            }
            evalPostfixExpression
        |] state

    and evalBinaryExpression (p1:Parser<exp, State1>) p2 (state:FParsec.State<State1>) =
        (parse {
            let! e1 = p1
            let e1 = if e1.Type <> typeof<IDynamic> then exp.Convert(e1, typeof<IDynamic>) :> exp else e1
            let folder e1 (op, e2) = 
                let args = [| exp.Convert(e2, typeof<IDynamic>) :> exp |]
                exp.Call (e1, op, args) :> exp
            let parser = attempt <| parse {
                do! skipIgnorableTokens
                let! op = p2
                do! skipIgnorableTokens
                let! e2 = p1
                return op, e2
            }
            let! r = manyFold e1 folder parser
            return r
        }) state

    and evalMultiplicativeExpression state =
        evalBinaryExpression evalUnaryExpression evalMultiplicativeOperator state

    and evalAdditiveExpression state =
        evalBinaryExpression evalMultiplicativeExpression evalAdditiveOperator state

    and evalShiftExpression state =
        evalBinaryExpression evalAdditiveExpression evalBitwiseShiftOperator state

    and evalRelationalExpression state =
        evalBinaryExpression evalShiftExpression evalRelationalOperator state
        
    and evalRelationalExpressionNoIn state =
        evalBinaryExpression evalShiftExpression evalRelationalOperatorNoIn state

    and evalEqualityExpression state =
        evalBinaryExpression evalRelationalExpression evalEqualityOperator state

    and evalEqualityExpressionNoIn state =
        evalBinaryExpression evalRelationalExpressionNoIn evalEqualityOperator state

    and evalBitwiseANDExpression state =
        evalBinaryExpression evalEqualityExpression evalBitwiseANDOperator state

    and evalBitwiseANDExpressionNoIn state =
        evalBinaryExpression evalEqualityExpressionNoIn evalBitwiseANDOperator state

    and evalBitwiseXORExpression state =
        evalBinaryExpression evalBitwiseANDExpression evalBitwiseXOROperator state

    and evalBitwiseXORExpressionNoIn state =
        evalBinaryExpression evalBitwiseANDExpressionNoIn evalBitwiseXOROperator state

    and evalBitwiseORExpression state =
        evalBinaryExpression evalBitwiseXORExpression evalBitwiseOROperator state

    and evalBitwiseORExpressionNoIn state =
        evalBinaryExpression evalBitwiseXORExpressionNoIn evalBitwiseOROperator state

    and evalLogicalANDExpression state =
        evalBinaryExpression evalBitwiseORExpression evalLogicalANDOperator state

    and evalLogicalANDExpressionNoIn state =
        evalBinaryExpression evalBitwiseORExpressionNoIn evalLogicalANDOperator state
    
    and evalLogicalORExpression state =
        evalBinaryExpression evalLogicalANDExpression evalLogicalOROperator state

    and evalLogicalORExpressionNoIn state =
        evalBinaryExpression evalLogicalANDExpressionNoIn evalLogicalOROperator state
        
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
                let e1 = exp.Call (e1, Reflection.IDynamic.convertToBoolean, [||]) :> exp
                let e1 = exp.Property (e1, "BaseValue") :> exp
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
            skipString "*=" |>> fun () -> Some Reflection.IDynamic.op_Multiplication
            skipString "/=" |>> fun () -> Some Reflection.IDynamic.op_Division
            skipString "%=" |>> fun () -> Some Reflection.IDynamic.op_Modulus
            skipString "+=" |>> fun () -> Some Reflection.IDynamic.op_Addition
            skipString "-=" |>> fun () -> Some Reflection.IDynamic.op_Subtraction
            skipString "<<=" |>> fun () -> Some Reflection.IDynamic.op_LeftShift
            skipString ">>>=" |>> fun () -> Some Reflection.IDynamic.op_UnsignedRightShift
            skipString ">>=" |>> fun () -> Some Reflection.IDynamic.op_SignedRightShift
            skipString "&=" |>> fun () -> Some Reflection.IDynamic.op_BitwiseAnd
            skipString "^=" |>> fun () -> Some Reflection.IDynamic.op_BitwiseXor
            skipString "|=" |>> fun () -> Some Reflection.IDynamic.op_BitwiseOr
        |]

    and evalAssignmentExpressionCommon parser1 parser2 state =
        (parse {
            let! e1 = parser1
            let! op = opt evalAssignmentOperator
            match op with
            | Some op ->
                let rec validate (e:exp) =
                    match e with
                    | :? UnaryExpression as e ->
                        if e.Operand.Type <> typeof<IReference> then
                            failwith ""
                    | _ -> failwith ""
                do! skipIgnorableTokens              
                validate e1    
                match op with
                | Some op ->
                    let left = exp.Convert (e1, typeof<IDynamic>)
                    let left = exp.Property (left, "Value") :> exp
                    let! right = parser2
                    let right = exp.Convert (right, typeof<IDynamic>)
                    let right = exp.Property (right, "Value") :> exp
                    let performOp = exp.Call (left, op, [| right |]) :> exp
                    return exp.Assign (left, performOp) :> exp
                | None ->
                    let left = exp.Convert (e1, typeof<IDynamic>)
                    let! right = parser2
                    let right = exp.Convert (right, typeof<IDynamic>)
                    return exp.Assign (exp.Property (left, "Value"), exp.Property (right, "Value")) :> exp
            | None ->
                return e1
        }) state
                    
    and evalAssignmentExpression state =
        evalAssignmentExpressionCommon evalConditionalExpression evalAssignmentExpression state

    and evalAssignmentExpressionNoIn state =
        evalAssignmentExpressionCommon evalConditionalExpressionNoIn evalAssignmentExpressionNoIn state

    and evalExpressionCommon parser state =
        (parse {
            let parser = attempt <| parse {
                do! skipIgnorableTokens
                let! e = parser
                return e
            }
            let! r = sepBy parser (attempt skipComma)
            let r = r |> List.map (fun e -> exp.Property (e, "Value") :> exp)
            return if r.Length = 1 then r.Head else exp.Block (r) :> exp
        }) state

    and evalExpression state =
        evalExpressionCommon evalAssignmentExpression state

    and evalExpressionNoIn state =
        evalExpressionCommon evalAssignmentExpressionNoIn state

    and evalStatement state =
        (parse {            
            do! skipIgnorableTokens
            let! e = 
                choice [|
                    evalBlock
                    evalVariableStatement
                    evalEmptyStatement
                    evalExpressionStatement
                    evalIfStatement
                    evalIterationStatement
                    evalContinueStatement
                    evalBreakStatement
                    evalReturnStatement
                    evalWithStatement
                    evalLabelledStatement
                    evalSwitchStatement
                    evalThrowStatement
                    evalTryStatement
                    evalDebuggerStatement
                |] 
            return e
        }) state

    and evalBlock state =
        (parse {
            let! (e:list<exp>) = betweenBraces evalStatementList
            let e = e |> List.filter (fun e -> e.Type <> typeof<Void>)
            if e.IsEmpty 
            then return exp.Constant (environment.Undefined :> IDynamic) :> exp 
            else return exp.Block (e) :> exp
        }) state 

    and evalStatementList state =
        many evalStatement state

    and evalVariableStatement state =
        (parse {
            do! skipToken "="
            do! skipIgnorableTokens
            let! e = evalVariableDeclarationList
            return e
        }) state

    and evalVariableDeclarationListCommon parser state =
        (parse {
            let parser = attempt <| parse {
                do! skipIgnorableTokens
                let! e = parser
                return e
            }
            let! r = sepBy parser (attempt skipComma)
            if r.IsEmpty 
            then return exp.Constant (environment.Undefined :> IDynamic) :> exp 
            elif r.Length = 1
            then return r.Head
            else return exp.Block (r) :> exp 
        }) state  

    and evalVariableDeclarationList state =
        evalVariableDeclarationListCommon evalVariableDeclaration state

    and evalVariableDeclarationListNoIn state =
        evalVariableDeclarationListCommon evalVariableDeclarationNoIn state

    and evalVariableDeclarationCommon parser state =
        (parse {
            let! identifier = evalIdentifier
            let! (oldState:State1) = getUserState
            do! setUserState { oldState with variables = identifier::oldState.variables }
            let! e2 = opt parser
            match e2 with
            | Some e2 ->
                return e2
            | None ->
                return exp.Empty() :> exp
        }) state

    and evalVariableDeclaration state =
        evalVariableDeclarationCommon evalInitialiser state

    and evalVariableDeclarationNoIn state =
        evalVariableDeclarationCommon evalInitialiserNoIn state

    and evalInitialiserCommon parser state =
        (parse {
            do! skipToken "="
            do! skipIgnorableTokens
            let! e = parser
            return e
        }) state

    and evalInitialiser state =
        evalInitialiserCommon evalAssignmentExpression state

    and evalInitialiserNoIn state =
        evalInitialiserCommon evalAssignmentExpressionNoIn state

    and evalEmptyStatement state =
        (parse {
            do! skipToken ";"
            return exp.Empty() :> exp
        }) state

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
            let! e = evalExpression           
            do! skipStatementTerminator
            let! (oldState:State1) = getUserState
            let strict, endOfPrologue = oldState.strict.Head 
            if not strict && not endOfPrologue then 
                let strict, endOfPrologue = check e
                do! setUserState ({ oldState with strict = (strict, endOfPrologue)::oldState.strict.Tail  })
                return e
            else
                return e
        }) state

    and evalIfStatement state =
        (parse {
            return exp.Empty() :> exp
        }) state

    and evalIterationStatement state =
        (parse {
            return exp.Empty() :> exp
        }) state

    and evalContinueStatement state =
        (parse {
            return exp.Empty() :> exp
        }) state

    and evalBreakStatement state =
        (parse {
            return exp.Empty() :> exp
        }) state

    and evalReturnStatement state =
        (parse {
            return exp.Empty() :> exp
        }) state

    and evalWithStatement state =
        (parse {
            return exp.Empty() :> exp
        }) state

    and evalSwitchStatement state =
        (parse {
            return exp.Empty() :> exp
        }) state

    and evalCaseBlock state =
        (parse {
            return exp.Empty() :> exp
        }) state        

    and evalCaseClauses state =
        (parse {
            return exp.Empty() :> exp
        }) state

    and evalCaseClause state =
        (parse {
            return exp.Empty() :> exp
        }) state

    and evalDefaultClause state =
        (parse {
            return exp.Empty() :> exp
        }) state     

    and evalLabelledStatement state =
        (parse {
            return exp.Empty() :> exp
        }) state

    and evalThrowStatement state =
        (parse {
            return exp.Empty() :> exp
        }) state

    and evalTryStatement state =
        (parse {
            do! skipToken "try"
            let! e1 = evalBlock
            let! e2 = opt evalCatch
            let! e3 = opt evalFinally
            return exp.Empty() :> exp
        }) state

    and evalCatch state =
        (parse {
            do! skipToken "catch"
            let! identifier = between (skipToken "(") (skipToken ")")  anyChar
            let! block = evalBlock
            return exp.Empty() :> exp
        }) state     

    and evalFinally state =
        (parse {
            do! skipToken "finally"
            let! block = evalBlock
            return block
        }) state

    and evalDebuggerStatement state =
        (parse {
            do! skipToken "debugger"
            do! skipStatementTerminator
            return exp.Empty() :> exp
        }) state

    and evalFunctionDeclaration state =
        let missingIdentifier = "The function declared at (Ln: {0}, Col: {1}) is missing an identifier."
        let badFPList = "The function '{0}' declared at (Ln: {1}, Col: {2}) has an incomplete formal parameter list."
        (parse {
            let! (oldState:State1) = getUserState
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
            let! (state:State1) = getUserState
            let func = FunctionDeclaration(identifier, formalParameterList, lazy(functionBody.Compile()), state.strict.Head |> fst)
            do! setUserState ({ oldState with functions = func::oldState.functions  })
            return exp.Empty() :> exp
        }) state

    and evalFunctionExpression state =
        (parse {
            if false then
                return exp.Empty() :> exp
        }) state

    and evalFormalParameterList state =
        (parse {
            return ReadOnlyList<string>.Empty
        }) state

    and evalFunctionBody state =
        (parse {            
            let! (oldState:State1) = getUserState
            let returnLabel = oldState.labels.Head.["return"]
            let! e1 = evalSourceElements            
            let body = exp.Block (e1 @ [ returnLabel :> exp ])
            return exp.Lambda<Code>(body, [| environmentParam; argsParam |])
        }) state

    and evalSourceElement state =
        (evalStatement <|> evalFunctionDeclaration) state

    and evalSourceElements state =
        manyTill evalSourceElement eof state      
         
    and evalProgram state =
        (parse {            
            let! (oldState:State1) = getUserState
            let returnLabel = oldState.labels.Head.["return"]
            let! e1 = evalSourceElements            
            let body = exp.Block e1
            let body =
                if body.Expressions.Count = 0 then
                    body.Update([||], [| returnLabel |]) 
                else
                    let count = body.Expressions.Count - 1
                    let last = body.Expressions.[count]
                    let last = exp.Return (returnLabel.Target, last)
                    body.Update([||], body.Expressions |> Seq.take count |> Seq.append [| last; returnLabel |])
            return exp.Lambda<Code>(body, [| environmentParam; argsParam |])
        }) state

    let createInitialState (strict:bool) =
        let returnLabel = exp.Label(exp.Label(typeof<IDynamic>, "return"), exp.Constant (environment.Undefined))
        { strict = [strict, false]; element = Nil; labels = [ Map.ofArray [| "return", returnLabel |] ]; functions = []; variables = [] }        

    let compile (parser:Parser<Expression<Code>, State1>) (input:string) (strict:bool) (streamName:string) =
        let initialState = createInitialState strict
        let result = runParserOnString parser initialState streamName input
        match result with
        | Success (expression, finalState, position) ->
            let variableDeclarations = ReadOnlyList<string>(finalState.variables)
            let functionDeclarations = ReadOnlyList<FunctionDeclaration>(finalState.functions)
            let strict = finalState.strict.Head |> fst
            ExecutableCode(expression.Compile(), variableDeclarations, functionDeclarations, strict)
        | Failure (message, error, finalState) -> 
            raise (environment.CreateSyntaxError message)         
        
    member this.CompileGlobalCode (input:string) =
        compile evalProgram input false "GlobalCode"
        
    member this.CompileEvalCode (input:string, strict:bool) =
        compile evalProgram input strict "EvalCode"

    member this.CompileFunctionCode (input:string, strict:bool) =
        compile evalFunctionBody input strict "FunctionCode"
