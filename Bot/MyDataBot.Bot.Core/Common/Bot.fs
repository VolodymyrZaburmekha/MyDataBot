namespace MyDataBot.Bot.Core.Common

open System

module TelegramBotCoreModels =
    type TelegramBotId =
        | TelegramBotId of Ulid

        member this.Value =
            match this with
            | TelegramBotId v -> v

    type TelegramUserName =
        | TelegramUserName of string

        member this.Value =
            match this with
            | TelegramUserName v -> v


    type TelegramBotSecret =
        | TelegramBotSecret of string

        member this.Value =
            match this with
            | TelegramBotSecret s -> s

    type TelegramMessageInternalId =
        | TelegramMessageInternalId of int

        member this.Value =
            match this with
            | TelegramMessageInternalId id -> id

    type TelegramUpdateId =
        | TelegramUpdateId of int

        member this.Value =
            match this with
            | TelegramUpdateId id -> id

    type TelegramChatId =
        | TelegramChatId of int64

        member this.Value =
            match this with
            | TelegramChatId id -> id

    type TelegramBotToken =
        | TelegramBotToken of string

        member this.Value =
            match this with
            | TelegramBotToken t -> t

    type TelegramBotMetadata =
        { Id: TelegramBotId
          Token: TelegramBotToken
          Secret: TelegramBotSecret
          Offset: int Option
          Limit: int Option }

    type TelegramMessageMetadata =
        { BotId: TelegramBotId
          Id: TelegramMessageInternalId
          UpdateId: TelegramUpdateId
          ChatId: TelegramChatId
          UserName: TelegramUserName
          LanguageCode: string
          Secret: TelegramBotSecret }


open TelegramBotCoreModels

module CommonBotModels =

    [<RequireQualifiedAccess>]
    type BotId =
        | TelegramBotId of TelegramBotId
        // add viber, etc later
        member this.Value =
            match this with
            | TelegramBotId v -> v.Value

    type MessageId =
        | MessageId of Ulid

        member this.Value =
            match this with
            | MessageId id -> id


    [<RequireQualifiedAccess>]
    type BotMessageMetadata = TelegramMessageMetadata of TelegramMessageMetadata

    [<RequireQualifiedAccess>]
    type Bot = TelegramBot of TelegramBotMetadata
// | add viber, etc here
