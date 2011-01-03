namespace Machete.Compiler
    
type InputElement =
| Nil
| Chr of char
| Str of string
| WhiteSpace
| LineTerminator
| LineTerminatorSequence
| Comment of InputElement
| MultiLineComment of string
| SingleLineComment of string
| Token of InputElement
| Identifier of InputElement
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
| Punctuator of InputElement
| DivPunctuator of InputElement
| Literal of InputElement
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
| RegularExpressionLiteral of InputElement * InputElement
| RegularExpressionBody of InputElement * InputElement
| RegularExpressionChars of InputElement * InputElement
| RegularExpressionFirstChar of InputElement
| RegularExpressionChar of InputElement
| RegularExpressionBackslashSequence of InputElement
| RegularExpressionNonTerminator of char
| RegularExpressionClass of InputElement
| RegularExpressionClassChars of InputElement * InputElement
| RegularExpressionClassChar of InputElement
| RegularExpressionFlags of InputElement * InputElement

