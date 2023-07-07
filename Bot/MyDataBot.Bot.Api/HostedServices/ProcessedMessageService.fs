namespace MyDataBot.Bot.Api.HostedServices

open System
open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open MyDataBot.Bot.Common.ValueObjects.Ids
open MyDataBot.Bot.Core.Common.Message
open MyDataBot.Bot.Dependencies.AsyncBotMessageProcessing
open MediatR
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open MyDataBot.Bot.Application.Commands

type ProcessedMessageService
    (
        logger: ILogger<ProcessedMessageService>,
        processMessageQueue: IProcessMessageQueue,
        serviceProvider: IServiceProvider
    ) =
    let processMessage (correlationId: CorrelationId) (msg: AiProcessedMessage) =
        task {
            try
                use scope = serviceProvider.CreateScope()
                let mediator = scope.ServiceProvider.GetRequiredService<IMediator>()

                let command =
                    { CorrelationId = correlationId
                      DateTimeUtc = DateTime.UtcNow
                      Data = msg }
                    : ProcessAiResponseCommand

                do! mediator.Send(command)

                return true
            with :? Exception as ex ->
                logger.LogError(
                    "{CorrelationId} | Handler processed message error: {Error}",
                    correlationId.ValueStr,
                    ex
                )

                return true
        }

    interface IHostedService with
        member this.StartAsync(cancellationToken) =
            processMessageQueue.SubscribeForProcessed processMessage
            logger.LogInformation("Start listening")
            Task.CompletedTask

        member this.StopAsync(cancellationToken) = Task.CompletedTask
