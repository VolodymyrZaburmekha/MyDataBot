namespace MyDataBot.Bot.Api.Controllers

open System
open MediatR
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Core
open MyDataBot.Bot.Application.Commands
open MyDataBot.Bot.Common.ValueObjects.Ids
open MyDataBot.Bot.Core.Common
open MyDataBot.Bot.Core.Common.CommonBotModels
open MyDataBot.Bot.Core.Common.TelegramBotCoreModels
open MyDataBot.Bot.Core.Common.Message
open Telegram.Bot.Types

module TelegramBotHelper =
    let tryGetSecret (request: HttpRequest) =
        request.Headers
        |> Seq.choose (fun kv ->
            match kv.Key, kv.Value with
            | "X-Telegram-Bot-Api-Secret-Token", values ->
                values |> Seq.tryFind (fun v -> not (String.IsNullOrWhiteSpace v))
            | _ -> None)
        |> Seq.tryHead


[<ApiController>]
[<Route("telegram")>]
type TelegramController(logger: ILogger<TelegramController>, correlationId: CorrelationId, mediator: IMediator) =
    inherit ControllerBase()

    [<HttpPost("bot/{botId}/register-update")>]
    member this.SubmitUpdate([<FromRoute>] botId: Ulid, [<FromBody>] update: Update) =
        task {
            match TelegramBotHelper.tryGetSecret this.Request with
            | Some secret ->
                let message =
                    { Id = CommonBotModels.MessageId(Ulid.NewUlid())
                      ReceivedAtUtc = DateTime.UtcNow
                      Text = update.Message.Text
                      SpecificMetadata =
                        BotMessageMetadata.TelegramMessageMetadata
                            { BotId = TelegramBotId botId
                              Id = TelegramMessageInternalId update.Message.MessageId
                              UpdateId = TelegramUpdateId update.Id
                              ChatId = TelegramChatId update.Message.Chat.Id
                              UserName = TelegramUserName update.Message.From.Username
                              LanguageCode = update.Message.From.LanguageCode
                              Secret = TelegramBotSecret secret } }

                let command: SendNewBotMessageForProcessingCommand =
                    { CorrelationId = correlationId
                      DateTimeUtc = DateTime.UtcNow
                      Data = message }

                let! _ = mediator.Send(command)

                return this.Ok() :> IActionResult
            | None ->
                logger.LogWarning("empty telegram secret", update)
                return this.Forbid() :> IActionResult
        }

