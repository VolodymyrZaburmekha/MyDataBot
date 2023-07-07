module MyDataBot.Bot.Core.Common.Message

open System
open MyDataBot.Bot.Core.Common.CommonBotModels


type DataFolder =
    | DataFolder of string

    member this.Value =
        match this with
        | DataFolder folder -> folder

type MessageForProcessing =
    { MessageId: MessageId
      MessageText: string
      DataFolder: DataFolder }

[<RequireQualifiedAccess>]
type AiProcessError =
    | RateLimitError
    | Unknown of string

type AiProcessResult = Result<string, AiProcessError>

type AiProcessedMessage =
    { MessageId: MessageId
      MessageText: string
      Result: AiProcessResult }

type IncomingBotMessage =
    { Id: MessageId
      ReceivedAtUtc: DateTime
      Text: string
      SpecificMetadata: BotMessageMetadata }


type BotMessageResponseId =
    | BotMessageResponseId of Ulid

    member this.Value =
        match this with
        | BotMessageResponseId id -> id

[<RequireQualifiedAccess>]
type ResponseType =
    | NotAiOk
    | NotAiGenericError
    | NotAiAccessError
    | NotAiQuotaError
    | AiOk
    | AiRateLimitError
    | AiUnknownError

type BotMessageResponse =
    { Id: BotMessageResponseId
      IncomingMessageId: MessageId
      DateTimeUtc: DateTime
      Text: string
      Delivered: bool
      ResponseType: ResponseType }
