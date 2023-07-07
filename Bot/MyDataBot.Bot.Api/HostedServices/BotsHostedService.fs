namespace MyDataBot.Bot.Api.HostedServices

open System
open System.Threading.Tasks
open MediatR
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open MyDataBot.Bot.Application.Commands
open MyDataBot.Bot.Common.ValueObjects.Ids
open MyDataBot.Bot.Dependencies.Data

type BotsHostedService(logger: ILogger<BotsHostedService>, serviceProvider: IServiceProvider) =
    let mutable startTask = None

    interface IHostedService with
        member this.StartAsync(cancellationToken) =
            startTask <-
                task {
                    try
                        use scope = serviceProvider.CreateScope()
                        let repo = scope.ServiceProvider.GetRequiredService<IBotsRepository>()
                        let mediator = scope.ServiceProvider.GetRequiredService<IMediator>()
                        let! bots = repo.GetAllBots cancellationToken

                        for bot in bots do
                            let command: InitBotByMetadataCommand =
                                { CorrelationId = CorrelationId(Ulid.NewUlid())
                                  DateTimeUtc = DateTime.UtcNow
                                  Data = { StartBot = bot } }

                            do! mediator.Send(command, cancellationToken)

                        do logger.LogInformation("Bots started {Count}", List.length bots)
                    with :? Exception as ex ->
                        logger.LogCritical("START BOTS ERROR! {Error}", ex)
                        return ()
                }
                |> Some

            Task.CompletedTask

        member this.StopAsync(cancellationToken) =
            task {
                try
                    use scope = serviceProvider.CreateScope()
                    let repo = scope.ServiceProvider.GetRequiredService<IBotsRepository>()
                    let mediator = scope.ServiceProvider.GetRequiredService<IMediator>()
                    let! bots = repo.GetAllBots cancellationToken

                    for bot in bots do
                        let command: StopBotByMetadataCommand =
                            { CorrelationId = CorrelationId(Ulid.NewUlid())
                              DateTimeUtc = DateTime.Now
                              Data = { StopBot = bot } }

                        do! mediator.Send(command, cancellationToken)

                    do logger.LogInformation("Bots stopped {Count}", List.length bots)
                    return ()
                with :? Exception as ex ->
                    logger.LogCritical("STOP BOTS ERROR! {Error}", ex)
                    return ()
            }
