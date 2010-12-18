namespace Machete.Compiler

module Parser =
    open System
    open Machete.Compiler.Lexer
    open Machete.Compiler.Tools

    type State = {
        dummy:int
    }

    type SimpleParser = Parser<InputElement, State, InputElement>
    type ComplexParser = Parser<InputElement, State, SourceElement>


    let rec passLineTerminator () : SimpleParser =
            parse {
                let! v = item
                match v with
                | LineTerminator -> 
                    return! passLineTerminator ()
                | _ -> 
                    return v
            }
         
    let expectIdentifierName : SimpleParser =
        parse {
            let! v = passLineTerminator ()
            match v with
            | IdentifierName (x, y) ->
                return v
            | _ -> ()
        }  
        
    let expectSpecificIdentifierName value : SimpleParser =
        parse {
            let! v = passLineTerminator ()
            match v with
            | IdentifierName (x, y) 
                when IdentifierNameParser.evalIdentifierName v = value ->
                        return v
            | _ -> ()
        }

    let expectSpecificIdentifierNames (value:Set<string>) : SimpleParser =
        parse {
            let! v = passLineTerminator ()
            match v with
            | IdentifierName (_, _) 
                when value.Contains (IdentifierNameParser.evalIdentifierName v) -> 
                    return v
            | _ -> ()
        } 

    let expectIdentifier : SimpleParser =
        parse {
            let! v = passLineTerminator ()
            match v with
            | IdentifierName (x, y)
                when not (CharSets.reservedWordSet.Contains (IdentifierNameParser.evalIdentifierName v)) ->
                    return Identifier v
            | _ -> ()
        }  
            
    let expectPunctuator value : SimpleParser =
        parse {
            let! v = passLineTerminator ()
            match v with
            | Punctuator (Str str)  
            | DivPunctuator (Str str)
                when str = value -> 
                    return v
            | _ -> ()
        } 
         
    let expectPunctuators (value:Set<string>) : SimpleParser =
        parse {
            let! v = maybe (passLineTerminator ())
            match v with
            | Some (Punctuator (Str str)) 
            | Some (DivPunctuator (Str str))
                when value.Contains str -> 
                    return v.Value
            |
             _ -> ()
        }  
    let expectOperatorsWithTransform<'a> (value:Map<string, 'a>) =
        parse {
            let! v = maybe (passLineTerminator ())
            match v with
            | Some (Punctuator (Str str)) 
            | Some (DivPunctuator (Str str)) -> 
                let r = value.TryFind str
                match r with
                | Some v ->
                    return v
                | None -> ()
            | Some v ->
                match v with
                | IdentifierName (_, _) ->
                    let str = Lexer.IdentifierNameParser.evalIdentifierName v
                    let r = value.TryFind str
                    match r with
                    | Some v ->
                        return v
                    | None -> ()
                | _ -> ()
            | _ -> ()
        } 
        
    let expectComma : SimpleParser = expectPunctuator ","  

    let nil : ComplexParser = result SourceElement.Nil
    
    let statementTerminator =
        parse {
            let! v = maybe item
            match v with
            | Some v ->
                match v with
                | Punctuator (Str ";") 
                | Punctuator (Str "}")
                | LineTerminator -> return ()
                | _ -> ()
            | None -> 
                return ()
        }



    let expectOpenParenthesis = expectPunctuator "("
    let expectCloseParenthesis = expectPunctuator ")"        
    let expectOpenBrace = expectPunctuator "{"
    let expectCloseBrace = expectPunctuator "}"        
    let expectOpenBracket = expectPunctuator "["
    let expectCloseBracket = expectPunctuator "]"
    let expectSemiColon = expectPunctuator ";"
    let expectEqualsSign = expectPunctuator "="
    let expectQuestionMark = expectPunctuator "?"
    let expectColon = expectPunctuator ":"
    let expectFullStop = expectPunctuator "."

    let expectDo = expectSpecificIdentifierName "do"
    let expectFor = expectSpecificIdentifierName "for"
    let expectVar = expectSpecificIdentifierName "var"
    let expectWhile = expectSpecificIdentifierName "while"
    let expectIn = expectSpecificIdentifierName "in"
    let expectIf = expectSpecificIdentifierName "if"
    let expectElse = expectSpecificIdentifierName "else"
    let expectFunction = expectSpecificIdentifierName "function"
    let expectNew = expectSpecificIdentifierName "new"

    let assignmentOperatorMap =
        Map.ofList [
            ("=", AssignmentOperator.SimpleAssignment)
            ("*=", AssignmentOperator.MultiplyAssignment)
            ("/=", AssignmentOperator.DivideAssignment)
            ("%=", AssignmentOperator.ModulusAssignment)
            ("+=", AssignmentOperator.PlusAssignment)
            ("-=", AssignmentOperator.MinusAssignment)
            ("<<=", AssignmentOperator.LeftShiftAssignment)
            (">>=", AssignmentOperator.SignedRightShiftAssignment)
            (">>>=", AssignmentOperator.UnsignedRightShiftAssignment)
            ("&=", AssignmentOperator.BitwiseAndAssignment)
            ("^=", AssignmentOperator.BitwiseXorAssignment)
            ("|=", AssignmentOperator.BitwiseOrAssignment)
        ]

    let expectAssignmentOperator = 
        expectOperatorsWithTransform assignmentOperatorMap
        //expectPunctuators (set ["=";"*=";"/=";"%=";"+=";"-=";"<<=";">>=";">>>=";"&=";"^=";"|="])

    let expectEqualityOperator = 
        expectPunctuators (set ["=="; "!="; "==="; "!=="])           
            >>= 
                fun p -> 
                    match p with
                    | Punctuator (Str "==") -> result EqualityOperator.Equal
                    | Punctuator (Str "!=") -> result EqualityOperator.DoesNotEqual
                    | Punctuator (Str "===") -> result EqualityOperator.StrictEqual
                    | Punctuator (Str "!==") -> result EqualityOperator.StrictDoesNotEqual

    let unaryOperatorMap =
        Map.ofList [
            ("++", UnaryOperator.Increment)
            ("--", UnaryOperator.Decrement)
            ("+", UnaryOperator.Plus)
            ("-", UnaryOperator.Minus)
            ("~", UnaryOperator.BitwiseNot)
            ("!", UnaryOperator.LogicalNot)
            ("delete", UnaryOperator.Delete)
            ("void", UnaryOperator.Void)
            ("typeof", UnaryOperator.Typeof)
        ]

    let relationalOperatorNoInMap =
        Map.ofList [
            ("<", RelationalOperator.LessThan)
            (">", RelationalOperator.GreaterThan)
            ("<=", RelationalOperator.LessThanOrEqual)
            (">=", RelationalOperator.GreaterThanOrEqual)
            ("instanceof", RelationalOperator.Instanceof)   
        ]

    let relationalOperatorMap =
        relationalOperatorNoInMap.Add ("in", RelationalOperator.In)

    let expectRelationalOperator = 
        expectOperatorsWithTransform relationalOperatorMap
    
    let expectRelationalOperatorNoIn = 
        expectOperatorsWithTransform relationalOperatorNoInMap
    //expectPunctuators (set ["<"; ">"; "<="; ">="; "instanceof"; "in"]) |>> InputElement


    //let expectRelationalOperatorNoIn = expectPunctuators (set ["<"; ">"; "<="; ">="; "instanceof"]) |>> InputElement

    let expectShiftOperator = 
        expectPunctuators (set ["<<"; ">>"; ">>>"])          
            >>= 
                fun p -> 
                    match p with
                    | Punctuator (Str "<<") -> result BitwiseShiftOperator.LeftShift
                    | Punctuator (Str ">>") -> result BitwiseShiftOperator.SignedRightShift
                    | Punctuator (Str ">>>") -> result BitwiseShiftOperator.UnsignedRightShift

    let expectAdditiveOperator = 
        expectPunctuators (set ["+"; "-"]) 
            >>= fun p -> match p with
                         | Punctuator (Str "+") -> result AdditiveOperator.Plus
                         | Punctuator (Str "-") -> result AdditiveOperator.Minus

    let expectMultiplicativeOperator = 
        expectPunctuators (set ["*"; "%"; "/"])          
            >>= 
                fun p -> 
                    match p with
                    | Punctuator (Str "*") -> result MultiplicativeOperator.Multiply
                    | Punctuator (Str "%") -> result MultiplicativeOperator.Modulus
                    | DivPunctuator (Str "/") -> result MultiplicativeOperator.Divide

    let expectPostfixOperator = 
        expectOperatorsWithTransform (Map.ofList [("++", PostfixOperator.Increment); ("--", PostfixOperator.Decrement)]) <|> result PostfixOperator.Nil
    
    //expectPunctuators (set ["++"; "--"]) |>> InputElement



    let expectUnaryOperator =
        expectOperatorsWithTransform unaryOperatorMap <|> result UnaryOperator.Nil 

    let expression, expressionRef = createParserRef<InputElement, State, SourceElement>()
    let expressionNoIn, expressionNoInRef = createParserRef<InputElement, State, SourceElement>()
    let memberExpression, memberExpressionRef = createParserRef<InputElement, State, SourceElement>()
    let callExpression, callExpressionRef = createParserRef<InputElement, State, SourceElement>()
    let newExpression, newExpressionRef = createParserRef<InputElement, State, SourceElement>()
    let assignmentExpression, assignmentExpressionRef = createParserRef<InputElement, State, SourceElement>()
    let assignmentExpressionNoIn, assignmentExpressionNoInRef = createParserRef<InputElement, State, SourceElement>()
    
    let bracketedExpression =
        between expectOpenBracket expectCloseBracket expression

    let expectDotNotation =
        parse {
            do! skip expectFullStop
            let! e = expectIdentifierName |>> InputElement
            return e
        }

    let expectLiteral = 
        parse {
            let! v = passLineTerminator ()
            match v with
            | IdentifierName (_, _) ->
                match IdentifierNameParser.evalIdentifierName v with
                | "true" | "false" as s -> return Literal (BooleanLiteral s)
                | "null" as s -> return Literal (NullLiteral s)
                | _ -> ()
            | StringLiteral _
            | NumericLiteral _ -> return Literal v
            | _ -> ()
        }
    

    let rec primaryExpression : ComplexParser =
       ((between expectOpenParenthesis expectCloseParenthesis expression) |>> PrimaryExpression) <|>
       (objectLiteral |>> PrimaryExpression) <|>
       (arrayLiteral |>> PrimaryExpression) <|>
       (expectIdentifier |>> InputElement |>> PrimaryExpression) <|>
        parse {
            let! v = passLineTerminator ()
            match v with
            | IdentifierName (_, _) ->
                match IdentifierNameParser.evalIdentifierName v with
                | "true" | "false" as s -> return PrimaryExpression (InputElement (Literal (BooleanLiteral s)))
                | "null" as s -> return PrimaryExpression (InputElement (Literal (NullLiteral s)))
                | "this" -> return PrimaryExpression (InputElement v)
                | _ -> ()
            | StringLiteral _
            | NumericLiteral _ -> return PrimaryExpression (InputElement (Literal v))
            | _ -> ()
        }
//        choice [
//            (expectLiteral |>> InputElement)
//            (expectSpecificIdentifierName "this" |>> InputElement)
//            (between expectOpenParenthesis expectCloseParenthesis expression)
//            (expectIdentifier |>> InputElement)
//            (objectLiteral)
//            (arrayLiteral)
//        ] |>> PrimaryExpression

    and arrayLiteral =
        parse {
            do! skip expectOpenBracket
            let! e1 = elementList <|> nil
            do! skip (maybe expectComma)
            let! e2 = elision <|> nil
            do! skip expectCloseBracket
            return ArrayLiteral (e1, e2)
        } 

    and elementList =
        parse {
            let! e1 = elision <|> nil
            let! e2 = assignmentExpression
            let e1 = ElementList (SourceElement.Nil, e1, e2)
            return! manyFold (parse {
                do! skip expectComma
                let! e1 = elision <|> nil
                let! e2 = assignmentExpression
                return e1, e2
            }) e1 (fun x (y, z) -> ElementList (x, y, z))
        }
        //manySepFold (tuple2 (elision <|> nil) assignmentExpression (fun (x, y) -> (x, y))) expectComma ElementList SourceElement.Nil
        //manySepFold (tuple2 (elision <|> nil) assignmentExpression (fun (x, y) -> x)) expectComma (fun (e, (y, z)) -> ElementList (x, y, z)) SourceElement.Nil //(fun x (y, z) -> ElementList (x, y, z))

    and elision = 
        many1Fold expectComma SourceElement.Nil (fun x y -> Elision (x))

    and objectLiteral =
        parse {
            do! skip expectOpenBrace
            let! e = propertyNameAndValueList
            do! skip (maybe expectComma)
            do! skip expectCloseBrace
            return ObjectLiteral e
        }

    and propertyNameAndValueList =
        manySepFold propertyAssignment expectComma PropertyNameAndValueList SourceElement.Nil //(fun x y -> PropertyNameAndValueList (x, y))

    and propertyAssignment =
        parse {
            let! e1 = propertyName
            do! skip expectColon
            let! e2 = assignmentExpression
            return PropertyAssignment (e1, SourceElement.Nil, e2)
        } <|> parse {
            do! skip (expectSpecificIdentifierName "get")
            let! e1 = propertyName
            do! skip expectOpenParenthesis 
            do! skip expectCloseParenthesis
            let! e2 = between expectOpenBrace expectCloseBrace functionBody
            return PropertyAssignment (e1, SourceElement.Nil, e2)
        } <|> parse {
            do! skip (expectSpecificIdentifierName "set")
            let! e1 = propertyName
            let! e2 = between expectOpenParenthesis expectCloseParenthesis propertySetParameterList
            let! e3 = between expectOpenBrace expectCloseBrace functionBody
            return PropertyAssignment (e1, e2, e3)
        }

    and propertyName =
        parse {
            let! v = passLineTerminator ()
            match v with
            | IdentifierName (_, _)
            | StringLiteral _
            | NumericLiteral _ -> return PropertyName v
            | _ -> ()
        } 

    and propertySetParameterList =
        expectIdentifier |>> PropertySetParameterList

    and arguments = 
        between expectOpenParenthesis expectCloseParenthesis (argumentList <|> nil) |>> Arguments

    and argumentList : ComplexParser = 
        manySepFold assignmentExpression expectComma ArgumentList SourceElement.Nil //(fun x y -> ArgumentList (x, y))     

    and leftHandSideExpression =
        choice [
            callExpression
            newExpression
        ] |>> LeftHandSideExpression

    and postfixExpression =
        parse {
            let! e1 = leftHandSideExpression
            let! e2 = expectPostfixOperator
            return PostfixExpression (e1, e2)
        } 
           
    and unaryExpression = 
        parse {
            let! e1 = expectUnaryOperator
            let! e2 = postfixExpression
            return UnaryExpression (e1, e2)
        }

    and multiplicativeExpression =
        manyWithSepFold unaryExpression expectMultiplicativeOperator MultiplicativeExpression SourceElement.Nil MultiplicativeOperator.Nil 
    
    and additiveExpression =
        manyWithSepFold multiplicativeExpression expectAdditiveOperator AdditiveExpression SourceElement.Nil AdditiveOperator.Nil
    
    and shiftExpression =
        manyWithSepFold additiveExpression expectShiftOperator ShiftExpression SourceElement.Nil BitwiseShiftOperator.Nil
    
    and relationalExpression =
        manyWithSepFold shiftExpression expectRelationalOperator RelationalExpression SourceElement.Nil RelationalOperator.Nil
    
    and relationalExpressionNoIn =
        manyWithSepFold shiftExpression expectRelationalOperatorNoIn RelationalExpressionNoIn SourceElement.Nil RelationalOperator.Nil

    and equalityExpression =
        manyWithSepFold relationalExpression expectEqualityOperator EqualityExpression SourceElement.Nil EqualityOperator.Nil 

    and equalityExpressionNoIn =
        manyWithSepFold relationalExpressionNoIn expectEqualityOperator EqualityExpression SourceElement.Nil EqualityOperator.Nil
    
    and bitwiseANDExpression =
        manySepFold equalityExpression (expectPunctuator "&") BitwiseANDExpression SourceElement.Nil

    and bitwiseANDExpressionNoIn =
        manySepFold equalityExpressionNoIn (expectPunctuator "&") BitwiseANDExpressionNoIn SourceElement.Nil

    and bitwiseXORExpression =
        manySepFold bitwiseANDExpression (expectPunctuator "^") BitwiseXORExpression SourceElement.Nil

    and bitwiseXORExpressionNoIn =
        manySepFold bitwiseANDExpressionNoIn (expectPunctuator "^") BitwiseXORExpressionNoIn SourceElement.Nil

    and bitwiseORExpression =
        manySepFold bitwiseXORExpression (expectPunctuator "|") BitwiseORExpression SourceElement.Nil

    and bitwiseORExpressionNoIn =
        manySepFold bitwiseXORExpressionNoIn (expectPunctuator "|") BitwiseORExpressionNoIn SourceElement.Nil

    and logicalANDExpression =
        manySepFold bitwiseORExpression (expectPunctuator "&&") LogicalANDExpression SourceElement.Nil

    and logicalANDExpressionNoIn =
        manySepFold bitwiseORExpressionNoIn (expectPunctuator "&&") LogicalANDExpressionNoIn SourceElement.Nil

    and logicalORExpression =
        manySepFold logicalANDExpression (expectPunctuator "||") LogicalORExpression SourceElement.Nil
         
    and logicalORExpressionNoIn =
        manySepFold logicalANDExpressionNoIn (expectPunctuator "||") LogicalORExpressionNoIn SourceElement.Nil

    and conditionalExpression =
        parse {
            let! e1 = logicalORExpression
            return! choice [ 
                parse {
                    do! skip expectQuestionMark
                    let! e2 = assignmentExpression
                    do! skip expectColon
                    let! e3 = assignmentExpression
                    return ConditionalExpression (e1, e2, e3)
                }
                parse {
                    
                    return ConditionalExpression (e1, SourceElement.Nil, SourceElement.Nil)
                }
            ]
        }

    and conditionalExpressionNoIn = 
        parse {
            let! e1 = logicalORExpressionNoIn
            return! parse {
                do! skip expectQuestionMark
                let! e2 = assignmentExpression
                do! skip expectColon
                let! e3 = assignmentExpressionNoIn
                return ConditionalExpressionNoIn (e1, e2, e3)
            }
            return ConditionalExpressionNoIn (e1, SourceElement.Nil, SourceElement.Nil)
        }

//    and assignmentOperator = 
//        expectAssignmentOperator |>> AssignmentOperator

    and functionExpression = 
        parse {
            do! skip expectFunction
            let! i = expectIdentifier <|> result Nil
            let! l = between expectOpenParenthesis expectCloseParenthesis (formalParameterList <|> nil)
            let! b = between expectOpenBrace expectCloseBrace functionBody             
            return FunctionExpression (i, l, b)
        }  
              
    and functionBody = 
        parse {
            let! x = sourceElements <|> nil
            return FunctionBody x
        }
        
    and formalParameterList = 
        manySepFold (expectIdentifier |>> InputElement) expectComma FormalParameterList SourceElement.Nil //(fun x y -> FormalParameterList (x, y))

    and functionDeclaration = 
        parse {
            do! skip expectFunction
            let! i = expectIdentifier
            let! l = between expectOpenParenthesis expectCloseParenthesis (formalParameterList <|> nil)
            let! b = between expectOpenBrace expectCloseBrace functionBody
            return FunctionDeclaration (i, l, b)
        }  

    and statement =
        parse {
            let! v = 
                blockParser <|> 
                variableStatement <|> 
                emptyStatement <|> 
                expressionStatement <|> 
                ifStatement <|> 
                iterationStatement <|>
                continueStatement <|>
                breakStatement <|>
                returnStatement <|>
                withStatement <|>
                switchStatement <|>
                labelledStatement <|>
                throwStatement <|>
                tryStatement <|>
                debuggerStatement
            return Statement v 
        }

    and statementList =  
        manyFold statement SourceElement.Nil (fun x y -> StatementList (x, y))
               
    and blockParser =
        between expectOpenBrace expectCloseBrace (statementList <|> nil) |>> Block

    and variableStatement =
        pipe3 expectVar variableDeclarationList statementTerminator (fun _ v _ -> VariableStatement v)

    and variableDeclarationList = 
        manySepFold variableDeclaration expectComma VariableDeclarationList SourceElement.Nil //(fun x y -> VariableDeclarationList (x, y))

    and variableDeclarationListNoIn = zero
        //manySepFold variableDeclarationNoIn expectComma SourceElement.Nil (fun x y -> VariableDeclarationListNoIn (x, y))

    and variableDeclaration =
        tuple2 expectIdentifier (initializer <|> nil) VariableDeclaration

    and variableDeclarationNoIn = zero
        //tuple2 expectIdentifier (initializerNoIn <|> nil) VariableDeclarationNoIn

    and initializer =
        parse {
            do! skip expectEqualsSign
            let! e = assignmentExpression
            return Initialiser e
        }

    and initializerNoIn =
        parse {
            do! skip expectEqualsSign
            let! e = assignmentExpressionNoIn
            return InitialiserNoIn e
        }

    and emptyStatement =
        parse {
            do! skip expectSemiColon
            return EmptyStatement
        }

    and expressionStatement =
        parse {
            do! isNotFollowedBy expectOpenBrace
            do! isNotFollowedBy expectFunction
            let! e = expression
            do! statementTerminator
            return ExpressionStatement e
        }

    and ifStatement =
        parse {
            do! skip expectIf
            let! e = between expectOpenParenthesis expectCloseParenthesis expression
            let! s1 = statement 
            let! x = maybe expectElse
            match x with
            | Some _ ->
                let! s2 = statement 
                return IfStatement (e, s1, s2) 
            | None -> 
                return IfStatement (e, s1, SourceElement.Nil)  
        }

    and iterationStatement =
        parse {
            let! s = between expectDo expectWhile statement 
            let! e = between expectOpenParenthesis expectCloseParenthesis expression
            do! skip statementTerminator
            return IterationStatement (s, e, SourceElement.Nil, SourceElement.Nil)
        } <|> parse {
            do! skip expectWhile
            let! e = between expectOpenParenthesis expectCloseParenthesis expression 
            let! s = statement 
            do! skip expectCloseParenthesis
            return IterationStatement (s, e, SourceElement.Nil, SourceElement.Nil)
        } <|> parse {
            do! skip expectFor
            do! skip expectOpenParenthesis
            let! e1, e2, e3 = 
                parse {
                    let! e1 = expressionNoIn <|> nil
                    do! skip expectSemiColon
                    let! e2 = expression <|> nil
                    do! skip expectSemiColon
                    let! e3 = expression <|> nil
                    return e1, e2, e3
                } <|> parse {
                    do! skip expectVar
                    let! e1 = variableDeclarationListNoIn
                    do! skip expectSemiColon
                    let! e2 = expression <|> nil
                    do! skip expectSemiColon
                    let! e3 = expression <|> nil
                    return e1, e2, e3
                } <|> parse {
                    let! e1 = leftHandSideExpression
                    do! skip expectIn
                    let! e2 = expression
                    return e1, e2, SourceElement.Nil
                } <|> parse {
                    do! skip expectVar
                    let! e1 = variableDeclarationNoIn
                    do! skip expectIn
                    let! e2 = expression
                    return e1, e2, SourceElement.Nil
                }   
            do! skip expectCloseParenthesis
            let! s = statement 
            return IterationStatement (e1, e2, e3, s)
        } 

    and continueStatement =
        pipe3 (expectSpecificIdentifierName "continue") (expectIdentifier <|> result InputElement.Nil) statementTerminator (fun x y z -> ContinueStatement y)

    and breakStatement =
        pipe3 (expectSpecificIdentifierName "break") (expectIdentifier <|> result InputElement.Nil) statementTerminator (fun x y z -> BreakStatement y)

    and returnStatement =
        (pipe2 (expectSpecificIdentifierName "return") statementTerminator (fun x y -> ReturnStatement SourceElement.Nil))
        <|>
        (pipe3 (expectSpecificIdentifierName "return") expression statementTerminator (fun x y z -> ReturnStatement y))

    and withStatement =
        pipe5 (expectSpecificIdentifierName "with") (expectPunctuator "(") expression (expectPunctuator ")") statement (fun a b c d e -> WithStatement (c, e))

    and switchStatement =
        pipe5 (expectSpecificIdentifierName "switch") (expectPunctuator "(") expression (expectPunctuator ")") caseBlock (fun a b c d e -> SwitchStatement (c, e))

    and caseBlock =
        pipe5 (expectPunctuator "{") (caseClauses <|> nil) (defaultClause <|> nil) (caseClauses <|> nil) (expectPunctuator "}") (fun a b c d e -> CaseBlock (b, c, d))

    and caseClauses =
        manyFold caseClause SourceElement.Nil (fun x y -> CaseClauses (x, y))

    and caseClause =
        pipe4 (expectSpecificIdentifierName "case") expression (expectPunctuator ":") (statementList <|> nil) (fun w x y z -> CaseClause (x, z))

    and defaultClause =
        pipe3 (expectSpecificIdentifierName "default") (expectPunctuator ":") (statementList <|> nil) (fun x y z -> DefaultClause z)

    and labelledStatement =
        pipe3 (expectIdentifier) (expectPunctuator ":") statement (fun x y z -> LabelledStatement (x, z))
               
    and throwStatement =
        pipe3 (expectSpecificIdentifierName "throw") expression statementTerminator (fun x y z -> ThrowStatement y)

    and tryStatement =
        pipe4 (expectSpecificIdentifierName "try") blockParser (catchParser <|> nil) (finallyParser <|> nil) (fun a b c d -> TryStatement (b, c, d))

    and catchParser =
        pipe5 (expectSpecificIdentifierName "catch") (expectPunctuator "(") expectIdentifier (expectPunctuator ")") blockParser (fun a b c d e -> Catch (c, e))

    and finallyParser =
        pipe2 (expectSpecificIdentifierName "finally") blockParser (fun x y -> Finally y)

    and debuggerStatement =
        pipe2 (expectSpecificIdentifierName "debugger") statementTerminator (fun x y -> DebuggerStatement)

    and sourceElement = 
        (statement <|> functionDeclaration) |>> SourceElement

    and sourceElements =  
        manyFold sourceElement SourceElement.Nil (fun x y -> SourceElements (x, y))

    let program = 
        sourceElements <|> nil |>> Program
        
    do 
        expressionRef :=
            parse {
                let! e1 = assignmentExpression
                return! parse {
                    let! _ = expectPunctuator ","
                    let! e2 = assignmentExpression
                    return Expression (e2, e1)
                } 
                return Expression (SourceElement.Nil, e1)
            } 

        expressionNoInRef := 
            parse {
                let! e1 = assignmentExpressionNoIn
                return! parse {
                    let! _ = expectPunctuator ","
                    let! e2 = assignmentExpressionNoIn
                    return ExpressionNoIn (e2, e1)
                } 
                return ExpressionNoIn (SourceElement.Nil, e1)
            } 

        memberExpressionRef :=
            parse {
                do! skip expectNew
                let! e = memberExpression
                let! a = arguments
                return MemberExpression (e, a)
            } <|>  parse {
                let! e1 = primaryExpression <|> functionExpression
                let e1 = MemberExpression (SourceElement.Nil, e1)
                let! e2 = manyFold (bracketedExpression <|> expectDotNotation) e1 (fun x y -> MemberExpression (x, y))
                return e2
            }

        callExpressionRef :=
            parse {
                let! e1 = memberExpression
                let! e2 = arguments
                let e1 = CallExpression (e1, e2)
                let! e2 = manyFold (arguments <|> bracketedExpression <|> expectDotNotation) e1 (fun x y -> CallExpression (x, y))
                return e2
            }

        newExpressionRef :=
            (memberExpression |>> NewExpression) <|> 
            parse {
                do! skip expectNew
                return! newExpression
            }

        assignmentExpressionRef :=
            parse {
                let! a = leftHandSideExpression
                let! b = expectAssignmentOperator
                let! c = assignmentExpression
                return AssignmentExpression (a, b, c)                
            }  <|> parse {
                let! e = conditionalExpression
                return AssignmentExpression (e, AssignmentOperator.Nil, SourceElement.Nil)
            } 

        assignmentExpressionNoInRef :=
            (conditionalExpressionNoIn |>> fun e -> AssignmentExpressionNoIn (e, AssignmentOperator.Nil, SourceElement.Nil)) <|> parse {
                let! a = leftHandSideExpression
                let! b = expectAssignmentOperator
                let! c = assignmentExpressionNoIn
                return AssignmentExpressionNoIn (a, b, c)  
            }

    let parse input =
        let rs = run program (Lexer.tokenize input) ({ dummy = 3 })
        seq {
            for r in rs do
                match r with
                | Success (v, s) -> yield v
                | Failure (ms, s) -> failwith (System.String.Join ("\r\n", ms))
        } |> Seq.head