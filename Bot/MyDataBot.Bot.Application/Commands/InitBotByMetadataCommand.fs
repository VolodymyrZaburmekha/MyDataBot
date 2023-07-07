namespace MyDataBot.Bot.Application.Commands

open System
open MediatR
open Microsoft.Extensions.Logging
open MyDataBot.Bot.Core.Common.CommonBotModels
open MyDataBot.Bot.Core.Common.TelegramBotCoreModels
open MyDataBot.Bot.Dependencies.BotLifecycle

[<CLIMutable>]
type HostConfig = { HostName: string }

type InitBotByMetadataResponse = unit
type StartBotData = { StartBot: Bot } // this record needed because it will mess up with StopBotByMetadataCommand (they have exactly the same signatures <bot,unit> otherwise)
type InitBotByMetadataCommand = Command<StartBotData, InitBotByMetadataResponse>

module private BotInitialization =
    let getWebhookUrl baseUrl (botId: TelegramBotId) =
        $"%s{baseUrl}/telegram/bot/%s{botId.Value.ToString()}/register-update"


type InitBotByMetadataCommandHandler
    (logger: ILogger<InitBotByMetadataCommandHandler>, hostConfig: HostConfig, clientFactory: ITelegramClientFactory) =

    interface IRequestHandler<InitBotByMetadataCommand, InitBotByMetadataResponse> with
        member this.Handle(request, cancellationToken) =
            task {
                match request.Data.StartBot with
                | Bot.TelegramBot tgBot ->
                    let client = clientFactory.Create tgBot
                    let url = BotInitialization.getWebhookUrl hostConfig.HostName tgBot.Id

                    do! client.SetWebhookAsync url tgBot.Secret

                    logger.LogInformation("Bot Initialized {TelegramBot}", tgBot)
            }

type StopBotByMetadataCommandResponse = unit
type StopBotData = { StopBot: Bot }
type StopBotByMetadataCommand = Command<StopBotData, StopBotByMetadataCommandResponse>

type StopBotByMetadataCommandHandler
    (logger: ILogger<StopBotByMetadataCommandHandler>, hostConfig: HostConfig, clientFactory: ITelegramClientFactory) =
    interface IRequestHandler<StopBotByMetadataCommand, StopBotByMetadataCommandResponse> with
        member this.Handle(request, cancellationToken) =
            task {
                match request.Data.StopBot with
                | Bot.TelegramBot tgBot ->
                    let client = clientFactory.Create tgBot
                    do! client.DeleteWebhookAsync()
                    logger.LogInformation("Bot stopped {TelegramBot}", tgBot)
            }
