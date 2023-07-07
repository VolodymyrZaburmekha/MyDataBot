namespace MyDataBot.Bot.Dependencies.Data

open System.Threading
open System.Threading.Tasks
open MyDataBot.Bot.Core.Common.CommonBotModels
open MyDataBot.Bot.Core.Common.TelegramBotCoreModels
open MyDataBot.Bot.Core.Common.Message

[<RequireQualifiedAccess>]
type MessangerMetadata = Telegram of botId: TelegramBotId * chatId: TelegramChatId

type IMessageLog =
    abstract member GetMetadataForResponse: ct: CancellationToken -> messageId: MessageId -> Task<MessangerMetadata>
    abstract member SaveMessage: ct: CancellationToken -> isForAi: bool -> message: IncomingBotMessage -> Task
    abstract member SaveResponse: ct: CancellationToken -> response: BotMessageResponse -> Task
