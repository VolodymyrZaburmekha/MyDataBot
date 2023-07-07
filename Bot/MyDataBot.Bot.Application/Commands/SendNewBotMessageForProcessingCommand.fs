namespace MyDataBot.Bot.Application.Commands

open System
open MediatR
open Microsoft.Extensions.Logging
open MyDataBot.Bot.Core.Common.CommonBotModels
open MyDataBot.Bot.Core.Common.TelegramBotCoreModels
open MyDataBot.Bot.Core.Common.Message
open MyDataBot.Bot.Core.UseCases.AskQuestion
open MyDataBot.Bot.Core.UseCases.AskQuestion.MessagePreProcessor
open MyDataBot.Bot.Dependencies.AsyncBotMessageProcessing
open MyDataBot.Bot.Dependencies.Data
open MyDataBot.Bot.Core.Common.BotDataFunctions


[<RequireQualifiedAccess>]
type SendNewBotMessageForProcessingCommandResponse =
    | Accepted
    | ShortcutProcessed
    | Forbidden

type SendNewBotMessageForProcessingCommand = Command<IncomingBotMessage, SendNewBotMessageForProcessingCommandResponse>

type ProcessNewBotMessageCommandHandler
    (
        mediator: IMediator,
        logger: ILogger<ProcessNewBotMessageCommandHandler>,
        messageLog: IMessageLog,
        quotaRepo: IQuotaRepository,
        queue: IProcessMessageQueue
    ) =
    interface IRequestHandler<SendNewBotMessageForProcessingCommand, SendNewBotMessageForProcessingCommandResponse> with
        member this.Handle(request, cancellationToken) =
            let processShortcut messageText resultType =
                let createSendMessageCommand text : SendMessageCommand =
                    match request.Data.SpecificMetadata with
                    | BotMessageMetadata.TelegramMessageMetadata tgMetadata ->
                        Command.createFrom
                            request
                            { Text = text
                              SendTo =
                                SendMessageTo.Telegram
                                    { BotId = tgMetadata.BotId
                                      ChatId = tgMetadata.ChatId } }

                task {
                    do! messageLog.SaveMessage cancellationToken false request.Data
                    let sendMsgCommand = createSendMessageCommand messageText
                    let! sendMsgResult = mediator.Send(sendMsgCommand)

                    match sendMsgResult with
                    | Ok _ ->
                        do!
                            task {
                                let response =
                                    { Id = BotMessageResponseId(Ulid.NewUlid())
                                      Delivered = true
                                      Text = messageText
                                      DateTimeUtc = DateTime.UtcNow
                                      IncomingMessageId = request.Data.Id
                                      ResponseType = resultType }
                                    : BotMessageResponse

                                do! messageLog.SaveResponse cancellationToken response
                            }

                        return SendNewBotMessageForProcessingCommandResponse.ShortcutProcessed
                    | Error e ->
                        logger.LogError("Shortcut process {Error} for command {SendMessageCommand}", e, sendMsgCommand)
                        return SendNewBotMessageForProcessingCommandResponse.Accepted
                }

            task {
                match MessagePreProcessor.getType request.Data with
                | MessageType.Command cmd ->
                    match cmd with
                    | MessageCommand.Start -> return! processShortcut "привіт!" ResponseType.NotAiOk
                | MessageType.Invalid(str, msg) ->
                    match str with
                    | InvalidMessageType.Empty ->
                        return!
                            processShortcut
                                "не можу опрацювати повідомлення без тексту 😔"
                                ResponseType.NotAiGenericError
                | MessageType.AiQuestion msg ->
                    let botId = IncomingBotMessage.getBotId msg
                    let! quotaOption = quotaRepo.TryGetQuotaByBotId cancellationToken botId

                    match quotaOption with
                    | None ->
                        logger.LogWarning("Quota not found for {BotId}", botId)

                        return!
                            processShortcut
                                "тарифний план не знайдений, оплатіть послуги або звʼяжіться з службою підтримки"
                                ResponseType.NotAiQuotaError
                    | Some quota ->
                        let decision = AskQuestion.ask request.DateTimeUtc quota msg

                        match decision with
                        | Error e ->
                            let! _ =
                                match e with
                                | AskQuestionError.NoData ->
                                    processShortcut
                                        "не знайдено даних, будь ласка, завантажте документ"
                                        ResponseType.NotAiQuotaError
                                | AskQuestionError.AccessRestricted ->
                                    processShortcut
                                        "доступ до ресурсу обмежений, зверніться до адмінісратора"
                                        ResponseType.NotAiAccessError
                                | AskQuestionError.QuotaOutdated _ ->
                                    processShortcut
                                        "час використання збіг, будь ласка оплатіть послуги, або зверніться до служби підтримки"
                                        ResponseType.NotAiQuotaError
                                | AskQuestionError.MessageLimitReached _ ->
                                    processShortcut
                                        "досягнуто ліміту повідомлень, будь ласка оплатіть послуги, або зверніться до служби підтримки"
                                        ResponseType.NotAiQuotaError

                            logger.LogInformation("Forbidden to ask with {Error}", e)
                            return SendNewBotMessageForProcessingCommandResponse.Forbidden
                        | Ok msg ->
                            do!
                                task {
                                    do! request.Data |> messageLog.SaveMessage cancellationToken true

                                    let setChatActionCommand: SetChatActionCommand =
                                        { Bot =
                                            match request.Data.SpecificMetadata with
                                            | BotMessageMetadata.TelegramMessageMetadata tg ->
                                                SetChatActionBot.Telegram { BotId = tg.BotId; ChatId = tg.ChatId }
                                          Action = ChatAction.Typing }
                                        |> Command.createFrom request

                                    let! _ = mediator.Send(setChatActionCommand, cancellationToken)

                                    let message =
                                        { MessageId = msg.Id
                                          MessageText = msg.Text
                                          DataFolder = Option.get quota.DataFolder }

                                    do! queue.PushToQueue request.CorrelationId message
                                }

                            return SendNewBotMessageForProcessingCommandResponse.Accepted
            }
