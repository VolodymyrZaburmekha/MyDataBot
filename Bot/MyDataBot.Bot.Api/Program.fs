namespace MyDataBot.Bot.Api

#nowarn "20"

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open MyDataBot.Bot.Api.Configuration
open MyDataBot.Bot.Api.Controllers
open MyDataBot.Bot.Application.Commands
open MyDataBot.Bot.Common.ValueObjects.Ids
open MyDataBot.Bot.Data.Repositories
open MyDataBot.Bot.Dependencies.AsyncBotMessageProcessing
open MyDataBot.Bot.Dependencies.BotLifecycle
open MyDataBot.Bot.Dependencies.Data
open MyDataBot.Bot.Nats
open MyDataBot.Bot.Api.HostedServices
open MyDataBot.Bot.Data
open MyDataBot.Bot.Telegram

module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)

        builder.Services.AddDbContext<BotDbContext>(fun options ->
            let config = builder.Configuration
            let connString = config.GetConnectionString("BotDb")
            options.UseNpgsql(connString)
            ())

        let _ = builder |> ServicesConfig.registerSettings<HostConfig> "HostConfig"

        builder.Services
            .AddHttpClient("telegram_bot_http_client")
            .AddTypedClient<ITelegramClientFactory>(fun httpClient sp ->
                TelegramClientFactory(httpClient, sp.GetService<ITelegramBotRepository>()) :> ITelegramClientFactory)

        builder.Services.AddControllers().AddNewtonsoftJson()
        builder.Services.AddScoped<CorrelationId>(fun _ -> CorrelationId(Ulid.NewUlid()))
        builder.Services.AddTransient<IMessageLog, MessageLog>()
        builder.Services.AddTransient<ITelegramBotRepository, TelegramBotRepository>()
        builder.Services.AddTransient<IBotsRepository, BotsRepository>()
        builder.Services.AddTransient<IQuotaRepository, QuotaRepository>()

        builder.Services.AddSingleton<IProcessMessageQueue, NatsProcessMessageQueue>(fun serviceProvider ->
            let logger = serviceProvider.GetService<ILogger<NatsProcessMessageQueue>>()
            NatsProcessMessageQueue(None, logger)) // todo: inject nats url

        builder.Services.AddMediatR(fun cfg ->
            cfg.RegisterServicesFromAssemblies(typeof<TelegramController>.Assembly, typeof<Command<_, _>>.Assembly)
            ())

        builder.Services.AddHostedService<ProcessedMessageService>()
        builder.Services.AddHostedService<BotsHostedService>()

        let app = builder.Build()

        app.UseHttpsRedirection()

        app.UseAuthorization()
        app.MapControllers()

        app.Run()

        exitCode
