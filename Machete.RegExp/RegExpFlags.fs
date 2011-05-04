namespace Machete.RegExp

open System

[<Flags>]
type RegExpFlags =
| None = 0x00000000
| Global = 0x00000001
| IgnoreCase = 0x00000002
| Multiline = 0x00000004
