namespace Machete

open System
open Machete.Interfaces
open Machete.Runtime
open Machete.Compiler

type internal Message =
| ExecuteScript of string * AsyncReplyChannel<obj>
| RegisterOutputHandler of Action<string> 

type Engine () = 

    let environment = new Environment()
    let compiler = new Compiler(environment)
    let handlers = Microsoft.FSharp.Collections.HashMultiMap<Action<string>, MailboxProcessor<Action<string>>>(HashIdentity.Structural)


    let checkOutput (inbox:MailboxProcessor<Action<string>>) = async {
        do! Async.SwitchToNewThread ()
        let! handler = inbox.Receive ()
        while true do
            try
                let str = environment.Output.Take()
                handler.Invoke str
            with | e -> ()
    }  

    let proccessMessages (inbox:MailboxProcessor<Message>) = async {
        do! Async.SwitchToNewThread ()
        while true do
            try
                let! msg = inbox.Receive ()
                match msg with
                | RegisterOutputHandler (handler) ->
                    let agent = MailboxProcessor.Start checkOutput
                    agent.Post handler
                    handlers.Add (handler, agent)    
                | ExecuteScript (script, channel) ->
                    try
                        let r = compiler.CompileGlobalCode(script)
                        let r = r.Invoke(environment, environment.EmptyArgs)
                        let r = 
                            match r.Value with
                            | :? INull as r -> null :> obj
                            | :? IBoolean as r -> r.BaseValue :> obj
                            | :? INumber as r -> r.BaseValue :> obj
                            | :? IString as r -> r.BaseValue :> obj
                            | _ -> r.ToString() :> obj
                        channel.Reply r
                    with | e ->
                        channel.Reply e
            with | e -> ()
    }
    
    let agent = lazy(MailboxProcessor.Start proccessMessages)

    let buildExecuteScriptMessage script channel = 
        ExecuteScript (script, channel)


    member this.ExecuteScript (script:string) =
        agent.Value.PostAndReply (buildExecuteScriptMessage script)

    member this.ExecuteScript (script:string, timeout:int) =
        agent.Value.PostAndReply (buildExecuteScriptMessage script, timeout)
        
    member this.ExecuteScriptAsync (script:string) =
        agent.Value.PostAndAsyncReply (buildExecuteScriptMessage script)

    member this.ExecuteScriptAsync (script:string, timeout:int) =
        agent.Value.PostAndAsyncReply (buildExecuteScriptMessage script, timeout)
            
    member this.ExecuteScriptAsTask (script:string) =
        agent.Value.PostAndAsyncReply (buildExecuteScriptMessage script) |> Async.StartAsTask 

    member this.ExecuteScriptAsTask (script:string, timeout:int) =
        agent.Value.PostAndAsyncReply (buildExecuteScriptMessage script, timeout) |> Async.StartAsTask 
        
    member this.RegisterOutputHandler (handler:Action<string>) =
        agent.Value.Post (RegisterOutputHandler handler)