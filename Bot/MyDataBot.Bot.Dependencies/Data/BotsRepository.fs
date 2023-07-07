namespace MyDataBot.Bot.Dependencies.Data

open System.Threading
open System.Threading.Tasks
open MyDataBot.Bot.Core.Common.CommonBotModels

type IBotsRepository =
    abstract member GetAllBots: ct: CancellationToken -> Task<Bot list>
