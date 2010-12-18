namespace Machete.Compiler

open System
open System.Linq.Expressions
open Machete
open Machete.Interfaces

type internal exp = System.Linq.Expressions.Expression
type internal label = System.Linq.Expressions.LabelExpression
type internal methodinfo = System.Reflection.MethodInfo
type internal dyn = Machete.Interfaces.IDynamic

type internal State = {
    strict : bool
    element : SourceElement
    labels : list<Map<string, label>>
    functions : list<string * string[] * SourceElement>
    variables : list<string>
    returnExpression : exp
} 

type internal PropertyType =
| DataProperty
| GetProperty
| SetProperty

type Compiler(environment:IEnvironment) =



    let call (e:exp) (m:methodinfo) (a:exp[]) = exp.Call (e, m, a) :> exp
    let assign (left:exp) (right:exp) = exp.Assign (left, right) :> exp
    let invoke (e:exp) (a:exp[]) = exp.Invoke (e, a) :> exp
    let condition test ifTrue ifFalse = exp.Condition (test, ifTrue, ifFalse) :> exp
    let constant v = exp.Constant v :> exp 
    let block es = exp.Block es :> exp      
    
    let environmentParam = exp.Parameter (Reflection.IEnvironment.t, "environment")
    let argsParam = exp.Parameter (Reflection.IArgs.t, "args")

    let getUndefined = call environmentParam Reflection.IEnvironment.get_Undefined [||]


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
        | Expression (Nil, e) | ExpressionNoIn (Nil, e) ->
            let r : exp = evalAssignmentExpression { state with element = e }
            call r Reflection.IDynamic.get_Value Array.empty
        | Expression (left, right) | ExpressionNoIn (left, right) ->
            let left = evalExpression { state with element = left }
            let r = evalAssignmentExpression { state with element = right }
            block [| left; call r Reflection.IDynamic.get_Value Array.empty |]
            
    and evalAssignmentExpression (state:State) =
        let simpleAssignment left right =
            let left = evalLeftHandSideExpression { state with element = left }
            let right = evalAssignmentExpression { state with element = right }
            let valueVar = exp.Variable(Reflection.IDynamic.t, "value") 
            let assign = exp.Assign(valueVar, call right Reflection.IDynamic.get_Value [| |]) :>exp
            let setLeft = call left Reflection.IDynamic.set_Value [| valueVar |]
            let variables = [| valueVar |]
            let body = [| assign; setLeft; valueVar:>exp |]
            exp.Block (variables, body) :> exp
        let compoundAssignment left right op =
            let left = evalLeftHandSideExpression { state with element = left }
            let right = evalAssignmentExpression { state with element = right }
            let getLeft = call left Reflection.IDynamic.get_Value Array.empty
            let getRight = call right Reflection.IDynamic.get_Value Array.empty
            let performOp = call left op [| right |]
            let valueVar = exp.Variable(Reflection.IDynamic.t, "value") 
            let assign = exp.Assign(valueVar, performOp) :> exp
            let setLeft = call left Reflection.IDynamic.set_Value [| assign |]
            let variables = [| valueVar |]
            let body = [| setLeft; valueVar:>exp |]
            exp.Block (variables, body) :> exp
        match state.element with
        | AssignmentExpression (left, AssignmentOperator.Nil, Nil) 
        | AssignmentExpressionNoIn (left, AssignmentOperator.Nil, Nil) ->
            evalConditionalExpression { state with element = left }
        | AssignmentExpression (left, AssignmentOperator.SimpleAssignment, right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator.SimpleAssignment, right) ->
            simpleAssignment left right
        | AssignmentExpression (left, AssignmentOperator.MultiplyAssignment, right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator.MultiplyAssignment, right) ->
            compoundAssignment left right Reflection.IDynamic.op_Multiplication
        | AssignmentExpression (left, AssignmentOperator.DivideAssignment, right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator.DivideAssignment, right) ->
            compoundAssignment left right Reflection.IDynamic.op_Division
        | AssignmentExpression (left, AssignmentOperator.ModulusAssignment, right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator.ModulusAssignment, right) ->
            compoundAssignment left right Reflection.IDynamic.op_Modulus
        | AssignmentExpression (left, AssignmentOperator.PlusAssignment, right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator.PlusAssignment, right) ->
            compoundAssignment left right Reflection.IDynamic.op_Addition
        | AssignmentExpression (left, AssignmentOperator.MinusAssignment, right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator.MinusAssignment, right) ->
            compoundAssignment left right Reflection.IDynamic.op_Subtraction
        | AssignmentExpression (left, AssignmentOperator.LeftShiftAssignment, right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator.LeftShiftAssignment, right) ->
            compoundAssignment left right Reflection.IDynamic.op_LeftShift
        | AssignmentExpression (left, AssignmentOperator.SignedRightShiftAssignment, right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator.SignedRightShiftAssignment, right) ->
            compoundAssignment left right Reflection.IDynamic.op_SignedRightShift
        | AssignmentExpression (left, AssignmentOperator.UnsignedRightShiftAssignment, right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator.UnsignedRightShiftAssignment, right) ->
            compoundAssignment left right Reflection.IDynamic.op_UnsignedRightShift
        | AssignmentExpression (left, AssignmentOperator.BitwiseAndAssignment, right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator.BitwiseAndAssignment, right) ->
            compoundAssignment left right Reflection.IDynamic.op_BitwiseAnd
        | AssignmentExpression (left, AssignmentOperator.BitwiseXorAssignment, right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator.BitwiseXorAssignment, right) ->
            compoundAssignment left right Reflection.IDynamic.op_BitwiseXor
        | AssignmentExpression (left, AssignmentOperator.BitwiseOrAssignment, right) 
        | AssignmentExpressionNoIn (left, AssignmentOperator.BitwiseOrAssignment, right) ->
            compoundAssignment left right Reflection.IDynamic.op_BitwiseOr

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
        | MemberExpression (Nil, e) ->
            match e with
            | PrimaryExpression (_) ->
                evalPrimaryExpression { state with element = e } 
            | FunctionExpression (_, _, _) ->
                evalFunctionExpression { state with element = e } 
        | MemberExpression (e1, e2) ->
            match e1, e2 with
            | MemberExpression (_, _), Expression (_, _) ->
                let baseValue = evalMemberExpression { state with element = e1 }
                let baseValue = call baseValue Reflection.IDynamic.get_Value Array.empty
                let baseValue = exp.Convert(baseValue, Reflection.IReferenceBase.t) :> exp
                let name = evalExpression { state with element = e2 }
                let name = call name Reflection.IDynamic.convertToString Array.empty
                let name = exp.Convert (name, Reflection.IString.t) :> exp
                let name = call name Reflection.IString.get_BaseValue Array.empty
                let args = [| name; baseValue; constant state.strict |]
                call environmentParam Reflection.IEnvironment.createReference args 
            | MemberExpression (_, _), InputElement (e3) ->
                let baseValue = evalMemberExpression { state with element = e1 }
                let baseValue = call baseValue Reflection.IDynamic.get_Value Array.empty
                let baseValue = exp.Convert(baseValue, Reflection.IReferenceBase.t) :> exp
                let name = constant (Lexer.IdentifierNameParser.evalIdentifierName e3)
                let args = [| name; baseValue; constant state.strict  |]
                call environmentParam Reflection.IEnvironment.createReference args 
            | MemberExpression (_, _), Arguments (_) ->
                let left = evalMemberExpression { state with element = e1 }
                let right = [| evalArguments { state with element = e2 } |]
                call left Reflection.IDynamic.op_Construct right 

    and evalArguments (state:State) =
        match state.element with
        | Arguments Nil -> 
            call environmentParam Reflection.IEnvironment.get_EmptyArgs Array.empty
        | Arguments e ->
            evalArgumentList { state with element = e }

    and evalArgumentList (state:State) =
        match state.element with
        | ArgumentList (Nil, e) -> 
            call environmentParam Reflection.IEnvironment.createArgsSingle [| evalAssignmentExpression { state with element = e } |]
        | ArgumentList (e1, e2) ->
            let first = evalArgumentList { state with element = e1 }
            let last = call environmentParam Reflection.IEnvironment.createArgsSingle [| evalAssignmentExpression { state with element = e2 } |]
            call environmentParam Reflection.IEnvironment.concatArgs [| first; last |]

    and evalCallExpression (state:State) =    
        match state.element with
        | CallExpression (e1, e2) ->
            match e1, e2 with
            | MemberExpression (_, _), Arguments (_) ->
                let left = evalMemberExpression { state with element = e1 }
                let right = [| evalArguments { state with element = e2 } |]
                call left Reflection.IDynamic.op_Call right 
            | CallExpression (_, _), Arguments (_) ->
                let left = evalCallExpression { state with element = e1 }
                let right = [| evalArguments { state with element = e2 } |]
                call left Reflection.IDynamic.op_Call right  
            | CallExpression (_, _), Expression (_, _) ->                
                let baseValue = evalCallExpression { state with element = e1 }
                let baseValue = call baseValue Reflection.IDynamic.get_Value Array.empty
                let baseValue = exp.Convert(baseValue, Reflection.IReferenceBase.t) :> exp
                let name = evalExpression { state with element = e2 }
                let name = call name Reflection.IDynamic.convertToString Array.empty
                let name = exp.Convert (name, Reflection.IString.t) :> exp
                let name = call name Reflection.IString.get_BaseValue Array.empty
                let args = [| name; baseValue; constant state.strict |]
                call environmentParam Reflection.IEnvironment.createReference args 
            | CallExpression (_, _), InputElement (e3) ->            
                let baseValue = evalMemberExpression { state with element = e1 }
                let baseValue = call baseValue Reflection.IDynamic.get_Value Array.empty
                let baseValue = exp.Convert(baseValue, Reflection.IReferenceBase.t) :> exp
                let name = constant (Lexer.IdentifierNameParser.evalIdentifierName e3)
                let args = [| name; baseValue; constant state.strict  |]
                call environmentParam Reflection.IEnvironment.createReference args 

    and evalNewExpression (state:State) =
        match state.element with
        | NewExpression e ->
            match e with
            | MemberExpression (_, _) ->
                evalMemberExpression { state with element = e }                
            | NewExpression _ -> 
                let left = evalNewExpression { state with element = e }
                let right = [| call environmentParam Reflection.IEnvironment.get_EmptyArgs Array.empty |]
                call left Reflection.IDynamic.op_Construct right 

    and evalLeftHandSideExpression (state:State) =
        match state.element with
        | LeftHandSideExpression e ->
            match e with
            | NewExpression _ -> 
                evalNewExpression { state with element = e }
            | CallExpression (_, _) -> 
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

    and evalObjectLiteral (state:State) =
        let objectVar = exp.Variable(typeof<IObject>, "object")
        let variables = [| objectVar |]
        let assignObject = assign objectVar (call environmentParam Reflection.IEnvironment.createObject Array.empty)
        match state.element with
        | ObjectLiteral Nil ->
            exp.Block(variables, [| assignObject; objectVar:>exp |]) :> exp
        | ObjectLiteral e ->
            let result = evalPropertyNameAndValueList objectVar [] { state with element = e }
            let byName = result |> Seq.groupBy (fun (n, t, e) -> n) |> Seq.map (fun (key, values) -> key, values |> Seq.toList) |> Map.ofSeq
            for kv in byName do
                let dCount = kv.Value |> List.filter (fun (n, t, e) -> match t with | DataProperty -> true | _ -> false) |> List.length
                let gCount = kv.Value |> List.filter (fun (n, t, e) -> match t with | GetProperty -> true | _ -> false) |> List.length
                let sCount = kv.Value |> List.filter (fun (n, t, e) -> match t with | SetProperty -> true | _ -> false) |> List.length
                match dCount, gCount, sCount with
                | 1, 0, 0 | 0, 1, 0 | 0, 0, 1 | 0, 1, 1 -> ()
                | _ -> environment.SyntaxErrorConstructor.Op_Construct(environment.EmptyArgs).Op_Throw()    
            let result = (assignObject::((objectVar:>exp::(result |> List.map (fun (n, t, e) -> e))) |> List.rev)) |> List.toArray     
            exp.Block(variables, result) :> exp


    and evalPropertyNameAndValueList (objectVar:ParameterExpression) (results:list<string * PropertyType * exp>) (state:State) =
        match state.element with
        | PropertyNameAndValueList (Nil, e1) ->
            let name, propType, desc = evalPropertyAssignment { state with element = e1 } 
            let define = call objectVar Reflection.IObject.defineOwnProperty [| constant name; desc; constant false |]
            (name, propType, define)::results
        | PropertyNameAndValueList (e1, e2) ->
            let results = evalPropertyNameAndValueList objectVar results { state with element = e1 } 
            let name, propType, desc = evalPropertyAssignment { state with element = e2 } 
            let define = call objectVar Reflection.IObject.defineOwnProperty [| constant name; desc; constant false |]
            (name, propType, define)::results

    and evalPropertyAssignment (state:State) =
        let nullTrue = exp.Convert(constant true, typeof<Nullable<bool>>)
        match state.element with
        | PropertyAssignment (e1, SourceElement.Nil, e2) ->
            let name = evalPropertyName { state with element = e1 } 
            match e2 with
            | AssignmentExpression (_, _, _) ->
                let v = evalAssignmentExpression { state with element = e2 } 
                let getValue = call v Reflection.IDynamic.get_Value Array.empty
                let createDesc = call environmentParam Reflection.IEnvironment.createDataDescriptor4 [| getValue; nullTrue; nullTrue; nullTrue  |]
                name, DataProperty, createDesc
            | FunctionBody _ ->                
                let code = lazy(compileFunctionCode(Array.empty, e2))
                let args = [| constant Array.empty<string>; constant state.strict; constant code |]  
                let createFunction = call environmentParam Reflection.IEnvironment.createFunction args
                let createDesc = call environmentParam Reflection.IEnvironment.createAccessorDescriptor3 [| createFunction; constant null; nullTrue; nullTrue  |]
                name, GetProperty, createDesc      
        | PropertyAssignment (e1, e2, e3) ->
            let name = evalPropertyName { state with element = e1 } 
            let setParams =  evalPropertySetParameterList { state with element = e2 }                 
            let code = lazy(compileFunctionCode(Array.empty, e2))
            let args = [| constant setParams; constant state.strict; constant code |]  
            let createFunction = call environmentParam Reflection.IEnvironment.createFunction args
            let createDesc = call environmentParam Reflection.IEnvironment.createAccessorDescriptor3 [| constant null; createFunction; nullTrue; nullTrue  |]
            name, SetProperty, createDesc 
             
    and evalPropertyName (state:State) =
        match state.element with
        | PropertyName e ->
            match e with
            | Lexer.IdentifierName (_, _) ->
                Lexer.IdentifierNameParser.evalIdentifierName e
            | Lexer.StringLiteral _ ->
                Lexer.StringLiteralParser.evalStringLiteral e
            | Lexer.NumericLiteral _ ->
                Lexer.NumericLiteralParser.evalNumericLiteral e |> string

    and evalPropertySetParameterList (state:State) =
        match state.element with
        | PropertySetParameterList e ->
            [| Lexer.IdentifierNameParser.evalIdentifierName e |]

    and evalArrayLiteral (state:State) =
        let arrayVar = exp.Variable(typeof<IObject>, "array")
        let lengthVar = exp.Variable(typeof<IPropertyDescriptor>, "length")
        let variables = [| arrayVar; lengthVar |]
        let assignArray = assign arrayVar (call environmentParam Reflection.IEnvironment.createArray Array.empty)
        let assignLength = assign lengthVar (call arrayVar Reflection.IObject.getOwnProperty [| constant "length" |]) 
        match state.element with
        | ArrayLiteral (Nil, e) ->
            let pad = call environmentParam Reflection.IEnvironment.createNumber [| constant (evalElision { state with element = e }) |]
            let setValue = call lengthVar Reflection.IPropertyDescriptor.set_Value [| pad |]
            exp.Block (variables, [|assignArray; assignLength; setValue; arrayVar:>exp|]) :> exp
        | ArrayLiteral (e, Nil) ->
            let result = assignArray::assignLength::(arrayVar:>exp::(evalElementList arrayVar lengthVar [] { state with element = e }) |> List.rev)
            let result = result |> List.toArray
            exp.Block(variables, result) :> exp
        | ArrayLiteral (e1, e2) ->
            let result = arrayVar:>exp::evalElementList arrayVar lengthVar []  { state with element = e1 } 
            let pad = call environmentParam Reflection.IEnvironment.createNumber [| constant (evalElision { state with element = e2 }) |]
            let len = call lengthVar Reflection.IPropertyDescriptor.get_Value Array.empty
            let value = call (call len Reflection.IDynamic.op_Addition [| pad |]) Reflection.IDynamic.convertToUInt32 Array.empty
            let setValue = call lengthVar Reflection.IPropertyDescriptor.set_Value [| value |]
            let result = (setValue::result) |> List.rev
            let result = assignArray::assignLength::result |> List.toArray
            exp.Block(variables, result) :> exp

    and evalElision (state:State) =
        match state.element with
        | Elision (Nil) -> 1.0
        | Elision (e) -> evalElision { state with element = e } + 1.0

    and evalElementList (arrayVar:ParameterExpression) (lengthVar:ParameterExpression) (results:list<exp>) (state:State) =
        match state.element with
        | ElementList (Nil, e1, e2) ->
            let initResult = evalAssignmentExpression { state with element = e2 }
            let initValue = call initResult Reflection.IDynamic.get_Value Array.empty
            let firstIndex = match e1 with | Nil -> 0.0 | Elision (_) -> evalElision { state with element = e1 }
            let nullTrue = exp.Convert(constant true, typeof<Nullable<bool>>)
            let createDesc = call environmentParam Reflection.IEnvironment.createDataDescriptor4 [| initValue; nullTrue; nullTrue; nullTrue  |]
            let createProp = call arrayVar Reflection.IObject.defineOwnProperty [| constant (firstIndex|>string); createDesc; constant false |]
            createProp::results
        | ElementList (e1, e2, e3) ->
            let results = evalElementList arrayVar lengthVar results { state with element = e1 }
            let initResult = evalAssignmentExpression { state with element = e3 }
            let initValue = call initResult Reflection.IDynamic.get_Value Array.empty
            let pad = match e2 with | Nil -> 0.0 | Elision (_) -> evalElision { state with element = e2 }
            let createPad = call environmentParam Reflection.IEnvironment.createNumber [| constant pad |]
            let len = call lengthVar Reflection.IPropertyDescriptor.get_Value Array.empty
            let add = call len Reflection.IDynamic.op_Addition [| createPad |]
            let toUInt32 = call add Reflection.IDynamic.convertToUInt32 Array.empty
            let toString = call toUInt32 Reflection.IDynamic.convertToString Array.empty
            let name = call (exp.Convert (toString, typeof<IString>)) Reflection.IString.get_BaseValue Array.empty
            let nullTrue = exp.Convert(constant true, typeof<Nullable<bool>>)
            let createDesc = call environmentParam Reflection.IEnvironment.createDataDescriptor4 [| initValue; nullTrue; nullTrue; nullTrue  |]
            let createProp = call arrayVar Reflection.IObject.defineOwnProperty [| name; createDesc; constant false |]
            createProp::results

    and evalStatement (state:State) =
        match state.element with
        | Statement e ->
            match e with
            | ExpressionStatement _ ->
                evalExpressionStatement { state with element = e }, state
            | EmptyStatement -> 
                evalEmptyStatement { state with element = e }, state  
            | VariableStatement _ ->
                evalVariableStatement { state with element = e } 
            | ReturnStatement _ ->
                evalReturnStatement { state with element = e } 

    and evalBlock (state:State) =
        match state.element with
        | Block Nil ->
            exp.Empty() :> exp, state
        | Block e ->
            evalStatementList { state with element = e }  

    and evalStatementList (state:State) = constant 1, state

    and evalVariableStatement (state:State) =
        match state.element with
        | VariableStatement e ->
            evalVariableDeclarationList { state with element = e } 

    and evalVariableDeclarationList (state:State) =
        match state.element with
        | VariableDeclarationList (Nil, e) ->
            evalVariableDeclaration { state with element = e } 
        | VariableDeclarationList (e1, e2) ->
            let first, state = evalVariableDeclarationList { state with element = e1 }
            let second, state = evalVariableDeclaration { state with element = e2 }
            block [| first; second |], state

    and evalVariableDeclarationListNoIn (state:State) = constant 1

    and evalVariableDeclaration (state:State) =
        match state.element with
        | VariableDeclaration (Lexer.Identifier e, Nil) ->
            let identifier = Lexer.IdentifierNameParser.evalIdentifierName e
            exp.Empty() :> exp, { state with variables = identifier::state.variables }
        | VariableDeclaration (e1, e2) ->
            let identifier = 
                match e1 with
                | Lexer.Identifier e ->
                    Lexer.IdentifierNameParser.evalIdentifierName e
            let left = evalIdentifier e1
            let right = evalInitialiser { state with element = e2 }
            call left Reflection.IDynamic.set_Value [| right |], { state with variables = identifier::state.variables }

    and evalVariableDeclarationNoIn (state:State) = constant 1

    and evalInitialiser (state:State) =
        match state.element with
        | Initialiser e ->
            evalAssignmentExpression { state with element = e }

    and evalInitialiserNoIn (state:State) = constant 1

    and evalEmptyStatement (state:State) =
        exp.Empty() :> exp

    and evalExpressionStatement (state:State) =
        match state.element with
        | ExpressionStatement e ->
            evalExpression { state with element = e } 

    and evalIfStatement (state:State) = constant 1

    and evalIterationStatement (state:State) = constant 1

    and evalContinueStatement (state:State) = constant 1

    and evalBreakStatement (state:State) = constant 1

    and evalReturnStatement (state:State) =
        let target = state.labels.Head.["return"].Target
        match state.element with
        | ReturnStatement Nil -> 
            exp.Return (target, getUndefined, typeof<dyn>) :> exp, state
        | ReturnStatement e -> 
            let r = evalExpression { state with element = e }
            exp.Return (target, r, typeof<dyn>) :> exp, state

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

    and evalFunctionDeclaration (state:State) =
        match state.element with
        | FunctionDeclaration (Lexer.Identifier e1, e2, e3) ->
            let identifier = Lexer.IdentifierNameParser.evalIdentifierName e1
            let formalParameterList = evalFormalParameterList { state with element = e2 }
            getUndefined, { state with functions = (identifier, formalParameterList, e3)::state.functions }

    and evalFunctionExpression (state:State) = constant 1

    and evalFormalParameterList (state:State) = Array.empty

    and evalFunctionBody (state:State) =
        let target = state.labels.Head.["return"].Target
        match state.element with
        | FunctionBody Nil -> 
            exp.Return (target, getUndefined, typeof<dyn>) :> exp, state
        | FunctionBody e -> 
            evalSourceElements { state with element = e }


    and evalSourceElement (state:State) =
        match state.element with
        | SourceElement e ->
            match e with
            | Statement _ ->
                let e, s = evalStatement { state with element = e } 
                if e.Type <> typeof<System.Void> then e, { s with returnExpression = e } else e, s
            | FunctionDeclaration (_, _, _) ->
                evalFunctionDeclaration { state with element = e }

    and evalSourceElements (state:State) : exp * State =
        match state.element with
        | SourceElements (Nil, e) ->
            evalSourceElement { state with element = e }
        | SourceElements (e1, e2) ->
            let e1, state = evalSourceElements { state with element = e1 } 
            let e2, state = evalSourceElement { state with element = e2 } 
            block [|e1;e2|] , state         

    and evalProgram (state:State) =
        match state.element with
        | Program e ->
            match e with
            | Nil ->
                exp.Empty() :> exp , state 
            | SourceElements (_, _) ->
                evalSourceElements { state with element = e }
                        
    and performDeclarationBinding (configurableBindings:bool) (state:State) (continuation:Code) (environment:IEnvironment) (args:IArgs) =
        let env = environment.Context.VariableEnviroment.Record
        for name in state.variables |> List.rev do
            if not (env.HasBinding name) then
                env.CreateMutableBinding (name, configurableBindings)
                env.SetMutableBinding (name, environment.Undefined, state.strict)
        for name, formalParameterList, functionBody in state.functions do
            let code = lazy(compileFunctionCode(formalParameterList, functionBody))
            let fo = environment.CreateFunction(formalParameterList, state.strict, code)
            if not (env.HasBinding name) then
                env.CreateMutableBinding (name, configurableBindings)
            env.SetMutableBinding (name, fo, state.strict)            
        let r = continuation.Invoke (environment, args)
        r
  
    and performFunctionArgumentsBinding (formalParameterList:string[]) (state:State) (continuation:Code) (environment:IEnvironment) (args:IArgs) =
        let i = ref -1
        let env = environment.Context.VariableEnviroment.Record
        for name in formalParameterList do
            incr i
            if not (env.HasBinding name) then
                env.CreateMutableBinding (name, false)
            env.SetMutableBinding (name, args.[!i], state.strict)                   
        continuation.Invoke (environment, args)           

    and compileGlobalCode (input:string) =
        let input = Parser.parse (input + ";")
        let returnLabel = exp.Label(exp.Label(typeof<dyn>, "return"), getUndefined)
        let body, state = evalProgram { strict = false; element = input; labels = [ Map.ofList [ ("return", returnLabel) ] ]; functions = []; variables = []; returnExpression = getUndefined }
        let continuation = exp.Lambda<Code>(block [| body; exp.Return(returnLabel.Target, state.returnExpression); returnLabel |], [| environmentParam; argsParam |]).Compile()
        Code(performDeclarationBinding true state continuation)
        
    and compileEvalCode (input:string) =
        let input = Parser.parse (input + ";")
        let returnLabel = exp.Label(exp.Label(typeof<dyn>, "return"), getUndefined)
        let body, state = evalProgram { strict = false; element = input; labels =  [ Map.ofList [ ("return", returnLabel) ] ]; functions = []; variables = []; returnExpression = getUndefined }
        let continuation = exp.Lambda<Code>(block [| body; returnLabel |], [| environmentParam; argsParam |]).Compile()
        Code(performDeclarationBinding false state continuation)

    and compileFunctionCode (formalParameterList:string[], functionBody:SourceElement) =
        let returnLabel = exp.Label(exp.Label(typeof<dyn>, "return"), getUndefined)
        let state = { strict = false; element = functionBody; labels = [ Map.ofList [ ("return", returnLabel) ] ]; functions = []; variables = []; returnExpression = getUndefined }
        let body, state = evalFunctionBody state
        let continuation = exp.Lambda<Code>(block [| body; returnLabel |], [| environmentParam; argsParam |]).Compile()
        let continuation = Code(performFunctionArgumentsBinding formalParameterList state continuation)
        Code(performDeclarationBinding true state continuation)

    member this.CompileGlobalCode (input:string) =
        compileGlobalCode input

    member this.CompileEvalCode (input:string) =
        compileEvalCode input

    member this.CompileFunctionCode (formalParameterList:string[], functionBody:SourceElement) =
        compileFunctionCode (formalParameterList, functionBody)
