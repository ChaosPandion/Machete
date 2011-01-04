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
    | ReadLine of AsyncReplyChannel<string>

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
                    | ReadLine (channel) ->
                        channel.Reply (Console.ReadLine())
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
        
    let readLine () =
        agent.PostAndReply (fun channel -> ReadLine channel)
