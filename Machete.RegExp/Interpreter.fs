namespace Machete.RegExp

module Interpreter =

    let execute (info:RegExpInfo) (input:string) (index:int) = 
        let root = TreeNode
        let rec execute node =
            match node with
            | Pattern (captureCount, disjunction) -> ()
            | Disjunction (alternative, disjunction) -> ()
            | Alternative (term, alternative) -> ()
            | Term term -> ()
    
        ()

