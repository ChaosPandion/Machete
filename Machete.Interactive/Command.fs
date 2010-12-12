namespace Machete.Interactive

type Command =
| GetTimeout
| SetTimeout of int
| Echo of string


