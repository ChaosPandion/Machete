namespace Machete.Interactive

open System

module ConsoleAgent =

    type Position = {
        top : int
        left : int
    }

    type RequestAction =
    | Write of obj * ConsoleColor
    | WriteLine of obj * ConsoleColor
    | GetPosition of AsyncReplyChannel<Position>
    | SetPosition of Position
    | ReadKey of AsyncReplyChannel<ConsoleKeyInfo>

    let processMessage (inbox:MailboxProcessor<RequestAction>) =
        async {
            while true do
                try
                    let! requestAction = inbox.Receive()
                    match requestAction with
                    | Write (value, color) ->
                        let oldColor = Console.ForegroundColor
                        Console.ForegroundColor <- color
                        Console.Write (value)
                        Console.ForegroundColor <- oldColor
                    | WriteLine (value, color) ->
                        let oldColor = Console.ForegroundColor
                        Console.ForegroundColor <- color
                        Console.WriteLine (value)
                        Console.ForegroundColor <- oldColor
                    | GetPosition (channel) ->
                        channel.Reply ({ top = Console.CursorTop; left = Console.CursorLeft })
                    | SetPosition (position) ->
                        Console.CursorTop <- position.top
                        Console.CursorLeft <- position.left
                    | ReadKey (channel) ->
                        channel.Reply (Console.ReadKey(true))
                with | e -> ()
        }

    let agent = MailboxProcessor.Start processMessage
                
    let write value color =
        agent.Post (Write (value, color))
        
    let writeLine value color =
        agent.Post (WriteLine (value, color))

    let getPosition () =
        agent.PostAndReply (fun channel -> GetPosition channel)
        
    let setPosition position =
        agent.Post (SetPosition position)
        
    let readKey () =
        agent.PostAndReply (fun channel -> ReadKey channel)
