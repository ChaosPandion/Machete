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
            let! v = passLineTerminator ()
            match v with
            | Punctuator (Str str) 
            | DivPunctuator (Str str)
                when value.Contains str -> 
                    return v
            | _ -> ()
        }  
        
    let expectComma : SimpleParser = expectPunctuator ","  

    let nil : ComplexParser = result SourceElement.Nil
    
    let statementTerminator =
        parse {
            let! v = expectPunctuator ";"
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

    let expectAssignmentOperator = expectPunctuators (set ["=";"*=";"/=";"%=";"+=";"-=";"<<=";">>=";">>>=";"&=";"^=";"|="])
    let expectEqualityOperator = expectPunctuators (set ["=="; "!="; "==="; "!=="]) |>> InputElement
    let expectRelationalOperator = expectPunctuators (set ["<"; ">"; "<="; ">="; "instanceof"; "in"]) |>> InputElement
    let expectRelationalOperatorNoIn = expectPunctuators (set ["<"; ">"; "<="; ">="; "instanceof"]) |>> InputElement
    let expectShiftOperator = expectPunctuators (set ["<<"; ">>"; ">>>"]) |>> InputElement
    let expectAdditiveOperator = expectPunctuators (set ["+"; "-"]) |>> InputElement
    let expectMultiplicativeOperator = expectPunctuators (set ["*"; "%"; "/"]) |>> InputElement
    let expectPostfixOperator = expectPunctuators (set ["++"; "--"]) |>> InputElement
    let expectUnaryOperator = expectPunctuators (set ["++"; "--"; "+"; "-"; "~"; "!"; "delete"; "void"; "typeof"])

    let expression, expressionRef = createParserRef<InputElement, State, SourceElement>()
    let expressionNoIn, expressionNoInRef = createParserRef<InputElement, State, SourceElement>()
    let memberExpression, memberExpressionRef = createParserRef<InputElement, State, SourceElement>()
    let callExpression, callExpressionRef = createParserRef<InputElement, State, SourceElement>()
    let newExpression, newExpressionRef = createParserRef<InputElement, State, SourceElement>()
    let assignmentExpression, assignmentExpressionRef = createParserRef<InputElement, State, SourceElement>()
    let assignmentExpressionNoIn, assignmentExpressionNoInRef = createParserRef<InputElement, State, SourceElement>()
    
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
        choice [
            (expectLiteral |>> InputElement)
            (expectSpecificIdentifierName "this" |>> InputElement)
            (between expectOpenParenthesis expectCloseParenthesis expression)
            (expectIdentifier |>> InputElement)
            (objectLiteral)
            (arrayLiteral)
        ] |>> PrimaryExpression

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
        manySepFold (tuple2 (elision <|> nil) assignmentExpression (fun t -> t)) expectComma SourceElement.Nil (fun x (y, z) -> ElementList (x, y, z))

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
        manySepFold propertyAssignment expectComma SourceElement.Nil (fun x y -> PropertyNameAndValueList (x, y))

    and propertyAssignment =
        parse {
            let! e1 = propertyName
            do! skip expectColon
            let! e2 = assignmentExpression
            return PropertyAssignment (e1, e2, SourceElement.Nil)
        } <|> parse {
            do! skip (expectSpecificIdentifierNames (set ["get"; "set"]))
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
        between expectOpenParenthesis expectCloseParenthesis argumentList |>> Arguments

    and argumentList : ComplexParser =
        manySepFold assignmentExpression expectComma SourceElement.Nil (fun x y -> ArgumentList (x, y))     

    and leftHandSideExpression =
        choice [
            newExpression
            callExpression
        ] |>> LeftHandSideExpression

    and postfixExpression =
        parse {
            let! e1 = leftHandSideExpression
            let! e2 = choice [expectPostfixOperator; nil]
            return PostfixExpression (e1, e2)
        } 
           
    and unaryExpression = 
        parse {
            let! e1 = choice [expectUnaryOperator |>> InputElement; nil]
            let! e2 = postfixExpression
            return UnaryExpression (e1, e2)
        }

    and multiplicativeExpression =
        parse {
            let! e1 = unaryExpression
            return! parse {
                let! e2 = expectMultiplicativeOperator
                let! e3 = multiplicativeExpression
                return MultiplicativeExpression (e1, e2, e3)
            } 
            return MultiplicativeExpression (SourceElement.Nil, SourceElement.Nil, e1)
        } 
    
    and additiveExpression =
        parse {
            let! e1 = multiplicativeExpression
            return! parse {
                let! e2 = expectAdditiveOperator
                let! e3 = additiveExpression
                return AdditiveExpression (e1, e2, e3)
            } 
            return AdditiveExpression (SourceElement.Nil, SourceElement.Nil, e1)
        } 
    
    and shiftExpression =
        parse {
            let! e1 = additiveExpression
            return! parse {
                let! e2 = expectShiftOperator
                let! e3 = shiftExpression
                return ShiftExpression (e1, e2, e3)
            } 
            return ShiftExpression (SourceElement.Nil, SourceElement.Nil, e1)
        }
    
    and relationalExpression =
        parse {
            let! e1 = shiftExpression
            return! parse {
                let! e2 = expectRelationalOperator
                let! e3 = relationalExpression
                return RelationalExpression (e1, e2, e3)
            } 
            return RelationalExpression (SourceElement.Nil, SourceElement.Nil, e1)
        }
    
    and relationalExpressionNoIn =
        parse {
            let! e1 = shiftExpression
            return! parse {
                let! e2 = expectRelationalOperatorNoIn
                let! e3 = relationalExpressionNoIn
                return RelationalExpressionNoIn (e1, e2, e3)
            } 
            return RelationalExpressionNoIn (SourceElement.Nil, SourceElement.Nil, e1)
        } 
    
    and equalityExpression = 
        parse {
            let! e1 = relationalExpression
            return! parse {
                let! e2 = expectEqualityOperator
                let! e3 = equalityExpression
                return EqualityExpression (e1, e2, e3)
            } 
            return EqualityExpression (SourceElement.Nil, SourceElement.Nil, e1)
        }

    and equalityExpressionNoIn =
        parse {
            let! e1 = relationalExpressionNoIn
            return! parse {
                let! e2 = expectEqualityOperator
                let! e3 = equalityExpressionNoIn
                return EqualityExpressionNoIn (e1, e2, e3)
            } 
            return EqualityExpressionNoIn (SourceElement.Nil, SourceElement.Nil, e1)
        } 
    
    and bitwiseANDExpression =
        parse {
            let! e1 = equalityExpression
            return! parse {
                let! _ = expectPunctuator "&"
                let! e2 = bitwiseANDExpression
                return BitwiseANDExpression (e2, e1)
            } 
            return BitwiseANDExpression (SourceElement.Nil, e1)
        }

    and bitwiseANDExpressionNoIn =
        parse {
            let! e1 = equalityExpressionNoIn
            return! parse {
                let! _ = expectPunctuator "&"
                let! e2 = bitwiseANDExpressionNoIn
                return BitwiseANDExpression (e2, e1)
            } 
            return BitwiseANDExpression (SourceElement.Nil, e1)
        }

    and bitwiseXORExpression =
        parse {
            let! e1 = bitwiseANDExpression
            return! parse {
                let! _ = expectPunctuator "^"
                let! e2 = bitwiseXORExpression
                return BitwiseXORExpression (e2, e1)
            } 
            return BitwiseXORExpression (SourceElement.Nil, e1)
        }

    and bitwiseXORExpressionNoIn =
        parse {
            let! e1 = bitwiseANDExpressionNoIn
            return! parse {
                let! _ = expectPunctuator "^"
                let! e2 = bitwiseXORExpressionNoIn
                return BitwiseXORExpressionNoIn (e2, e1)
            } 
            return BitwiseXORExpressionNoIn (SourceElement.Nil, e1)
        }

    and bitwiseORExpression =
        parse {
            let! e1 = bitwiseXORExpression
            return! parse {
                let! _ = expectPunctuator "|"
                let! e2 = bitwiseORExpression
                return BitwiseORExpression (e2, e1)
            } 
            return BitwiseORExpression (SourceElement.Nil, e1)
        }

    and bitwiseORExpressionNoIn =
        parse {
            let! e1 = bitwiseXORExpressionNoIn
            return! parse {
                let! _ = expectPunctuator "|"
                let! e2 = bitwiseORExpressionNoIn
                return BitwiseORExpressionNoIn (e2, e1)
            } 
            return BitwiseORExpressionNoIn (SourceElement.Nil, e1)
        }

    and logicalANDExpression =
        parse {
            let! e1 = bitwiseORExpression
            return! parse {
                let! _ = expectPunctuator "&&"
                let! e2 = logicalANDExpression
                return LogicalANDExpression (e2, e1)
            } 
            return LogicalANDExpression (SourceElement.Nil, e1)
        }

    and logicalANDExpressionNoIn =
        parse {
            let! e1 = bitwiseORExpressionNoIn
            return! parse {
                let! _ = expectPunctuator "&&"
                let! e2 = logicalANDExpressionNoIn
                return LogicalANDExpressionNoIn (e2, e1)
            } 
            return LogicalANDExpressionNoIn (SourceElement.Nil, e1)
        }

    and logicalORExpression =
        parse {
            let! e1 = logicalANDExpression
            return! parse {
                let! _ = expectPunctuator "||"
                let! e2 = logicalORExpression
                return LogicalORExpression (e2, e1)
            } 
            return LogicalORExpression (SourceElement.Nil, e1)
        }
         
    and logicalORExpressionNoIn =
        parse {
            let! e1 = logicalANDExpressionNoIn
            return! parse {
                let! _ = expectPunctuator "||"
                let! e2 = logicalORExpressionNoIn
                return LogicalORExpressionNoIn (e2, e1)
            } 
            return LogicalORExpressionNoIn (SourceElement.Nil, e1)
        }

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

    and assignmentOperator = 
        expectAssignmentOperator |>> AssignmentOperator

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
            let! x = statementList <|> nil
            return FunctionBody x
        }
        
    and formalParameterList = 
        manySepFold expectIdentifier expectComma SourceElement.Nil (fun x y -> FormalParameterList (x, y))

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
        manySepFold variableDeclaration expectComma SourceElement.Nil (fun x y -> VariableDeclarationList (x, y))

    and variableDeclarationListNoIn =
        manySepFold variableDeclarationNoIn expectComma SourceElement.Nil (fun x y -> VariableDeclarationListNoIn (x, y))

    and variableDeclaration =
        tuple2 expectIdentifier (initializer <|> nil) VariableDeclaration

    and variableDeclarationNoIn =
        tuple2 expectIdentifier (initializerNoIn <|> nil) VariableDeclarationNoIn

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
        pipe5 (expectSpecificIdentifierName "catch") (expectPunctuator "(") expectIdentifier (expectPunctuator ")") blockParser (fun a b c d e -> Catch (d, e))

    and finallyParser =
        pipe2 (expectSpecificIdentifierName "finally") blockParser (fun x y -> Finally y)

    and debuggerStatement =
        pipe2 (expectSpecificIdentifierName "debugger") statementTerminator (fun x y -> DebuggerStatement)

    let sourceElement = 
        parse {
            let! v = choice [statement; functionDeclaration] |>> SourceElement
            return v
        }

    let sourceElements =  
        manyFold sourceElement SourceElement.Nil (fun x y -> SourceElements (x, y))

    let program = 
        choice [sourceElements; nil] |>> Program

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
            choice [       
                parse {
                    let! e = choice [primaryExpression; functionExpression]
                    return MemberExpression (SourceElement.Nil, e)
                }
                parse {
                    let! e1 = memberExpression
                    do! skip expectFullStop
                    let! e2 = between expectOpenBracket expectCloseBracket expression
                    return MemberExpression (e1, e2)
                }
                parse {
                    let! e1 = memberExpression
                    do! skip expectFullStop
                    let! e2 = expectIdentifierName |>> InputElement
                    return MemberExpression (e1, e2)
                }
                parse {
                    do! skip expectNew
                    let! e1 = memberExpression
                    let! e2 = arguments
                    return MemberExpression (e1, e2)
                }
            ]

        callExpressionRef :=
            choice [  
                parse {
                    let! e1 = callExpression
                    let! e2 = 
                        choice [
                            arguments;
                            (between expectOpenBracket expectCloseBracket expression);
                            parse {
                                do! skip expectFullStop
                                let! e3 = expression
                                return e3
                            }
                        ]
                    return CallExpression (e1, e2)
                }; parse {
                    let! e1 = memberExpression
                    let! e2 = arguments
                    return CallExpression (e1, e2)
                } 
            ]

        newExpressionRef :=
            choice [
                parse {
                    let! e = memberExpression
                    return NewExpression e
                }; parse {
                    do! skip expectNew
                    let! e = newExpression
                    return NewExpression e
                }
            ]

        assignmentExpressionRef :=
            choice [
                parse {
                    let! e = conditionalExpression
                    return AssignmentExpression (e, SourceElement.Nil, SourceElement.Nil)
                }
                parse {
                    let! a = leftHandSideExpression
                    let! b = assignmentOperator
                    let! c = assignmentExpression
                    return AssignmentExpression (a, b, c)                
                }
            ]

        assignmentExpressionNoInRef :=
            (conditionalExpressionNoIn |>> fun e -> AssignmentExpressionNoIn (e, SourceElement.Nil, SourceElement.Nil)) <|> parse {
                let! a = leftHandSideExpression
                let! b = assignmentOperator
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