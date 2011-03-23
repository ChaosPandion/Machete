namespace Machete.Parser

open System
open FParsec
open FParsec.Primitives
open FParsec.CharParsers
open InputElementParsers

module SourceElementParsers = 

    let skippable = [
        parseWhiteSpace
        parseLineTerminator
        parseMultiLineComment
        parseSingleLineComment
    ]

    let skip parser state = 
        (skipMany (choice skippable) >>. parser) state

    let skipIdentifierName name state =
        (skip (parseSpecificIdentifierName name) |>> ignore) state

    let skipPunctuator name state =
        (skip (pstring name) |>> ignore) state

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
               
    let rec parsePrimaryExpression state =
        (parse {
            return ()
        }) state

    and parseGeneratorExpression state =
        (parse {
            return ()
        }) state
                
    and parseObjectLiteral state =
        (parse {
            return ()
        }) state

    and parsePropertyNameAndValueList state =
        (parse {
            return ()
        }) state

    and parsePropertyAssignment state =
        (parse {
            return ()
        }) state
                  
    and parsePropertyName state =
        (parse {
            return ()
        }) state

    and parsePropertySetParameterList state =
        (parse {
            return ()
        }) state

    and parseArrayLiteral state =
        (parse {
            return ()
        }) state

    and parseElision state =
        (parse {
            return ()
        }) state

    and parseElementList state =
        (parse {
            return ()
        }) state

    and parseMemberExpression state =
        (parse {
            return ()
        }) state
        
    and parseNewExpression state =
        (parse {
            return ()
        }) state

    and parseCallExpression state =
        (parse {
            return ()
        }) state

    and parseArguments state =
        (parse {
            return ()
        }) state

    and parseArgumentList state =
        (parse {
            return ()
        }) state

    and parseLeftHandSideExpression state =
        (parse {
            return ()
        }) state

    and parsePostfixExpression state =
        (parse {
            return ()
        }) state

    and parseUnaryExpression state =
        (parse {
            return ()
        }) state

    and parseMultiplicativeExpression state =
        (parse {
            return ()
        }) state

    and parseAdditiveExpression state =
        (parse {
            return ()
        }) state

    and parseShiftExpression state =
        (parse {
            return ()
        }) state

    and parseRelationalExpression state =
        (parse {
            return ()
        }) state    
        
    and parseRelationalExpressionNoIn state =
        (parse {
            return ()
        }) state

    and parseEqualityExpression state =
        (parse {
            return ()
        }) state

    and parseEqualityExpressionNoIn state =
        (parse {
            return ()
        }) state

    and parseBitwiseANDExpression state =
        (parse {
            return ()
        }) state

    and parseBitwiseANDExpressionNoIn state =
        (parse {
            return ()
        }) state

    and parseBitwiseXORExpression state =
        (parse {
            return ()
        }) state

    and parseBitwiseXORExpressionNoIn state =
        (parse {
            return ()
        }) state

    and parseBitwiseORExpression state =
        (parse {
            return ()
        }) state

    and parseBitwiseORExpressionNoIn state =
        (parse {
            return ()
        }) state

    and parseLogicalANDExpression state =
        (parse {
            return ()
        }) state

    and parseLogicalANDExpressionNoIn state =
        (parse {
            return ()
        }) state
    
    and parseLogicalORExpression state =
        (parse {
            return ()
        }) state

    and parseLogicalORExpressionNoIn state =
        (parse {
            return ()
        }) state

    and parseConditionalExpression state =
        (parse {
            return ()
        }) state

    and parseConditionalExpressionNoIn state =
        (parse {
            return ()
        }) state
                    
    and parseAssignmentExpression state =
        (parse {
            return ()
        }) state

    and parseAssignmentExpressionNoIn state =
        (parse {
            return ()
        }) state

    and parseExpression state =
        (parse {
            return Statement
        }) state

    and parseExpressionNoIn state =
        (parse {
            return ()
        }) state

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
            return ()
        }) state

    and parseVariableDeclarationList state =
        (parse {
            return ()
        }) state

    and parseVariableDeclarationListNoIn state =
        (parse {
            return ()
        }) state

    and parseVariableDeclaration state =
        (parse {
            return ()
        }) state

    and parseVariableDeclarationNoIn state =
        (parse {
            return ()
        }) state

    and parseInitialiser state =
        (parse {
            return ()
        }) state

    and parseInitialiserNoIn state =
        (parse {
            return ()
        }) state

    and parseEmptyStatement state =
        (parse {
            return ()
        }) state

    and parseExpressionStatement state =
        (parse {
            return ()
        }) state
        
    and parseIfStatement state =
        (parse {
            return ()
        }) state

    and parseForeachIterationStatement state =
        (parse {
            return ()
        }) state
     
    and parseYieldStatement state =
        (parse {
            return ()
        }) state

    and parseYieldBreakStatement state =
        (parse {
            return ()
        }) state
        
    and parseYieldContinueStatement state =
        (parse {
            return ()
        }) state

    and parseContinueStatement state =
        (parse {
            return ()
        }) state

    and parseBreakStatement state =
        (parse {
            return ()
        }) state

    and parseReturnStatement state =
        (parse {
            return ()
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
            return ()
        }) state

    and parseFunctionExpression state =
        (parse {
            return ()
        }) state

    and parseLambdaExpression state =
        (parse {
            return ()
        }) state

    and parseFormalParameterList state =
        (parse {
            return ()
        }) state

    and parseFunctionBody state =
        (parse {
            return ()
        }) state

    and parseSourceElement state =
        (parse {
            return ()
        }) state

    and parseSourceElements state =
        (parse {
            return ()
        }) state
         
    and parseProgram state =
        (parse {
            return ()
        }) state

