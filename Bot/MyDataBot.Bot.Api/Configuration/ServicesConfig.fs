module MyDataBot.Bot.Api.Configuration.ServicesConfig

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration


let registerSettings<'Tsettings when 'Tsettings: not struct> settingsSection (builder: WebApplicationBuilder) =
    let settings =
        builder.Configuration.GetSection(key = settingsSection).Get<'Tsettings>()

    let _ = builder.Services.AddSingleton<'Tsettings>(settings)
    settings