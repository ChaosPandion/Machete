namespace Machete.Compiler

    open Lexer

    type PostfixOperator = 
    | Nil
    | Increment
    | Decrement

    type UnaryOperator = 
    | Nil
    | Delete
    | Void
    | Typeof
    | Increment
    | Decrement
    | Plus
    | Minus
    | BitwiseNot
    | LogicalNot

    type MultiplicativeOperator =
    | Nil 
    | Multiply
    | Divide
    | Modulus

    type AdditiveOperator = 
    | Nil
    | Plus
    | Minus

    type BitwiseShiftOperator =
    | Nil 
    | LeftShift
    | SignedRightShift
    | UnsignedRightShift

    type RelationalOperator = 
    | Nil
    | LessThan
    | GreaterThan
    | LessThanOrEqual
    | GreaterThanOrEqual
    | Instanceof
    | In

    type EqualityOperator = 
    | Nil
    | Equal
    | DoesNotEqual
    | StrictEqual
    | StrictDoesNotEqual

    type AssignmentOperator =
    | Nil
    | Equal
    | MultiplyEqual
    | DivideEqual
    | ModulusEqual
    | PlusEqual
    | MinusEqual
    | LeftShiftEqual
    | RightShiftEqual
    | UnsignedRightShiftEqual
    | BitwiseAndEqual
    | BitwiseXorEqual
    | BitwiseOrEqual

    type NewOperator = 
    | Nil
    | New

    type SourceElement =
    | Nil
    | InputElement of InputElement
    | Expression of SourceElement * SourceElement
    | ExpressionNoIn of SourceElement * SourceElement
    | AssignmentOperator of InputElement
    | AssignmentExpression of SourceElement * SourceElement * SourceElement
    | AssignmentExpressionNoIn of SourceElement * SourceElement * SourceElement
    | ConditionalExpression of SourceElement * SourceElement * SourceElement
    | ConditionalExpressionNoIn of SourceElement * SourceElement * SourceElement
    | LogicalORExpression of SourceElement * SourceElement
    | LogicalORExpressionNoIn of SourceElement * SourceElement
    | LogicalANDExpression of SourceElement * SourceElement
    | LogicalANDExpressionNoIn of SourceElement * SourceElement
    | BitwiseORExpression of SourceElement * SourceElement
    | BitwiseORExpressionNoIn of SourceElement * SourceElement
    | BitwiseXORExpression of SourceElement * SourceElement
    | BitwiseXORExpressionNoIn of SourceElement * SourceElement
    | BitwiseANDExpression of SourceElement * SourceElement
    | BitwiseANDExpressionNoIn of SourceElement * SourceElement
    | EqualityExpression of SourceElement * EqualityOperator * SourceElement
    | EqualityExpressionNoIn of SourceElement * EqualityOperator * SourceElement
    | RelationalExpression of SourceElement * RelationalOperator * SourceElement
    | RelationalExpressionNoIn of SourceElement * RelationalOperator * SourceElement
    | ShiftExpression of SourceElement * BitwiseShiftOperator * SourceElement
    | AdditiveExpression of SourceElement * AdditiveOperator * SourceElement
    | MultiplicativeExpression of SourceElement * MultiplicativeOperator * SourceElement
    | UnaryExpression of UnaryOperator * SourceElement
    | PostfixExpression of SourceElement * PostfixOperator

    | MemberExpression of SourceElement * SourceElement
    | MemberExpressionRest of SourceElement * SourceElement

    | Arguments of SourceElement
    | ArgumentList of SourceElement * SourceElement

    | CallExpression of SourceElement * SourceElement * SourceElement
    | CallExpressionRest of SourceElement * SourceElement

    | NewExpression of SourceElement
    | LeftHandSideExpression of SourceElement
    | PrimaryExpression of SourceElement
    | ObjectLiteral of SourceElement
    | PropertyNameAndValueList of SourceElement * SourceElement
    | PropertyAssignment of SourceElement * SourceElement * SourceElement 
    | PropertyName of InputElement
    | PropertySetParameterList of InputElement
    | ArrayLiteral of SourceElement * SourceElement
    | Elision of SourceElement
    | ElementList of SourceElement * SourceElement * SourceElement     
    | Statement of SourceElement
    | Block of SourceElement
    | StatementList of SourceElement * SourceElement
    | VariableStatement of SourceElement
    | VariableDeclarationList of SourceElement * SourceElement
    | VariableDeclarationListNoIn of SourceElement * SourceElement
    | VariableDeclaration of InputElement * SourceElement
    | VariableDeclarationNoIn of InputElement * SourceElement
    | Initialiser of SourceElement
    | InitialiserNoIn of SourceElement
    | EmptyStatement
    | ExpressionStatement of SourceElement
    | IfStatement of SourceElement * SourceElement * SourceElement 
    | IterationStatement of SourceElement * SourceElement * SourceElement * SourceElement
    | ContinueStatement of InputElement
    | BreakStatement of InputElement
    | ReturnStatement of SourceElement
    | WithStatement of SourceElement * SourceElement
    | SwitchStatement of SourceElement * SourceElement
    | CaseBlock of SourceElement * SourceElement * SourceElement
    | CaseClauses of SourceElement * SourceElement
    | CaseClause of SourceElement * SourceElement
    | DefaultClause of SourceElement
    | LabelledStatement of InputElement * SourceElement
    | ThrowStatement of SourceElement
    | TryStatement of SourceElement * SourceElement * SourceElement
    | Catch of InputElement * SourceElement
    | Finally of SourceElement
    | DebuggerStatement    
    | FunctionDeclaration of InputElement * SourceElement * SourceElement
    | FunctionExpression of InputElement * SourceElement * SourceElement
    | FormalParameterList of SourceElement * SourceElement
    | FunctionBody of SourceElement    
    | SourceElement of SourceElement
    | SourceElements of SourceElement * SourceElement
    | Program of SourceElement

