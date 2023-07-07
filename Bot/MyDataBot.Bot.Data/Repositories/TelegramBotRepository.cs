using Microsoft.FSharp.Core;
using MyDataBot.Bot.Core.Common;
using MyDataBot.Bot.Data.Mappers;
using MyDataBot.Bot.Data.Models;
using MyDataBot.Bot.Dependencies.Data;

namespace MyDataBot.Bot.Data.Repositories;

public class TelegramBotRepository : ITelegramBotRepository
{
    private readonly BotDbContext _context;

    public TelegramBotRepository(BotDbContext context)
    {
        _context = context;
    }

    public async Task<FSharpOption<TelegramBotCoreModels.TelegramBotMetadata>> TryGetBotMetadata(CancellationToken ct,
        TelegramBotCoreModels.TelegramBotId botId)
    {
        var bot = await _context.Bots.FindAsync(botId.Value);
        if (bot is { BotType: BotType.Telegram } tgBot)
        {
            var metadata = new TelegramBotCoreModels.TelegramBotMetadata(
                id: tgBot.Id.ToTgBotId(),
                token: TelegramBotCoreModels.TelegramBotToken.NewTelegramBotToken(tgBot.TelegramToken),
                secret: TelegramBotCoreModels.TelegramBotSecret.NewTelegramBotSecret(tgBot.TelegramSecret),
                offset: tgBot.TelegramOffset.ToFsharpOption(),
                limit: tgBot.TelegramLimit.ToFsharpOption()
            );
            return metadata;
        }

        return default(TelegramBotCoreModels.TelegramBotMetadata).ToFsharpOption();
    }
}