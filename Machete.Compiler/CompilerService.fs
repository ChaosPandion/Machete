namespace Machete.Compiler


open System
open System.Collections
open System.Collections.Generic
open System.Text
open System.Linq.Expressions
open System.Reflection
open FParsec.CharParsers
open FParsec.Primitives
open Machete.Interfaces

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


type internal State1 = {
    strict : list<bool * bool>
    element : SourceElement
    labels : list<Map<string, Label>>
    functions : list<FunctionDeclaration>
    variables : list<string>
}

type CompilerService (environment:IEnvironment) as this =

    let environmentParam = exp.Parameter (Reflection.IEnvironment.t, "environment")
    let argsParam = exp.Parameter (Reflection.IArgs.t, "args")
    
    let propEmptyArgs = exp.Property(environmentParam, "EmptyArgs") :> exp
    let propContext = exp.Property (environmentParam, "Context") :> exp
    let propUndefined = exp.Property (environmentParam, "Undefined") :> exp
    let propThisBinding = exp.Property (propContext, "ThisBinding") :> exp
    let propLexicalEnviroment = exp.Property (propContext, "LexicalEnviroment") :> exp
    let propVariableEnvironment = exp.Property (propContext, "VariableEnviroment") :> exp
    
    let nullIDynamic = exp.Constant (null, typeof<IDynamic>) :> exp
    let trueNullableBool = exp.Constant (true, typeof<Nullable<bool>>) :> exp
    let falseNullableBool = exp.Constant (false, typeof<Nullable<bool>>) :> exp
    let nullNullableBool = exp.Constant (null, typeof<Nullable<bool>>) :> exp

    let equalityTestMethod = this.GetType().GetMethod ("equalityTest", BindingFlags.Static ||| BindingFlags.NonPublic)
        
    // White Space
    let rec evalWhiteSpace state =
        satisfy CharSets.isWhiteSpace state     

    // Line Terminators
    and evalLineTerminator state =
        satisfy CharSets.isLineTerminator state           
    and evalLineTerminatorSequence state =
        (newline |>> string <|> (satisfy CharSets.isLineTerminator |>> string)) state       

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
        (attempt evalHexIntegerLiteral <|> evalDecimalLiteral) state
    and evalDecimalLiteral state =
        choice [|
            parse {
                let! e1 = evalDecimalIntegerLiteral
                let! e2 = opt (pchar '.')
                match e2 with
                | Some _ ->
                    let! e2 = opt (evalDecimalDigits -1.0 1.0)
                    let! e3 = opt evalExponentPart
                    match e2, e3 with
                    | Some e2, Some e3 ->
                        return (e1 + e2) * e3
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
                let! e1 = evalDecimalDigits -1.0 1.0
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
                let! r = evalDecimalDigits 1.0 0.0
                return r
        }) state
    and evalDecimalDigits sign modifier state =
        (parse {
            let p = if modifier = 0.0 then many1Rev evalDecimalDigit else many1 evalDecimalDigit
            let! e = p
            let e = e |> List.mapi (fun i n -> n * (10.0 ** (sign * (double i + modifier))))
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
            return 10.0 ** r
        }) state
    and evalExponentIndicator state =
        (skipAnyOf "eE") state
    and evalSignedInteger state =
        (parse {
            let! r = opt (anyOf "+-")
            match r with
            | Some '-' ->  
                let! r = evalDecimalDigits 1.0 0.0
                return -r
            | _ -> 
                let! r = evalDecimalDigits 1.0 0.0
                return r
        }) state  

    and evalHexIntegerLiteral state =
        (parse {
            do! skipString "0x" <|> skipString "0X"
            let! e = many1Rev evalHexDigit
            let e = e |> List.mapi (fun i n -> n * (16.0 ** (double i)))
            return e |> List.reduce (+)
        }) state

    and evalHexDigit state =
        (anyOf "0123456789ABCDEFabcdef" |>> completeHexDigit) state

    and completeHexDigit c =
        match c with
        | c when c >= '0' && c <= '9' -> 
            double c - 48.0 
        | c when c >= 'A' && c <= 'F' -> 
            double c - 55.0
        | c when c >= 'a' && c <= 'f' -> 
            double c - 87.0

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
            let! r = evalLineTerminatorSequence 
            return ""
        }) state 
    and evalEscapeSequence state =
        choice [|
            parse {
                do! skipChar '0'
                do! notFollowedBy digit
                return "\u0000"
            }
            evalHexEscapeSequence
            evalUnicodeEscapeSequence
            evalCharacterEscapeSequence
        |] state
    and evalCharacterEscapeSequence state =
        choice [|
            evalSingleEscapeCharacter
            evalNonEscapeCharacter
        |] state
    and evalSingleEscapeCharacter state =
        (parse {
            let! c = anyOf "\'\"\\bfnrtv"
            let c = 
                match c with
                | 'b' -> '\u0008'
                | 't' -> '\u0009'
                | 'n' -> '\u000A'
                | 'v' -> '\u000B'
                | 'f' -> '\u000C'
                | 'r' -> '\u000D'
                | '\"' -> '\u0022'
                | '\'' -> '\u0027'
                | '\\' -> '\u005C'
            return c.ToString()
        }) state
    and evalNonEscapeCharacter state =
        (parse {
            do! notFollowedBy evalSingleEscapeCharacter
            do! notFollowedBy evalLineTerminator
            let! c = anyChar
            return c.ToString()
        }) state
    and evalEscapeCharacter state =
        choice [|
            evalSingleEscapeCharacter
            digit |>> string
            pchar 'x' |>> string
            pchar 'u' |>> string
        |] state
    and evalHexEscapeSequence state =
        (parse {
            do! skipChar 'x'
            let! first = evalHexDigit
            let! second = evalHexDigit
            return (16.0 * first + second) |> char |> string
        }) state
    and evalUnicodeEscapeSequence state =
        (parse {
            do! skipChar 'u'
            let! first = evalHexDigit
            let! second = evalHexDigit
            let! third = evalHexDigit
            let! fourth = evalHexDigit
            return (4096.0 * first + 256.0 * second + 16.0 * third + fourth) |> char |> string
        }) state

    // Regular Expression Literal
    and evalRegularExpressionLiteral state =
        (parse {
            do! skipChar '/'
            let! body = evalRegularExpressionBody
            do! skipChar '/'
            let! flags = evalRegularExpressionFlags
            return body, flags
        }) state
    and evalRegularExpressionBody state =
        (parse {
            let! first = evalRegularExpressionFirstChar
            let! rest = evalRegularExpressionChars 
            return first + rest
        }) state
    and evalRegularExpressionChars state =
        (parse {
            let! r = manyStrings evalRegularExpressionChar
            return r
        }) state
    and evalRegularExpressionFirstChar state =
        choice [|
            satisfy (fun c -> c <> '*' && c <> '\\' && c <> '/' && c <> ']') |>> string
            evalRegularExpressionBackslashSequence
            evalRegularExpressionClass
        |] state
    and evalRegularExpressionChar state =
        choice [|
            satisfy (fun c -> c <> '\\' && c <> '/' && c <> ']') |>> string
            evalRegularExpressionBackslashSequence
            evalRegularExpressionClass
        |] state
    and evalRegularExpressionBackslashSequence state =
        (parse {
            do! skipChar '\\'
            let! r = evalRegularExpressionNonTerminator
            return r |> string
        }) state
    and evalRegularExpressionNonTerminator state =
        (parse {
            do! notFollowedBy evalLineTerminator
            let! c = anyChar
            return c |> string
        }) state
    and evalRegularExpressionClass state =
        (parse {
            do! skipChar '['
            let! r = evalRegularExpressionClassChars
            do! skipChar ']'
            return r
        }) state
    and evalRegularExpressionClassChars state =
        (parse {
            let! r = manyStrings evalRegularExpressionClassChar
            return r
        }) state
    and evalRegularExpressionClassChar state =
        choice [|
            satisfy (fun c -> c <> '\\' && c <> ']') |>> string
            evalRegularExpressionBackslashSequence
        |] state
    and evalRegularExpressionFlags state =
        (parse {
            let! r = manyStrings evalIdentifierPart
            return r
        }) state

    // Source Elements

    let skipIgnorableTokens state =
        optional (skipMany ((evalWhiteSpace |>> ignore) <|> (evalLineTerminator |>> ignore) <|> (evalComment |>> ignore))) state
        
    let skipToken value state =
        (skipIgnorableTokens >>. skipString value >>. skipIgnorableTokens ) state
        
    let skipStatementTerminator state =
        (parse {
            do! skipMany evalWhiteSpace
            let! r = opt (lookAhead (anyOf ";}")) 
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
                    else 
                        do! eof
                        return ()
        }) state
         
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
            skipString ">>>" |>> fun () -> Reflection.IDynamic.op_UnsignedRightShift
            skipString ">>" |>> fun () -> Reflection.IDynamic.op_SignedRightShift
        |]
        
    let evalRelationalOperator =  
        choice [|
            skipString "<=" |>> fun () -> Reflection.IDynamic.op_LessthanOrEqual
            skipString ">=" |>> fun () -> Reflection.IDynamic.op_GreaterthanOrEqual
            skipString "<" |>> fun () -> Reflection.IDynamic.op_Lessthan
            skipString ">" |>> fun () -> Reflection.IDynamic.op_Greaterthan
            skipString "instanceof" |>> fun () -> Reflection.IDynamic.op_Instanceof
            skipString "in" |>> fun () -> Reflection.IDynamic.op_In
        |]
        
    let evalRelationalOperatorNoIn =  
        choice [|
            skipString "<=" |>> fun () -> Reflection.IDynamic.op_LessthanOrEqual
            skipString ">=" |>> fun () -> Reflection.IDynamic.op_GreaterthanOrEqual
            skipString "<" |>> fun () -> Reflection.IDynamic.op_Lessthan
            skipString ">" |>> fun () -> Reflection.IDynamic.op_Greaterthan
            skipString "instanceof" |>> fun () -> Reflection.IDynamic.op_Instanceof
            skipString "in" |>> fun () -> Reflection.IDynamic.op_In
        |]

    let evalEqualityOperator =  
        choice [|
            skipString "===" |>> fun () -> Reflection.IDynamic.op_StrictEquals
            skipString "!==" |>> fun () -> Reflection.IDynamic.op_StrictDoesNotEquals
            skipString "==" |>> fun () -> Reflection.IDynamic.op_Equals
            skipString "!=" |>> fun () -> Reflection.IDynamic.op_DoesNotEquals
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
                    evalThis
                    evalIdentifierReference
                    evalLiteral
                    evalArrayLiteral
                    evalObjectLiteral
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
            let args = [| exp.Constant (identifier) :> exp; exp.Constant (s.strict.Head |> fst) :> exp |]
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
        (parse {
            do! skipToken "("
            do! skipIgnorableTokens
            let! e = evalExpression
            do! skipToken ")"    
            return e
        }) state
        
    and evalObjectLiteral state =
        (parse {
            do! skipToken "{"
            let! s = getUserState
            let objectVar = exp.Variable (typeof<IObject>, "objectVar") 
            let defineOwnProperty name value =
                let name = exp.Constant (name) :> exp
                let strict = exp.Constant (s.strict.Head |> fst) :> exp
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
            let setElement (pad, e) =
                let name = exp.Constant ("length") :> exp
                let strict = exp.Constant (s.strict.Head |> fst) :> exp
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
            let! first = p
            let! r = many (attempt (skipToken "," >>. p))
            return first::r
        }) state

    and evalMemberExpression state =
        (attempt (parse {
            do! skipToken "new"
            let! e = skipEval evalMemberExpression
            let! a = evalArguments
            return exp.Call (e, Reflection.IDynamic.op_Construct, [| a |]) :> exp
          }) <|>  parse {
            let! e1 = evalFunctionExpression <|> evalPrimaryExpression 
            let e1 = exp.Convert (e1, typeof<IDynamic>) :> exp
            let! rest = many (attempt evalBracketNotation <|> attempt evalDotNotation)
            if rest.IsEmpty then
                return e1
            else
                let e1 = exp.Property (e1, "Value") :> exp
                let! s = getUserState
                let rest = 
                    (rest |> List.fold (
                        fun acc (t, e) ->
                            match t with
                            | "bracket" ->  
                                let convert = exp.Call (e, Reflection.IDynamic.convertToString, [||]) :> exp  
                                let convert = exp.Property (convert, "BaseValue") :> exp                     
                                let args = [| convert; exp.Convert(acc, typeof<IReferenceBase>) :> exp; exp.Constant (s.strict.Head |> fst) :> exp |]
                                let r = exp.Call (environmentParam, Reflection.IEnvironment.createReference, args) :> exp 
                                let r = exp.Convert (r, typeof<IDynamic>)
                                exp.Property (r, "Value") :> exp
                            | "dot" ->                         
                                let args = [| e; exp.Convert(acc, typeof<IReferenceBase>) :> exp; exp.Constant (s.strict.Head |> fst) :> exp |]
                                let r = exp.Call (environmentParam, Reflection.IEnvironment.createReference, args) :> exp 
                                let r = exp.Convert (r, typeof<IDynamic>)
                                exp.Property (r, "Value") :> exp        
                    ) e1)
                return (rest :?> MemberExpression).Expression                  
        }) state
        
    and evalNewExpression state =
        choice [|
            attempt (parse {
                do! skipToken "new"
                let! r = evalNewExpression   
                let args = [| exp.Constant (environment.EmptyArgs) :> exp |]
                return exp.Call (r, Reflection.IDynamic.op_Construct, args) :> exp
            })
            evalMemberExpression
        |] state

    and evalCallExpression e1 state =
        (parse {
            do! skipIgnorableTokens
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
                        let convert = exp.Property (convert, "BaseValue") :> exp                       
                        let args = [| convert; exp.Convert(acc, typeof<IReferenceBase>) :> exp; exp.Constant (s.strict.Head |> fst) :> exp |]
                        let r = exp.Call (environmentParam, Reflection.IEnvironment.createReference, args) :> exp  
                        let r = exp.Convert (r, typeof<IDynamic>)
                        exp.Property (r, "Value") :> exp
                    | "dot" ->                         
                        let args = [| e; exp.Convert(acc, typeof<IReferenceBase>) :> exp; exp.Constant (s.strict.Head |> fst) :> exp |]
                        let r = exp.Call (environmentParam, Reflection.IEnvironment.createReference, args) :> exp 
                        let r = exp.Convert (r, typeof<IDynamic>)
                        exp.Property (r, "Value") :> exp            
            ) first
        }) state

    and evalArguments state =
        (parse {
            do! skipToken "("
            let! r = evalArgumentList
            do! skipToken ")"
            return r
        }) state

    and evalArgumentList state =
        (parse {
            let! r = sepBy (skipIgnorableTokens >>. evalAssignmentExpression) (skipToken ",")
            let args = [| exp.NewArrayInit (typeof<IDynamic>, r) :> exp |]
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
        choice [| 
            parse {
                let! e1 = evalMemberExpression
                let! e2 = opt (attempt (evalCallExpression e1))
                return if e2.IsSome then e2.Value else e1
            }
            evalNewExpression
        |]state

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
                do! skipIgnorableTokens
                match op with
                | Some op ->
                    let left = e1
                    let left = exp.Convert (e1, typeof<IDynamic>) :> exp
                    let left = exp.Property (left, "Value") :> exp
                    let! right = parser2
                    let right = exp.Convert (right, typeof<IDynamic>) :> exp
                    let right = exp.Property (right, "Value") :> exp
                    let performOp = exp.Call (left, op, [| right |]) :> exp
                    return exp.Assign (left, performOp) :> exp
                | None ->
                    let left = e1
                    let left = exp.Convert (e1, typeof<IDynamic>) :> exp
                    let left = exp.Property (left, "Value")
                    let! right = parser2
                    let right = exp.Convert (right, typeof<IDynamic>) :> exp
                    let right = exp.Property (right, "Value")
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
                let r = r |> List.map (fun e -> exp.Property (e, "Value") :> exp)
                return if r.Length = 1 then r.Head else exp.Block (r) :> exp
        }) state

    and evalExpression state =
        evalExpressionCommon evalAssignmentExpression state

    and evalExpressionNoIn state =
        evalExpressionCommon evalAssignmentExpressionNoIn state

     
    and evalStatement state =
        (skipIgnorableTokens >>. choice [|
            attempt evalBlock
            attempt evalVariableStatement
            attempt evalIfStatement
            attempt evalIterationStatement
            attempt evalContinueStatement
            attempt evalBreakStatement
            attempt evalReturnStatement
            attempt evalWithStatement
            attempt evalLabelledStatement
            attempt evalSwitchStatement
            attempt evalThrowStatement
            attempt evalTryStatement
            attempt evalDebuggerStatement
            attempt evalEmptyStatement
            attempt evalExpressionStatement
        |]) state

    and evalBlock state =
        (parse {
            do! skipToken "{"
            do! skipIgnorableTokens
            let! e = opt evalStatementList
            do! skipToken "}"
            match e with
            | Some e -> return e
            | None -> return exp.Convert (propUndefined, typeof<IDynamic>) :> exp
        }) state 

    and evalStatementList state =
        (parse {
            let! r = many (attempt evalStatement)
            let e = r |> List.filter (fun e -> e.Type <> typeof<Void> || e.NodeType = ExpressionType.Goto || e.NodeType = ExpressionType.Conditional)
            if e.IsEmpty 
            then return exp.Convert (propUndefined, typeof<IDynamic>) :> exp
            else 
                let r = exp.Block (e)
                if r.Type <> typeof<IDynamic> then
                    return exp.Block (typeof<IDynamic>, Seq.append r.Expressions [| propUndefined |]) :> exp
                else
                    return r :> exp
        }) state

    and evalVariableStatement state =
        (skipToken "var" >>. evalVariableDeclarationList .>> skipStatementTerminator) state

    and evalVariableDeclarationListCommon parser state =
        (parse {
            let! r = sepBy (attempt (skipIgnorableTokens >>. parser)) (attempt skipComma)
            if r.IsEmpty 
            then return propUndefined
            else return exp.Block (typeof<IDynamic>, r @ [ propUndefined ]) :> exp 
        }) state  

    and evalVariableDeclarationList state =
        evalVariableDeclarationListCommon evalVariableDeclaration state

    and evalVariableDeclarationListNoIn state =
        evalVariableDeclarationListCommon evalVariableDeclarationNoIn state

    and evalVariableDeclarationCommon parser state =
        (parse {
            let! identifier = evalIdentifier
            let! oldState = getUserState
            do! setUserState { oldState with variables = identifier::oldState.variables; labels = oldState.labels }
            let args = [| exp.Constant (identifier) :> exp; exp.Constant (oldState.strict.Head |> fst) :> exp |]
            let reference = exp.Call (propLexicalEnviroment, 
                                      Reflection.ILexicalEnvironment.getIdentifierReference, 
                                      args) :> exp

            let reference = exp.Convert (reference, typeof<IDynamic>) :> exp
            let reference = exp.Property (reference, "Value") :> exp
            let! e2 = opt (attempt parser)
            match e2 with
            | Some e2 ->
                return exp.Assign (reference, e2) :> exp
            | None ->
                return exp.Assign (reference, exp.Constant (environment.Undefined)) :> exp
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
            do! notFollowedByString "{" .>> notFollowedByString "function"
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
            let! e = skipToken "if" >>. skipToken "(" >>. evalExpression .>> skipToken ")"
            let! s1 = evalStatement
            let e = exp.Call (e, Reflection.IDynamic.convertToBoolean, [||])
            let e = exp.Property (e, "BaseValue") :> exp
            let! r = opt (attempt (skipToken "else"))
            match r with
            | Some _ ->
                do! skipIgnorableTokens
                let! s2 = evalStatement
                return exp.IfThenElse(e, s1, s2) :> exp
            | None ->
                return exp.IfThen(e, s1) :> exp
        }) state

    and evalIterationStatement state =
        (parse {             
            let breakLabelExp = exp.Label(exp.Label(typeof<Void>, "breakLoop"))              
            let breakLabel = { labelExpression = breakLabelExp }
            let continueLabelExp = exp.Label(exp.Label(typeof<Void>, "continueLoop"))   
            let continueLabel = { labelExpression = continueLabelExp }
            let breakExpression = exp.Break (breakLabel.labelExpression.Target)
            let evalDoWhileIterationStatement state =
                (parse { 
                    let! s = skipToken "do" >>. evalStatement
                    let! e = skipToken "while" >>. skipToken "(" >>. evalExpression .>> skipToken ")" .>> skipStatementTerminator
                    let e = exp.Call (e, Reflection.IDynamic.convertToBoolean, [||])
                    let e = exp.Property (e, "BaseValue") :> exp
                    let e = exp.Not e
                    let body = exp.Block ([| s; exp.IfThen (e, breakExpression) :> exp |])
                    return exp.Loop (body, breakLabel.labelExpression.Target, continueLabel.labelExpression.Target) :> exp
                }) state                
            let evalWhileIterationStatement state =
                (parse { 
                    do! skipToken "while"
                    do! skipToken "("
                    do! skipIgnorableTokens
                    let! e = evalExpression
                    let e = exp.Call (e, Reflection.IDynamic.convertToBoolean, [||])
                    let e = exp.Property (e, "BaseValue") :> exp
                    do! skipToken ")" 
                    do! skipIgnorableTokens
                    let! s = evalStatement
                    let body = exp.IfThenElse (e, s, breakExpression)
                    return exp.Loop (body, breakLabel.labelExpression.Target, continueLabel.labelExpression.Target) :> exp
                }) state                
            let evalForIterationStatements state =
                let buildInit (e:option<exp>) =
                    match e with
                    | Some e -> e
                    | None -> propUndefined 
                let buildCondition (e:option<exp>) =
                    match e with
                    | Some e -> 
                        let e = exp.Call (e, Reflection.IDynamic.convertToBoolean, [||])
                        exp.Property (e, "BaseValue") :> exp
                    | None -> exp.Constant (true) :> exp  
                let buildFollowUp (e:option<exp>) =
                    match e with
                    | Some e -> 
                        exp.Property (e, "Value") :> exp
                    | None -> propUndefined           
                let evalFor state = 
                    (parse {
                        let! e1 = opt evalExpressionNoIn
                        do! skipToken ";"
                        let! e2 = opt evalExpression
                        do! skipToken ";"
                        let! e3 = opt evalExpression
                        do! skipToken ")"
                        do! skipIgnorableTokens
                        let! s = evalStatement
                        let init = buildInit e1
                        let condition = buildCondition e2
                        let followUp = buildFollowUp e3
                        let r = exp.IfThenElse (condition, s, breakExpression) :> exp
                        let r = exp.Block ([| r; followUp |]) :> exp
                        let r = exp.Loop (r, breakLabelExp.Target, continueLabelExp.Target) :> exp
                        return exp.Block ([| init; r; propUndefined |]) :> exp
                    }) state
                let evalForWithVar state = 
                    (parse {
                        do! skipToken "var"
                        do! skipIgnorableTokens
                        let! e1 = evalVariableDeclarationListNoIn
                        do! skipToken ";"
                        do! skipIgnorableTokens
                        let! e2 = opt evalExpression
                        do! skipToken ";"
                        do! skipIgnorableTokens
                        let! e3 = opt evalExpression
                        do! skipToken ")"
                        do! skipIgnorableTokens
                        let! s = evalStatement
                        let init = e1
                        let condition = buildCondition e2
                        let followUp = buildFollowUp e3
                        let r = exp.IfThenElse (condition, s, breakExpression) :> exp
                        let r = exp.Block ([| r; followUp |]) :> exp
                        let r = exp.Loop (r, breakLabelExp.Target, continueLabelExp.Target) :> exp
                        return exp.Block ([| init; r; propUndefined |]) :> exp
                    }) state
                let evalForIn state = 
                    (parse {
                        let! e1 = evalLeftHandSideExpression
                        do! skipToken "in"
                        do! skipIgnorableTokens
                        let! e2 = evalExpression
                        do! skipToken ")"
                        do! skipIgnorableTokens
                        let! s = evalStatement
                        let enumeratorVar = exp.Variable(typeof<IEnumerator<string>>, "enumerator")
                        let obj = exp.Call (e2, Reflection.IDynamic.convertToObject, Array.empty) :> exp
                        let asEnumerableString = exp.Convert(obj, typeof<IEnumerable<string>>)
                        let getEnumerator = exp.Call (asEnumerableString, Reflection.IEnumerableString.getEnumerator, Array.empty) :> exp
                        let assignEnumeratorVar = exp.Assign (enumeratorVar, getEnumerator) :> exp
                        let asEnumeratorString = exp.Convert(enumeratorVar, typeof<IEnumerator<string>>)                 
                        let current = exp.Call (asEnumeratorString, Reflection.IEnumeratorString.get_Current, Array.empty) :> exp
                        let asEnumerator = exp.Convert(enumeratorVar, typeof<IEnumerator>)
                        let moveNext = exp.Call (asEnumerator, Reflection.IEnumerator.moveNext, Array.empty)
                        let asDisposable = exp.Convert(enumeratorVar, typeof<IDisposable>)
                        let dispose = exp.Call (asDisposable, Reflection.IDisposable.dispose, Array.empty) :> exp
                        let createString = exp.Call (environmentParam, Reflection.IEnvironment.createString, [| current |]) :> exp
                        let putCurrent = exp.Call (e1, Reflection.IDynamic.set_Value, [| createString |]) :> exp
                        let ifTrue = exp.Block ([| putCurrent; s |]) :> exp
                        let loopCondition = exp.IfThenElse (moveNext, ifTrue, exp.Break(breakLabelExp.Target))
                        let loop = exp.Loop (loopCondition, breakLabelExp.Target, continueLabelExp.Target)
                        let initTest = exp.Not(exp.Or (exp.TypeIs (e1, typeof<IUndefined>), exp.TypeIs (e1, typeof<INull>)))
                        let initCondition = exp.IfThen (initTest, loop) :> exp
                        return exp.Block ([| enumeratorVar |], exp.TryFinally (exp.Block ([| assignEnumeratorVar; initCondition |]) :> exp, dispose)) :> exp
                    }) state
                let evalForInWithVar state = 
                    (parse {
                        do! skipToken "var"
                        do! skipIgnorableTokens
                        let! e1 = evalVariableDeclarationNoIn
                        do! skipToken "in"
                        do! skipIgnorableTokens
                        let! e2 = evalExpression
                        do! skipToken ")"
                        do! skipIgnorableTokens
                        let! s = evalStatement
                        let! currentState = getUserState
                        let enumeratorVar = exp.Variable(typeof<IEnumerator<string>>, "enumerator")
                        let varName = currentState.variables.Head 
                        let args = [| exp.Constant (varName) :> exp; exp.Constant (currentState.strict.Head |> fst) :> exp |]
                        let varRef = exp.Call (propLexicalEnviroment, Reflection.ILexicalEnvironment.getIdentifierReference, args) :> exp
                        let experValue = exp.Property (e2, "Value") :> exp
                        let obj = exp.Call (experValue, Reflection.IDynamic.convertToObject, Array.empty)              
                        let asEnumerableString = exp.Convert(obj, typeof<IEnumerable<string>>)
                        let getEnumerator = exp.Call (asEnumerableString, Reflection.IEnumerableString.getEnumerator, Array.empty) :> exp
                        let assignEnumeratorVar = exp.Assign (enumeratorVar, getEnumerator) :> exp       
                        let asEnumeratorString = exp.Convert(enumeratorVar, typeof<IEnumerator<string>>)      
                        let current = exp.Call (asEnumeratorString, Reflection.IEnumeratorString.get_Current, Array.empty) :> exp
                        let asEnumerator = exp.Convert(enumeratorVar, typeof<IEnumerator>)
                        let moveNext = exp.Call (asEnumerator, Reflection.IEnumerator.moveNext, Array.empty)
                        let asDisposable = exp.Convert(enumeratorVar, typeof<IDisposable>)
                        let dispose = exp.Call (asDisposable, Reflection.IDisposable.dispose, Array.empty) :> exp 
                        let createString = exp.Call (environmentParam, Reflection.IEnvironment.createString, [| current |]) :> exp
                        let putCurrent = exp.Call (varRef, Reflection.IDynamic.set_Value, [| createString |]) :> exp
                        let ifTrue = exp.Block ([| putCurrent; s |]) :> exp
                        let loopCondition = exp.IfThenElse (moveNext, ifTrue, exp.Break(breakLabel.labelExpression.Target))
                        let loop = exp.Loop (loopCondition, breakLabel.labelExpression.Target, continueLabel.labelExpression.Target)
                        let initTest = exp.Not(exp.Or (exp.TypeIs (experValue, typeof<IUndefined>), exp.TypeIs (experValue, typeof<INull>)))
                        let initCondition = exp.IfThen (initTest, loop) :> exp
                        let r = exp.Block ([| assignEnumeratorVar; initCondition |]) :> exp
                        return exp.Block ([| enumeratorVar |], exp.TryFinally (r , dispose)) :> exp
                    }) state
                (parse {
                    do! skipToken "for"
                    do! skipToken "("
                    do! skipIgnorableTokens  
                    let! e = attempt evalForWithVar <|> attempt evalForInWithVar <|> attempt evalFor <|> attempt evalForIn
                    return e
                }) state            
            let! currentState = getUserState
            let labels = currentState.labels.Head.Add("breakLoop", breakLabel).Add("continueLoop", continueLabel)
            do! setUserState { currentState with labels = labels::currentState.labels }
            let! r = choice [| evalDoWhileIterationStatement; evalWhileIterationStatement; evalForIterationStatements |]
            do! setUserState currentState
            return exp.Block ([| r; propUndefined |]) :> exp
        }) state

    and evalContinueStatement state =
        let identifierNotFound = "The continue statement is invalid because the label '{0}' does not exist in the surrounding scope."
        let noSurroundingLoop = "The continue statement with no identifier requires a surrounding iteration statement."
        (parse {
            do! skipToken "continue"
            do! skipMany evalWhiteSpace
            let! identifier = opt evalIdentifier      
            do! skipStatementTerminator
            let! currentState = getUserState 
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
            do! skipToken "break"
            do! skipMany evalWhiteSpace
            let! identifier = opt evalIdentifier      
            do! skipStatementTerminator
            let! currentState = getUserState 
            let labels = currentState.labels.Head
            match identifier with
            | Some identifier ->
                let label = labels.TryFind ("break" + identifier)
                match label with
                | Some label ->
                    return exp.Break (label.labelExpression.Target, propUndefined, typeof<IDynamic>) :> exp
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
                    return exp.Break (labelSwitch.labelExpression.Target, propUndefined, typeof<IDynamic>) :> exp
                | None, Some labelLoop ->
                    return exp.Break (labelLoop.labelExpression.Target, propUndefined, typeof<IDynamic>) :> exp
                | None, None ->
                    let msg = String.Format (noSurrounding, identifier) 
                    let err = environment.CreateSyntaxError msg
                    raise err
        }) state

    and evalReturnStatement state =
        (parse {
            do! skipToken "return"
            do! skipMany evalWhiteSpace
            let! e = opt evalExpression      
            do! skipStatementTerminator
            let returnLabel = state.UserState.labels.Head.["return"].labelExpression
            let e = match e with | Some e -> exp.Property(e, "Value") :> exp | None -> propUndefined
            return exp.Return (returnLabel.Target, e, typeof<IDynamic>) :> exp
        }) state

    and evalWithStatement state =
        (parse {
            let! e = skipToken "with" >>. skipToken "(" >>. evalExpression .>> skipToken ")"
            let! s = evalStatement
            let! currentState = getUserState 
            match currentState.strict.Head with
            | false, _ ->              
                let oldEnvVar = exp.Variable(typeof<ILexicalEnvironment>, "oldEnv")
                let newEnvVar = exp.Variable(typeof<ILexicalEnvironment>, "newEnv")
                let variables = [| oldEnvVar; newEnvVar |]
                let obj = exp.Call (e, Reflection.IDynamic.convertToObject, Array.empty) :> exp
                let assignOldEnvVar = exp.Assign (oldEnvVar, propLexicalEnviroment) :> exp
                let newObjEnv = exp.Call (oldEnvVar, Reflection.ILexicalEnvironment.newObjectEnvironment, [| obj; exp.Constant (true) :> exp |]) :> exp
                let assignNewEnvVar = exp.Assign (newEnvVar, newObjEnv) :> exp
                let assignNewEnv = exp.Assign (propLexicalEnviroment, newEnvVar) :> exp 
                let assignOldEnv = exp.Assign (propLexicalEnviroment, oldEnvVar) :> exp  
                let tryBody = exp.Block ([| assignOldEnvVar; assignNewEnvVar; assignNewEnv; s |])
                let finallyBody = exp.Block ([| assignOldEnv |])
                return exp.Block (variables, exp.TryFinally (tryBody, finallyBody)) :> exp
            | _ ->
                raise (environment.CreateSyntaxError "The with statement is not allowed in strict mode.")
        }) state

    and evalSwitchStatement state =
        (parse {
            let! e = skipToken "switch" >>. skipToken "(" >>. evalExpression .>> skipToken ")"
            let breakName = "breakSwitch" 
            let breakLabelExp = exp.Label(exp.Label(typeof<IDynamic>, breakName), propUndefined)
            let breakLabel = breakName, { labelExpression = breakLabelExp }
            let! currentState = getUserState 
            let labels = currentState.labels.Head.Add(breakLabel) 
            do! setUserState { currentState with labels = labels::currentState.labels }
            do! skipIgnorableTokens
            let! caseBlock = evalCaseBlock
            do! setUserState currentState
            let switch = 
                match caseBlock with
                | [], None, [] ->
                    exp.Switch (typeof<IDynamic>, e, propUndefined, equalityTestMethod, [||]) :> exp
                | [], Some defaultClause, [] ->
                    exp.Switch (typeof<IDynamic>, e, defaultClause, equalityTestMethod, [||]) :> exp
                | beforeCaseClauses, None, afterCaseClauses ->
                    exp.Switch (typeof<IDynamic>, e, propUndefined, equalityTestMethod, beforeCaseClauses @ afterCaseClauses) :> exp
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
            | None -> return exp.SwitchCase (propUndefined, [| e1 |])
        }) state

    and evalDefaultClause state =
        (parse {
            do! skipToken "default" >>. skipToken ":"
            let! e = opt evalStatementList            
            match e with
            | Some e -> return e
            | None -> return exp.Convert (propUndefined, typeof<IDynamic>) :> exp
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
            do! setUserState { currentState with labels = labels::currentState.labels }
            let! s = evalStatement
            do! setUserState currentState
            return exp.Block (typeof<IDynamic>, [| continueLabelExp :> exp; s; breakLabelExp :> exp; propUndefined |]) :> exp
        }) state

    and evalThrowStatement state =
        (parse {
            do! skipToken "throw"
            do! skipMany evalWhiteSpace
            let! e = evalExpression 
            return exp.Block ([| exp.Call (e, Reflection.IDynamic.op_Throw, [||]) :> exp; propUndefined |]) :> exp
        }) state

    and evalTryStatement state =
        let missingCatchFinally = "The try statement requires either a catch block or a finally block."
        (parse {
            do! skipToken "try"
            let! e1 = evalBlock
            let! e2 = opt (attempt evalCatch)
            let! e3 = opt (attempt evalFinally)
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
            do! skipToken "catch"
            do! skipToken "("
            let! identifier = evalIdentifier
            do! skipToken ")"
            let! block = evalBlock
            let catchVar = exp.Variable(typeof<MacheteRuntimeException>, "catch")
            let oldEnvVar = exp.Variable(typeof<ILexicalEnvironment>, "oldEnv")
            let catchEnvVar = exp.Variable(typeof<ILexicalEnvironment>, "catchEnv")
            let catchRecVar = exp.Variable(typeof<IEnvironmentRecord>, "catchRec")
            let assignOldEnv = exp.Assign (oldEnvVar, propLexicalEnviroment) :> exp
            let newEnv = exp.Call (oldEnvVar, Reflection.ILexicalEnvironment.newDeclarativeEnvironment, Array.empty) :> exp 
            let assignCatchEnv = exp.Assign (catchEnvVar, newEnv) :> exp
            let getRec = exp.Call (catchEnvVar, Reflection.ILexicalEnvironment.get_Record, Array.empty) :> exp 
            let assignCatchRec = exp.Assign (catchRecVar, exp.Convert(getRec, typeof<IEnvironmentRecord>)) :> exp
            let identifier = exp.Constant (identifier) :> exp
            let boolFalse = exp.Constant (false) :> exp
            let createBinding = exp.Call (catchRecVar, Reflection.IEnvironmentRecord.createMutableBinding, [| identifier; boolFalse |]) :> exp 
            let getThrown = exp.Call (catchVar, Reflection.MacheteRuntimeException.get_Thrown, Array.empty) :> exp 
            let setBinding = exp.Call (catchRecVar, Reflection.IEnvironmentRecord.setMutableBinding, [| identifier; getThrown; boolFalse |]) :> exp
            let assignEnv = exp.Assign (propLexicalEnviroment,  catchEnvVar) :> exp 
            let tryBody = exp.Block ([| assignOldEnv; assignCatchEnv; assignCatchRec; createBinding; setBinding; assignEnv; block |]) :> exp
            let assignOldEnv = exp.Assign (propLexicalEnviroment,  oldEnvVar) :> exp 
            let finallyBody = exp.Block ([| assignOldEnv |]) :> exp      
            let body = exp.Block ([| oldEnvVar; catchEnvVar; catchRecVar |], exp.TryFinally (tryBody, finallyBody)) :> exp      
            return exp.Catch (catchVar, body)
        }) state     

    and evalFinally state =
        (skipToken "finally" >>. evalBlock) state

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
            let variableDeclarations = ReadOnlyList<string>(state.variables |> List.rev)
            let functionDeclarations = ReadOnlyList<FunctionDeclaration>(state.functions |> List.rev)
            let ec = ExecutableCode (functionBody.Compile(), variableDeclarations, functionDeclarations, state.strict.Head |> fst)
            let func = FunctionDeclaration(identifier, formalParameterList, ec)
            do! setUserState ({ oldState with functions = func::oldState.functions  })
            return exp.Empty() :> exp
        }) state

    and evalFunctionExpression state =
        (parse {
            let! (oldState:State1) = getUserState
            let! posStart = getPosition
            do! skipToken "function"
            do! skipIgnorableTokens
            let! identifier = opt evalIdentifier
            do! skipToken "("
            do! skipIgnorableTokens
            let! formalParameterList = evalFormalParameterList
            do! skipToken ")"
            do! skipToken "{"
            do! skipIgnorableTokens
            let! functionBody = evalFunctionBody
            do! skipToken "}"
            let! (newState:State1) = getUserState
            let variableDeclarations = ReadOnlyList<string>(newState.variables |> List.rev)
            let functionDeclarations = ReadOnlyList<FunctionDeclaration>(newState.functions |> List.rev)
            let strict = newState.strict.Head |> fst
            let exC = ExecutableCode (functionBody.Compile(), variableDeclarations, functionDeclarations, strict)
            let args = [| exp.Constant(exC):>exp; exp.Constant(formalParameterList) :> exp; propLexicalEnviroment |]
            let func = exp.Call (environmentParam, Reflection.IEnvironment.createFunction3, args) :> exp
            return func
        }) state

    and evalFormalParameterList state =
        (parse {
            let! r = sepBy (attempt (skipIgnorableTokens >>. evalIdentifier)) (skipToken ",")
            return ReadOnlyList<string> r
        }) state

    and evalFunctionBody state =
        (parse {            
            let! (oldState:State1) = getUserState
            let returnLabel = oldState.labels.Head.["return"]
            let! e1 = many (attempt evalSourceElement)            
            let body = exp.Block (e1 @ [ returnLabel.labelExpression :> exp ])
            return exp.Lambda<Code>(body, [| environmentParam; argsParam |])
        }) state

    and evalSourceElement state =
        (attempt evalStatement <|> evalFunctionDeclaration) state

    and evalSourceElements state =
        manyTill (evalSourceElement) eof state      
         
    and evalProgram (state:FParsec.State<State1>) =
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
            let! (oldState:State1) = getUserState
            let returnLabel = oldState.labels.Head.["return"]
            let! e1 = evalSourceElements
            let body = complete e1
            return exp.Lambda<Code>(body, [| environmentParam; argsParam |])
        }) state

    let createInitialState (strict:bool) =
        let returnLabel = { labelExpression = exp.Label(exp.Label(typeof<IDynamic>, "return"), exp.Constant (environment.Undefined)) }
        { strict = [strict, false]; element = Nil; labels = [ Map.ofArray [| "return", returnLabel |] ]; functions = []; variables = [] }        

    let compile (parser:Parser<Expression<Code>, State1>) (input:string) (strict:bool) (streamName:string) =
        let initialState = createInitialState strict
        let input = input.Trim()
        let result = runParserOnString parser initialState streamName input
        match result with
        | Success (expression, finalState, position) ->
            let variableDeclarations = ReadOnlyList<string>(finalState.variables |> List.rev)
            let functionDeclarations = ReadOnlyList<FunctionDeclaration>(finalState.functions |> List.rev)
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

    static member private equalityTest (left:obj, right:obj) =
        let left = left :?> IDynamic
        let right = right :?> IDynamic
        left.Op_StrictEquals(right).ConvertToBoolean().BaseValue