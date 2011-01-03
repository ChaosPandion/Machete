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


    let rec passLineTerminator state =
        (parse {
            let! v = item
            match v with
            | LineTerminator -> 
                return! passLineTerminator
            | _ -> 
                return v
        }) state
         
    let expectIdentifierName state =
        (parse {
            let! v = passLineTerminator
            match v with
            | IdentifierName (x, y) ->
                return v
            | _ -> ()
        }) state
        
    let expectSpecificIdentifierName value state =
        (parse {
            let! v = passLineTerminator
            match v with
            | IdentifierName (x, y) 
                //when Lexer.evalIdentifierName v = value ->
                when IdentifierNameParser.evalIdentifierName v = value ->
                        return v
            | _ -> ()
        }) state

    let expectSpecificIdentifierNames (value:Set<string>) state =
        (parse {
            let! v = passLineTerminator
            match v with
            | IdentifierName (_, _) 
                //when value.Contains (Lexer.evalIdentifierName v) ->
                when value.Contains (IdentifierNameParser.evalIdentifierName v) -> 
                    return v
            | _ -> ()
        }) state 

    let expectIdentifier state =
        (parse {
            let! v = passLineTerminator
            match v with
            | IdentifierName (x, y)
                //when not (CharSets.reservedWordSet.Contains (Lexer.evalIdentifierName v)) ->
                when not (CharSets.reservedWordSet.Contains (IdentifierNameParser.evalIdentifierName v)) ->
                    return Identifier v
            | _ -> ()
        }) state  
            
    let expectPunctuator value state =
        (parse {
            let! v = passLineTerminator
            match v with
            | Punctuator (Str str)  
            | DivPunctuator (Str str)
                when str = value -> 
                    return v
            | _ -> ()
        }) state 
         
    let expectPunctuators (value:Set<string>) state =
        (parse {
            let! v = maybe passLineTerminator
            match v with
            | Some (Punctuator (Str str)) 
            | Some (DivPunctuator (Str str))
                when value.Contains str -> 
                    return v.Value
            |
             _ -> ()
        }) state
         
    let expectOperatorsWithTransform<'a> (value:Map<string, 'a>) state =
        (parse {
            let! v = maybe passLineTerminator
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
                    //let str = Lexer.evalIdentifierName v
                    let r = value.TryFind str
                    match r with
                    | Some v ->
                        return v
                    | None -> ()
                | _ -> ()
            | _ -> ()
        }) state 
        
    let expectComma state = 
        expectPunctuator "," state  

    let nil state = 
        result SourceElement.Nil state
    
    let statementTerminator state =
        (parse {
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
        }) state



    let expectOpenParenthesis state = 
        expectPunctuator "(" state

    let expectCloseParenthesis state = 
        expectPunctuator ")" state 
               
    let expectOpenBrace state = 
        expectPunctuator "{" state

    let expectCloseBrace state = 
        expectPunctuator "}" state 
           
    let expectOpenBracket state = 
        expectPunctuator "[" state

    let expectCloseBracket state = 
        expectPunctuator "]" state

    let expectSemiColon state = 
        expectPunctuator ";" state

    let expectEqualsSign state = 
        expectPunctuator "=" state

    let expectQuestionMark state = 
        expectPunctuator "?" state

    let expectColon state = 
        expectPunctuator ":" state

    let expectFullStop state = 
        expectPunctuator "." state

    let expectDo state = 
        expectSpecificIdentifierName "do" state

    let expectFor state = 
        expectSpecificIdentifierName "for" state

    let expectVar state = 
        expectSpecificIdentifierName "var" state

    let expectWhile state = 
        expectSpecificIdentifierName "while" state

    let expectIn state = 
        expectSpecificIdentifierName "in" state

    let expectIf state = 
        expectSpecificIdentifierName "if" state

    let expectElse state = 
        expectSpecificIdentifierName "else" state

    let expectFunction state = 
        expectSpecificIdentifierName "function" state

    let expectNew state = 
        expectSpecificIdentifierName "new" state

    let expectContinue state = 
        expectSpecificIdentifierName "continue" state

    let expectReturn state = 
        expectSpecificIdentifierName "return" state

    let expectBreak state = 
        expectSpecificIdentifierName "break" state

    let assignmentOperatorMap =
        Map.ofArray [|
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
        |]        

    let equalityOperatorMap =
        Map.ofArray [|
            ("==", EqualityOperator.Equal)
            ("!=", EqualityOperator.DoesNotEqual)
            ("===", EqualityOperator.StrictEqual)
            ("!==", EqualityOperator.StrictDoesNotEqual)
        |]

    let unaryOperatorMap =
        Map.ofArray [|
            ("++", UnaryOperator.Increment)
            ("--", UnaryOperator.Decrement)
            ("+", UnaryOperator.Plus)
            ("-", UnaryOperator.Minus)
            ("~", UnaryOperator.BitwiseNot)
            ("!", UnaryOperator.LogicalNot)
            ("delete", UnaryOperator.Delete)
            ("void", UnaryOperator.Void)
            ("typeof", UnaryOperator.Typeof)
        |]

    let relationalOperatorNoInMap =
        Map.ofArray [|
            ("<", RelationalOperator.LessThan)
            (">", RelationalOperator.GreaterThan)
            ("<=", RelationalOperator.LessThanOrEqual)
            (">=", RelationalOperator.GreaterThanOrEqual)
            ("instanceof", RelationalOperator.Instanceof)   
        |]

    let shiftOperatorMap =
        Map.ofArray [|
            ("<<", BitwiseShiftOperator.LeftShift)
            (">>", BitwiseShiftOperator.SignedRightShift)
            (">>>", BitwiseShiftOperator.UnsignedRightShift)
        |]
        
    let additiveOperatorMap =
        Map.ofArray [|
            ("+", AdditiveOperator.Plus)
            ("-", AdditiveOperator.Minus)
        |]

    let multiplicativeOperatorMap =
        Map.ofArray [|
            ("*", MultiplicativeOperator.Multiply)
            ("%", MultiplicativeOperator.Modulus)
            ("/", MultiplicativeOperator.Divide)
        |]

    let postfixOperatorMap =
        Map.ofArray [|
            ("++", PostfixOperator.Increment)
            ("--", PostfixOperator.Decrement)
        |]

    let relationalOperatorMap =
        relationalOperatorNoInMap.Add ("in", RelationalOperator.In)
       
    let expectAssignmentOperator state = 
        expectOperatorsWithTransform assignmentOperatorMap state
         
    let expectEqualityOperator state = 
        expectOperatorsWithTransform equalityOperatorMap state

    let expectRelationalOperator state = 
        expectOperatorsWithTransform relationalOperatorMap state
    
    let expectRelationalOperatorNoIn state = 
        expectOperatorsWithTransform relationalOperatorNoInMap state

    let expectShiftOperator state = 
        expectOperatorsWithTransform shiftOperatorMap state 

    let expectAdditiveOperator state =
        expectOperatorsWithTransform additiveOperatorMap state

    let expectMultiplicativeOperator state =
        expectOperatorsWithTransform multiplicativeOperatorMap state

    let expectPostfixOperator state = 
        (expectOperatorsWithTransform postfixOperatorMap <|> result PostfixOperator.Nil) state
    
    let expectUnaryOperator state =
        expectOperatorsWithTransform unaryOperatorMap state
    
    let rec bracketedExpression state =
        between expectOpenBracket expectCloseBracket expression state

    and expectDotNotation state =
        (parse {
            do! skip expectFullStop
            let! e = expectIdentifierName |>> InputElement
            return e
        }) state

    and expectLiteral state = 
        (parse {
            let! v = passLineTerminator
            match v with
            | IdentifierName (_, _) ->
                //let str = Lexer.evalIdentifierName v
                let str = Lexer.IdentifierNameParser.evalIdentifierName v
                match str with
                | "true" | "false" as s -> return Literal (BooleanLiteral s)
                | "null" as s -> return Literal (NullLiteral s)
                | _ -> ()
            | StringLiteral _
            | NumericLiteral _ -> return Literal v
            | _ -> ()
        }) state
    

    and primaryExpression state =
        (
            ((between expectOpenParenthesis expectCloseParenthesis expression) |>> PrimaryExpression) <|>
            (objectLiteral |>> PrimaryExpression) <|>
            (arrayLiteral |>> PrimaryExpression) <|>
            (expectIdentifier |>> InputElement |>> PrimaryExpression) <|>
            parse {
                let! v = passLineTerminator
                match v with
                | IdentifierName (_, _) ->
                    //let str = Lexer.evalIdentifierName v
                    let str = Lexer.IdentifierNameParser.evalIdentifierName v
                    match str with
                    | "true" | "false" as s -> return PrimaryExpression (InputElement (Literal (BooleanLiteral s)))
                    | "null" as s -> return PrimaryExpression (InputElement (Literal (NullLiteral s)))
                    | "this" -> return PrimaryExpression (InputElement v)
                    | _ -> ()
                | StringLiteral _
                | NumericLiteral _
                | RegularExpressionLiteral (_, _) -> return PrimaryExpression (InputElement (Literal v))
                | _ -> ()
            }
        ) state

    and arrayLiteral state =
        (parse {
            do! skip expectOpenBracket
            let! e1 = elementList <|> nil
            do! skip (maybe expectComma)
            let! e2 = elision <|> nil
            do! skip expectCloseBracket
            return ArrayLiteral (e1, e2)
        }) state 

    and elementList state =
        (parse {
            let! e1 = elision <|> nil
            let! e2 = assignmentExpression
            let e1 = ElementList (SourceElement.Nil, e1, e2)
            return! manyFold (parse {
                do! skip expectComma
                let! e1 = elision <|> nil
                let! e2 = assignmentExpression
                return e1, e2
            }) e1 (fun x (y, z) -> ElementList (x, y, z))
        }) state

    and elision state = 
        many1Fold expectComma SourceElement.Nil (fun x y -> Elision (x)) state

    and objectLiteral state =
        (parse {
            do! skip expectOpenBrace
            let! e = propertyNameAndValueList <|> nil
            do! skip (maybe expectComma)
            do! skip expectCloseBrace
            return ObjectLiteral e
        }) state

    and propertyNameAndValueList state =
        manySepFold propertyAssignment expectComma PropertyNameAndValueList SourceElement.Nil state

    and propertyAssignment state =
        (
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
        ) state

    and propertyName state =
        (parse {
            let! v = passLineTerminator
            match v with
            | IdentifierName (_, _)
            | StringLiteral _
            | NumericLiteral _ -> return PropertyName v
            | _ -> ()
        }) state 

    and propertySetParameterList state =
        (expectIdentifier |>> PropertySetParameterList) state

    and arguments state = 
        (between expectOpenParenthesis expectCloseParenthesis (argumentList <|> nil) |>> Arguments) state

    and argumentList state = 
        manySepFold assignmentExpression expectComma ArgumentList SourceElement.Nil state   
        
    and memberExpression state = 
        (parse {
            do! skip expectNew
            let! e = memberExpression
            let! a = arguments
            return MemberExpression (e, a)
         } <|>  parse {
            let! e1 = primaryExpression <|> functionExpression
            let e1 = MemberExpression (SourceElement.Nil, e1)
            let! e2 = manyFold (bracketedExpression <|> expectDotNotation) e1 (fun x y -> MemberExpression (x, y))
            return e2
        }) state

    and callExpression state = 
        (parse {
            let! e1 = memberExpression
            let! e2 = arguments
            let e1 = CallExpression (e1, e2)
            let! e2 = manyFold (arguments <|> bracketedExpression <|> expectDotNotation) e1 (fun x y -> CallExpression (x, y))
            return e2
        }) state

    and newExpression state = 
        ((memberExpression |>> NewExpression) <|> 
         parse {
            do! skip expectNew
            return! newExpression
        }) state 

    and leftHandSideExpression state =
        ((callExpression <|> newExpression) |>> LeftHandSideExpression) state

    and postfixExpression state =
        (parse {
            let! e1 = leftHandSideExpression
            let! e2 = expectPostfixOperator
            return PostfixExpression (e1, e2)
        }) state
           
    and unaryExpression state = 
        (parse {
            let! e1 = expectUnaryOperator
            let! e2 = unaryExpression
            return UnaryExpression (e1, e2)
         } <|> parse {
            let! e1 = postfixExpression
            return UnaryExpression (UnaryOperator.Nil, e1)
        }) state

    and multiplicativeExpression state =
        manyWithSepFold unaryExpression expectMultiplicativeOperator MultiplicativeExpression SourceElement.Nil MultiplicativeOperator.Nil state  
    
    and additiveExpression state =
        manyWithSepFold multiplicativeExpression expectAdditiveOperator AdditiveExpression SourceElement.Nil AdditiveOperator.Nil state
    
    and shiftExpression state =
        manyWithSepFold additiveExpression expectShiftOperator ShiftExpression SourceElement.Nil BitwiseShiftOperator.Nil state
    
    and relationalExpression state =
        manyWithSepFold shiftExpression expectRelationalOperator RelationalExpression SourceElement.Nil RelationalOperator.Nil state
    
    and relationalExpressionNoIn state =
        manyWithSepFold shiftExpression expectRelationalOperatorNoIn RelationalExpressionNoIn SourceElement.Nil RelationalOperator.Nil state

    and equalityExpression state =
        manyWithSepFold relationalExpression expectEqualityOperator EqualityExpression SourceElement.Nil EqualityOperator.Nil state 

    and equalityExpressionNoIn state =
        manyWithSepFold relationalExpressionNoIn expectEqualityOperator EqualityExpression SourceElement.Nil EqualityOperator.Nil state
    
    and bitwiseANDExpression state =
        manySepFold equalityExpression (expectPunctuator "&") BitwiseANDExpression SourceElement.Nil state

    and bitwiseANDExpressionNoIn state =
        manySepFold equalityExpressionNoIn (expectPunctuator "&") BitwiseANDExpressionNoIn SourceElement.Nil state

    and bitwiseXORExpression state =
        manySepFold bitwiseANDExpression (expectPunctuator "^") BitwiseXORExpression SourceElement.Nil state

    and bitwiseXORExpressionNoIn state =
        manySepFold bitwiseANDExpressionNoIn (expectPunctuator "^") BitwiseXORExpressionNoIn SourceElement.Nil state

    and bitwiseORExpression state =
        manySepFold bitwiseXORExpression (expectPunctuator "|") BitwiseORExpression SourceElement.Nil state

    and bitwiseORExpressionNoIn state =
        manySepFold bitwiseXORExpressionNoIn (expectPunctuator "|") BitwiseORExpressionNoIn SourceElement.Nil state

    and logicalANDExpression state =
        manySepFold bitwiseORExpression (expectPunctuator "&&") LogicalANDExpression SourceElement.Nil state

    and logicalANDExpressionNoIn state =
        manySepFold bitwiseORExpressionNoIn (expectPunctuator "&&") LogicalANDExpressionNoIn SourceElement.Nil state

    and logicalORExpression state =
        manySepFold logicalANDExpression (expectPunctuator "||") LogicalORExpression SourceElement.Nil state
         
    and logicalORExpressionNoIn state =
        manySepFold logicalANDExpressionNoIn (expectPunctuator "||") LogicalORExpressionNoIn SourceElement.Nil state

    and conditionalExpression state =
        (parse {
            let! e1 = logicalORExpression
            return! 
                parse {
                    do! skip expectQuestionMark
                    let! e2 = assignmentExpression
                    do! skip expectColon
                    let! e3 = assignmentExpression
                    return ConditionalExpression (e1, e2, e3)
                } <|> parse {                    
                    return ConditionalExpression (e1, SourceElement.Nil, SourceElement.Nil)
                }            
        }) state

    and conditionalExpressionNoIn state = 
        (parse {
            let! e1 = logicalORExpressionNoIn
            return! parse {
                do! skip expectQuestionMark
                let! e2 = assignmentExpression
                do! skip expectColon
                let! e3 = assignmentExpressionNoIn
                return ConditionalExpressionNoIn (e1, e2, e3)
            }
            return ConditionalExpressionNoIn (e1, SourceElement.Nil, SourceElement.Nil)
        }) state

    and assignmentExpression state = 
        (parse {
            let! a = leftHandSideExpression
            let! b = expectAssignmentOperator
            let! c = assignmentExpression
            return AssignmentExpression (a, b, c)                
         }  <|> parse {
            let! e = conditionalExpression
            return AssignmentExpression (e, AssignmentOperator.Nil, SourceElement.Nil)
        }) state 

    and assignmentExpressionNoIn state = 
        (parse {
            let! a = leftHandSideExpression
            let! b = expectAssignmentOperator
            let! c = assignmentExpressionNoIn
            return AssignmentExpressionNoIn (a, b, c)                
         }  <|> parse {
            let! e = conditionalExpression
            return AssignmentExpressionNoIn (e, AssignmentOperator.Nil, SourceElement.Nil)
        }) state  
//        ((conditionalExpressionNoIn |>> fun e -> AssignmentExpressionNoIn (e, AssignmentOperator.Nil, SourceElement.Nil)) 
//            <|> parse {
//            let! a = leftHandSideExpression
//            let! b = expectAssignmentOperator
//            let! c = assignmentExpressionNoIn
//            return AssignmentExpressionNoIn (a, b, c)  
//        }) state 

    and expression state =
        (parse {
            let! e1 = assignmentExpression
            let e1 = Expression (SourceElement.Nil, e1)
            return! parse {
                do! skip expectComma
                return! manySepFold assignmentExpression expectComma Expression e1
            }
            return e1
        }) state

    and expressionNoIn state =
        (parse {
            let! e1 = assignmentExpressionNoIn
            let e1 = ExpressionNoIn (SourceElement.Nil, e1)
            return! parse {
                do! skip expectComma
                return! manySepFold assignmentExpressionNoIn expectComma ExpressionNoIn e1
            }
            return e1
        }) state

    and functionExpression state = 
        (parse {
            do! skip expectFunction
            let! i = expectIdentifier <|> result InputElement.Nil
            let! l = between expectOpenParenthesis expectCloseParenthesis (formalParameterList <|> nil)
            let! b = between expectOpenBrace expectCloseBrace functionBody             
            return FunctionExpression (i, l, b)
        }) state  
              
    and functionBody state = 
        (parse {
            let! x = sourceElements <|> nil
            return FunctionBody x
        }) state
        
    and formalParameterList state = 
        manySepFold (expectIdentifier |>> InputElement) expectComma FormalParameterList SourceElement.Nil state

    and functionDeclaration state = 
        (parse {
            do! skip expectFunction
            let! i = expectIdentifier <|> fail "All function declarations require an identifier."
            let! l = between expectOpenParenthesis expectCloseParenthesis (formalParameterList <|> nil)
            let! b = between expectOpenBrace expectCloseBrace functionBody
            return FunctionDeclaration (i, l, b)
        }) state  

    and statement state =
        (parse {
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
        }) state

    and statementList state =  
        manyFold statement SourceElement.Nil (fun x y -> StatementList (x, y)) state
               
    and blockParser state =
        (between expectOpenBrace expectCloseBrace (statementList <|> nil) |>> Block) state

    and variableStatement state =
        (pipe3 expectVar variableDeclarationList statementTerminator (fun _ v _ -> VariableStatement v)) state

    and variableDeclarationList state = 
        (manySepFold variableDeclaration expectComma VariableDeclarationList SourceElement.Nil) state

    and variableDeclarationListNoIn state =
        manySepFold variableDeclarationNoIn expectComma VariableDeclarationListNoIn SourceElement.Nil state

    and variableDeclaration state =
        tuple2 expectIdentifier (initializer <|> nil) VariableDeclaration state

    and variableDeclarationNoIn state = 
        tuple2 expectIdentifier (initializerNoIn <|> nil) VariableDeclarationNoIn state

    and initializer state =
        (parse {
            do! skip expectEqualsSign
            let! e = assignmentExpression
            return Initialiser e
        }) state

    and initializerNoIn state =
        (parse {
            do! skip expectEqualsSign
            let! e = assignmentExpressionNoIn
            return InitialiserNoIn e
        }) state

    and emptyStatement state =
        (parse {
            do! skip expectSemiColon
            return EmptyStatement
        }) state

    and expressionStatement state =
        (parse {
            do! isNotFollowedBy expectOpenBrace
            do! isNotFollowedBy expectFunction
            let! e = expression
            do! statementTerminator
            return ExpressionStatement e
        }) state

    and ifStatement state =
        (parse {
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
        }) state

    and iterationStatement state =
        (parse {
            let! s = between expectDo expectWhile statement 
            let! e = between expectOpenParenthesis expectCloseParenthesis expression
            do! skip statementTerminator
            return IterationStatement (s, e, SourceElement.Nil, SourceElement.Nil)
         } <|> parse {
            do! skip expectWhile
            let! e = between expectOpenParenthesis expectCloseParenthesis expression 
            let! s = statement
            return IterationStatement (e, s, SourceElement.Nil, SourceElement.Nil)
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
        }) state 

    and continueStatement state =
        (parse {
            do! skip expectContinue
            let! e = expectIdentifier <|> result InputElement.Nil
            do! skip statementTerminator
            return ContinueStatement e
        }) state

    and breakStatement state =
        (parse {
            do! skip expectBreak
            let! e = expectIdentifier <|> result InputElement.Nil
            do! skip statementTerminator
            return BreakStatement e
        }) state

    and returnStatement state =
        (parse {
            do! skip expectReturn
            let! e = 
                parse {
                    do! skip statementTerminator
                    return SourceElement.Nil
                } <|> parse {
                    let! e = expression
                    do! skip statementTerminator
                    return e
                }
            return ReturnStatement e
        }) state

    and withStatement state =
        pipe5 (expectSpecificIdentifierName "with") (expectPunctuator "(") expression (expectPunctuator ")") statement (fun a b c d e -> WithStatement (c, e)) state

    and switchStatement state =
        pipe5 (expectSpecificIdentifierName "switch") (expectPunctuator "(") expression (expectPunctuator ")") caseBlock (fun a b c d e -> SwitchStatement (c, e)) state

    and caseBlock state =
        pipe5 (expectPunctuator "{") (caseClauses <|> nil) (defaultClause <|> nil) (caseClauses <|> nil) (expectPunctuator "}") (fun a b c d e -> CaseBlock (b, c, d)) state

    and caseClauses state =
        manyFold caseClause SourceElement.Nil (fun x y -> CaseClauses (x, y)) state

    and caseClause state =
        pipe4 (expectSpecificIdentifierName "case") expression (expectPunctuator ":") (statementList <|> nil) (fun w x y z -> CaseClause (x, z)) state

    and defaultClause state =
        pipe3 (expectSpecificIdentifierName "default") (expectPunctuator ":") (statementList <|> nil) (fun x y z -> DefaultClause z) state

    and labelledStatement state =
        pipe3 (expectIdentifier) (expectPunctuator ":") statement (fun x y z -> LabelledStatement (x, z)) state
               
    and throwStatement state =
        pipe3 (expectSpecificIdentifierName "throw") expression statementTerminator (fun x y z -> ThrowStatement y) state

    and tryStatement state =
        pipe4 (expectSpecificIdentifierName "try") blockParser (catchParser <|> nil) (finallyParser <|> nil) (fun a b c d -> TryStatement (b, c, d)) state

    and catchParser state =
        pipe5 (expectSpecificIdentifierName "catch") (expectPunctuator "(") expectIdentifier (expectPunctuator ")") blockParser (fun a b c d e -> Catch (c, e)) state

    and finallyParser state =
        pipe2 (expectSpecificIdentifierName "finally") blockParser (fun x y -> Finally y) state

    and debuggerStatement state =
        pipe2 (expectSpecificIdentifierName "debugger") statementTerminator (fun x y -> DebuggerStatement) state

    and sourceElement state = 
        ((statement <|> functionDeclaration) |>> SourceElement) state

    and sourceElements state =  
        manyFold sourceElement SourceElement.Nil (fun x y -> SourceElements (x, y)) state

    and program state = 
        (sourceElements <|> nil |>> Program) state

    let parse input =
        let rs = program (State((Lexer.tokenize input), ({ dummy = 3 })))
        match rs with
        | Success (v, s) -> v
        | Failure (ms, s) -> failwith (System.String.Join ("\r\n", ms))