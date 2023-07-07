namespace MyDataBot.Bot.Application.Commands

open System
open System.Threading.Tasks
open MediatR
open FsToolkit.ErrorHandling
open Microsoft.Extensions.Logging
open MyDataBot.Bot.Common.Utils
open MyDataBot.Bot.Core.Common.CommonBotModels
open MyDataBot.Bot.Core.Common.TelegramBotCoreModels
open MyDataBot.Bot.Dependencies.BotLifecycle

type SendMessageToTelegramMetadata =
    { BotId: TelegramBotId
      ChatId: TelegramChatId }

[<RequireQualifiedAccess>]
type SendMessageTo = Telegram of SendMessageToTelegramMetadata

type SendMessageData = { SendTo: SendMessageTo; Text: string }

[<RequireQualifiedAccess>]
type SendBotMessageError =
    | BotNotFound of BotId
    | TelegramError of SendTelegramMessageError

type SendBotMessageCommandResponse = Result<unit, SendBotMessageError>

type SendMessageCommand = Command<SendMessageData, SendBotMessageCommandResponse>

type SendTelegramBotMessageCommandHandler
    (logger: ILogger<SendTelegramBotMessageCommandHandler>, telegramClientFactory: ITelegramClientFactory) =
    interface IRequestHandler<SendMessageCommand, SendBotMessageCommandResponse> with
        member this.Handle(request, cancellationToken) =
            taskResult {
                match request.Data.SendTo with
                | SendMessageTo.Telegram tgMetadata ->
                    let! client =
                        tgMetadata.BotId
                        |> telegramClientFactory.CreateAsync cancellationToken
                        |> TaskResult.mapError (fun e ->
                            match e with
                            | TelegramClientCreationError.NotFound ->
                                SendBotMessageError.BotNotFound(BotId.TelegramBotId tgMetadata.BotId))

                    let data = request.Data

                    let! _ =
                        client.SendTextMessageAsync tgMetadata.ChatId data.Text
                        |> TaskResult.mapError SendBotMessageError.TelegramError

                    return ()
            // add more messengers here
            }
            |> TaskResultUtils.onErrorDo (fun error ->
                match error with
                | SendBotMessageError.TelegramError tgError ->
                    match tgError with
                    | SendTelegramMessageError.ApiRequestException ex ->
                        logger.LogError(
                            "{CorrelationId} | SendTelegramMessageError Request: {Request} Exception: {Exception}",
                            request.CorrelationId.ValueStr,
                            request,
                            ex
                        )
                | SendBotMessageError.BotNotFound id ->
                    logger.LogWarning("{CorrelationId} | BotNotFound : {BotId}", id))
