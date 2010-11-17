namespace Machete.RegExp

open System.Collections
open System.Collections.Generic

type Match (succeeded:bool, input:string, index:int, items:string array) =
    do
        assert(input <> null)
        assert(index >= -1)
        assert(items <> null)
    member this.Succeeded = succeeded
    member this.Input = input
    member this.Index = index
    member this.Item with get index = items.[index]
    member this.Length = items.Length
    interface IEnumerable<string> with
        member this.GetEnumerator():string IEnumerator = (items |> Seq.ofArray).GetEnumerator()
        member this.GetEnumerator():IEnumerator = items.GetEnumerator()