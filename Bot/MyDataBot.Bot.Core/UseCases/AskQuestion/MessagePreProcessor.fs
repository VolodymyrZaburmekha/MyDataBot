namespace MyDataBot.Bot.Core.UseCases.AskQuestion.MessagePreProcessor

open MyDataBot.Bot.Core.Common.Message

[<RequireQualifiedAccess>]
type InvalidMessageType = | Empty

[<RequireQualifiedAccess>]
type MessageCommand = | Start

[<RequireQualifiedAccess>]
type MessageType =
    | Invalid of InvalidMessageType * IncomingBotMessage
    | Command of MessageCommand
    | AiQuestion of IncomingBotMessage

module MessagePreProcessor =
    let getType (message: IncomingBotMessage) =
        match message.Text with
        | null
        | "" -> MessageType.Invalid(InvalidMessageType.Empty, message)
        | start when start.ToLower() = "/start" -> MessageType.Command(MessageCommand.Start)
        | _ -> MessageType.AiQuestion message
