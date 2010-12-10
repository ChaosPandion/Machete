namespace Machete

open Machete.Runtime

type internal Message =
| ExecuteScript of string * AsyncReplyChannel<obj>

type Engine () =   

    let proccessMessages (inbox:MailboxProcessor<Message>) = async {
        do! Async.SwitchToNewThread ()
        let environment = Environment.Instance.Value
        while true do
            let! msg = inbox.Receive ()
            match msg with
            | ExecuteScript (script, channel) ->
                channel.Reply (environment.ExecuteScript script)
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