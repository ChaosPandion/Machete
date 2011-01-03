namespace Machete.Json

open Machete.Compiler

type Position = {
    line : int64
    column : int64
} 

type Token =
| JsonWhiteSpace of char
| JsonString of Token
| JsonStringCharacters of Token * Token
| JsonStringCharacter of char * Token
| JsonEscapeSequence of Token
| JsonEscapeCharacter of char
| JsonNumber of char option * InputElement * Token * InputElement
| JsonFraction of InputElement
| JsonNullLiteral of string
| JsonBooleanLiteral of string
| JsonInputElement of InputElement
| BeginArray
| EndArray
| BeginObject
| EndObject
| NameSeparator
| ValueSeparator
| JsonNil
| End


