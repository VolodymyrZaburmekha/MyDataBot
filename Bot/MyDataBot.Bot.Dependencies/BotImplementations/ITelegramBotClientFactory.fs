namespace MyDataBot.Bot.Dependencies.BotLifecycle

open System
open System.Threading
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open MyDataBot.Bot.Core.Common.TelegramBotCoreModels

[<RequireQualifiedAccess>]
type SendTelegramMessageError = ApiRequestException of Exception

[<RequireQualifiedAccess>]
type SetTelegramChatTypingError = ApiRequestException of Exception

type ITelegramBotClient =
    abstract member SetWebhookAsync: url: string -> secret: TelegramBotSecret -> Task
    abstract member DeleteWebhookAsync: unit -> Task

    abstract member SendTextMessageAsync:
        chatId: TelegramChatId -> text: string -> TaskResult<unit, SendTelegramMessageError>

    abstract member SetChatTypingAsync: chatId: TelegramChatId -> TaskResult<unit, SetTelegramChatTypingError>


[<RequireQualifiedAccess>]
type TelegramClientCreationError = | NotFound

type ITelegramClientFactory =
    abstract member CreateAsync:
        ct: CancellationToken -> id: TelegramBotId -> Task<Result<ITelegramBotClient, TelegramClientCreationError>>

    abstract member Create: metadata: TelegramBotMetadata -> ITelegramBotClient
