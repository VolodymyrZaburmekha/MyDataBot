using Microsoft.EntityFrameworkCore;
using Microsoft.FSharp.Collections;
using MyDataBot.Bot.Core.Common;
using MyDataBot.Bot.Data.Mappers;
using MyDataBot.Bot.Dependencies.Data;

namespace MyDataBot.Bot.Data.Repositories;

public class BotsRepository : IBotsRepository
{
    private readonly BotDbContext _context;

    public BotsRepository(BotDbContext context)
    {
        _context = context;
    }

    public async Task<FSharpList<CommonBotModels.Bot>> GetAllBots(CancellationToken ct)
    {
        var botDbos = await _context.Bots.ToArrayAsync(ct);

        return botDbos.Select(dbo => CommonBotModels.Bot.NewTelegramBot(
            new TelegramBotCoreModels.TelegramBotMetadata(
                id: dbo.Id.ToTgBotId(),
                token: dbo.TelegramToken.ToTgToken(),
                secret: dbo.TelegramSecret.ToTgSecret(),
                offset: dbo.TelegramOffset.ToFsharpOption(),
                limit: dbo.TelegramLimit.ToFsharpOption()
            )
        )).ToFsharpList();
    }
}