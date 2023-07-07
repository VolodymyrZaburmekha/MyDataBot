namespace MyDataBot.Bot.Dependencies.Data

open System.Threading
open System.Threading.Tasks
open MyDataBot.Bot.Core.Common.CommonBotModels
open MyDataBot.Bot.Core.UseCases.AskQuestion

type IQuotaRepository =
    abstract member TryGetQuotaByBotId: ct: CancellationToken -> botId: BotId -> Task<Quota option>
