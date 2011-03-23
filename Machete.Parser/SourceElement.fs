namespace Machete.Parser

type PostfixOperator =
| Simple

type UnaryOperator =
| Simple

type MultiplicativeOperator =
| Simple

type AdditiveOperator =
| Simple

type ShiftOperator =
| Simple

type RelationalOperator =
| Simple

type EqualityOperator =
| Simple

type AssignmentOperator =
| Simple

type SourceElement =

| PrimaryExpression of SourceElement
| ArrayLiteral of SourceElement * SourceElement option
| ElementList of SourceElement option * SourceElement * SourceElement option
| Elision of SourceElement option
| ObjectLiteral of SourceElement option
| PropertyNameAndValueList of SourceElement * SourceElement option
| PropertyAssignment of SourceElement
| ValuePropertyAssignment of string * SourceElement
| GetterPropertyAssignment of string * SourceElement
| SetterPropertyAssignment of string * string * SourceElement
| MemberExpression of SourceElement * SourceElement option
| NewExpression of SourceElement
| CallExpression of SourceElement * SourceElement
| Arguments of SourceElement option
| ArgumentList of SourceElement * SourceElement option
| LeftHandSideExpression of SourceElement
| PostfixExpression of SourceElement * PostfixOperator option
| UnaryExpression of UnaryOperator option * SourceElement
| MultiplicativeExpression of SourceElement * (MultiplicativeOperator * SourceElement) option
| AdditiveExpression of SourceElement * (AdditiveOperator * SourceElement) option
| ShiftExpression of SourceElement * (ShiftOperator * SourceElement) option
| RelationalExpression of SourceElement * (RelationalOperator * SourceElement) option
| EqualityExpression of SourceElement * (EqualityOperator * SourceElement) option
| BitwiseANDExpression of SourceElement * SourceElement option
| BitwiseXORExpression of SourceElement * SourceElement option
| BitwiseORExpression of SourceElement * SourceElement option
| LogicalANDExpression of SourceElement * SourceElement option
| LogicalORExpression of SourceElement * SourceElement option
| ConditionalExpression of SourceElement * (SourceElement * SourceElement) option
| AssignmentExpression of SourceElement * (AssignmentOperator * SourceElement) option
| Expression of SourceElement * SourceElement option

| Statement
| Block
| StatementList
| VariableStatement
| VariableDeclarationList 
| VariableDeclaration
| Initializer
| EmptyStatement 
| ExpressionStatement
| IfStatement 
| IterationStatement 
| DoWhileStatement
| WhileStatement
| ForStatement
| ForInStatement 
| ContinueStatement of string option
| BreakStatement of string option
| ReturnStatement of SourceElement option

| WithStatement of SourceElement * SourceElement

| LabelledStatement of string * SourceElement 

| SwitchStatement of SourceElement * SourceElement
| CaseBlock of SourceElement option * SourceElement option * SourceElement option
| CaseClauses of SourceElement * SourceElement option 
| CaseClause of SourceElement * SourceElement option
| DefaultClause of SourceElement option

| ThrowStatement of SourceElement 

| TryStatement of SourceElement * SourceElement option * SourceElement option
| Catch of string * SourceElement
| Finally of SourceElement

| DebuggerStatement

| ForEachStatement of string * SourceElement * SourceElement * ElementData

| YieldStatement of SourceElement * ElementData
| YieldBreakStatement of ElementData
| YieldContinueStatement of SourceElement * ElementData

| FunctionDeclaration
| FunctionExpression
| LambdaExpression
| FormalParameterList
| FunctionBody
| Program of SourceElement
| SourceElements of SourceElement * SourceElement option
| SourceElement of SourceElement