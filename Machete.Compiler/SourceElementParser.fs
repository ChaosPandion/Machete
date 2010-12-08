namespace Machete.Compiler

module Program =
    open FParsec.CharParsers
    open FParsec.Primitives
    open Tools1
    let main () =
        let r = Parser.parse "2 + 2;3+3;"
        printfn "%s" (r.ToString())
//        let cat = parse {
//            let! a = satisfy (fun c -> c = 'c')
//            let! b = satisfy (fun c -> c = 'a')
//            let! c = satisfy (fun c -> c = 't')
//            return a.ToString() + b.ToString() + c.ToString()
//        }
//        let catty = parse {
//            let! a = satisfy (fun c -> c = 'c')
//            let! b = satisfy (fun c -> c = 'a')
//            let! c = satisfy (fun c -> c = 't')
//            let! d = satisfy (fun c -> c = 't')
//            let! e = satisfy (fun c -> c = 'y')
//            return a.ToString() + b.ToString() + c.ToString() + d.ToString() + e.ToString()
//        }
//        let parser = parse {
//            let! a = catty <|> cat
//            let! b = satisfy (fun c -> c = 't')
//            let! c = satisfy (fun c -> c = 'y')
//            return a + b.ToString() + c.ToString()
//        }
//        let r = run (many parser) (LazyList.ofSeq "catty") () 
//        
//        for x in r do
//            match x with
//            | Success (v, s) ->
//                printfn "%s" (v.ToString())
//            | Failure (ms, s) ->
//                failwith (System.String.Join ("\r\n", ms))
        ()
    main()
//
//module SourceElementParser =
//
//    open System
//    open Machete.Tools.BacktrackingPrimitives
//
//    let binaryExpressionTail punctuators other = 
//        parser {
//            let! a = (matchPunctuator punctuators) +++ (matchKeyWord punctuators)
//            let! b = other
//            return [a; b]
//        }
//
//    let binaryExpression goalSymbol other tail = parser {
//            let! a =  other
//            let! b = many tail                
//            let symbols = [a]
//            let root = Node(goalSymbol, symbols)
//            return fold goalSymbol root b
//        }
//
//
//
//    let rec primaryExpression = 
//        parser {
//            let! a = simplePrimaryExpression              
//            return Node(NodeType.PrimaryExpression, [a])
//        } +++ objectLiteral +++ arrayLiteral +++ parser {
//            let! a = openParenthesis
//            let! b = expression
//            let! c = closeParenthesis
//            return Node(NodeType.PrimaryExpression, [a;b;c] )
//        } 
//    and simplePrimaryExpression = 
//        passLineTerminator (
//            fun x ->
//                match x with
//                | Identifier token
//                | Literal token -> true                        
//                | KeyWord token when token.Value = "this"-> true
//                | _ -> false
//        ) 
//    and arrayLiteral = 
//        parser {
//            let! a = openSquareBracket
//            let! b = maybeOne elision 
//            let! c = maybeOne elementList 
//            let! d = maybeOne comma
//            let! e = maybeOne elision 
//            let! f = closeSquareBracket
//            let symbols = [a] @ b @ c @ d @ e @ [f] 
//            return Node(NodeType.ArrayLiteral, symbols)
//        }
//    and elementListTail = 
//        parser {
//            let! a = comma
//            let! b = maybeOne elision 
//            let! c = assignmentExpression
//            return [a] @ b @ [c]
//        }
//    and elementList = 
//        parser {
//            let! a = maybeOne elision 
//            let! b = assignmentExpression 
//            let! c = many elementListTail
//            let symbols = a @ [b]
//            let nodeType = NodeType.ElementList
//            let root = Node(nodeType, symbols)
//            return fold nodeType root c 
//        }
//    and elision = 
//        parser {
//            let! a = comma
//            let! b = many comma
//            let root = Node(NodeType.Elision, [a])
//            return singleFold NodeType.Elision root b 
//        }
//    and objectLiteral =
//        parser {
//            let! a = openCurlyBracket
//            let! b = maybeOne propertyNameAndValueList 
//            let! c = maybeOne comma 
//            let! d = closeCurlyBracket
//            let symbols = [a] @ b @ c @ [d]
//            return Node(NodeType.ObjectLiteral, symbols)
//        }
//    and propertyNameAndValueList = 
//        parser {
//            let! a = propertyAssignment 
//            let! b = many propertyNameAndValueListTail
//            let symbols = [a] 
//            let nodeType = NodeType.PropertyNameAndValueList
//            let root = Node(nodeType, symbols)
//            return fold nodeType root b
//        }
//    and propertyNameAndValueListTail = 
//        parser {
//            let! a = comma
//            let! b = propertyAssignment
//            return [a;b]
//        }
//    and propertyAssignment =
//         standardPropertyAssignment +++ getPropertyAssignment +++ setPropertyAssignment
//    and standardPropertyAssignment = 
//        parser {
//            let! a = propertyName
//            let! b = colon 
//            let! c = assignmentExpression
//            return Node(NodeType.PropertyAssignment, [a;b;c])
//        }
//    and getPropertyAssignment =
//        parser {
//            let! a = getIdentifierName
//            let! b = propertyName
//            let! c = openParenthesis 
//            let! d = closeParenthesis 
//            let! e = openCurlyBracket 
//            let! f = functionBody
//            let! g = closeCurlyBracket 
//            return Node(NodeType.PropertyAssignment, [a;b;c;d;e;f;g])
//        }
//    and setPropertyAssignment = 
//        parser {
//            let! a = setIdentifierName
//            return! parser {
//                let! b = propertyName
//                let! c = openParenthesis 
//                let! d = propertySetParameterList
//                let! e = closeParenthesis 
//                let! f = openCurlyBracket 
//                let! g = functionBody
//                let! h = closeCurlyBracket
//                return Node(NodeType.PropertyAssignment, [a;b;c;d;e;f;g;h])
//            } +++ parser {
//                failwith "SHIT!"
//            }
//        }
//    and propertyName =
//        parser {
//            let! a = propertyNameChild        
//            return Node(NodeType.PropertyName, [a])   
//        }
//    and propertyNameChild = 
//        passLineTerminator (
//                fun x ->
//                    match x.TokenType with
//                    | TokenType.IdentifierName
//                    | TokenType.StringLiteral
//                    | TokenType.NumericLiteral -> true
//                    | _ -> false
//            )  
//    and propertySetParameterList =
//        parser {
//            let! a = identifier         
//            return Node(NodeType.PropertySetParameterList, [a])    
//        }
//    and memberExpressionTail =
//        parser {
//            let! a = openSquareBracket
//            let! b = expression
//            let! c = closeSquareBracket
//            return [a; b; c]
//        } +++ parser {
//            let! a = fullStop
//            let! b = identifierName
//            return [a; b]
//        }
//    and memberExpression =
//        parser {
//            let! a = primaryExpression +++ functionExpression
//            let! b = many memberExpressionTail
//            let symbols = [a]  
//            let root = Node(NodeType.MemberExpression, symbols)
//            return fold NodeType.MemberExpression root b
//        } +++ parser {
//            let! a = newKeyWord
//            let! b = memberExpression
//            let! c = arguments
//            let symbols = [a; b; c]
//            return Node(NodeType.MemberExpression, symbols)
//        }
//    and newExpression = 
//        parser {
//            let! a = memberExpression
//            let symbols = [a]
//            return Node(NodeType.NewExpression, symbols)
//        } +++ parser {
//            let! a = newKeyWord
//            let! b = newExpression
//            let symbols = [a; b]
//            return Node(NodeType.NewExpression, symbols)
//        }
//    and callExpressionTail =
//        parser {
//            let! a = arguments
//            return [a]
//        } +++ parser {
//            let! a = openSquareBracket
//            let! b = expression
//            let! c = closeSquareBracket
//            return [a; b; c]
//        } +++ parser {
//            let! a = fullStop
//            let! b = identifierName
//            return [a; b]
//        }            
//    and callExpression = 
//        parser {
//            let! a = memberExpression
//            let! b = arguments
//            let symbols = [a; b]
//            let! r = many callExpressionTail
//            let root = Node(NodeType.CallExpression, symbols)
//            return fold NodeType.CallExpression root r
//        }
//    and arguments = parser {
//            let! a = openParenthesis
//            let! b = maybeOne argumentList
//            let! c = maybeOne closeParenthesis
//            if c.IsEmpty then
//                return! fun input state ->
//                    let baseMsg = "Missing closing parenthesis for Arguments."
//                    match input with
//                    | LazyList.Cons(x, xs) -> 
//                        let msg = sprintf "%s\nValue:%s\nLine:%i\nColumn:%i" baseMsg x.Value x.Line x.Column
//                        let token = Token(TokenType.TokenError, msg, x.Line, x.Column)
//                        let child = Node(NodeType.NodeError, [a] @ b @ [Node(NodeType.TokenNode, [], token)])
//                        [Result(Node(NodeType.Arguments, [child]), msg::state, xs)] 
//                    | _ -> 
//                        let t = a.Token.Value
//                        let msg = sprintf "%s\nLine:%i\nColumn:%i" baseMsg t.Line t.Column
//                        let token = Token(TokenType.TokenError, msg, t.Line, t.Column)
//                        let child = Node(NodeType.NodeError, [a] @ b, token)
//                        [Result(Node(NodeType.Arguments, [child]), msg::state, input)]
//            else
//                let symbols = [a] @ b @ c
//                return Node(NodeType.Arguments, symbols)
//        }
//    and argumentListTail = parser {
//            let! a = comma
//            let! b = maybeOne assignmentExpression
//            if b.IsEmpty then
//                return! fun input state ->
//                    let baseMsg = "Missing AssignmentExpression in ArgumentList."
//                    match input with
//                    | LazyList.Cons(x, xs) when x.Value <> ")" && x.Value <> "," -> 
//                        let msg = sprintf "%s\nValue:%s\nLine:%i\nColumn:%i" baseMsg x.Value x.Line x.Column
//                        [Result([Node(NodeType.NodeError, [a; Node(NodeType.TokenNode, [], x)])], msg::state, xs)] 
//                    | _ -> 
//                        let t = a.Token.Value
//                        let msg = sprintf "%s\nLine:%i\nColumn:%i" baseMsg t.Line t.Column
//                        [Result([Node(NodeType.NodeError, [a])], msg::state, input)]
//            else
//                return a::b
//        }
//    and argumentList = parser {
//            let! head = assignmentExpression
//            let! tail = many argumentListTail
//            let symbols = [head]
//            let root = Node(NodeType.ArgumentList, symbols)
//            return fold NodeType.ArgumentList root tail
//        }
//    and leftHandSideExpression = parser { 
//            let! r = callExpression +++ newExpression
//            let symbols = [r]
//            return Node(NodeType.LeftHandSideExpression, symbols)
//        }
//    and postfixExpression = 
//        parser {
//            let! r = leftHandSideExpression
//            let! b = maybeOne postFixOperatorPunctuator             
//            let symbols = [r] @ b
//            return Node(NodeType.PostfixExpression, symbols)
//        }    
//    and unaryExpression = 
//        parser {
//            let! a = postfixExpression
//            let symbols = [a]
//            return Node(NodeType.UnaryExpression, symbols)
//        } +++ parser {
//            let! a = unaryOperatorPunctuators +++ unaryOperatorKeyWords
//            let! b = unaryExpression               
//            let symbols = [a;b]
//            return Node(NodeType.UnaryExpression, symbols)
//        }
//    and multiplicativeExpression =   
//        binaryExpression NodeType.MultiplicativeExpression unaryExpression (binaryExpressionTail ["*"; "/"; "%"] unaryExpression)
//    and additiveExpression =   
//        binaryExpression NodeType.AdditiveExpression multiplicativeExpression (binaryExpressionTail ["+"; "-"] multiplicativeExpression)
//    and shiftExpression =   
//        binaryExpression NodeType.ShiftExpression additiveExpression (binaryExpressionTail ["<<"; ">>"; ">>>"] additiveExpression)
//    and relationalExpression = 
//        binaryExpression NodeType.RelationalExpression shiftExpression (binaryExpressionTail ["<"; ">"; "<="; ">="; "instanceof"; "in"] shiftExpression)
//    and relationalExpressionNoIn =  
//        binaryExpression NodeType.RelationalExpressionNoIn shiftExpression (binaryExpressionTail ["<"; ">"; "<="; ">="; "instanceof"] shiftExpression)
//    and equalityExpression = 
//        binaryExpression NodeType.EqualityExpression relationalExpression (binaryExpressionTail ["=="; "!="; "==="; "!=="] relationalExpression)
//    and equalityExpressionNoIn = 
//        binaryExpression NodeType.EqualityExpressionNoIn relationalExpressionNoIn (binaryExpressionTail ["=="; "!="; "==="; "!=="] relationalExpressionNoIn)
//    and bitwiseANDExpression = 
//        binaryExpression NodeType.BitwiseANDExpression equalityExpression (binaryExpressionTail ["&"] equalityExpression)
//    and bitwiseANDExpressionNoIn = 
//        binaryExpression NodeType.BitwiseANDExpressionNoIn equalityExpressionNoIn (binaryExpressionTail ["&"] equalityExpressionNoIn)
//    and bitwiseXORExpression = 
//        binaryExpression NodeType.BitwiseXORExpression bitwiseANDExpression (binaryExpressionTail ["^"] bitwiseANDExpression)
//    and bitwiseXORExpressionNoIn = 
//        binaryExpression NodeType.BitwiseANDExpressionNoIn bitwiseANDExpressionNoIn (binaryExpressionTail ["^"] bitwiseANDExpressionNoIn)
//    and bitwiseORExpression = 
//        binaryExpression NodeType.BitwiseORExpression bitwiseXORExpression (binaryExpressionTail ["|"] bitwiseXORExpression)
//    and bitwiseORExpressionNoIn = 
//        binaryExpression NodeType.BitwiseORExpressionNoIn bitwiseXORExpressionNoIn (binaryExpressionTail ["|"] bitwiseXORExpressionNoIn)
//    and logicalANDExpression = 
//        binaryExpression NodeType.LogicalANDExpression bitwiseORExpression (binaryExpressionTail ["&&"] bitwiseORExpression)
//    and logicalANDExpressionNoIn = 
//        binaryExpression NodeType.LogicalANDExpressionNoIn bitwiseORExpressionNoIn (binaryExpressionTail ["&&"] bitwiseORExpressionNoIn)
//    and logicalORExpression = 
//        binaryExpression NodeType.LogicalORExpression logicalANDExpression (binaryExpressionTail ["||"] logicalANDExpression)
//    and logicalORExpressionNoIn = 
//        binaryExpression NodeType.LogicalORExpressionNoIn logicalANDExpressionNoIn (binaryExpressionTail ["||"] logicalANDExpressionNoIn)
//    and conditionalExpressionTail = 
//        parser {
//            let! a = questionMark
//            let! b = assignmentExpression
//            let! c = colon
//            let! d = assignmentExpression
//            return [a; b; c; d]
//        }
//    and conditionalExpression = 
//        parser {
//            let! a = logicalORExpression
//            let! b = many conditionalExpressionTail
//            return if b.IsEmpty then Node(NodeType.ConditionalExpression, [a]) else fold NodeType.ConditionalExpression a b              
//        }
//    and conditionalExpressionNoInTail = 
//        parser {
//            let! a = questionMark
//            let! b = assignmentExpression
//            let! c = colon
//            let! d = assignmentExpressionNoIn
//            return [a; b; c; d]
//        }
//    and conditionalExpressionNoIn = 
//        parser {
//            let! a = logicalORExpressionNoIn
//            let! b = many conditionalExpressionNoInTail
//            return if b.IsEmpty then Node(NodeType.ConditionalExpressionNoIn, [a]) else fold NodeType.ConditionalExpressionNoIn a b                 
//        } 
//    and assignmentExpression =
//        parser {
//            let! a = leftHandSideExpression
//            let! z = maybeOne questionMark
//            if z.IsEmpty then
//                let! b = assignmentOperator
//                let! c = assignmentExpression
//                let symbols = [a; b; c]
//                return Node(NodeType.AssignmentExpression, symbols)
//        } +++ parser {
//            let! a = conditionalExpression
//            let symbols = [a]
//            return Node(NodeType.AssignmentExpression, symbols)
//        }
//    and assignmentExpressionNoIn =
//        parser {
//            let! a = leftHandSideExpression
//            let! b = assignmentOperator
//            let! c = assignmentExpressionNoIn
//            return AssignmentExpressionNoIn(a, b, c)
//        } +++ parser {
//            let! a = conditionalExpressionNoIn
//            let symbols = [a]
//            return Node(NodeType.AssignmentExpressionNoIn, symbols)
//        }
//    and assignmentOperator = parser {
//            let! a = assignmentOperatorPunctuator
//            let symbols = [a]
//            return Node(NodeType.AssignmentOperator, symbols)
//        }
//    and expression =
//        //binaryExpression Expression assignmentExpression (binaryExpressionTail [","] assignmentExpression)
//    and expressionNoIn = 
//        binaryExpression ExpressionNoIn assignmentExpressionNoIn (binaryExpressionTail [","] assignmentExpressionNoIn)
//               
//    
//
//    
//
//    
//    
//    
//    
//
//    
//
//    and functionExpression = parser {
//            let! _ = functionKeyWord
//            let! a = maybe identifierPassLineTerminator
//            let! _ = openParenthesis
//            let! b = maybe formalParameterList
//            let! _ = closeParenthesis
//            let! _ = openCurlyBracket
//            let! c = functionBody
//            let! _ = closeCurlyBracket    
//            return FunctionExpression(a, b, c)
//        }
//    and identifier input state =
//        match input with
//        | LazyList.Cons (a, b) ->
//            match a with
//            | Identifier token ->
//                [Result(Token(token), state, b)]
//            | _ -> []
//        | LazyList.Nil -> []  
//    and identifierPassLineTerminator input state =
//        match input with
//        | LazyList.Cons (token:Token, b) ->
//            match token.TokenType with
//            | TokenType.LineTerminator -> identifierPassLineTerminator b state 
//            | _ ->
//                match token with
//                | Identifier token ->
//                    [Result(Token(token), state, b)]
//                | _ -> []
//        | LazyList.Nil -> []       
//    and functionBody = 
//        parser {
//            let! a = maybe statementList      
//            return FunctionBody(a)
//        }
//    and formalParameterListTail = 
//        parser {
//            let! _ = comma
//            let! a = identifierPassLineTerminator
//            return [a]
//        }
//    and formalParameterList = 
//        parser {
//            let! a = identifierPassLineTerminator
//            let! b = many formalParameterListTail
//            let root = FormalParameterList(Nil, a)
//            return fold FormalParameterList root b
//        }
//
//    and functionDeclaration = 
//        parser {
//            let! _ = functionKeyWord
//            let! a = identifierPassLineTerminator
//            let! _ = openParenthesis
//            let! b = maybe formalParameterList
//            let! _ = closeParenthesis
//            let! _ = openCurlyBracket
//            let! c = functionBody
//            let! _ = closeCurlyBracket               
//            return FunctionDeclaration(a, b, c)
//        }  
//    and statement =
//        blockParser +++ 
//        variableStatement +++ 
//        emptyStatement +++ 
//        expressionStatement +++ 
//        ifStatement +++ 
//        iterationStatement +++
//        continueStatement +++
//        breakStatement +++
//        returnStatement +++
//        withStatement +++
//        switchStatement +++
//        labelledStatement +++
//        throwStatement +++
//        tryStatement +++
//        debuggerStatement 
//
//    and statementList = 
//        parser {
//            let! a = statement 
//            let! b = many statement
//            let root = StatementList(Nil, a)
//            return singleFold StatementList root b
//        }        
//    and blockParser =
//        parser {
//            let! _ = openCurlyBracket
//            let! a = maybe statementList
//            let! _ = closeCurlyBracket
//            return Block(a)
//        }
//    and variableStatement =
//        parser {
//            let! _ = varKeyWord
//            let! a = variableDeclarationList
//            let! _ = statementTerminator
//            return VariableStatement(a)
//
//        }
//    and variableDeclarationList =
//        parser {
//            let! a = variableDeclaration 
//            let! b = many variableDeclarationListTail
//            let root = VariableDeclarationList(Nil, a)
//            return fold VariableDeclarationList root b
//        }
//    and variableDeclarationListTail =
//        parser {
//            let! _ = comma
//            let! a = variableDeclaration
//            return [a]
//        }
//    and variableDeclarationListNoIn =
//        parser {
//            let! a = variableDeclarationNoIn 
//            let! b = many variableDeclarationListNoInTail
//            let root = VariableDeclarationListNoIn(Nil, a)
//            return fold VariableDeclarationListNoIn root b
//
//        }
//    and variableDeclarationListNoInTail =
//        parser {
//            let! _ = comma
//            let! a = variableDeclarationNoIn
//            return [a]
//        }
//    and variableDeclaration =
//        parser {
//            let! a = identifierPassLineTerminator
//            let! b = maybe initializer
//            return VariableDeclaration(a, b)
//        }
//    and variableDeclarationNoIn =
//        parser {
//            let! a = identifierPassLineTerminator
//            let! b = maybe initializerNoIn    
//            return VariableDeclarationNoIn(a, b)
//        }
//    and initializer =
//        parser {
//            let! _ = equalsSign
//            let! a = assignmentExpression
//            return Initialiser(a)
//        }
//    and initializerNoIn =
//        parser {
//            let! _ = equalsSign
//            let! a = assignmentExpressionNoIn
//            return InitialiserNoIn(a)
//        }
//    and emptyStatement =
//        parser {
//            let! _ = semiColon
//            return EmptyStatement
//        }
//    and expressionStatement =
//        parser {
//            let! a = maybeOne openCurlyBracket
//            if a.IsEmpty then
//                let! b = maybeOne functionKeyWord
//                if b.IsEmpty then
//                    let! c = expression
//                    let! _ = statementTerminator
//                    return ExpressionStatement(c)         
//        }
//    and ifStatement =
//        parser {
//            let! _ = ifKeyWord
//            let! _ = openParenthesis
//            let! a = expression
//            let! _ = closeParenthesis
//            let! b = statement
//            let! c = maybeOne elseKeyWord
//            if c.IsEmpty then
//                return IfStatement(a, b, Nil)
//            else
//                let! d = statement
//                return IfStatement(a, b, d)
//        }
//    and iterationStatement = 
//        parser {
//            let! _ = doKeyWord
//            let! a = statement
//            let! _ = whileKeyWord
//            let! _ = openParenthesis
//            let! b = expression
//            let! _ = closeParenthesis
//            let! _ = statementTerminator
//            return IterationStatement(a, b, Nil, Nil)
//        } +++ parser {
//            let! _ = whileKeyWord
//            let! _ = openParenthesis
//            let! a = expression
//            let! _ = closeParenthesis
//            let! b = statement
//            return IterationStatement(a, b, Nil, Nil)
//        } +++ parser {
//            let! _ = forKeyWord
//            let! _ = openParenthesis
//            let! a = maybe expressionNoIn
//            let! _ = semiColon
//            let! b = maybe expression
//            let! _ = semiColon
//            let! c = maybe expression
//            let! _ = closeParenthesis
//            let! d = statement
//            return IterationStatement(a, b, c, d)
//        } +++ parser {
//            let! _ = forKeyWord
//            let! _ = openParenthesis
//            let! _ = varKeyWord
//            let! a = variableDeclarationListNoIn
//            let! _ = semiColon
//            let! b = maybe expression
//            let! _ = semiColon
//            let! c = maybe expression
//            let! _ = closeParenthesis
//            let! d = statement
//            return IterationStatement(a, b, c, d)
//        } +++ parser {
//            let! _ = forKeyWord
//            let! _ = openParenthesis
//            let! a = leftHandSideExpression
//            let! _ = inKeyWord
//            let! b = expression
//            let! _ = closeParenthesis
//            let! c = statement
//            return IterationStatement(a, b, c, Nil)
//        }  +++ parser {
//            let! _ = forKeyWord
//            let! _ = openParenthesis
//            let! _ = varKeyWord
//            let! a = variableDeclarationNoIn
//            let! _ = inKeyWord
//            let! b = expression
//            let! _ = closeParenthesis
//            let! c = statement
//            return IterationStatement(a, b, c, Nil)
//        }
//    and continueStatement =
//        parser {
//            let! _ = continueKeyWord
//            let! a = maybeOne identifier
//            let! _ = statementTerminator
//            return ContinueStatement(a)
//        }
//    and breakStatement =
//        parser {
//            let! _ = breakKeyWord
//            let! a = maybe identifier
//            let! _ = statementTerminator
//            return BreakStatement(a)
//        }
//    and returnStatement =
//        parser {
//            let! _ = returnKeyWord
//            let! b = maybeOne statementTerminator
//            if not b.IsEmpty then
//                return ReturnStatement(Nil)
//            else 
//                let! a = expression
//                let! _ = statementTerminator
//                return ReturnStatement(a)
//        }
//    and withStatement =
//        parser {
//            let! _ = withKeyWord
//            let! _ = openParenthesis
//            let! a = expression
//            let! _ = closeParenthesis
//            let! b = statement
//            return WithStatement(a, b)
//        }
//    and switchStatement =
//        parser {
//            let! _ = switchKeyWord
//            let! _ = openParenthesis
//            let! a = expression
//            let! _ = closeParenthesis
//            let! b = caseBlock
//            return SwitchStatement(a, b)
//        }
//    and caseBlock =
//        parser {
//            let! _ = openCurlyBracket
//            let! a = maybe caseClauses
//            let! b = maybe defaultClause
//            let! c = maybe caseClauses
//            let! _ = closeCurlyBracket
//            return CaseBlock(a, b, c)
//        }
//    and caseClauses =
//        parser {
//            let! a = caseClause
//            let! b = many caseClause
//            let root = CaseClauses(Nil, a)
//            return singleFold CaseClauses root b 
//        }
//    and caseClause =
//        parser {
//            let! _ = caseKeyWord
//            let! a = expression
//            let! _ = colon
//            let! b = maybeOne statementList
//            return CaseClause(a, if b.IsEmpty then Nil else b.Head)
//        }
//    and defaultClause =
//        parser {
//            let! _ = defaultKeyWord
//            let! _ = colon
//            let! a = maybeOne statementList
//            return DefaultClause(if a.IsEmpty then Nil else a.Head)
//        }
//    and labelledStatement =
//        parser {
//            let! a = identifierPassLineTerminator
//            let! _ = colon
//            let! c = statement
//            return LabelledStatement(a, c)
//        }        
//    and throwStatement =
//        parser {
//            let! _ = throwKeyWord
//            let! a = expression
//            let! _ = statementTerminator
//            return ThrowStatement(a)
//        }
//    and tryStatement =
//        parser {
//            let! _ = tryKeyWord
//            let! a = blockParser
//            let! b = maybeOne catchParser 
//            let! c = maybeOne finallyParser
//            let b = if b.IsEmpty then Nil else b.Head
//            let c = if c.IsEmpty then Nil else c.Head
//            return TryStatement(a, b, c)
//        }
//    and catchParser =
//        parser {
//            let! _ = catchKeyWord
//            let! _ = openParenthesis
//            let! a = identifierPassLineTerminator
//            let! _ = closeParenthesis
//            let! b = blockParser
//            return Catch(a, b)
//        }
//    and finallyParser =
//        parser {
//            let! _ = finallyKeyWord
//            let! a = blockParser
//            return Finally a
//        }
//    and debuggerStatement =
//        parser {
//            let! _ = debuggerKeyWord
//            let! _ = statementTerminator
//            return DebuggerStatement
//        }
//
//    let sourceElement = parser {
//        let! r = statement +++ functionDeclaration
//        return SourceElement(r)
//    }
//
//    let sourceElements = parser {
//        let! a = sourceElement
//        let! b = many sourceElement
//        let root = SourceElements(Nil, a)
//        return singleFold SourceElements root b
//    }
//
//    let program = parser {
//        let! a = maybeOne sourceElements
//        return Program(if a.IsEmpty then Nil else a.Head)
//    }
