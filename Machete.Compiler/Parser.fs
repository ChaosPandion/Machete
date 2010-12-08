namespace Machete.Compiler

module Parser =
    open System
    open Machete.Compiler.Lexer
    open Machete.Tools.BacktrackingPrimitives

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
                when IdentifierName.evalIdentifierName v = value ->
                        return v
            | _ -> ()
        } 

    let expectIdentifier : SimpleParser =
        parse {
            let! v = passLineTerminator ()
            match v with
            | IdentifierName (x, y)
                when not (CharSets.reservedWordSet.Contains (IdentifierName.evalIdentifierName v)) ->
                    return v
            | _ -> ()
        }  
            
    let expectPunctuator value : SimpleParser =
        parse {
            let! v = passLineTerminator ()
            match v with
            | Punctuator (Str str) 
                when str = value -> 
                    return v
            | _ -> ()
        }
        
    let expectComma : SimpleParser = expectPunctuator ","  

    let nil : ComplexParser = result SourceElement.Nil
    
    let statementTerminator =
        expectPunctuator ";" |>> fun a -> ()


    let expectOpenParenthesis = expectPunctuator "("
    let expectCloseParenthesis = expectPunctuator ")"        
    let expectOpenBrace = expectPunctuator "{"
    let expectCloseBrace = expectPunctuator "}"
    let expectSemiColon = expectPunctuator ";"
    let expectEqualsSign = expectPunctuator "="

    let expectDo = expectSpecificIdentifierName "do"
    let expectFor = expectSpecificIdentifierName "for"
    let expectVar = expectSpecificIdentifierName "var"
    let expectWhile = expectSpecificIdentifierName "while"
    let expectIn = expectSpecificIdentifierName "in"
    let expectIf = expectSpecificIdentifierName "if"
    let expectElse = expectSpecificIdentifierName "else"
    let expectFunction = expectSpecificIdentifierName "function"



    let binaryExpressionTail punctuators other = 
        parser {
            let! a = (matchPunctuator punctuators) +++ (matchKeyWord punctuators)
            let! b = other
            return [a; b]
        }

    let binaryExpression goalSymbol other tail = parser {
            let! a =  other
            let! b = many tail                
            let symbols = [a]
            let root = Node(goalSymbol, symbols)
            return fold goalSymbol root b
        }



    let rec primaryExpression = 
        parser {
            let! a = simplePrimaryExpression              
            return Node(NodeType.PrimaryExpression, [a])
        } +++ objectLiteral +++ arrayLiteral +++ parser {
            let! a = openParenthesis
            let! b = expression
            let! c = closeParenthesis
            return Node(NodeType.PrimaryExpression, [a;b;c] )
        } 
    and simplePrimaryExpression = 
        passLineTerminator (
            fun x ->
                match x with
                | Identifier token
                | Literal token -> true                        
                | KeyWord token when token.Value = "this"-> true
                | _ -> false
        ) 
    and arrayLiteral = 
        parser {
            let! a = openSquareBracket
            let! b = maybeOne elision 
            let! c = maybeOne elementList 
            let! d = maybeOne comma
            let! e = maybeOne elision 
            let! f = closeSquareBracket
            let symbols = [a] @ b @ c @ d @ e @ [f] 
            return Node(NodeType.ArrayLiteral, symbols)
        }
    and elementListTail = 
        parser {
            let! a = comma
            let! b = maybeOne elision 
            let! c = assignmentExpression
            return [a] @ b @ [c]
        }
    and elementList = 
        parser {
            let! a = maybeOne elision 
            let! b = assignmentExpression 
            let! c = many elementListTail
            let symbols = a @ [b]
            let nodeType = NodeType.ElementList
            let root = Node(nodeType, symbols)
            return fold nodeType root c 
        }
    and elision = 
        parser {
            let! a = comma
            let! b = many comma
            let root = Node(NodeType.Elision, [a])
            return singleFold NodeType.Elision root b 
        }
    and objectLiteral =
        parser {
            let! a = openCurlyBracket
            let! b = maybeOne propertyNameAndValueList 
            let! c = maybeOne comma 
            let! d = closeCurlyBracket
            let symbols = [a] @ b @ c @ [d]
            return Node(NodeType.ObjectLiteral, symbols)
        }
    and propertyNameAndValueList = 
        parser {
            let! a = propertyAssignment 
            let! b = many propertyNameAndValueListTail
            let symbols = [a] 
            let nodeType = NodeType.PropertyNameAndValueList
            let root = Node(nodeType, symbols)
            return fold nodeType root b
        }
    and propertyNameAndValueListTail = 
        parser {
            let! a = comma
            let! b = propertyAssignment
            return [a;b]
        }
    and propertyAssignment =
         standardPropertyAssignment +++ getPropertyAssignment +++ setPropertyAssignment
    and standardPropertyAssignment = 
        parser {
            let! a = propertyName
            let! b = colon 
            let! c = assignmentExpression
            return Node(NodeType.PropertyAssignment, [a;b;c])
        }
    and getPropertyAssignment =
        parser {
            let! a = getIdentifierName
            let! b = propertyName
            let! c = openParenthesis 
            let! d = closeParenthesis 
            let! e = openCurlyBracket 
            let! f = functionBody
            let! g = closeCurlyBracket 
            return Node(NodeType.PropertyAssignment, [a;b;c;d;e;f;g])
        }
    and setPropertyAssignment = 
        parser {
            let! a = setIdentifierName
            return! parser {
                let! b = propertyName
                let! c = openParenthesis 
                let! d = propertySetParameterList
                let! e = closeParenthesis 
                let! f = openCurlyBracket 
                let! g = functionBody
                let! h = closeCurlyBracket
                return Node(NodeType.PropertyAssignment, [a;b;c;d;e;f;g;h])
            } +++ parser {
                failwith "SHIT!"
            }
        }
    and propertyName =
        parser {
            let! a = propertyNameChild        
            return Node(NodeType.PropertyName, [a])   
        }
    and propertyNameChild = 
        passLineTerminator (
                fun x ->
                    match x.TokenType with
                    | TokenType.IdentifierName
                    | TokenType.StringLiteral
                    | TokenType.NumericLiteral -> true
                    | _ -> false
            )  
    and propertySetParameterList =
        parser {
            let! a = identifier         
            return Node(NodeType.PropertySetParameterList, [a])    
        }
    and memberExpressionTail =
        parser {
            let! a = openSquareBracket
            let! b = expression
            let! c = closeSquareBracket
            return [a; b; c]
        } +++ parser {
            let! a = fullStop
            let! b = identifierName
            return [a; b]
        }
    and memberExpression =
        parser {
            let! a = primaryExpression +++ functionExpression
            let! b = many memberExpressionTail
            let symbols = [a]  
            let root = Node(NodeType.MemberExpression, symbols)
            return fold NodeType.MemberExpression root b
        } +++ parser {
            let! a = newKeyWord
            let! b = memberExpression
            let! c = arguments
            let symbols = [a; b; c]
            return Node(NodeType.MemberExpression, symbols)
        }
    and newExpression = 
        parser {
            let! a = memberExpression
            let symbols = [a]
            return Node(NodeType.NewExpression, symbols)
        } +++ parser {
            let! a = newKeyWord
            let! b = newExpression
            let symbols = [a; b]
            return Node(NodeType.NewExpression, symbols)
        }
    and callExpressionTail =
        parser {
            let! a = arguments
            return [a]
        } +++ parser {
            let! a = openSquareBracket
            let! b = expression
            let! c = closeSquareBracket
            return [a; b; c]
        } +++ parser {
            let! a = fullStop
            let! b = identifierName
            return [a; b]
        }            
    and callExpression = 
        parser {
            let! a = memberExpression
            let! b = arguments
            let symbols = [a; b]
            let! r = many callExpressionTail
            let root = Node(NodeType.CallExpression, symbols)
            return fold NodeType.CallExpression root r
        }
    and arguments = parser {
            let! a = openParenthesis
            let! b = maybeOne argumentList
            let! c = maybeOne closeParenthesis
            if c.IsEmpty then
                return! fun input state ->
                    let baseMsg = "Missing closing parenthesis for Arguments."
                    match input with
                    | LazyList.Cons(x, xs) -> 
                        let msg = sprintf "%s\nValue:%s\nLine:%i\nColumn:%i" baseMsg x.Value x.Line x.Column
                        let token = Token(TokenType.TokenError, msg, x.Line, x.Column)
                        let child = Node(NodeType.NodeError, [a] @ b @ [Node(NodeType.TokenNode, [], token)])
                        [Result(Node(NodeType.Arguments, [child]), msg::state, xs)] 
                    | _ -> 
                        let t = a.Token.Value
                        let msg = sprintf "%s\nLine:%i\nColumn:%i" baseMsg t.Line t.Column
                        let token = Token(TokenType.TokenError, msg, t.Line, t.Column)
                        let child = Node(NodeType.NodeError, [a] @ b, token)
                        [Result(Node(NodeType.Arguments, [child]), msg::state, input)]
            else
                let symbols = [a] @ b @ c
                return Node(NodeType.Arguments, symbols)
        }
    and argumentListTail = parser {
            let! a = comma
            let! b = maybeOne assignmentExpression
            if b.IsEmpty then
                return! fun input state ->
                    let baseMsg = "Missing AssignmentExpression in ArgumentList."
                    match input with
                    | LazyList.Cons(x, xs) when x.Value <> ")" && x.Value <> "," -> 
                        let msg = sprintf "%s\nValue:%s\nLine:%i\nColumn:%i" baseMsg x.Value x.Line x.Column
                        [Result([Node(NodeType.NodeError, [a; Node(NodeType.TokenNode, [], x)])], msg::state, xs)] 
                    | _ -> 
                        let t = a.Token.Value
                        let msg = sprintf "%s\nLine:%i\nColumn:%i" baseMsg t.Line t.Column
                        [Result([Node(NodeType.NodeError, [a])], msg::state, input)]
            else
                return a::b
        }
    and argumentList = parser {
            let! head = assignmentExpression
            let! tail = many argumentListTail
            let symbols = [head]
            let root = Node(NodeType.ArgumentList, symbols)
            return fold NodeType.ArgumentList root tail
        }
    and leftHandSideExpression = parser { 
            let! r = callExpression +++ newExpression
            let symbols = [r]
            return Node(NodeType.LeftHandSideExpression, symbols)
        }
    and postfixExpression = 
        parser {
            let! r = leftHandSideExpression
            let! b = maybeOne postFixOperatorPunctuator             
            let symbols = [r] @ b
            return Node(NodeType.PostfixExpression, symbols)
        }    
    and unaryExpression = 
        parser {
            let! a = postfixExpression
            let symbols = [a]
            return Node(NodeType.UnaryExpression, symbols)
        } +++ parser {
            let! a = unaryOperatorPunctuators +++ unaryOperatorKeyWords
            let! b = unaryExpression               
            let symbols = [a;b]
            return Node(NodeType.UnaryExpression, symbols)
        }

    and multiplicativeExpression =   
        binaryExpression NodeType.MultiplicativeExpression unaryExpression (binaryExpressionTail ["*"; "/"; "%"] unaryExpression)
    
    and additiveExpression =   
        binaryExpression NodeType.AdditiveExpression multiplicativeExpression (binaryExpressionTail ["+"; "-"] multiplicativeExpression)
    
    and shiftExpression =   
        binaryExpression NodeType.ShiftExpression additiveExpression (binaryExpressionTail ["<<"; ">>"; ">>>"] additiveExpression)
    
    and relationalExpression = 
        binaryExpression NodeType.RelationalExpression shiftExpression (binaryExpressionTail ["<"; ">"; "<="; ">="; "instanceof"; "in"] shiftExpression)
    
    and relationalExpressionNoIn =  
        binaryExpression NodeType.RelationalExpressionNoIn shiftExpression (binaryExpressionTail ["<"; ">"; "<="; ">="; "instanceof"] shiftExpression)
    
    and equalityExpression = 
        binaryExpression NodeType.EqualityExpression relationalExpression (binaryExpressionTail ["=="; "!="; "==="; "!=="] relationalExpression)
    and equalityExpressionNoIn = 
        binaryExpression NodeType.EqualityExpressionNoIn relationalExpressionNoIn (binaryExpressionTail ["=="; "!="; "==="; "!=="] relationalExpressionNoIn)
    
    and bitwiseANDExpression = 
        manySeparatedFold equalityExpression (expectPunctuator "&") SourceElement.Nil (fun x y -> BitwiseANDExpression (x, y))
    and bitwiseANDExpressionNoIn = 
        manySeparatedFold equalityExpressionNoIn (expectPunctuator "&") SourceElement.Nil (fun x y -> BitwiseANDExpressionNoIn (x, y))
    and bitwiseXORExpression = 
        manySeparatedFold bitwiseANDExpression (expectPunctuator "^") SourceElement.Nil (fun x y -> BitwiseXORExpression (x, y))
    and bitwiseXORExpressionNoIn = 
        manySeparatedFold bitwiseANDExpressionNoIn (expectPunctuator "^") SourceElement.Nil (fun x y -> BitwiseXORExpressionNoIn (x, y))
    and bitwiseORExpression = 
        manySeparatedFold bitwiseXORExpression (expectPunctuator "|") SourceElement.Nil (fun x y -> BitwiseORExpression (x, y))
    and bitwiseORExpressionNoIn = 
        manySeparatedFold bitwiseXORExpressionNoIn (expectPunctuator "|") SourceElement.Nil (fun x y -> BitwiseORExpressionNoIn (x, y))

    and logicalANDExpression = 
        manySeparatedFold bitwiseORExpression (expectPunctuator "&&") SourceElement.Nil (fun x y -> LogicalANDExpression (x, y))
    and logicalANDExpressionNoIn = 
        manySeparatedFold bitwiseORExpressionNoIn (expectPunctuator "&&") SourceElement.Nil (fun x y -> LogicalANDExpressionNoIn (x, y)) 
    and logicalORExpression =
        manySeparatedFold logicalANDExpression (expectPunctuator "||") SourceElement.Nil (fun x y -> LogicalORExpression (x, y)) 
    and logicalORExpressionNoIn =
        manySeparatedFold logicalANDExpressionNoIn (expectPunctuator "||") SourceElement.Nil (fun x y -> LogicalORExpressionNoIn (x, y))

    and conditionalExpressionTail = 
        parser {
            let! a = questionMark
            let! b = assignmentExpression
            let! c = colon
            let! d = assignmentExpression
            return [a; b; c; d]
        }
    and conditionalExpression = 
        parser {
            let! a = logicalORExpression
            let! b = many conditionalExpressionTail
            return if b.IsEmpty then Node(NodeType.ConditionalExpression, [a]) else fold NodeType.ConditionalExpression a b              
        }
    and conditionalExpressionNoInTail = 
        parser {
            let! a = questionMark
            let! b = assignmentExpression
            let! c = colon
            let! d = assignmentExpressionNoIn
            return [a; b; c; d]
        }
    and conditionalExpressionNoIn = 
        parser {
            let! a = logicalORExpressionNoIn
            let! b = many conditionalExpressionNoInTail
            return if b.IsEmpty then Node(NodeType.ConditionalExpressionNoIn, [a]) else fold NodeType.ConditionalExpressionNoIn a b                 
        } 
    and assignmentExpression =
        parser {
            let! a = leftHandSideExpression
            let! z = maybeOne questionMark
            if z.IsEmpty then
                let! b = assignmentOperator
                let! c = assignmentExpression
                let symbols = [a; b; c]
                return Node(NodeType.AssignmentExpression, symbols)
        } +++ parser {
            let! a = conditionalExpression
            let symbols = [a]
            return Node(NodeType.AssignmentExpression, symbols)
        }
    and assignmentExpressionNoIn =
        parse {
            let! a = leftHandSideExpression
            let! b = assignmentOperator
            let! c = assignmentExpressionNoIn
            return AssignmentExpressionNoIn(a, b, c)
        } <|> parse {
            let! a = conditionalExpressionNoIn
            return AssignmentExpressionNoIn a
        }
    and assignmentOperator = parse {
            let! a = assignmentOperatorPunctuator
            let symbols = [a]
            return Node(NodeType.AssignmentOperator, symbols)
        }
    and expression =
        binaryExpression Expression assignmentExpression (binaryExpressionTail [","] assignmentExpression)
    and expressionNoIn = 
        binaryExpression ExpressionNoIn assignmentExpressionNoIn (binaryExpressionTail [","] assignmentExpressionNoIn)

    and functionExpression = 
        parse {
            do! skip expectFunction
            let! i = expectIdentifier <|> result Nil
            do! skip expectOpenParenthesis
            let! l = formalParameterList <|> nil
            do! skip expectCloseParenthesis
            do! skip expectOpenBrace
            let! b = functionBody
            do! skip expectCloseBrace             
            return FunctionExpression (i, l, b)
        }  
              
    and functionBody = 
        (statementList <|> nil) |>> FunctionBody
        
    and formalParameterList = 
        manySeparatedFold expectIdentifier expectComma SourceElement.Nil (fun x y -> FormalParameterList (x, y))

    and functionDeclaration = 
        parse {
            do! skip expectFunction
            let! i = expectIdentifier
            do! skip expectOpenParenthesis
            let! l = formalParameterList <|> nil
            do! skip expectCloseParenthesis
            do! skip expectOpenBrace
            let! b = functionBody
            do! skip expectCloseBrace             
            return FunctionDeclaration (i, l, b)
        }  

    and statement =
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

    and statementList =  
        manyFold statement SourceElement.Nil (fun x y -> StatementList (x, y))
               
    and blockParser =
        between expectOpenBrace expectCloseBrace (statementList <|> nil) |>> Block

    and variableStatement =
        pipe3 expectVar variableDeclarationList statementTerminator (fun _ v _ -> VariableStatement v)

    and variableDeclarationList =
        manySeparatedFold variableDeclaration expectComma SourceElement.Nil (fun x y -> VariableDeclarationList (x, y))

    and variableDeclarationListNoIn =
        manySeparatedFold variableDeclarationNoIn expectComma SourceElement.Nil (fun x y -> VariableDeclarationListNoIn (x, y))

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
            do! skip expectOpenParenthesis
            let! e = expression 
            do! skip expectCloseParenthesis
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
            do! skip expectDo
            let! s = statement 
            do! skip expectWhile
            do! skip expectOpenParenthesis
            let! e = expression 
            do! skip expectCloseParenthesis
            do! skip statementTerminator
            return IterationStatement (s, e, SourceElement.Nil, SourceElement.Nil)
        } <|> parse {
            do! skip expectWhile
            do! skip expectOpenParenthesis
            let! e = expression
            do! skip expectCloseParenthesis 
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
        (statement <|> functionDeclaration) |>> SourceElement 

    let sourceElements = 
        manyFold sourceElement SourceElement.Nil (fun x y -> SourceElements(x, y))

    let program = 
        (sourceElements <|> nil) |>> Program