namespace MyDataBot.Bot.Application.Commands

open System
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.TaskResult
open MediatR
open Microsoft.Extensions.Logging
open MyDataBot.Bot.Common.Utils
open MyDataBot.Bot.Core.Common.BotDataFunctions
open MyDataBot.Bot.Core.Common.Message
open MyDataBot.Bot.Dependencies.Data

type ProcessAiResponseCommandResponse = unit

type ProcessAiResponseCommand = Command<AiProcessedMessage, ProcessAiResponseCommandResponse>

type ProcessAiResponseCommandHandler
    (mediator: IMediator, messageLog: IMessageLog, logger: ILogger<ProcessAiResponseCommandHandler>) =

    interface IRequestHandler<ProcessAiResponseCommand, ProcessAiResponseCommandResponse> with
        member this.Handle(request, cancellationToken) =
            let toMessageCommandSendTo metadata =
                match metadata with
                | MessangerMetadata.Telegram(botId, chatId) -> SendMessageTo.Telegram { BotId = botId; ChatId = chatId }

            let response =
                request.Data
                |> BotMessageResponse.fromAiProcessedMessage
                    (BotMessageResponseId(Ulid.NewUlid()))
                    request.DateTimeUtc
                    true

            taskResult {
                Console.WriteLine("PROCESS")
                let! sendMetadata = messageLog.GetMetadataForResponse cancellationToken request.Data.MessageId

                let sendMsgCommand: SendMessageCommand =
                    Command.createFrom
                        request
                        { SendTo = toMessageCommandSendTo sendMetadata
                          Text = response.Text }

                do! mediator.Send(sendMsgCommand, cancellationToken)

                do! response |> messageLog.SaveResponse cancellationToken
                return ()
            }
            |> TaskResultUtils.onErrorDoTask (fun e ->
                task {
                    logger.LogError(
                        "{CorrelationId} | ProcessAiResponse failed {error} ",
                        request.CorrelationId.ValueStr,
                        e
                    )

                    do! { response with Delivered = false } |> messageLog.SaveResponse cancellationToken
                    return ()
                })
            |> TaskResult.ignoreError
