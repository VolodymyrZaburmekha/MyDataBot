namespace MyDataBot.Bot.Nats

open System
open System.Collections.Generic
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Core
open MyDataBot.Bot.Common.ValueObjects.Ids
open MyDataBot.Bot.Core.Common.CommonBotModels
open MyDataBot.Bot.Core.Common.Message
open MyDataBot.Bot.Dependencies.AsyncBotMessageProcessing
open NATS.Client.JetStream
open MyDataBot.Bot.Nats.Nats.NatsEncoding

module private NatsConstants =
    [<Literal>]
    let MessageProcessStream = "MESSAGE_PROCESSING"

    module NewMessage =
        [<Literal>]
        let NewMessageToProcessSubject = "new-message-to-process"

    module ProcessedMessage =
        [<Literal>]
        let MessageProcessedSubject = "message-processed"

        [<Literal>]
        let MessageProcessedQueue = "bot-response-queue"

        [<Literal>]
        let MessageProcessedDurable = MessageProcessedQueue

module Dtos =
    type MessageForProcessingDto =
        { CorrelationId: Ulid
          MessageId: Ulid
          MessageText: string
          DataFolder: string }

    type ProcessedMessageDto =
        { CorrelationId: Ulid
          MessageId: Ulid
          MessageText: string
          MessageResponse: string
          ResultType: string }

    let newMessageToDto (correlationId: CorrelationId) (msg: MessageForProcessing) =
        { CorrelationId = correlationId.Value
          MessageId = msg.MessageId.Value
          MessageText = msg.MessageText
          DataFolder = msg.DataFolder.Value }
        : MessageForProcessingDto

    let processedMessageToDomain (dto: ProcessedMessageDto) =
        { MessageId = MessageId dto.MessageId
          MessageText = dto.MessageText
          Result =
            match dto.ResultType with
            | null
            | ""
            | "Ok" -> Ok dto.MessageResponse
            | "RateLimitError" -> Result.Error AiProcessError.RateLimitError
            | "UnknownError" -> Result.Error(AiProcessError.Unknown "UnknownError")
            | str -> Result.Error(AiProcessError.Unknown str) }
        : AiProcessedMessage

type NatsProcessMessageQueue(url: string option, logger: ILogger<NatsProcessMessageQueue>) =
    let locker = Object()
    let subscribers = List<IJetStreamPushAsyncSubscription>()

    let connection =
        lazy
            (let conn = Nats.createConnection url

             let _ =
                 Nats.createStream
                     conn
                     NatsConstants.MessageProcessStream
                     [ NatsConstants.NewMessage.NewMessageToProcessSubject
                       NatsConstants.ProcessedMessage.MessageProcessedSubject ]

             conn)

    let stream = lazy (connection.Value.CreateJetStreamContext())


    member private this.addSubscriber subscriber =
        lock locker (fun () -> subscribers.Add(subscriber))
        ()

    interface IProcessMessageQueue with

        member this.PushToQueue (correlationId) (message) =
            let msg =
                message
                |> Dtos.newMessageToDto correlationId
                |> createObjectEncodedMsg NatsConstants.NewMessage.NewMessageToProcessSubject

            let _ = connection.Force().Publish(msg)
            () |> Task.FromResult

        member this.SubscribeForProcessed(handler) =
            let handlerWrapper dto =
                let processedMessage = dto |> Dtos.processedMessageToDomain
                let correlationId = CorrelationId dto.CorrelationId
                handler correlationId processedMessage

            let errorHandler (ex: Exception) =
                logger.LogError("Can't process message {Exception}", ex)
                Task.FromResult true

            let subscriber =
                Nats.createSubscriber
                    handlerWrapper
                    errorHandler
                    (stream.Force())
                    NatsConstants.ProcessedMessage.MessageProcessedSubject
                    NatsConstants.ProcessedMessage.MessageProcessedQueue
                    (Some NatsConstants.ProcessedMessage.MessageProcessedDurable)

            this.addSubscriber subscriber
