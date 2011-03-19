namespace Machete.Parser

type InputElement =
| WhiteSpace of char * ElementData
| LineTerminator of char * ElementData
| MultiLineComment of string * bool * ElementData
| SingleLineComment of string * ElementData
| IdentifierName of string * ElementData
| NullLiteral of ElementData
| BooleanLiteral of bool * ElementData
| NumericLiteral of double * ElementData
| StringLiteral of string * ElementData
| RegularExpressionLiteral of string * string * ElementData