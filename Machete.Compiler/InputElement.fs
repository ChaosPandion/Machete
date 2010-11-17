namespace Machete.Compiler

type InputElement =
| Nil
| Char of char
| WhiteSpace
| LineTerminator
| LineTerminatorSequence
| Comment
| MultiLineComment
| MultiLineCommentChars
| PostAsteriskCommentChars
| MultiLineNotAsteriskChar
| MultiLineNotForwardSlashOrAsteriskChar
| SingleLineComment
| SingleLineCommentChars
| SingleLineCommentChar
| Token

| Identifier
| IdentifierName of InputElement * InputElement
| IdentifierStart of InputElement
| IdentifierPart of InputElement
| UnicodeLetter of char
| UnicodeCombiningMark of char
| UnicodeDigit of char
| UnicodeConnectorPunctuation of char
| ReservedWord
| Keyword
| FutureReservedWord

| Punctuator
| DivPunctuator
| Literal
| NullLiteral of string
| BooleanLiteral of string

| NumericLiteral of InputElement
| DecimalLiteral of InputElement * InputElement * InputElement * InputElement
| DecimalIntegerLiteral of InputElement * InputElement
| DecimalDigits of InputElement * InputElement
| DecimalDigit of InputElement
| NonZeroDigit of InputElement
| ExponentPart of InputElement * InputElement
| ExponentIndicator of InputElement
| SignedInteger of InputElement * InputElement
| HexIntegerLiteral of InputElement * InputElement
| HexDigit of InputElement
| DecimalPoint

| StringLiteral of InputElement
| DoubleStringCharacters of InputElement * InputElement
| SingleStringCharacters of InputElement * InputElement
| DoubleStringCharacter of InputElement
| SingleStringCharacter of InputElement
| LineContinuation
| EscapeSequence of InputElement
| CharacterEscapeSequence of InputElement
| SingleEscapeCharacter of InputElement
| NonEscapeCharacter of InputElement
| EscapeCharacter of InputElement
| HexEscapeSequence of InputElement * InputElement
| UnicodeEscapeSequence of InputElement * InputElement * InputElement * InputElement


| RegularExpressionLiteral
| RegularExpressionBody
| RegularExpressionChars
| RegularExpressionFirstChar
| RegularExpressionChar
| RegularExpressionBackslashSequence
| RegularExpressionNonTerminator
| RegularExpressionClass
| RegularExpressionClassChars
| RegularExpressionClassChar
| RegularExpressionFlags


type Statement =
| Block of list<Statement> 
| VariableStatement 
| EmptyStatement 
| ExpressionStatement 
| IfStatement 
| IterationStatement 
| ContinueStatement 
| BreakStatement 
| ReturnStatement 
| WithStatement 
| LabelledStatement 
| SwitchStatement 
| ThrowStatement 
| TryStatement 
| DebuggerStatement 