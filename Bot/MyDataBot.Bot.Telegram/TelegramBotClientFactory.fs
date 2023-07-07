namespace MyDataBot.Bot.Telegram

open System.Net.Http
open FsToolkit.ErrorHandling
open MyDataBot.Bot.Dependencies.BotLifecycle
open MyDataBot.Bot.Dependencies.Data
open MyDataBot.Bot.Core.Common.TelegramBotCoreModels
open Telegram.Bot
open Telegram.Bot.Exceptions
open Telegram.Bot.Types.Enums

type private TelegramBotClientWrapper(client: ITelegramBotClient) =
    interface MyDataBot.Bot.Dependencies.BotLifecycle.ITelegramBotClient with
        member this.SetWebhookAsync (url: string) (secret: TelegramBotSecret) =
            client.SetWebhookAsync(url = url, secretToken = secret.Value, allowedUpdates = [| UpdateType.Message |])

        member this.DeleteWebhookAsync() = client.DeleteWebhookAsync()

        member this.SendTextMessageAsync (chatId) (text) =
            task {
                try
                    let! _ = client.SendTextMessageAsync(chatId = chatId.Value, text = text)
                    return Ok()
                with :? ApiRequestException as ex ->
                    return Error(SendTelegramMessageError.ApiRequestException(ex))
            }

        member this.SetChatTypingAsync chatId =
            task {
                try
                    let! _ = client.SendChatActionAsync(chatId = chatId.Value, chatAction = ChatAction.Typing)
                    return Ok()
                with :? ApiRequestException as ex ->
                    return Error(SetTelegramChatTypingError.ApiRequestException(ex))
            }



type TelegramClientFactory(httpClient: HttpClient, botRepo: ITelegramBotRepository) =
    let create metadata =
        let options = TelegramBotClientOptions(metadata.Token.Value)
        let client = TelegramBotClient(options, httpClient) :> ITelegramBotClient
        TelegramBotClientWrapper(client) :> MyDataBot.Bot.Dependencies.BotLifecycle.ITelegramBotClient

    interface ITelegramClientFactory with
        member this.Create(metadata) = metadata |> create

        member this.CreateAsync ct id =
            taskResult {
                let! metadata =
                    id
                    |> botRepo.TryGetBotMetadata ct
                    |> TaskResult.requireSome TelegramClientCreationError.NotFound

                return metadata |> create
            }
