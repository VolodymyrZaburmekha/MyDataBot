namespace MyDataBot.Bot.Core.UseCases.AskQuestion

open System
open Microsoft.FSharp.Collections
open MyDataBot.Bot.Core.Common.CommonBotModels
open MyDataBot.Bot.Core.Common.TelegramBotCoreModels
open MyDataBot.Bot.Core.Common.Message

[<Measure>]
type question

[<RequireQualifiedAccess>]
type BotUser = TelegramUser of TelegramUserName

[<RequireQualifiedAccess>]
type BotUserAccess =
    | AllowAll
    | AllowAllExcept of BotUser List
    | ForbidAllExcept of BotUser List

type QuotaId =
    | QuotaId of Ulid

    member this.Value =
        match this with
        | QuotaId id -> id

type License = { BoughtAtUtc: DateTime }

type QuotaReason =
    | Trial
    | License


type Quota =
    { Id: QuotaId
      DataFolder: DataFolder option
      AllowedAiQuestionsCount: int<question>
      AiQuestionsCount: int<question>
      AccessRule: BotUserAccess
      DateTimeFromUtc: DateTime
      DateTimeToUtc: DateTime option
      Reason: QuotaReason
      Bots: BotId list }

[<RequireQualifiedAccess>]
type AskQuestionError =
    | MessageLimitReached of quota: Quota * currentQuestionNumber: int<question>
    | QuotaOutdated of quota: Quota * currentDateTimeUtc: DateTime
    | AccessRestricted
    | NoData

[<RequireQualifiedAccess>]
type AskQuestionDecision = Result<IncomingBotMessage, AskQuestionError>

type AskQuestion = DateTime -> Quota -> IncomingBotMessage -> AskQuestionDecision

module AskQuestion =
    let ask: AskQuestion =
        fun currentDateTimeUtc quota question ->
            if currentDateTimeUtc > (quota.DateTimeToUtc |> Option.defaultValue (currentDateTimeUtc.AddDays(1))) then
                Error(AskQuestionError.QuotaOutdated(quota, currentDateTimeUtc))
            elif quota.AllowedAiQuestionsCount < quota.AiQuestionsCount then
                Error(AskQuestionError.MessageLimitReached(quota, quota.AiQuestionsCount))
            elif quota.DataFolder |> Option.isNone then
                Error AskQuestionError.NoData
            else
                let botUser =
                    match question.SpecificMetadata with
                    | BotMessageMetadata.TelegramMessageMetadata tg -> BotUser.TelegramUser tg.UserName

                match quota.AccessRule with
                | BotUserAccess.AllowAll -> Ok question
                | BotUserAccess.AllowAllExcept blacklistedUsers ->
                    if (blacklistedUsers |> List.exists (fun bl -> bl = botUser)) then
                        Error AskQuestionError.AccessRestricted
                    else
                        Ok question
                | BotUserAccess.ForbidAllExcept allowedUsers ->
                    if (allowedUsers |> List.exists (fun au -> au = botUser)) then
                        Ok question
                    else
                        Error AskQuestionError.AccessRestricted
