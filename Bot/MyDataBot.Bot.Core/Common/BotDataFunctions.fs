namespace MyDataBot.Bot.Core.Common.BotDataFunctions

open System
open MyDataBot.Bot.Core.Common.CommonBotModels
open MyDataBot.Bot.Core.Common.Message

module IncomingBotMessage =
    let getBotId message =
        match message.SpecificMetadata with
        | BotMessageMetadata.TelegramMessageMetadata tg -> BotId.TelegramBotId tg.BotId

module BotMessageResponse =
    let fromAiProcessedMessage id createdAtUtc delivered (msg: AiProcessedMessage) =
        let text, responseType =
            match msg.Result with
            | Ok txt -> txt, ResponseType.AiOk
            | Error er ->
                match er with
                | AiProcessError.Unknown _ ->
                    "виникли проблеми з вашим запитом, повторіть будь-ласка пізніше", ResponseType.AiUnknownError
                | AiProcessError.RateLimitError ->
                    "рейт ліміт перевищено, спробуйте пізніше будь ласка", ResponseType.AiRateLimitError

        { Id = id
          Delivered = delivered
          Text = text
          ResponseType = responseType
          DateTimeUtc = createdAtUtc
          IncomingMessageId = msg.MessageId }
        : BotMessageResponse
