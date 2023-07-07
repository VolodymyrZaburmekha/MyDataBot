namespace MyDataBot.Bot.Dependencies.AsyncBotMessageProcessing

open System.Threading.Tasks
open MyDataBot.Bot.Common.ValueObjects.Ids
open MyDataBot.Bot.Core.Common.Message

type IProcessMessageQueue =
    abstract member PushToQueue: correlationId: CorrelationId -> message: MessageForProcessing -> Task<unit>
    abstract member SubscribeForProcessed: handler: (CorrelationId -> AiProcessedMessage -> Task<bool>) -> unit
