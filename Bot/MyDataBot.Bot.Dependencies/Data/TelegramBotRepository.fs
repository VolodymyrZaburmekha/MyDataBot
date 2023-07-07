namespace MyDataBot.Bot.Dependencies.Data

open System.Threading
open System.Threading.Tasks
open MyDataBot.Bot.Core.Common.TelegramBotCoreModels

type ITelegramBotRepository =
    abstract member TryGetBotMetadata: ct: CancellationToken -> botId: TelegramBotId -> Task<TelegramBotMetadata Option>
 