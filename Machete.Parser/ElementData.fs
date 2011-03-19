namespace Machete.Parser

type ElementData = {
    line : int64
    column : int64
} with
    override x.ToString () =
        System.String.Format("(Ln {0}, Col {1})", x.line, x.column)

