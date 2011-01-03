namespace Machete.Compiler

open System
open System.Collections
open System.Collections.Generic
open System.Reflection
open Machete
open Machete.Interfaces
open Tools.TreeTraverser


type internal exp = System.Linq.Expressions.Expression
type internal label = System.Linq.Expressions.LabelExpression
type internal methodinfo = System.Reflection.MethodInfo
type internal dyn = Machete.Interfaces.IDynamic

type internal Function = {
    identifier : string
    formalParameterList : ReadOnlyList<string>
    strict : bool
    functionBody : SourceElement 
}

type internal State = {
    strict : bool
    element : SourceElement
    labels : list<Map<string, label>>
    functions : list<Function>
    variables : list<string>
} 

type internal PropertyType =
| DataProperty
| GetProperty
| SetProperty


type Compiler(environment:IEnvironment) as this =


    let call (e:exp) (m:methodinfo) (a:exp[]) = exp.Call (e, m, a) :> exp
    let assign (left:exp) (right:exp) = exp.Assign (left, right) :> exp
    let invoke (e:exp) (a:exp[]) = exp.Invoke (e, a) :> exp
    let condition test ifTrue ifFalse = exp.Condition (test, ifTrue, ifFalse) :> exp
    let constant v = exp.Constant v :> exp 
    let block es = exp.Block es :> exp  
    let empty = exp.Empty() :> exp    
    
    let environmentParam = exp.Parameter (Reflection.IEnvironment.t, "environment")
    let argsParam = exp.Parameter (Reflection.IArgs.t, "args")

    let getContext = call environmentParam Reflection.IEnvironment.get_Context Array.empty
    let getUndefined = call environmentParam Reflection.IEnvironment.get_Undefined Array.empty   

    let convertToBool (e:exp) =          
        let e = call e Reflection.IDynamic.convertToBoolean Array.empty
        call e Reflection.IBoolean.get_BaseValue Array.empty

    let equalityTestMethod = this.GetType().GetMethod ("equalityTest", BindingFlags.Static ||| BindingFlags.NonPublic)


    let isStrictCode e =
        let isStrictCode = traverse {
            do! function | SourceElement e -> Some e | _ -> None
            do! function | Statement e -> Some e | _ -> None
            do! function | ExpressionStatement e -> Some e | _ -> None
            do! function | Expression (Nil, e) -> Some e | _ -> None
            do! function | AssignmentExpression (e, AssignmentOperator.Nil, Nil) -> Some e | _ -> None
            do! function | ConditionalExpression (e, Nil, Nil) -> Some e | _ -> None            
            do! function | LogicalORExpression (Nil, e) -> Some e | _ -> None
            do! function | LogicalANDExpression (Nil, e) -> Some e | _ -> None
            do! function | BitwiseORExpression (Nil, e) -> Some e | _ -> None
            do! function | BitwiseXORExpression (Nil, e) -> Some e | _ -> None
            do! function | BitwiseANDExpression (Nil, e) -> Some e | _ -> None            
            do! function | EqualityExpression (Nil, EqualityOperator.Nil, e) -> Some e | _ -> None
            do! function | RelationalExpression (Nil, RelationalOperator.Nil, e) -> Some e | _ -> None
            do! function | ShiftExpression (Nil, BitwiseShiftOperator.Nil, e) -> Some e | _ -> None            
            do! function | AdditiveExpression (Nil, AdditiveOperator.Nil, e) -> Some e | _ -> None
            do! function | MultiplicativeExpression (Nil, MultiplicativeOperator.Nil, e) -> Some e | _ -> None
            do! function | UnaryExpression (UnaryOperator.Nil, e) -> Some e | _ -> None
            do! function | PostfixExpression (e, PostfixOperator.Nil) -> Some e | _ -> None            
            do! function | LeftHandSideExpression (e) -> Some e | _ -> None
            do! function | NewExpression (e) -> Some e | _ -> None
            do! function | MemberExpression (Nil, e) -> Some e | _ -> None
            do! function | PrimaryExpression (e) -> Some e | _ -> None
            do! function | InputElement (e) -> Some e | _ -> None
            do! function | Lexer.Literal (e) -> Some e | _ -> None
            return! fun e ->
                        match e with 
                        | Lexer.StringLiteral _ 
                            when Lexer.StringLiteralParser.evalStringLiteral e = "use strict" -> Some true 
                        | Lexer.StringLiteral _ -> Some false
                        | _ -> None
        }

        let rec check e =
            match e with
            | Nil -> None
            | SourceElements (Nil, e) -> isStrictCode e
            | SourceElements (e1, e2) ->
                let r = check e1
                match r with
                | Some false -> isStrictCode e2  
                | _ -> r

        match check e with
        | Some true -> true
        | _ -> false

    let evalIdentifier e (strict:bool) =
        match e with
        | Lexer.Identifier e ->
            let identifier = Lexer.IdentifierNameParser.evalIdentifierName e
            let context = call environmentParam Reflection.IEnvironment.get_Context [||]
            let lex = call context Reflection.IExecutionContext.get_LexicalEnviroment [||]
            call lex Reflection.ILexicalEnvironment.getIdentifierReference [| constant identifier; constant strict|]

    let rec evalExpression (state:State) =
        let rec evalExpression result state =                  
            match state.element with
            | Expression (Nil, e) | ExpressionNoIn (Nil, e) ->
                let r = evalAssignmentExpression { state with element = e }
                call r Reflection.IDynamic.get_Value Array.empty :: result
            | Expression (e1, e2) | ExpressionNoIn (e1, e2) ->
                let result = evalExpression result { state with element = e1 } 
                let r = evalAssignmentExpression { state with element = e2 }
                call r Reflection.IDynamic.get_Value Array.empty :: result
        let result = evalExpression [] state |> List.rev
        exp.Block (result) :> exp
            
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
            let test = call test Reflection.IDynamic.convertToBoolean Array.empty
            let test = call test Reflection.IBoolean.get_BaseValue Array.empty
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
            call (evalUnaryExpression { state with element = e }) Reflection.IDynamic.op_PrefixIncrement [||]
        | UnaryExpression (UnaryOperator.Decrement, e) ->
            call (evalUnaryExpression { state with element = e }) Reflection.IDynamic.op_PrefixDecrement [||]
        | UnaryExpression (UnaryOperator.Plus, e) ->
            call (evalUnaryExpression { state with element = e }) Reflection.IDynamic.op_Plus [||]
        | UnaryExpression (UnaryOperator.Minus, e) ->
            call (evalUnaryExpression { state with element = e }) Reflection.IDynamic.op_Minus [||]
        | UnaryExpression (UnaryOperator.BitwiseNot, e) ->
            call (evalUnaryExpression { state with element = e }) Reflection.IDynamic.op_BitwiseNot [||]
        | UnaryExpression (UnaryOperator.LogicalNot, e) ->
            call (evalUnaryExpression { state with element = e }) Reflection.IDynamic.op_LogicalNot [||]
        | UnaryExpression (UnaryOperator.Delete, e) ->
            call (evalUnaryExpression { state with element = e }) Reflection.IDynamic.op_Delete [||]
        | UnaryExpression (UnaryOperator.Void, e) ->
            call (evalUnaryExpression { state with element = e }) Reflection.IDynamic.op_Void [||]
        | UnaryExpression (UnaryOperator.Typeof, e) ->
            call (evalUnaryExpression { state with element = e }) Reflection.IDynamic.op_Typeof [||]

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
                let baseValue = exp.Call (baseValue, Reflection.IDynamic.get_Value, Array.empty)
                let baseValue = exp.Convert(baseValue, Reflection.IReferenceBase.t) :> exp
                let name = evalExpression { state with element = e2 }
                let name = exp.Call (name, Reflection.IDynamic.convertToString, Array.empty)
                let name = exp.Convert (name, Reflection.IString.t) :> exp
                let name = exp.Call (name, Reflection.IString.get_BaseValue, Array.empty) :> exp
                let args = [| name; baseValue; constant state.strict |]
                call environmentParam Reflection.IEnvironment.createReference args 
            | MemberExpression (_, _), InputElement (e3) ->
                let baseValue = evalMemberExpression { state with element = e1 }
                let baseValue = exp.Call (baseValue, Reflection.IDynamic.get_Value, Array.empty)
                let baseValue = exp.Convert(baseValue, Reflection.IReferenceBase.t) :> exp
                let name = exp.Constant (Lexer.IdentifierNameParser.evalIdentifierName e3) :> exp
                let args = [| name; baseValue; constant state.strict  |]
                call environmentParam Reflection.IEnvironment.createReference args 
            | MemberExpression (_, _), Arguments (_) ->
                let left = evalMemberExpression { state with element = e1 }
                let right = [| evalArguments { state with element = e2 } |]
                exp.Call (left, Reflection.IDynamic.op_Construct, right) :> exp 

    and evalArguments (state:State) =
        match state.element with
        | Arguments Nil -> 
            call environmentParam Reflection.IEnvironment.get_EmptyArgs Array.empty
        | Arguments e ->
            let result = evalArgumentList [] { state with element = e } |> List.rev
            let newArray = exp.NewArrayInit(typeof<IDynamic>, result)
            call environmentParam Reflection.IEnvironment.createArgsMany [| newArray |]

    and evalArgumentList (result:list<exp>) (state:State) =
        match state.element with
        | ArgumentList (Nil, e) -> 
            let arg = evalAssignmentExpression { state with element = e }
            let arg = call arg Reflection.IDynamic.get_Value Array.empty
            arg::result
        | ArgumentList (e1, e2) ->
            let result = evalArgumentList result { state with element = e1 } 
            let arg = evalAssignmentExpression { state with element = e2 }
            let arg = call arg Reflection.IDynamic.get_Value Array.empty
            arg::result

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
                let baseValue = evalCallExpression { state with element = e1 }
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
                    evalIdentifier e state.strict 
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
                    | Lexer.RegularExpressionLiteral (_, _) ->
                        let body, flags = Lexer.RegularExpressionLiteralParser.evalRegularExpressionLiteral e
                        call environmentParam Reflection.IEnvironment.createRegExp [| constant body; constant flags  |]
            | ArrayLiteral (_, _) ->
                evalArrayLiteral { state with element = e }    
            | ObjectLiteral _ ->
                evalObjectLiteral { state with element = e }    
            | Expression (_, _) ->
                evalExpression { state with element = e }  

    and evalObjectLiteral (state:State) =
        let objectVar = exp.Variable(typeof<IObject>, "object")
        let variables = [| objectVar |]
        let createObject = exp.Call (environmentParam, Reflection.IEnvironment.createObject, Array.empty)
        let assignObject = exp.Assign (objectVar, createObject) :> exp
        match state.element with
        | ObjectLiteral Nil ->
            exp.Block(variables, [| assignObject; objectVar:>exp |]) :> exp
        | ObjectLiteral e ->
            let result = evalPropertyNameAndValueList objectVar [] { state with element = e } 
            let result = (assignObject::((objectVar:>exp::(result |> List.map (fun (n, t, e) -> e))) |> List.rev)) |> List.toArray     
            exp.Block(variables, result) :> exp


    and evalPropertyNameAndValueList (objectVar:System.Linq.Expressions.ParameterExpression) (results:list<string * PropertyType * exp>) (state:State) =
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
        let nullDyn = exp.Constant(null, typeof<IDynamic>) :> exp
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
                let code = lazy(compileFunctionCode(ReadOnlyList<string>.Empty, e2))
                let args = [| constant ReadOnlyList<string>.Empty; constant state.strict; constant code |]  
                let createFunction = call environmentParam Reflection.IEnvironment.createFunction1 args
                let createDesc = call environmentParam Reflection.IEnvironment.createAccessorDescriptor3 [| createFunction; nullDyn; nullTrue; nullTrue  |]
                name, GetProperty, createDesc      
        | PropertyAssignment (e1, e2, e3) ->
            let name = evalPropertyName { state with element = e1 } 
            let setParams =  evalPropertySetParameterList { state with element = e2 }                 
            let code = lazy(compileFunctionCode(setParams, e3))
            let args = [| constant setParams; constant state.strict; constant code |]  
            let createFunction = call environmentParam Reflection.IEnvironment.createFunction1 args
            let createDesc = call environmentParam Reflection.IEnvironment.createAccessorDescriptor3 [| nullDyn; createFunction; nullTrue; nullTrue  |]
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
        | PropertySetParameterList (Lexer.Identifier e) ->
            ReadOnlyList<string>([| Lexer.IdentifierNameParser.evalIdentifierName e |])

    and evalArrayLiteral (state:State) =
        let arrayVar = exp.Variable(typeof<IObject>, "array")
        let lengthVar = exp.Variable(typeof<IPropertyDescriptor>, "length")
        let variables = [| arrayVar; lengthVar |]
        let createArray = exp.Call (environmentParam, Reflection.IEnvironment.createArray, Array.empty)
        let assignArray = exp.Assign (arrayVar, createArray) :> exp
        let getLength = exp.Call (arrayVar, Reflection.IObject.getOwnProperty, [| constant "length" |])
        let assignLength = exp.Assign (lengthVar, getLength) :> exp
        match state.element with
        | ArrayLiteral (Nil, e) ->
            let pad = exp.Call (environmentParam, Reflection.IEnvironment.createNumber, [| constant (evalElision { state with element = e }) |]) :> exp
            let setValue = exp.Call (lengthVar, Reflection.IPropertyDescriptor.set_Value, [| pad |]) :> exp
            exp.Block (variables, [|assignArray; assignLength; setValue; arrayVar:>exp|]) :> exp
        | ArrayLiteral (e, Nil) ->
            let result = assignArray::assignLength::(arrayVar:>exp::(evalElementList arrayVar lengthVar [] { state with element = e }) |> List.rev)
            let result = result |> List.toArray
            exp.Block(variables, result) :> exp
        | ArrayLiteral (e1, e2) ->
            let result = arrayVar:>exp::evalElementList arrayVar lengthVar []  { state with element = e1 } 
            let pad = exp.Call (environmentParam, Reflection.IEnvironment.createNumber, [| constant (evalElision { state with element = e2 }) |]) :> exp
            let len = exp.Call (lengthVar, Reflection.IPropertyDescriptor.get_Value, Array.empty) :> exp
            let addPad = exp.Call (len, Reflection.IDynamic.op_Addition, [| pad |]) :> exp
            let value = exp.Call (addPad, Reflection.IDynamic.convertToUInt32, Array.empty) :> exp
            let setValue = exp.Call (lengthVar, Reflection.IPropertyDescriptor.set_Value, [| value |]) :> exp
            let result = [ assignArray; assignLength ] @ (result |> List.rev) @ [ setValue; arrayVar ]
            exp.Block(variables, result) :> exp

    and evalElision (state:State) =
        match state.element with
        | Nil -> 0.0
        | Elision (Nil) -> 1.0
        | Elision (e) -> evalElision { state with element = e } + 1.0

    and evalElementList (arrayVar:System.Linq.Expressions.ParameterExpression) (lengthVar:System.Linq.Expressions.ParameterExpression) (results:list<exp>) (state:State) =
        match state.element with
        | ElementList (Nil, e1, e2) ->
            let initResult = evalAssignmentExpression { state with element = e2 }
            let initValue = exp.Call (initResult, Reflection.IDynamic.get_Value, Array.empty) :> exp
            let firstIndex = match e1 with | Nil -> 0.0 | Elision (_) -> evalElision { state with element = e1 }
            let nullTrue = exp.Convert(constant true, typeof<Nullable<bool>>) :> exp
            let createDesc = exp.Call (environmentParam, Reflection.IEnvironment.createDataDescriptor4, [| initValue; nullTrue; nullTrue; nullTrue  |]) :> exp
            let createProp = exp.Call (arrayVar, Reflection.IObject.defineOwnProperty, [| constant (firstIndex|>string); createDesc; constant false |]) :> exp
            createProp::results
        | ElementList (e1, e2, e3) ->
            let results = evalElementList arrayVar lengthVar results { state with element = e1 }
            let initResult = evalAssignmentExpression { state with element = e3 }
            let initValue = exp.Call (initResult, Reflection.IDynamic.get_Value, Array.empty) :> exp
            let pad = match e2 with | Nil -> 0.0 | Elision (_) -> evalElision { state with element = e2 }
            let createPad = exp.Call (environmentParam, Reflection.IEnvironment.createNumber, [| constant pad |]) :> exp
            let len = exp.Call (lengthVar, Reflection.IPropertyDescriptor.get_Value, Array.empty) :> exp
            let add = exp.Call (len, Reflection.IDynamic.op_Addition, [| createPad |]) :> exp
            let toUInt32 = exp.Call (add, Reflection.IDynamic.convertToUInt32, Array.empty) :> exp
            let toString = exp.Call (toUInt32, Reflection.IDynamic.convertToString, Array.empty) :> exp
            let name = exp.Call ((exp.Convert (toString, typeof<IString>)), Reflection.IString.get_BaseValue, Array.empty) :> exp
            let nullTrue = exp.Convert(constant true, typeof<Nullable<bool>>) :> exp
            let createDesc = exp.Call (environmentParam, Reflection.IEnvironment.createDataDescriptor4, [| initValue; nullTrue; nullTrue; nullTrue  |]) :> exp
            let createProp = exp.Call (arrayVar, Reflection.IObject.defineOwnProperty, [| name; createDesc; constant false |]) :> exp
            createProp::results

    and evalStatement (state:State) =
        match state.element with
        | Statement e ->
            match e with
            | Block (_) ->
                evalBlock { state with element = e }
            | VariableStatement _ ->
                evalVariableStatement { state with element = e } 
            | EmptyStatement -> 
                evalEmptyStatement { state with element = e }, state
            | ExpressionStatement _ ->
                evalExpressionStatement { state with element = e }, state
            | IfStatement (_, _, _) ->
                evalIfStatement { state with element = e }, state 
            | IterationStatement (_, _, _, _) ->
                evalIterationStatement { state with element = e }  
            | ContinueStatement (_) ->
                evalContinueStatement { state with element = e }, state 
            | BreakStatement (_) ->
                evalBreakStatement { state with element = e }, state
            | ReturnStatement _ ->
                evalReturnStatement { state with element = e }
            | WithStatement (_, _) ->
                evalWithStatement { state with element = e }, state
            | LabelledStatement (_, _) ->
                evalLabelledStatement { state with element = e }
            | SwitchStatement (_, _) ->
                evalSwitchStatement { state with element = e }
            | ThrowStatement (_) ->
                evalThrowStatement { state with element = e }, state
            | TryStatement (_, _, _) ->
                evalTryStatement { state with element = e }, state
            | DebuggerStatement (_) ->
                evalDebuggerStatement { state with element = e }, state

    and evalBlock (state:State) =
        match state.element with
        | Block Nil ->
            empty, state
        | Block e ->
            evalStatementList { state with element = e }  

    and evalStatementList (state:State) =
        let rec evalStatementList result state =                  
            match state.element with
            | StatementList (Nil, e) ->
                let r, s = evalStatement { state with element = e }
                r :: result, s
            | StatementList (e1, e2) ->
                let result, s = evalStatementList result { state with element = e1 } 
                let r, s = evalStatement { s with element = e2 }
                r :: result, s
        let result, state = evalStatementList [] state
        let result = if not result.IsEmpty && result.Head.Type <> typeof<IDynamic> then getUndefined::result else result
        exp.Block (typeof<IDynamic>, result |> List.rev) :> exp, state

    and evalVariableStatement (state:State) =
        match state.element with
        | VariableStatement e ->
            evalVariableDeclarationList { state with element = e } 

    and evalVariableDeclarationList (state:State) =
        match state.element with
        | VariableDeclarationList (Nil, e) | VariableDeclarationListNoIn (Nil, e) ->
            evalVariableDeclaration { state with element = e } 
        | VariableDeclarationList (e1, e2) | VariableDeclarationListNoIn (e1, e2) ->
            let first, state = evalVariableDeclarationList { state with element = e1 }
            let second, state = evalVariableDeclaration { state with element = e2 }
            block [| first; second |], state

    and evalVariableDeclaration (state:State) =
        let strictValidation identifier =
            if state.strict then                
                let msg = "The identifier '{0}' is not allowed in strict mode."
                match identifier with 
                | "eval" 
                | "arguments" -> 
                    raise (environment.CreateSyntaxError (String.Format (msg, identifier)))
                | _ -> ()
        match state.element with
        | VariableDeclaration (Lexer.Identifier e, Nil) | VariableDeclarationNoIn (Lexer.Identifier e, Nil) ->
            let identifier = Lexer.IdentifierNameParser.evalIdentifierName e
            strictValidation identifier
            exp.Empty() :> exp, { state with variables = identifier::state.variables }
        | VariableDeclaration (e1, e2) | VariableDeclarationNoIn (e1, e2) ->
            let identifier = 
                match e1 with
                | Lexer.Identifier e ->
                    Lexer.IdentifierNameParser.evalIdentifierName e
            strictValidation identifier
            let left = evalIdentifier e1 state.strict 
            let right = evalInitialiser { state with element = e2 }
            let right = call right Reflection.IDynamic.get_Value Array.empty
            call left Reflection.IDynamic.set_Value [| right |], { state with variables = identifier::state.variables }

    and evalInitialiser (state:State) =
        match state.element with
        | Initialiser e | InitialiserNoIn e ->
            evalAssignmentExpression { state with element = e }

    and evalEmptyStatement (state:State) =
        empty

    and evalExpressionStatement (state:State) =
        match state.element with
        | ExpressionStatement e ->
            evalExpression { state with element = e } 

    and evalIfStatement (state:State) =
        match state.element with
        | IfStatement (e1, e2, SourceElement.Nil) ->
            let e1 = evalExpression { state with element = e1 } 
            let e1 = call e1 Reflection.IDynamic.convertToBoolean Array.empty
            let e1 = call e1 Reflection.IBoolean.get_BaseValue Array.empty
            let e2, state = evalStatement { state with element = e2 }
            exp.IfThen(e1, e2) :> exp
        | IfStatement (e1, e2, e3) ->
            let e1 = evalExpression { state with element = e1 } 
            let e1 = call e1 Reflection.IDynamic.convertToBoolean Array.empty
            let e1 = call e1 Reflection.IBoolean.get_BaseValue Array.empty
            let e2, state = evalStatement { state with element = e2 }
            let e3, state = evalStatement { state with element = e3 }
            exp.IfThenElse(e1, e2, e3) :> exp

    and evalIterationStatement (state:State) =
        let breakLabel = exp.Label(exp.Label(typeof<Void>, "breakLoop"))
        let continueLabel = exp.Label(exp.Label(typeof<Void>, "continueLoop"))
        let labels = state.labels.Head.Add("breakLoop", breakLabel).Add("continueLoop", continueLabel)
        match state.element with
        | IterationStatement (e1, e2, e3, e4) ->
            match e1, e2, e3, e4 with

            | Statement (_), Expression (_, _), SourceElement.Nil, SourceElement.Nil -> // IterationStatement: do Statement while ( Expression ); 
                let e1, s1 = evalStatement { state with labels = labels::state.labels; element = e1 } 
                let e2 = evalExpression { s1 with element = e2 }
                let e2 = call e2 Reflection.IDynamic.convertToBoolean Array.empty
                let e2 = call e2 Reflection.IBoolean.get_BaseValue Array.empty
                let body = exp.Block([| e1; exp.IfThen(exp.Not(e2), exp.Break(breakLabel.Target)) :> exp |])
                exp.Loop (body, breakLabel.Target, continueLabel.Target) :> exp, { s1 with labels = s1.labels.Tail }

            | Expression (_, _), Statement (_), SourceElement.Nil, SourceElement.Nil -> // IterationStatement: while ( Expression ) Statement   
                let eStatement, s1 = evalStatement { state with labels = labels::state.labels; element = e2 } 
                let e2 = evalExpression { s1 with element = e1 }
                let e2 = call e2 Reflection.IDynamic.convertToBoolean Array.empty
                let e2 = call e2 Reflection.IBoolean.get_BaseValue Array.empty
                let body = block [| exp.IfThenElse(exp.Not e2, exp.Break(breakLabel.Target), eStatement) |]
                exp.Loop (body, breakLabel.Target, continueLabel.Target) :> exp, { s1 with labels = s1.labels.Tail }
                
            | SourceElement.Nil, SourceElement.Nil, SourceElement.Nil, Statement (_) ->                                             
                let e4, state = evalStatement { state with labels = labels::state.labels; element = e4 } 
                let loop = exp.Loop (e4, breakLabel.Target, continueLabel.Target) :> exp
                loop,  { state with labels = state.labels.Tail }

            | SourceElement.Nil, Expression (_, _), SourceElement.Nil, Statement (_) -> 
                let e2 = convertToBool (evalExpression { state with element = e2 })
                let e4, state = evalStatement { state with labels = labels::state.labels; element = e4 }
                let body = exp.IfThenElse(e2, block [| e4 |], exp.Break(breakLabel.Target))
                let loop = exp.Loop (body, breakLabel.Target, continueLabel.Target) :> exp
                loop,  { state with labels = state.labels.Tail }
            
            | SourceElement.Nil, SourceElement.Nil, Expression (_, _), Statement (_) ->                             
                let e3 = evalExpression { state with element = e3 }
                let e3 = call e3 Reflection.IDynamic.get_Value Array.empty                              
                let e4, state = evalStatement { state with labels = labels::state.labels; element = e4 }  
                let loop = exp.Loop (block [| e3; e4 |], breakLabel.Target, continueLabel.Target) :> exp
                loop,  { state with labels = state.labels.Tail }
                
            | SourceElement.Nil, Expression (_, _), Expression (_, _), Statement (_) -> 
                let e2 = convertToBool (evalExpression { state with element = e2 })                       
                let e3 = evalExpression { state with element = e3 }
                let e3 = call e3 Reflection.IDynamic.get_Value Array.empty   
                let e4, state = evalStatement { state with labels = labels::state.labels; element = e4 }                      
                let body = exp.IfThenElse(e2, block [| e4; e3 |], exp.Break(breakLabel.Target))
                let loop = exp.Loop (body, breakLabel.Target, continueLabel.Target) :> exp
                loop,  { state with labels = state.labels.Tail }

            | ExpressionNoIn (_), SourceElement.Nil, SourceElement.Nil, Statement (_) ->             
                let e1 = evalExpression { state with element = e1 }
                let e1 = exp.Call (e1, Reflection.IDynamic.get_Value, Array.empty) :> exp                              
                let e4, state = evalStatement { state with labels = labels::state.labels; element = e4 } 
                let loop = exp.Loop (e4, breakLabel.Target, continueLabel.Target) :> exp
                exp.Block ([| e1; loop |]) :> exp,  { state with labels = state.labels.Tail }

            | ExpressionNoIn (_), Expression (_, _), SourceElement.Nil, Statement (_) ->             
                let e1 = evalExpression { state with element = e1 }
                let e1 = exp.Call (e1, Reflection.IDynamic.get_Value, Array.empty) :> exp
                let e2 = convertToBool (evalExpression { state with element = e2 })                
                let e4, state = evalStatement { state with labels = labels::state.labels; element = e4 }
                let body = exp.IfThenElse(e2, block [| e4 |], exp.Break(breakLabel.Target))
                let loop = exp.Loop (body, breakLabel.Target, continueLabel.Target) :> exp
                exp.Block ([| e1; loop |]) :> exp,  { state with labels = state.labels.Tail }

            | ExpressionNoIn (_), Expression (_, _), Expression (_, _), Statement (_) ->
                let e1 = evalExpression { state with element = e1 }
                let e1 = exp.Call (e1, Reflection.IDynamic.get_Value, Array.empty) :> exp
                let e2 = evalExpression { state with element = e2 }
                let e2 = convertToBool e2                 
                let e3 = evalExpression { state with element = e3 }
                let e3 = exp.Call (e2, Reflection.IDynamic.get_Value, Array.empty)
                let e4, state = evalStatement { state with labels = labels::state.labels; element = e4 }
                let body = exp.IfThenElse(e2, block [| e4; e3 |], exp.Break(breakLabel.Target))
                let loop = exp.Loop (body, breakLabel.Target, continueLabel.Target) :> exp
                exp.Block ([| e1; loop |]) :> exp,  { state with labels = state.labels.Tail }

            | VariableDeclarationListNoIn (_), SourceElement.Nil, SourceElement.Nil, Statement (_) -> // for ( var VariableDeclarationListNoIn; ; ) Statement 
                let e1, state = evalVariableDeclarationList { state with element = e1 }
                let e4, state = evalStatement { state with labels = labels::state.labels; element = e4 } 
                let body = exp.Loop (e4, breakLabel.Target, continueLabel.Target) :> exp
                let body = exp.Block([| e1; body; |]) :> exp
                body,  { state with labels = state.labels.Tail }
                
            | VariableDeclarationListNoIn (_), Expression (_, _), SourceElement.Nil, Statement (_) ->
                let e1, state = evalVariableDeclarationList { state with element = e1 }
                let e2 = evalExpression { state with element = e2 }
                let e4, state = evalStatement { state with labels = labels::state.labels; element = e4 } 
                let e2 = convertToBool e2 
                let body = exp.IfThenElse(e2, e4, exp.Break(breakLabel.Target))
                let body = exp.Loop (body, breakLabel.Target, continueLabel.Target) :> exp
                let body = exp.Block([| e1; body; |]) :> exp
                body,  { state with labels = state.labels.Tail }

            | VariableDeclarationListNoIn (_), Expression (_, _), Expression (_, _), Statement (_) ->
                let e1, state = evalVariableDeclarationList { state with element = e1 }
                let e2 = evalExpression { state with element = e2 }
                let e3 = evalExpression { state with element = e3 }
                let e4, state = evalStatement { state with labels = labels::state.labels; element = e4 } 
                let e2 = convertToBool e2 
                let e3 = exp.Call (e3, Reflection.IDynamic.get_Value, Array.empty)
                let body = exp.IfThenElse(e2, block [| e4; e3; |], exp.Break(breakLabel.Target))
                let loop = exp.Loop (body, breakLabel.Target, continueLabel.Target) :> exp
                let body = exp.Block ([| e1; loop |])
                body :> exp,  { state with labels = state.labels.Tail }

            | LeftHandSideExpression (_), Expression (_, _), SourceElement.Nil, Statement (_) ->
                let enumeratorVar = exp.Variable(typeof<IEnumerator<string>>, "enumerator")
                let lhsRef = evalLeftHandSideExpression { state with element = e1 }
                let experRef = evalExpression { state with element = e2 }
                let experValue = exp.Call (experRef, Reflection.IDynamic.get_Value, Array.empty) :> exp
                let obj = exp.Call (experValue, Reflection.IDynamic.convertToObject, Array.empty) :> exp
                let asEnumerableString = exp.Convert(obj, typeof<IEnumerable<string>>)
                let getEnumerator = exp.Call (asEnumerableString, Reflection.IEnumerableString.getEnumerator, Array.empty) :> exp
                let assignEnumeratorVar = exp.Assign (enumeratorVar, getEnumerator)
                let asEnumeratorString = exp.Convert(enumeratorVar, typeof<IEnumerator<string>>)                 
                let current = exp.Call (asEnumeratorString, Reflection.IEnumeratorString.get_Current, Array.empty) :> exp
                let asEnumerator = exp.Convert(enumeratorVar, typeof<IEnumerator>)
                let moveNext = exp.Call (asEnumerator, Reflection.IEnumerator.moveNext, Array.empty)
                let asDisposable = exp.Convert(enumeratorVar, typeof<IDisposable>)
                let dispose = exp.Call (asDisposable, Reflection.IDisposable.dispose, Array.empty) :> exp  
                let eStatement, state = evalStatement { state with labels = labels::state.labels; element = e4 } 
                let createString = exp.Call (environmentParam, Reflection.IEnvironment.createString, [| current |]) :> exp
                let putCurrent = exp.Call (lhsRef, Reflection.IDynamic.set_Value, [| createString |]) :> exp
                let ifTrue = exp.Block ([| putCurrent; eStatement |]) :> exp
                let loopCondition = exp.IfThenElse (moveNext, ifTrue, exp.Break(breakLabel.Target))
                let loop = exp.Loop (loopCondition, breakLabel.Target, continueLabel.Target)
                let initTest = exp.Not(exp.Or (exp.TypeIs (experValue, typeof<IUndefined>), exp.TypeIs (experValue, typeof<INull>)))
                let initCondition = exp.IfThen (initTest, loop)
                exp.Block ([| enumeratorVar |], exp.TryFinally (block [| assignEnumeratorVar; initCondition |], dispose)) :> exp, { state with labels = state.labels.Tail }

            | VariableDeclarationNoIn (ie, _), Expression (_, _), SourceElement.Nil, Statement (_) ->    
                let enumeratorVar = exp.Variable(typeof<IEnumerator<string>>, "enumerator")
                let varName, state = evalVariableDeclaration { state with element = e1 }
                let varRef = evalIdentifier ie state.strict 
                let experRef = evalExpression { state with element = e2 }
                let experValue = exp.Call (experRef, Reflection.IDynamic.get_Value, Array.empty)
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
                let eStatement, state = evalStatement { state with labels = labels::state.labels; element = e4 } 
                let putCurrent = call varRef Reflection.IDynamic.set_Value [| call environmentParam Reflection.IEnvironment.createString [| current |] |]
                let ifTrue = exp.Block ([| putCurrent; eStatement |]) :> exp
                let loopCondition = exp.IfThenElse (moveNext, ifTrue, exp.Break(breakLabel.Target))
                let loop = exp.Loop (loopCondition, breakLabel.Target, continueLabel.Target)
                let initTest = exp.Not(exp.Or (exp.TypeIs (experValue, typeof<IUndefined>), exp.TypeIs (experValue, typeof<INull>)))
                let initCondition = exp.IfThen (initTest, loop)
                exp.Block ([| enumeratorVar |], exp.TryFinally (block [| assignEnumeratorVar; initCondition |], dispose)) :> exp, { state with labels = state.labels.Tail }


    and evalContinueStatement (state:State) =
        let labels = state.labels.Head
        match state.element with
        | ContinueStatement (Lexer.Nil) ->
            let r1 = labels.TryFind "continueSwitch"
            let r2 = labels.TryFind "continueLoop"
            match r1, r2 with
            | Some r1, Some _
            | Some r1, None  ->
                exp.Goto (r1.Target) :> exp
            | None, Some r2 ->
                exp.Goto (r2.Target) :> exp
            | None, None ->
                let msg = "The continue statement with no identifier requires a surrounding loop or switch statement."
                let err = environment.CreateSyntaxError msg
                raise err
        | ContinueStatement (Lexer.Identifier e1) ->
            let identifier = Lexer.IdentifierNameParser.evalIdentifierName e1
            let r1 = labels.TryFind ("continue" + identifier)
            match r1 with
            | Some r1 ->
                exp.Goto (r1.Target) :> exp
            | None ->
                let msg = "The continue statement is invalid because the label '" + identifier + "' does not exist in the surrounding scope."
                let err = environment.CreateSyntaxError msg
                raise err

    and evalBreakStatement (state:State) =
        let labels = state.labels.Head
        match state.element with
        | BreakStatement (Lexer.Nil) ->
            let r1 = labels.TryFind "breakSwitch"
            let r2 = labels.TryFind "breakLoop"
            match r1, r2 with
            | Some r1, Some _
            | Some r1, None  ->
                exp.Goto (r1.Target) :> exp
            | None, Some r2 ->
                exp.Goto (r2.Target) :> exp
            | None, None ->
                let msg = "The break statement with no identifier requires a surrounding loop or switch statement."
                let err = environment.CreateSyntaxError msg
                raise err
        | BreakStatement (Lexer.Identifier e1) ->
            let identifier = Lexer.IdentifierNameParser.evalIdentifierName e1
            let r1 = labels.TryFind ("break" + identifier)
            match r1 with
            | Some r1 ->
                exp.Goto (r1.Target) :> exp
            | None ->
                let msg = "The break statement is invalid because the label '" + identifier + "' does not exist in the surrounding scope."
                let err = environment.CreateSyntaxError msg
                raise err

    and evalReturnStatement (state:State) =
        let target = state.labels.Head.["return"].Target
        match state.element with
        | ReturnStatement Nil -> 
            exp.Return (target, getUndefined, typeof<dyn>) :> exp, state
        | ReturnStatement e -> 
            let r = evalExpression { state with element = e }
            exp.Return (target, r, typeof<dyn>) :> exp, state

    and evalWithStatement (state:State) =
        if state.strict then
            raise (environment.CreateSyntaxError "The with statement is not allowed in strict mode.")
        else
            match state.element with
            | WithStatement (e1, e2) ->
                let oldEnvVar = exp.Variable(typeof<ILexicalEnvironment>, "oldEnv")
                let newEnvVar = exp.Variable(typeof<ILexicalEnvironment>, "newEnv")
                let variables = [| oldEnvVar; newEnvVar |]
                let value = evalExpression { state with element = e1 }
                let obj = exp.Call (value, Reflection.IDynamic.convertToObject, Array.empty) :> exp
                let getLexEnv = exp.Call (getContext, Reflection.IExecutionContext.get_LexicalEnviroment, Array.empty) :> exp
                let assignOldEnvVar = exp.Assign (oldEnvVar, getLexEnv) :> exp
                let newObjEnv = exp.Call (oldEnvVar, Reflection.ILexicalEnvironment.newObjectEnvironment, [| obj; constant true |]) :> exp
                let assignNewEnvVar = exp.Assign (newEnvVar, newObjEnv) :> exp
                let assignNewEnv = exp.Call (getContext, Reflection.IExecutionContext.set_LexicalEnviroment, [| newEnvVar :> exp |]) :> exp
                let assignOldEnv = exp.Call (getContext, Reflection.IExecutionContext.set_LexicalEnviroment, [| oldEnvVar :> exp |]) :> exp
                let body, state = evalStatement { state with element = e2 } 
                let tryBody = exp.Block ([| assignOldEnvVar; assignNewEnvVar; assignNewEnv; body |])
                let finallyBody = exp.Block ([| assignOldEnv |])
                exp.Block (variables, exp.TryFinally (tryBody, finallyBody)) :> exp

    and evalSwitchStatement (state:State) = 
        match state.element with
        | SwitchStatement (e1, e2) ->
            let breakLabel = exp.Label(exp.Label(typeof<Void>, "breakSwitch"))
            let value = evalExpression { state with element = e1 }
            let state = { state with labels = state.labels.Head.Add ("breakSwitch", breakLabel) :: state.labels; element = e2 }
            let leftCases, defaultCase, rightCases = evalCaseBlock state
            let caseClauses = (leftCases @ rightCases) |> List.toArray
            let switch = exp.Switch (value, defaultCase, equalityTestMethod, caseClauses) :> exp
            let body = [| switch; breakLabel :> exp |]
            exp.Block (body) :> exp, { state with labels = state.labels.Tail }

    and evalCaseBlock (state:State) =
        match state.element with
        | CaseBlock (SourceElement.Nil, SourceElement.Nil, e3) ->
            [], exp.Empty() :> exp, evalCaseClauses { state with element = e3 }
        | CaseBlock (SourceElement.Nil, e2, SourceElement.Nil) ->
            [], evalDefaultClause { state with element = e2 }, []
        | CaseBlock (SourceElement.Nil, e2, e3) ->
            [], evalDefaultClause { state with element = e2 }, evalCaseClauses { state with element = e3 }
        | CaseBlock (e1, SourceElement.Nil, SourceElement.Nil) ->
            evalCaseClauses { state with element = e1 }, exp.Empty() :> exp, []
        | CaseBlock (e1, e2, SourceElement.Nil) ->
            evalCaseClauses { state with element = e1 }, evalDefaultClause { state with element = e2 }, []
        | CaseBlock (e1, e2, e3) ->
            evalCaseClauses { state with element = e1 }, evalDefaultClause { state with element = e2 }, evalCaseClauses { state with element = e3 }         

    and evalCaseClauses (state:State) =
        let rec run (result:list<System.Linq.Expressions.SwitchCase>) (element:SourceElement) =
            match element with
            | CaseClauses (Nil, e1) ->
                evalCaseClause { state with element = e1 } :: result
            | CaseClauses (e1, e2) ->
                let result = run result e1
                evalCaseClause { state with element = e2 } :: result
        run [] state.element

    and evalCaseClause (state:State) =
        match state.element with
        | CaseClause (e1, Nil) ->
            let e = evalExpression { state with element = e1 }
            exp.SwitchCase (exp.Empty() :> exp, [| e |])
        | CaseClause (e1, e2) ->
            let e1 = evalExpression { state with element = e1 }
            let e2, s = evalStatementList { state with element = e2 }
            exp.SwitchCase (e2, [| e1 |])

    and evalDefaultClause (state:State) =
        match state.element with
        | DefaultClause Nil ->
            exp.Empty() :> exp
        | DefaultClause e ->
            let e, s = evalStatementList { state with element = e }
            e       

    and evalLabelledStatement (state:State) =
        match state.element with
        | LabelledStatement (Lexer.Identifier e1, e2) ->
            let identifier = Lexer.IdentifierNameParser.evalIdentifierName e1
            let breakName = "break" + identifier
            let continueName = "continue" + identifier 
            let breakLabel = exp.Label(exp.Label(typeof<Void>, breakName))
            let continueLabel = exp.Label(exp.Label(typeof<Void>, continueName))
            let labels = state.labels.Head.Add(breakName, breakLabel).Add(continueName, continueLabel)
            let state = { state with labels = labels::state.labels; element = e2 }
            let result, state = evalStatement state
            block [| continueLabel; result; breakLabel |], { state with labels = state.labels.Tail }

    and evalThrowStatement (state:State) =
        match state.element with
        | ThrowStatement e ->
            let e = evalExpression { state with element = e }
            call e Reflection.IDynamic.op_Throw Array.empty

    and evalTryStatement (state:State) =
        match state.element with
        | TryStatement (e1, SourceElement.Nil, e2) ->             
            let execBlock, state = evalBlock { state with element = e1 } 
            let execFinally, state = evalFinally { state with element = e2 } 
            exp.TryFinally (execBlock, execFinally) :> exp    
        | TryStatement (e1, e2, SourceElement.Nil) ->             
            let execBlock, state = evalBlock { state with element = e1 } 
            let execCatch = evalCatch { state with element = e2 } 
            exp.TryCatch (execBlock, [| execCatch |]) :> exp          
        | TryStatement (e1, e2, e3) ->             
            let body, state = evalBlock { state with element = e1 } 
            let execCatch = evalCatch { state with element = e2 }  
            let execFinally, state = evalFinally { state with element = e3 } 
            exp.TryCatchFinally (body, execFinally, [| execCatch |]) :> exp

    and evalCatch (state:State) =
        match state.element with
        | Catch (Lexer.Identifier e1, e2) ->
            let catchVar = exp.Variable(typeof<MacheteRuntimeException>, "catch")
            let oldEnvVar = exp.Variable(typeof<ILexicalEnvironment>, "oldEnv")
            let catchEnvVar = exp.Variable(typeof<ILexicalEnvironment>, "catchEnv")
            let catchRecVar = exp.Variable(typeof<IEnvironmentRecord>, "catchRec")
            let identifier = Lexer.IdentifierNameParser.evalIdentifierName e1
            let execBlock, state = evalBlock { state with element = e2 }
            let assignOldEnv = exp.Assign (oldEnvVar, (call getContext Reflection.IExecutionContext.get_LexicalEnviroment Array.empty)) :> exp
            let newEnv = exp.Call (oldEnvVar, Reflection.ILexicalEnvironment.newDeclarativeEnvironment, Array.empty) :> exp 
            let assignCatchEnv = exp.Assign (catchEnvVar, newEnv) :> exp
            let getRec = exp.Call (catchEnvVar, Reflection.ILexicalEnvironment.get_Record, Array.empty) :> exp 
            let assignCatchRec = exp.Assign (catchRecVar, exp.Convert(getRec, typeof<IEnvironmentRecord>)) :> exp
            let createBinding = exp.Call (catchRecVar, Reflection.IEnvironmentRecord.createMutableBinding, [| constant identifier; constant false |]) :> exp 
            let getThrown = exp.Call (catchVar, Reflection.MacheteRuntimeException.get_Thrown, Array.empty) :> exp 
            let setBinding = exp.Call (catchRecVar, Reflection.IEnvironmentRecord.setMutableBinding, [| constant identifier; getThrown; constant false |]) :> exp 
            let assignEnv = exp.Call (getContext, Reflection.IExecutionContext.set_LexicalEnviroment, [| catchEnvVar :> exp  |]) :> exp 
            let tryBody = exp.Block ([| assignOldEnv; assignCatchEnv; assignCatchRec; createBinding; setBinding; assignEnv; execBlock |]) :> exp
            let finallyBody = exp.Block ([| call getContext Reflection.IExecutionContext.set_LexicalEnviroment [| oldEnvVar |] |]) :> exp      
            let body = exp.Block ([| oldEnvVar; catchEnvVar; catchRecVar |], exp.TryFinally (tryBody, finallyBody)) :> exp      
            exp.Catch (catchVar, body)       

    and evalFinally (state:State) =
        match state.element with
        | Finally e ->
            evalBlock { state with element = e }

    and evalDebuggerStatement (state:State) = 
        exp.Empty() :> exp

    and evalFunctionDeclaration (state:State) =
        match state.element with
        | FunctionDeclaration (Lexer.Identifier e1, e2, e3) ->
            let strict = match e3 with | FunctionBody e -> isStrictCode e
            let state =  { state with strict = strict }  
            let identifier = Lexer.IdentifierNameParser.evalIdentifierName e1    
            let f = { 
                identifier = identifier 
                formalParameterList =
                    match e2 with 
                    | SourceElement.Nil -> ReadOnlyList<string>.Empty 
                    | _ -> evalFormalParameterList identifier { state with element = e2 }  
                strict = strict; 
                functionBody = e3 
            }
            empty, { state with functions = f::state.functions }

    and evalFunctionExpression (state:State) =
        match state.element with
        | FunctionExpression (Lexer.Nil, e1, e2) ->
            let strict = match e2 with | FunctionBody e -> isStrictCode e
            let state =  { state with strict = strict }  
            let formalParameterList = match e1 with | SourceElement.Nil -> ReadOnlyList<string>.Empty | _ -> evalFormalParameterList "(No Identifier)" { state with element = e1 }          
            let code = lazy(compileFunctionCode(formalParameterList, e2))
            let args = [| constant formalParameterList; constant state.strict; constant code |]  
            call environmentParam Reflection.IEnvironment.createFunction1 args
        | FunctionExpression (Lexer.Identifier e1, e2, e3) -> 
            let identifier = Lexer.IdentifierNameParser.evalIdentifierName e1
            let strict = match e3 with | FunctionBody e -> isStrictCode e
            let state =  { state with strict = strict } 
            let scopeVar = exp.Variable(typeof<ILexicalEnvironment>, "scope")
            let functionVar = exp.Variable(typeof<IObject>, "function")
            let getEnv = call environmentParam Reflection.IEnvironment.get_Context Array.empty
            let getEnv = call getEnv Reflection.IExecutionContext.get_LexicalEnviroment Array.empty
            let getEnv = call getEnv Reflection.ILexicalEnvironment.newDeclarativeEnvironment Array.empty
            let assignScope = exp.Assign(scopeVar, getEnv) :> exp
            let getRecord = exp.Convert(call scopeVar Reflection.ILexicalEnvironment.get_Record Array.empty, typeof<IDeclarativeEnvironmentRecord>)
            let formalParameterList = match e2 with | SourceElement.Nil -> ReadOnlyList<string>.Empty | _ -> evalFormalParameterList identifier { state with element = e2 }          
            let code = lazy(compileFunctionCode(formalParameterList, e3))
            let args = [| constant formalParameterList; constant state.strict; constant code; scopeVar:>exp |] 
            let createFunction = call environmentParam Reflection.IEnvironment.createFunction2 args
            let assignFunction = exp.Assign (functionVar, createFunction) :> exp
            let createBinding = call getRecord Reflection.IDeclarativeEnvironmentRecord.createImmutableBinding [| constant identifier |] 
            let initBinding = call getRecord Reflection.IDeclarativeEnvironmentRecord.initializeImmutableBinding [| constant identifier; functionVar :> exp |]
            exp.Block ([| scopeVar; functionVar |], [| assignScope; assignFunction; initBinding; functionVar :> exp |]) :> exp

    and evalFormalParameterList (functionIdentifier:string) (state:State) =
        let errorFormat = "The function '{0}' contains a duplicated parameter named '{1}'."
        let validate (identifier:string) (set:Set<string>) =            
            if set.Contains identifier then
                raise (environment.CreateSyntaxError (String.Format (errorFormat, functionIdentifier, identifier)))
        let rec run (result:list<string>) (set:Set<string>) (element:SourceElement) =
            match element with
            | FormalParameterList (SourceElement.Nil, InputElement(Lexer.Identifier e1)) ->
                let identifier = Lexer.IdentifierNameParser.evalIdentifierName e1
                validate identifier set
                identifier::result
            | FormalParameterList (e1, InputElement(Lexer.Identifier e2)) ->
                let identifier = Lexer.IdentifierNameParser.evalIdentifierName e2
                validate identifier set
                identifier::run result (set.Add identifier) e1
        ReadOnlyList<string>(run [] Set.empty state.element |> List.rev)

    and evalFunctionBody (state:State) =
        let target = state.labels.Head.["return"].Target
        match state.element with
        | FunctionBody Nil -> 
            exp.Return (target, getUndefined, typeof<dyn>) :> exp, state
        | FunctionBody e -> 
            let e, s = evalSourceElements { state with element = e }
            e :> exp, s

    and evalSourceElement (state:State) =
        match state.element with
        | SourceElement e ->
            match e with
            | Statement _ ->
                let e, s = evalStatement { state with element = e } 
                e, s
            | FunctionDeclaration (_, _, _) ->
                evalFunctionDeclaration { state with element = e }

    and evalSourceElements (state:State) : System.Linq.Expressions.BlockExpression * State =
        let rec evalSourceElements result state =
            let filter result e state =
                let r, s = evalSourceElement { state with element = e }
                match r.NodeType with
                | System.Linq.Expressions.ExpressionType.Default -> result, s 
                | _ -> r::result, s                   
            match state.element with
            | SourceElements (Nil, e) ->
                filter result e state
            | SourceElements (e1, e2) ->
                let result, state = evalSourceElements result { state with element = e1 } 
                filter result e2 state
        let strict = isStrictCode state.element
        let state =  { state with strict = strict }        
        let result, state = evalSourceElements [] state        
        let label = state.labels.Head.["return"]
        if result.IsEmpty then
            let result = (label:>exp)::result |> List.rev
            exp.Block(typeof<IDynamic>, result), state
        else
            let noRet = result.Head.Type <> typeof<IDynamic>
            let result = if noRet then result else (exp.Return(label.Target, result.Head, typeof<IDynamic>):>exp)::result.Tail
            let result = (label:>exp)::result |> List.rev
            exp.Block(typeof<IDynamic>, result), state            
         
    and evalProgram (state:State) =
        match state.element with
        | Program e ->
            match e with
            | Nil ->
                exp.Block([| exp.Empty() :> exp |]) , state 
            | SourceElements (_, _) ->
                evalSourceElements { state with element = e }
                        
    and performDeclarationBinding (configurableBindings:bool) (state:State) (continuation:Code) (environment:IEnvironment) (args:IArgs) =
        let env = environment.Context.VariableEnviroment.Record
        for name in state.variables |> List.rev do
            if not (env.HasBinding name) then
                env.CreateMutableBinding (name, configurableBindings)
                env.SetMutableBinding (name, environment.Undefined, state.strict)
        for f in state.functions do
            let code = lazy(compileFunctionCode(f.formalParameterList, f.functionBody))
            let fo = environment.CreateFunction(f.formalParameterList, f.strict, code)
            if not (env.HasBinding f.identifier) then
                env.CreateMutableBinding (f.identifier, configurableBindings)
            env.SetMutableBinding (f.identifier, fo, f.strict)            
        continuation.Invoke (environment, args)
  
    and performFunctionArgumentsBinding (formalParameterList:ReadOnlyList<string>) (state:State) (continuation:Code) (environment:IEnvironment) (args:IArgs) =
        let i = ref -1
        let env = environment.Context.VariableEnviroment.Record
        for name in formalParameterList do
            incr i
            if not (env.HasBinding name) then
                env.CreateMutableBinding (name, false)
            env.SetMutableBinding (name, args.[!i], state.strict)
        if not (env.HasBinding "arguments") then
            let argsObj = environment.CreateArguments(formalParameterList, args, state.strict)
            if state.strict then
                (env:?>IDeclarativeEnvironmentRecord).CreateImmutableBinding ("arguments")
                (env:?>IDeclarativeEnvironmentRecord).InitializeImmutableBinding ("arguments", argsObj)
            else
                env.CreateMutableBinding ("arguments", false)
                env.SetMutableBinding ("arguments", argsObj, false)                          
        continuation.Invoke (environment, args)           

    and compileGlobalCode (input:string) =
        let input = Parser.parse (input + ";")
        let returnLabel = exp.Label(exp.Label(typeof<dyn>, "return"), getUndefined)
        let body, state = evalProgram { strict = false; element = input; labels = [ Map.ofList [ ("return", returnLabel) ] ]; functions = []; variables = [] }
        let continuation = exp.Lambda<Code>(body, [| environmentParam; argsParam |]).Compile()
        Code(performDeclarationBinding true state continuation)
        
    and compileEvalCode (input:string) =
        let input = Parser.parse (input + ";")
        let returnLabel = exp.Label(exp.Label(typeof<dyn>, "return"), getUndefined)
        let body, state = evalProgram { strict = false; element = input; labels =  [ Map.ofList [ ("return", returnLabel) ] ]; functions = []; variables = [] }
        let continuation = exp.Lambda<Code>(block [| body:>exp; returnLabel |], [| environmentParam; argsParam |]).Compile()
        Code(performDeclarationBinding false state continuation)

    and compileFunctionCode (formalParameterList:ReadOnlyList<string>, functionBody:SourceElement) =
        let returnLabel = exp.Label(exp.Label(typeof<dyn>, "return"), getUndefined)
        let state = { strict = false; element = functionBody; labels = [ Map.ofList [ ("return", returnLabel) ] ]; functions = []; variables = [] }
        let body, state = evalFunctionBody state
        let continuation = exp.Lambda<Code>(body, [| environmentParam; argsParam |]).Compile()
        let continuation = Code(performFunctionArgumentsBinding formalParameterList state continuation)
        Code(performDeclarationBinding true state continuation)

    member this.CompileGlobalCode (input:string) =
        compileGlobalCode input

    member this.CompileEvalCode (input:string) =
        compileEvalCode input

    member this.CompileFunctionCode (formalParameterList:ReadOnlyList<string>, functionBody:SourceElement) =
        compileFunctionCode (formalParameterList, functionBody)

    member this.CompileFunctionCode (formalParameterList:ReadOnlyList<string>, functionBody:string) =
        let functionBody = 
            let r = Parser.parse functionBody
            match r with
            | Program e -> e
        compileFunctionCode (formalParameterList, FunctionBody(functionBody))

    static member private equalityTest (left:obj, right:obj) =
        let left = left :?> IDynamic
        let right = right :?> IDynamic
        left.Op_StrictEquals(right).ConvertToBoolean().BaseValue
