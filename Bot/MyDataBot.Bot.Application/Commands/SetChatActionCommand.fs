namespace MyDataBot.Bot.Application.Commands

open System
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open MediatR
open Microsoft.Extensions.Logging
open MyDataBot.Bot.Core.Common.CommonBotModels
open MyDataBot.Bot.Core.Common.TelegramBotCoreModels
open MyDataBot.Bot.Dependencies.BotLifecycle
open MyDataBot.Bot.Common.Utils


[<RequireQualifiedAccess>]
type SetChatActionError =
    | Exception of Exception
    | BotNotFound of BotId

type SetChatActionCommandResponse = Result<unit, SetChatActionError>

type SetChatActionTelegramMetadata =
    { BotId: TelegramBotId
      ChatId: TelegramChatId }

[<RequireQualifiedAccess>]
type SetChatActionBot = Telegram of SetChatActionTelegramMetadata

[<RequireQualifiedAccess>]
type ChatAction = | Typing

type SetChatActionData =
    { Bot: SetChatActionBot
      Action: ChatAction }

type SetChatActionCommand = Command<SetChatActionData, SetChatActionCommandResponse>

type SetChatActionCommandHandler(clientFactory: ITelegramClientFactory, logger: ILogger<SetChatActionCommandHandler>) =
    interface IRequestHandler<SetChatActionCommand, SetChatActionCommandResponse> with
        member this.Handle(request, cancellationToken) =
            taskResult {
                match request.Data.Bot with
                | SetChatActionBot.Telegram tgMetadata ->
                    let! client =
                        tgMetadata.BotId
                        |> clientFactory.CreateAsync cancellationToken
                        |> TaskResult.mapError (fun e ->
                            match e with
                            | TelegramClientCreationError.NotFound ->
                                SetChatActionError.BotNotFound(BotId.TelegramBotId tgMetadata.BotId))

                    do!
                        client.SetChatTypingAsync tgMetadata.ChatId
                        |> TaskResult.mapError (fun error ->
                            match error with
                            | SetTelegramChatTypingError.ApiRequestException ex -> SetChatActionError.Exception ex)

                    return ()
            }
            |> TaskResultUtils.onErrorDo (fun error ->
                match error with
                | SetChatActionError.BotNotFound id ->
                    logger.LogWarning("{CorrelationId} | SendChatActionError bot not found: {BotId}", id)
                | SetChatActionError.Exception ex ->
                    logger.LogWarning("{CorrelationId} | SendChatActionError exception occured: {Exception}", ex))
