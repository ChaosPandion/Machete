namespace Machete.Parser

open System
open FParsec.OperatorPrecedenceParser
open FParsec.CharParsers
open FParsec.Primitives
open InputElementParsers

module SourceElementParsers =

    let skippable = [
        parseWhiteSpace
        parseLineTerminator
        parseMultiLineComment
        parseSingleLineComment
    ]

    let skipOver state = 
        (skipMany (choice skippable)) state

    let skipOverThen parser state = 
        (skipMany (choice skippable) .>> parser) state
        
    let skipIdentifierName name state =
        (skipOverThen (parseSpecificIdentifierName name) |>> ignore) state

    let skipPunctuator name state =
        (skipOverThen (pstring name) |>> ignore) state

    let betweenBraces parser state =
        (skipPunctuator "{" >>. parser .>> skipPunctuator "}") state

    let betweenBrackets parser state =
        (skipPunctuator "[" >>. parser .>> skipPunctuator "]") state

    let betweenParentheses parser state =
        (skipPunctuator "(" >>. parser .>> skipPunctuator ")") state

    let skipStatementTerminator state =
        (parse {
            return ()
        }) state  

    let postfixIncrement = PostfixOp ("++", skipOver, 17, true, fun x -> PostfixExpression (x, Some PostfixIncrement))
    let postfixDecrement = PostfixOp ("--", skipOver, 17, true, fun x -> PostfixExpression (x, Some PostfixDecrement))
    let delete = PrefixOp ("delete", skipOver, 16, true, fun x -> UnaryExpression (Some Delete, x))
    let typeof = PrefixOp ("typeof", skipOver, 16, true, fun x -> UnaryExpression (Some Typeof, x))
    let void' = PrefixOp ("void", skipOver, 16, true, fun x -> UnaryExpression (Some UnaryOperator.Void, x))
    let prefixIncrement = PrefixOp ("++", skipOver, 15, true, fun x -> UnaryExpression (Some PrefixIncrement, x))
    let prefixDecrement = PrefixOp ("--", skipOver, 15, true, fun x -> UnaryExpression (Some PrefixDecrement, x))
    let plus = PrefixOp ("+", skipOver, 15, true, fun x -> UnaryExpression (Some Plus, x))
    let minus = PrefixOp ("-", skipOver, 15, true, fun x -> UnaryExpression (Some Minus, x))
    let bitwiseNot = PrefixOp ("~", skipOver, 15, true, fun x -> UnaryExpression (Some BitwiseNot, x))
    let logicalNot = PrefixOp ("!", skipOver, 15, true, fun x -> UnaryExpression (Some LogicalNot, x))         
    let multiply = InfixOp("*", skipOver, 14, Assoc.Left, fun x y -> MultiplicativeExpression (x, Some (Multiply, y)))
    let divide = InfixOp("/", skipOver, 14, Assoc.Left, fun x y -> MultiplicativeExpression (x, Some (Divide, y)))
    let remainder = InfixOp("%", skipOver, 14, Assoc.Left, fun x y -> MultiplicativeExpression (x, Some (Remainder, y)))          
    let add = InfixOp("+", skipOver, 13, Assoc.Left, fun x y -> AdditiveExpression (x, Some (Add, y)))
    let subtract = InfixOp("-", skipOver, 13, Assoc.Left, fun x y -> AdditiveExpression (x, Some (Subtract, y)))
    let leftShift = InfixOp("<<", skipOver, 12, Assoc.Left, fun x y -> ShiftExpression (x, Some (LeftShift, y)))  
    let signedRightShift = InfixOp(">>", skipOver, 12, Assoc.Left, fun x y -> ShiftExpression (x, Some (SignedRightShift, y)))
    let unsignedRightShift = InfixOp(">>>", skipOver, 12, Assoc.Left, fun x y -> ShiftExpression (x, Some (UnsignedRightShift, y)))
    let lessThan = InfixOp("<", skipOver, 11, Assoc.Left, fun x y -> RelationalExpression (x, Some (LessThan, y)))
    let greaterThan = InfixOp(">", skipOver, 11, Assoc.Left, fun x y -> RelationalExpression (x, Some (GreaterThan, y)))
    let lessThanOrEqual = InfixOp("<=", skipOver, 11, Assoc.Left, fun x y -> RelationalExpression (x, Some (LessThanOrEqual, y)))
    let greaterThanOrEqual = InfixOp(">=", skipOver, 11, Assoc.Left, fun x y -> RelationalExpression (x, Some (GreaterThanOrEqual, y)))
    let instanceof = InfixOp("instanceof", skipOver, 11, Assoc.Left, fun x y -> RelationalExpression (x, Some (Instanceof, y)))
    let in' = InfixOp("in", skipOver, 11, Assoc.Left, fun x y -> RelationalExpression (x, Some (In, y)))        
    let equals = InfixOp("==", skipOver, 10, Assoc.Left, fun x y -> EqualityExpression (x, Some (Equals, y)))
    let doesNotEquals = InfixOp("!=", skipOver, 10, Assoc.Left, fun x y -> EqualityExpression (x, Some (DoesNotEquals, y)))
    let strictEquals = InfixOp("===", skipOver, 10, Assoc.Left, fun x y -> EqualityExpression (x, Some (StrictEquals, y)))
    let strictDoesNotEquals = InfixOp("!==", skipOver, 10, Assoc.Left, fun x y -> EqualityExpression (x, Some (StrictDoesNotEquals, y)))
    let bitwiseAnd = InfixOp("&", skipOver, 9, Assoc.Left, fun x y -> BitwiseANDExpression (x, Some y))
    let bitwiseXor = InfixOp("^", skipOver, 8, Assoc.Left, fun x y -> BitwiseXORExpression (x, Some y))
    let bitwiseOr = InfixOp("|", skipOver, 7, Assoc.Left, fun x y -> BitwiseORExpression (x, Some y))
    let logicalAnd = InfixOp("&&", skipOver, 6, Assoc.Left, fun x y -> LogicalANDExpression (x, Some y))
    let logicalOr = InfixOp("||", skipOver, 5, Assoc.Left, fun x y -> LogicalORExpression (x, Some y))
    let conditional = TernaryOp("?", skipOver, ":", skipOver, 4, Assoc.Right, fun x y z -> ConditionalExpression (x, Some (y, z)))
    let simpleAssignment = InfixOp("=", skipOver, 2, Assoc.Right, fun x y -> AssignmentExpression (x, Some (Simple, y)))
    let compoundMultiplyAssignment = InfixOp("*=", skipOver, 2, Assoc.Right, fun x y -> AssignmentExpression (x, Some (CompoundMultiply, y)))
    let compoundDivideAssignment = InfixOp("/=", skipOver, 2, Assoc.Right, fun x y -> AssignmentExpression (x, Some (CompoundDivide, y)))
    let compoundRemainderAssignment = InfixOp("%=", skipOver, 2, Assoc.Right, fun x y -> AssignmentExpression (x, Some (CompoundRemainder, y)))
    let compoundAddAssignment = InfixOp("+=", skipOver, 2, Assoc.Right, fun x y -> AssignmentExpression (x, Some (CompoundAdd, y)))
    let compoundSubtractAssignment = InfixOp("-=", skipOver, 2, Assoc.Right, fun x y -> AssignmentExpression (x, Some (CompoundSubtract, y)))        
    let compoundLeftShiftAssignment = InfixOp("<<=", skipOver, 2, Assoc.Right, fun x y -> AssignmentExpression (x, Some (CompoundLeftShift, y)))
    let compoundSignedRightShiftAssignment = InfixOp(">>=", skipOver, 2, Assoc.Right, fun x y -> AssignmentExpression (x, Some (CompoundSignedRightShift, y)))
    let compoundUnsignedRightShiftAssignment = InfixOp(">>>=", skipOver, 2, Assoc.Right, fun x y -> AssignmentExpression (x, Some (CompoundUnsignedRightShift, y)))
    let compoundBitwiseAndAssignment = InfixOp("&=", skipOver, 2, Assoc.Right, fun x y -> AssignmentExpression (x, Some (CompoundBitwiseAnd, y)))
    let compoundBitwiseXorAssignment = InfixOp("^=", skipOver, 2, Assoc.Right, fun x y -> AssignmentExpression (x, Some (CompoundBitwiseXor, y)))
    let compoundBitwiseOrAssignment = InfixOp("|=", skipOver, 2, Assoc.Right, fun x y -> AssignmentExpression (x, Some (CompoundBitwiseOr, y)))
    let comma = InfixOp(",", skipOver, 1, Assoc.Left, fun x y -> Expression (x, Some y))

    let expressionOperators = [ 
        postfixIncrement
        postfixDecrement
        delete 
        typeof
        void'
        prefixIncrement
        prefixDecrement
        plus
        minus
        bitwiseNot
        logicalNot 
        multiply
        divide
        remainder
        add
        subtract
        leftShift
        signedRightShift
        unsignedRightShift
        lessThan
        greaterThan
        lessThanOrEqual
        greaterThanOrEqual
        instanceof
        in'
        equals
        doesNotEquals
        strictEquals
        strictDoesNotEquals
        bitwiseAnd 
        bitwiseXor 
        bitwiseOr
        logicalAnd 
        logicalOr
        conditional
        simpleAssignment       
        compoundMultiplyAssignment
        compoundDivideAssignment
        compoundRemainderAssignment
        compoundAddAssignment
        compoundSubtractAssignment
        compoundLeftShiftAssignment
        compoundSignedRightShiftAssignment
        compoundUnsignedRightShiftAssignment
        compoundBitwiseAndAssignment
        compoundBitwiseXorAssignment
        compoundBitwiseOrAssignment
        comma
    ]

    let expressionNoInOperators () = 
        expressionOperators |> List.filter (fun op -> op <> in')

    let assignmentExpressionOperators () = 
        expressionOperators |> List.filter (fun op -> op <> comma)

    let assignmentExpressionNoInOperators () = 
        expressionOperators |> List.filter (fun op -> op <> comma && op <> in')
                    
    let rec parseTerm state = 
        (skipOver >>. parseLeftHandSideExpression .>> skipOver) state
        
    and parsePrimaryExpression state = 
        (parse {        
            let! r =  skipOver >>. parseLiteral .>> skipOver
            return PrimaryExpression r
        }) state
        
    and parseFunctionExpression state = 
        (parse {        
            let! r = parseLiteral .>> skipOver
            return PrimaryExpression r
        }) state
        
    and parseLambdaExpression state = 
        (parse {        
            let! r = parseLiteral .>> skipOver
            return PrimaryExpression r
        }) state

    and parseMemberExpression state = 
        (
        (attempt <| parse {
            do! skipIdentifierName "new"        
            let! r = parseMemberExpression
            let! a = opt parseArguments
            return MemberExpression (r, a)
        }) <|> parse {        
            let! r = parsePrimaryExpression
            let! t = opt parseMemberExpressionTail
            return MemberExpression (r, t)
        }
        ) state

    and parseMemberExpressionTail state = 
        (parse {
            let! c = skipOver >>. anyOf ".["    
            match c with
            | '.' ->
                let! e = parseIdentifierName
                match e with
                | IdentifierName (s, d) ->
                    let e = PrimaryExpression (StringLiteral (s, d))
                    let! t = opt parseMemberExpressionTail
                    return MemberExpressionTail (e, t)
                | _ -> raise (exn())
            | '[' ->
                let! e = skipOver >>. parseExpression .>> skipOver .>> skipChar ']'
                let! t = opt parseMemberExpressionTail
                return MemberExpressionTail (e, t)            
            | _ -> raise (exn())
        }
        ) state

    and parseNewExpression state = 
        (
        parse {
            do! skipIdentifierName "new"        
            let! r = parseNewExpression
            return NewExpression r
        } <|> parse {        
            let! r = parseMemberExpression
            return NewExpression r
        }
        ) state

    and parseCallExpression state = 
        (parse {        
            let! r = parseLiteral .>> skipOver
            return PrimaryExpression r
        }) state

    and parseCallExpressionTail state = 
        (parse {        
            let! r = parseLiteral .>> skipOver
            return PrimaryExpression r
        }) state

    and parseArguments state = 
        (attempt ((skipPunctuator "(" >>. opt parseArgumentList .>> skipPunctuator ")") |>> Arguments)) state

    and parseArgumentList state = 
        (parse {        
            let! r = parseAssignmentExpression
            let! t = opt parseArgumentListTail
            return ArgumentList (r, t)
        }) state

    and parseArgumentListTail state = 
        (parse {        
            do! skipPunctuator ","
            let! r = parseAssignmentExpression
            let! t = opt parseArgumentListTail
            return ArgumentList (r, t)
        }) state

    and parseLeftHandSideExpression state = 
        ( 
        parse {        
            let! r = parseMemberExpression
            let! a = opt parseArguments
            match a with
            | Some _ ->
                let! t = opt parseCallExpressionTail                
                return CallExpression (r, a, t)
            | None ->
                return r
        } <|> parse {
            do! skipIdentifierName "new"        
            let! r = parseNewExpression
            return NewExpression r
        }
        ) state

    and parseExpression =
        let p = new OperatorPrecedenceParser<SourceElement, unit>()
        let e = p.ExpressionParser            
        p.TermParser <- parseTerm
        p.AddOperators expressionOperators
        fun state -> 
            (skipOver >>. e) state

    and parseExpressionNoIn =
        let p = new OperatorPrecedenceParser<SourceElement, unit>()
        let e = p.ExpressionParser            
        p.TermParser <- parseTerm
        p.AddOperators (expressionNoInOperators ())
        fun state -> 
            (skipOver >>. e) state

    and parseAssignmentExpression =
        let p = new OperatorPrecedenceParser<SourceElement, unit>()
        let e = p.ExpressionParser            
        p.TermParser <- parseTerm
        p.AddOperators (assignmentExpressionOperators ())
        fun state -> 
            (skipOver >>. e) state

    and parseAssignmentExpressionNoIn =
        let p = new OperatorPrecedenceParser<SourceElement, unit>()
        let e = p.ExpressionParser            
        p.TermParser <- parseTerm
        p.AddOperators (assignmentExpressionNoInOperators ())
        fun state -> 
            (skipOver >>. e) state

    and parseStatement state =
        (parse {
            return Statement
        }) state

    and parseBlock state =
        (parse {
            return Statement
        }) state

    and parseStatementList state =
        (parse {
            return StatementList
        }) state

    and parseVariableStatement state =
        (parse {
            return Program None
        }) state

    and parseVariableDeclarationList state =
        (parse {
            return Program None
        }) state

    and parseVariableDeclarationListNoIn state =
        (parse {
            return Program None
        }) state

    and parseVariableDeclaration state =
        (parse {
            return Program None
        }) state

    and parseVariableDeclarationNoIn state =
        (parse {
            return Program None
        }) state

    and parseInitialiser state =
        (parse {
            return Program None
        }) state

    and parseInitialiserNoIn state =
        (parse {
            return Program None
        }) state

    and parseEmptyStatement state =
        (parse {
            return Program None
        }) state

    and parseExpressionStatement state =
        (parse {
            return Program None
        }) state
        
    and parseIfStatement state =
        (parse {
            return Program None
        }) state

    and parseForeachIterationStatement state =
        (parse {
            return Program None
        }) state
     
    and parseYieldStatement state =
        (parse {
            return Program None
        }) state

    and parseYieldBreakStatement state =
        (parse {
            return Program None
        }) state
        
    and parseYieldContinueStatement state =
        (parse {
            return Program None
        }) state

    and parseContinueStatement state =
        (parse {
            return Program None
        }) state

    and parseBreakStatement state =
        (parse {
            return Program None
        }) state

    and parseReturnStatement state =
        (parse {
            return Program None
        }) state

    and parseWithStatement state =
        (parse {
            do! skipIdentifierName "with"
            let! e = betweenParentheses parseExpression
            let! s = parseStatement
            return WithStatement (e, s)
        }) state

    and parseSwitchStatement state =
        (parse {
            do! skipIdentifierName "switch"
            let! e = betweenParentheses parseExpression
            let! c = parseCaseBlock
            return SwitchStatement (e, c)
        }) state

    and parseCaseBlock state =
        (parse {
            do! skipPunctuator "{"
            let! a = opt parseCaseClauses
            let! b = opt parseDefaultClause
            let! c = opt parseCaseClauses
            do! skipPunctuator "}"
            return CaseBlock (a, b, c)
        }) state        

    and parseCaseClauses state =
        (parse {
            let! head = parseCaseClause
            let! tail = opt parseCaseClauses
            return CaseClauses (head, tail)
        }) state

    and parseCaseClause state =
        (parse {
            do! skipIdentifierName "case"
            let! e = parseExpression
            do! skipPunctuator ":"
            let! s = opt parseStatementList
            return CaseClause (e, s)
        }) state

    and parseDefaultClause state =
        (parse {
            do! skipIdentifierName "default"
            do! skipPunctuator ":"
            let! s = opt parseStatementList
            return DefaultClause s
        }) state  

    and parseLabelledStatement state =
        (parse {
            let! i = parseIdentifier
            do! skipPunctuator ":"
            let! s = parseStatement
            match i with
            | IdentifierName (i, d) ->
                return LabelledStatement (i, s)
        }) state

    and parseThrowStatement state =
        (parse {
            do! skipIdentifierName "throw"
            let! e = parseExpression
            do! skipStatementTerminator
            return ThrowStatement e
        }) state

    and parseTryStatement state =
        (parse {
            do! skipIdentifierName "try"
            let! b = parseBlock
            let! c = opt parseCatch
            let! f = opt parseFinally
            return TryStatement (b, c, f)
        }) state

    and parseCatch state =
        (parse {
            do! skipIdentifierName "catch"
            let! i = betweenBraces parseIdentifier
            let! b = parseBlock
            match i with
            | IdentifierName (s, d) ->
                return Catch (s, b)
            | _ -> raise (exn())
        }) state    

    and parseFinally state =
        (parse {
            do! skipIdentifierName "finally"
            let! b = parseBlock
            return Finally b
        }) state

    and parseDebuggerStatement state =
        (parse {
            do! skipIdentifierName "debugger"
            do! skipStatementTerminator
            return DebuggerStatement
        }) state    

    and parseFunctionDeclaration state =
        (parse {
            return Program None
        }) state

    and parseFormalParameterList state =
        (parse {
            return Program None
        }) state

    and parseFunctionBody state =
        (parse {
            return Program None
        }) state

    and parseSourceElement state =
        (parse {
            let! r = parseFunctionDeclaration <|> parseStatement
            return SourceElement r
        }) state

    and parseSourceElements state =
        (parse {
            let! first = parseSourceElement
            let! rest = many1Fold first (fun a b -> SourceElements (a, b)) (opt parseSourceElement)
            return rest
        }) state
         
    and parseProgram state =
        (parse {
            let! s = opt parseSourceElements 
            return Program s
        }) state