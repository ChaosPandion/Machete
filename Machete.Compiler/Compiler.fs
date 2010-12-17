namespace Machete.Compiler

open Machete

type internal exp = System.Linq.Expressions.Expression
type internal label = System.Linq.Expressions.LabelExpression
type internal methodinfo = System.Reflection.MethodInfo
type internal dyn = Machete.Interfaces.IDynamic

//type internal State (element:SourceElement, labelMap:Map<string, label>, breakList:list<label>, continueList:list<label>) = struct
//        member this.Element = element
//        member this.LabelMap = labelMap
//        member this.BreakList = breakList
//        member this.ContinueList = continueList
//        member this.WithElement element = State (element, labelMap, breakList, continueList)
//    end
//type internal State (element:SourceElement, labels:list<Map<string, label>>, functions:list<string>, variables:list<string>) = struct
//        member this.Element = element
//        member this.Labels = labels
//        member this.Functions = functions
//        member this.Variables = variables
//        member this.WithElement element = State (element, labels, functions, variables)
//    end
//type Code = delegate of Machete.Interfaces.IEnvironment * Machete.Interfaces.IArgs -> Machete.Interfaces.IDynamic

type internal State = {
    element:SourceElement
    labels:list<Map<string, label>>
    functions:list<string>
    variables:list<string>
} 

type Compiler () =

    let call (e:exp) (m:methodinfo) (a:exp[]) = exp.Call (e, m, a) :> exp
    let invoke (e:exp) (a:exp[]) = exp.Invoke (e, a) :> exp
    let condition test ifTrue ifFalse = exp.Condition (test, ifTrue, ifFalse) :> exp
    let constant v = exp.Constant v :> exp 
    let block es = exp.Block es :> exp      
    
    let environmentParam = exp.Parameter (Reflection.IEnvironment.t, "environment")
    let argsParam = exp.Parameter (Reflection.IArgs.t, "args")

//    let checkReference (value:Machete.Interfaces.IDynamic) =
//        match value with
//        | :? Machete.Interfaces.IReference as ref ->
//            match ref.IsStrictReference, ref.Base, ref.Name with
//            | true, :? Machete.Interfaces.IEnvironmentRecord, "eval" 
//            | true, :? Machete.Interfaces.IEnvironmentRecord, "arguments" ->
//                environment.CreateSyntaxError().Op_Throw();
//            | _ -> ()
//        | _ -> ()
//
//    let simpleAssignment (left:Machete.Interfaces.IDynamic) (right:Machete.Interfaces.IDynamic) =
//        checkReference left
//        left.Value <- right.Value
//
//    let compoundAssignment (left:Machete.Interfaces.IDynamic) (right:Machete.Interfaces.IDynamic) op =
//        let value = op left right
//        checkReference left
//        left.Value <- value
//        value

    let evalIdentifier identifier =
        match identifier with
        | Lexer.Identifier e ->
            let identifier = Lexer.IdentifierNameParser.evalIdentifierName e
            let context = call environmentParam Reflection.IEnvironment.get_Context [||]
            let lex = call context Reflection.IExecutionContext.get_LexicalEnviroment [||]
            call lex Reflection.ILexicalEnvironment.getIdentifierReference [| constant identifier; constant false|]

    let rec evalExpression (state:State) =
        match state.element with
        | Expression (Nil, right) | ExpressionNoIn (Nil, right) ->
            evalAssignmentExpression { state with element = right }
//            let inst = evalAssignmentExpression { state with element = right }
//            let test = exp.TypeIs (inst, Reflection.IReferenceBase.t)
//            let ifTrue = call (exp.Convert(inst, Reflection.IReferenceBase.t)) Reflection.IReferenceBase.get [||] 
//            let ifFalse = call environmentParam Reflection.IEnvironment.get_Undefined [||]
//            condition test ifTrue ifFalse
        | Expression (left, right) | ExpressionNoIn (left, right) ->
            let left = evalExpression { state with element = left }
            let inst = evalAssignmentExpression { state with element = right }
            let test = exp.TypeIs (inst, Reflection.IReferenceBase.t)
            let ifTrue = call (exp.Convert(inst, Reflection.IReferenceBase.t)) Reflection.IReferenceBase.get [||] 
            let ifFalse = call environmentParam Reflection.IEnvironment.get_Undefined [||]
            block [| left; condition test ifTrue ifFalse |]
            
    and evalAssignmentExpression (state:State) =
        match state.element with
        | AssignmentExpression (left, Nil, Nil) | AssignmentExpressionNoIn (left, Nil, Nil) ->
            evalConditionalExpression { state with element = left }
        | AssignmentExpression (left, AssignmentOperator (Lexer.Punctuator (Lexer.Str "=")), right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator (Lexer.Punctuator (Lexer.Str "=")), right) ->
            let left = evalLeftHandSideExpression { state with element = left }
            let right = evalAssignmentExpression { state with element = right }
            let valueVar = exp.Variable(Reflection.IDynamic.t, "value") 
            exp.Block( 
                [|valueVar|], [|
                    exp.Assign(valueVar, call right Reflection.IDynamic.get_Value [| |]) :>exp
                    call left Reflection.IDynamic.set_Value [| valueVar |]
                    valueVar :>exp
                |]
            ) :> exp
            
            //invoke (constant simpleAssignment) [|left;right|]
        | AssignmentExpression (left, AssignmentOperator (Lexer.Punctuator (Lexer.Str v)), right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator (Lexer.Punctuator (Lexer.Str v)), right)
        | AssignmentExpression (left, AssignmentOperator (Lexer.DivPunctuator (Lexer.Str v)), right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator (Lexer.DivPunctuator (Lexer.Str v)), right) ->
            let args = [|
                evalLeftHandSideExpression { state with element = left };
                evalAssignmentExpression { state with element = right };
                (match v with
                | "*=" -> constant (fun (l:dyn) (r:dyn) -> l.Op_Multiplication r)
                | "/=" -> constant (fun (l:dyn) (r:dyn) -> l.Op_Division r)
                | "%=" -> constant (fun (l:dyn) (r:dyn) -> l.Op_Modulus r)
                | "+=" -> constant (fun (l:dyn) (r:dyn) -> l.Op_Addition r)
                | "-=" -> constant (fun (l:dyn) (r:dyn) -> l.Op_Subtraction r)
                | "<<=" -> constant (fun (l:dyn) (r:dyn) -> l.Op_LeftShift r)
                | ">>=" -> constant (fun (l:dyn) (r:dyn) -> l.Op_SignedRightShift r)
                | ">>>=" -> constant (fun (l:dyn) (r:dyn) -> l.Op_UnsignedRightShift r)
                | "&=" -> constant (fun (l:dyn) (r:dyn) -> l.Op_BitwiseAnd r)
                | "^=" -> constant (fun (l:dyn) (r:dyn) -> l.Op_BitwiseXor r)
                | "|=" -> constant (fun (l:dyn) (r:dyn) -> l.Op_BitwiseOr r)
                | _ -> failwith "")
            |]
            constant 1
            //invoke (constant compoundAssignment) args 

    and evalConditionalExpression (state:State) =
        match state.element with
        | ConditionalExpression (left, Nil, Nil) | ConditionalExpressionNoIn (left, Nil, Nil) ->
            evalLogicalORExpression { state with element = left }
        | ConditionalExpression (test, ifTrue, ifFalse) | ConditionalExpressionNoIn (test, ifTrue, ifFalse) ->
            let test = evalLogicalORExpression { state with element = test }
            let ifTrue = evalAssignmentExpression { state with element = ifTrue }
            let ifFalse = evalAssignmentExpression { state with element = ifFalse }
            condition test ifTrue ifFalse
    
    and evalLogicalORExpression (state:State) =
        match state.element with
        | LogicalORExpression (Nil, right) | LogicalORExpressionNoIn (Nil, right) ->
            evalLogicalANDExpression { state with element = right }
        | LogicalORExpression (left, right) | LogicalORExpressionNoIn (left, right) ->
            let inst = evalLogicalORExpression { state with element = left }
            let args = [| evalLogicalANDExpression { state with element = right } |]
            call inst Reflection.IDynamic.op_LogicalOr args

    and evalLogicalANDExpression (state:State) =
        match state.element with
        | LogicalANDExpression (Nil, right) | LogicalANDExpressionNoIn (Nil, right) ->
            evalBitwiseORExpression { state with element = right }
        | LogicalANDExpression (left, right) | LogicalANDExpressionNoIn (left, right) ->
            let inst = evalLogicalANDExpression { state with element = left }
            let args = [| evalBitwiseORExpression { state with element = right } |]
            call inst Reflection.IDynamic.op_LogicalAnd args

    and evalBitwiseORExpression (state:State) =
        match state.element with
        | BitwiseORExpression (Nil, right) | BitwiseORExpressionNoIn (Nil, right) ->
            evalBitwiseXORExpression { state with element = right }
        | BitwiseORExpression (left, right) | BitwiseORExpressionNoIn (left, right) ->
            let inst = evalBitwiseORExpression { state with element = left }
            let args = [| evalBitwiseXORExpression { state with element = right } |]
            call inst Reflection.IDynamic.op_BitwiseOr args

    and evalBitwiseXORExpression (state:State) =
        match state.element with
        | BitwiseXORExpression (Nil, right) | BitwiseXORExpressionNoIn (Nil, right) ->
            evalBitwiseANDExpression { state with element = right }
        | BitwiseXORExpression (left, right) | BitwiseXORExpressionNoIn (left, right) ->
            let inst = evalBitwiseXORExpression { state with element = left }
            let args = [| evalBitwiseANDExpression { state with element = right } |]
            call inst Reflection.IDynamic.op_BitwiseXor args

    and evalBitwiseANDExpression (state:State) =
        match state.element with
        | BitwiseANDExpression (Nil, right) | BitwiseANDExpressionNoIn (Nil, right) ->
            evalEqualityExpression { state with element = right }
        | BitwiseANDExpression (left, right) | BitwiseANDExpressionNoIn (left, right) ->
            let inst = evalBitwiseANDExpression { state with element = left }
            let args = [| evalEqualityExpression { state with element = right } |]
            call inst Reflection.IDynamic.op_BitwiseAnd args

    and evalEqualityExpression (state:State) = 
        match state.element with
        | EqualityExpression (Nil, EqualityOperator.Nil, e) | EqualityExpressionNoIn (Nil, EqualityOperator.Nil, e) ->
            evalRelationalExpression { state with element = e }
        | EqualityExpression (e1, EqualityOperator.Equal, e2) | EqualityExpressionNoIn (e1, EqualityOperator.Equal, e2) ->
            call (evalEqualityExpression { state with element = e1 }) Reflection.IDynamic.op_Equals [| (evalRelationalExpression { state with element = e2 }) |]
        | EqualityExpression (e1, EqualityOperator.DoesNotEqual, e2) | EqualityExpressionNoIn (e1, EqualityOperator.DoesNotEqual, e2) ->
            call (evalEqualityExpression { state with element = e1 }) Reflection.IDynamic.op_DoesNotEquals [| (evalRelationalExpression { state with element = e2 }) |]
        | EqualityExpression (e1, EqualityOperator.StrictEqual, e2) | EqualityExpressionNoIn (e1, EqualityOperator.StrictEqual, e2) ->
            call (evalEqualityExpression { state with element = e1 }) Reflection.IDynamic.op_StrictEquals [| (evalRelationalExpression { state with element = e2 }) |] 
        | EqualityExpression (e1, EqualityOperator.StrictDoesNotEqual, e2) | EqualityExpressionNoIn (e1, EqualityOperator.StrictDoesNotEqual, e2) ->
            call (evalEqualityExpression { state with element = e1 }) Reflection.IDynamic.op_StrictDoesNotEquals [| (evalRelationalExpression { state with element = e2 }) |] 

    and evalRelationalExpression (state:State) =    
        match state.element with
        | RelationalExpression (Nil, RelationalOperator.Nil, e) | RelationalExpressionNoIn (Nil, RelationalOperator.Nil, e) ->
            evalShiftExpression { state with element = e }
        | RelationalExpression (e1, RelationalOperator.LessThan, e2) | RelationalExpressionNoIn (e1, RelationalOperator.LessThan, e2) ->
            call (evalRelationalExpression { state with element = e1 }) Reflection.IDynamic.op_Lessthan [| (evalShiftExpression { state with element = e2 }) |]
        | RelationalExpression (e1, RelationalOperator.GreaterThan, e2) | RelationalExpressionNoIn (e1, RelationalOperator.GreaterThan, e2) ->
            call (evalRelationalExpression { state with element = e1 }) Reflection.IDynamic.op_Greaterthan [| (evalShiftExpression { state with element = e2 }) |]
        | RelationalExpression (e1, RelationalOperator.LessThanOrEqual, e2) | RelationalExpressionNoIn (e1, RelationalOperator.LessThanOrEqual, e2) ->
            call (evalRelationalExpression { state with element = e1 }) Reflection.IDynamic.op_LessthanOrEqual [| (evalShiftExpression { state with element = e2 }) |]
        | RelationalExpression (e1, RelationalOperator.GreaterThanOrEqual, e2) | RelationalExpressionNoIn (e1, RelationalOperator.GreaterThanOrEqual, e2) ->
            call (evalRelationalExpression { state with element = e1 }) Reflection.IDynamic.op_GreaterthanOrEqual [| (evalShiftExpression { state with element = e2 }) |]
        | RelationalExpression (e1, RelationalOperator.Instanceof, e2) | RelationalExpressionNoIn (e1, RelationalOperator.Instanceof, e2) ->
            call (evalRelationalExpression { state with element = e1 }) Reflection.IDynamic.op_Instanceof [| (evalShiftExpression { state with element = e2 }) |]
        | RelationalExpression (e1, RelationalOperator.In, e2) ->
            call (evalRelationalExpression { state with element = e1 }) Reflection.IDynamic.op_In [| (evalShiftExpression { state with element = e2 }) |]

    and evalShiftExpression (state:State) =    
        match state.element with
        | ShiftExpression (Nil, BitwiseShiftOperator.Nil, e) ->
            evalAdditiveExpression { state with element = e }
        | ShiftExpression (e1, BitwiseShiftOperator.LeftShift, e2) ->
            call (evalShiftExpression { state with element = e1 }) Reflection.IDynamic.op_LeftShift [| (evalAdditiveExpression { state with element = e2 }) |]
        | ShiftExpression (e1, BitwiseShiftOperator.SignedRightShift, e2) ->
            call (evalShiftExpression { state with element = e1 }) Reflection.IDynamic.op_SignedRightShift [| (evalAdditiveExpression { state with element = e2 }) |]
        | ShiftExpression (e1, BitwiseShiftOperator.UnsignedRightShift, e2) ->
            call (evalShiftExpression { state with element = e1 }) Reflection.IDynamic.op_UnsignedRightShift [| (evalAdditiveExpression { state with element = e2 }) |]

    and evalAdditiveExpression (state:State) =    
        match state.element with
        | AdditiveExpression (Nil, AdditiveOperator.Nil, e) ->
            evalMultiplicativeExpression { state with element = e }
        | AdditiveExpression (e1, AdditiveOperator.Plus, e2) ->
            call (evalAdditiveExpression { state with element = e1 }) Reflection.IDynamic.op_Addition [| (evalMultiplicativeExpression { state with element = e2 }) |]
        | AdditiveExpression (e1, AdditiveOperator.Minus, e2) ->
            call (evalAdditiveExpression { state with element = e1 }) Reflection.IDynamic.op_Subtraction [| (evalMultiplicativeExpression { state with element = e2 }) |]

    and evalMultiplicativeExpression (state:State) =    
        match state.element with
        | MultiplicativeExpression (Nil, MultiplicativeOperator.Nil, e) ->
            evalUnaryExpression { state with element = e }
        | MultiplicativeExpression (e1, MultiplicativeOperator.Multiply, e2) ->
            call (evalMultiplicativeExpression { state with element = e1 }) Reflection.IDynamic.op_Multiplication [| (evalUnaryExpression { state with element = e2 }) |]
        | MultiplicativeExpression (e1, MultiplicativeOperator.Modulus, e2) ->
            call (evalMultiplicativeExpression { state with element = e1 }) Reflection.IDynamic.op_Modulus [| (evalUnaryExpression { state with element = e2 }) |]
        | MultiplicativeExpression (e1, MultiplicativeOperator.Divide, e2) ->
            call (evalMultiplicativeExpression { state with element = e1 }) Reflection.IDynamic.op_Division [| (evalUnaryExpression { state with element = e2 }) |]

    and evalUnaryExpression (state:State) =    
        match state.element with
        | UnaryExpression (UnaryOperator.Nil, e) ->
            evalPostfixExpression { state with element = e }
        | UnaryExpression (UnaryOperator.Increment, e) ->
            call (evalPostfixExpression { state with element = e }) Reflection.IDynamic.op_PrefixIncrement [||]
        | UnaryExpression (UnaryOperator.Decrement, e) ->
            call (evalPostfixExpression { state with element = e }) Reflection.IDynamic.op_PrefixDecrement [||]
        | UnaryExpression (UnaryOperator.Plus, e) ->
            call (evalPostfixExpression { state with element = e }) Reflection.IDynamic.op_Plus [||]
        | UnaryExpression (UnaryOperator.Minus, e) ->
            call (evalPostfixExpression { state with element = e }) Reflection.IDynamic.op_Minus [||]
        | UnaryExpression (UnaryOperator.BitwiseNot, e) ->
            call (evalPostfixExpression { state with element = e }) Reflection.IDynamic.op_BitwiseNot [||]
        | UnaryExpression (UnaryOperator.LogicalNot, e) ->
            call (evalPostfixExpression { state with element = e }) Reflection.IDynamic.op_LogicalNot [||]
        | UnaryExpression (UnaryOperator.Delete, e) ->
            call (evalPostfixExpression { state with element = e }) Reflection.IDynamic.op_Delete [||]
        | UnaryExpression (UnaryOperator.Void, e) ->
            call (evalPostfixExpression { state with element = e }) Reflection.IDynamic.op_Void [||]
        | UnaryExpression (UnaryOperator.Typeof, e) ->
            call (evalPostfixExpression { state with element = e }) Reflection.IDynamic.op_Typeof [||]

    and evalPostfixExpression (state:State) =    
        match state.element with
        | PostfixExpression (e, PostfixOperator.Nil) ->
            evalLeftHandSideExpression { state with element = e }
        | PostfixExpression (e, PostfixOperator.Increment) ->
            call (evalLeftHandSideExpression { state with element = e }) Reflection.IDynamic.op_PostfixIncrement [||]
        | PostfixExpression (e, PostfixOperator.Decrement) ->
            call (evalLeftHandSideExpression { state with element = e }) Reflection.IDynamic.op_PostfixDecrement [||]

    and evalMemberExpression (state:State) =    
        match state.element with
        | MemberExpression (e, Nil) ->
            match e with
            | PrimaryExpression (_) ->
                evalPrimaryExpression { state with element = e } 
            | FunctionExpression (_, _, _) ->
                evalFunctionExpression { state with element = e } 
        | MemberExpression (e1, e2) ->
            match e1, e2 with
            | MemberExpression (_, _), Expression (_, _) -> constant 1 
            | MemberExpression (_, _), InputElement (e3) -> constant 1  
            | MemberExpression (_, _), Arguments (_) ->
                let left = evalMemberExpression { state with element = e1 }
                let right = [| evalArguments { state with element = e2 } |]
                call left Reflection.IDynamic.op_Construct right 

    and evalArguments (state:State) = constant 1

    and evalArgumentList (state:State) = constant 1

    and evalCallExpression (state:State) = constant 1

    and evalNewExpression (state:State) =
        match state.element with
        | NewExpression e ->
            match e with
            | MemberExpression (_, _) ->
                evalMemberExpression { state with element = e }                
            | NewExpression _ -> 
                evalNewExpression { state with element = e }

    and evalLeftHandSideExpression (state:State) =
        match state.element with
        | LeftHandSideExpression e ->
            match e with
            | NewExpression _ -> 
                evalNewExpression { state with element = e }
            | CallExpression (_, _, _) -> 
                evalCallExpression { state with element = e }

    and evalPrimaryExpression (state:State) =
        match state.element with
        | PrimaryExpression e ->
            match e with
            | InputElement e ->
                match e with
                | Lexer.IdentifierName (_, _) ->
                    let context = call environmentParam Reflection.IEnvironment.get_Context [||]
                    call context Reflection.IExecutionContext.get_ThisBinding [||]
                | Lexer.Identifier _ ->
                    evalIdentifier e
                | Lexer.Literal e ->
                    match e with
                    | Lexer.NullLiteral "null" ->
                        call environmentParam Reflection.IEnvironment.get_Null Array.empty
                    | Lexer.BooleanLiteral "true" ->
                        call environmentParam Reflection.IEnvironment.createBoolean [| constant true |]
                    | Lexer.BooleanLiteral "false" ->
                        call environmentParam Reflection.IEnvironment.createBoolean [| constant false |]
                    | Lexer.NumericLiteral _ ->
                        call environmentParam Reflection.IEnvironment.createNumber [| constant (Lexer.NumericLiteralParser.evalNumericLiteral e) |]
                    | Lexer.StringLiteral _ ->
                        call environmentParam Reflection.IEnvironment.createString [| constant (Lexer.StringLiteralParser.evalStringLiteral e) |]
                    | Lexer.RegularExpressionLiteral (_, _, _, _) ->
                        constant 1
            | ArrayLiteral (_, _) ->
                evalArrayLiteral { state with element = e }    
            | ObjectLiteral _ ->
                evalObjectLiteral { state with element = e }    
            | Expression (_, _) ->
                evalExpression { state with element = e }  

    and evalObjectLiteral (state:State) = constant 1

    and evalPropertyNameAndValueList (state:State) = constant 1

    and evalPropertyAssignment (state:State) = constant 1

    and evalPropertyName (state:State) = constant 1

    and evalPropertySetParameterList (state:State) = constant 1

    and evalArrayLiteral (state:State) =
        match state.element with
        | ArrayLiteral (Nil, e) ->
            let var = exp.Variable(Reflection.IObject.t, "array")
            let construct = call environmentParam Reflection.IEnvironment.constructArray Array.empty
            let assign = exp.Assign (var, construct) :> exp
            let pad = call environmentParam Reflection.IEnvironment.createNumber [| constant (evalElision { state with element = e }) |]
            let args = [| constant "length"; pad; constant false  |]
            let put = call var Reflection.IObject.put args
            exp.Block ([|var|], [|assign; put|]) :> exp
        | ArrayLiteral (e, Nil) ->
            evalElementList { state with element = e }
        | ArrayLiteral (e1, e2) ->
            let var = exp.Variable(Reflection.IObject.t, "array")
            let assign = exp.Assign (var, evalElementList { state with element = e1 }) :> exp
            let pad = call environmentParam Reflection.IEnvironment.createNumber [| constant (evalElision { state with element = e2 }) |]
            let get = call var Reflection.IObject.get [| constant "length" |] 
            let value = call (call pad Reflection.IDynamic.op_Addition [| get |]) Reflection.IDynamic.convertToUInt32 Array.empty
            let put = call var Reflection.IObject.put [| constant "length"; value; constant false  |]
            exp.Block ([|var|], [|assign; put|]) :> exp

    and evalElision (state:State) =
        match state.element with
        | Elision (Nil) -> 
            1.0
        | Elision (e) -> 
            evalElision { state with element = e } + 1.0

    and evalElementList (state:State) = constant 1

    and evalStatement (state:State) =
        match state.element with
        | Statement e ->
            match e with
            | ExpressionStatement _ ->
                evalExpressionStatement { state with element = e } 
            | EmptyStatement -> 
                evalEmptyStatement { state with element = e }  
            | VariableStatement _ ->
                evalVariableStatement { state with element = e } 

    and evalBlock (state:State) =
        match state.element with
        | Block Nil ->
            exp.Empty() :> exp
        | Block e ->
            evalStatementList { state with element = e }    

    and evalStatementList (state:State) = constant 1

    and evalVariableStatement (state:State) =
        match state.element with
        | VariableStatement e ->
            evalVariableDeclarationList { state with element = e } 

    and evalVariableDeclarationList (state:State) =
        match state.element with
        | VariableDeclarationList (Nil, e) ->
            evalVariableDeclaration { state with element = e } 
        | VariableDeclarationList (e1, e2) ->
            block [|
                evalVariableDeclarationList { state with element = e1 }
                evalVariableDeclaration { state with element = e2 }
            |]

    and evalVariableDeclarationListNoIn (state:State) = constant 1

    and evalVariableDeclaration (state:State) =
        match state.element with
        | VariableDeclaration (Lexer.Identifier e, Nil) ->
            exp.Empty():>exp
        | VariableDeclaration (e1, e2) ->
            //let identifier = Lexer.IdentifierNameParser.evalIdentifierName e1
            let left = evalIdentifier e1
            let right = evalInitialiser { state with element = e2 }
            call left Reflection.IDynamic.set_Value [| right |]

    and evalVariableDeclarationNoIn (state:State) = constant 1

    and evalInitialiser (state:State) =
        match state.element with
        | Initialiser e ->
            evalAssignmentExpression { state with element = e }

    and evalInitialiserNoIn (state:State) = constant 1

    and evalEmptyStatement (state:State) = constant 1

    and evalExpressionStatement (state:State) =
        match state.element with
        | ExpressionStatement e ->
            evalExpression { state with element = e } 

    and evalIfStatement (state:State) = constant 1

    and evalIterationStatement (state:State) = constant 1

    and evalContinueStatement (state:State) = constant 1

    and evalBreakStatement (state:State) = constant 1

    and evalReturnStatement (state:State) = constant 1

    and evalWithStatement (state:State) = constant 1

    and evalSwitchStatement (state:State) = constant 1

    and evalCaseBlock (state:State) = constant 1

    and evalCaseClauses (state:State) = constant 1

    and evalCaseClause (state:State) = constant 1

    and evalDefaultClause (state:State) = constant 1

    and evalLabelledStatement (state:State) = constant 1

    and evalThrowStatement (state:State) = constant 1

    and evalTryStatement (state:State) = constant 1

    and evalCatch (state:State) = constant 1

    and evalFinally (state:State) = constant 1

    and evalDebuggerStatement (state:State) = constant 1

    and evalFunctionDeclaration (state:State) = constant 1

    and evalFunctionExpression (state:State) = constant 1

    and evalFormalParameterList (state:State) = constant 1

    and evalFunctionBody (state:State) = constant 1

    and evalSourceElement (state:State) =
        match state.element with
        | SourceElement e ->
            match e with
            | Statement _ ->
                evalStatement { state with element = e } 
            | FunctionDeclaration (_, _, _) ->
                evalFunctionDeclaration { state with element = e } 

    and evalSourceElements (state:State) =
        match state.element with
        | SourceElements (Nil, e) ->
            evalSourceElement { state with element = e } 
        | SourceElements (e1, e2) ->
            let e1 = evalSourceElements { state with element = e1 } 
            let e2 = evalSourceElement { state with element = e2 } 
            block [|e1;e2|]          

    and evalProgram (state:State) =
        match state.element with
        | Program e ->
            match e with
            | Nil ->
                exp.Empty() :> exp
            | SourceElements (_, _) ->
                evalSourceElements { state with element = e }    

    member this.Compile (input:string) =
        let input = Parser.parse (input + ";")
        let state = { element = input; labels = []; functions = []; variables = [] }
        let body = evalProgram state
        if body.Type = typeof<System.Void> then
            exp.Lambda<Machete.Interfaces.Code>(block [| body; call environmentParam Reflection.IEnvironment.get_Undefined [||] |], [| environmentParam; argsParam |]).Compile()    
        else    
            exp.Lambda<Machete.Interfaces.Code>(body, [| environmentParam; argsParam |]).Compile()

    member this.Compile (input:SourceElement) =
        let state = { element = input; labels = []; functions = []; variables = [] }
        let body = 
            match input with
            | FunctionDeclaration (_, _, _) ->
                evalFunctionDeclaration state
            | FunctionExpression (_, _, _) ->
                evalFunctionExpression state
        exp.Lambda<Machete.Interfaces.Code>(body, [| environmentParam; argsParam |]).Compile()


