using MyDataBot.Bot.Core.Common;

namespace MyDataBot.Bot.Data.Mappers;

public static class IdMappers
{
    public static TelegramBotCoreModels.TelegramBotId ToTgBotId(this Ulid tgUlid)
        => TelegramBotCoreModels.TelegramBotId.NewTelegramBotId(tgUlid);

    public static TelegramBotCoreModels.TelegramBotToken ToTgToken(this string token)
        => TelegramBotCoreModels.TelegramBotToken.NewTelegramBotToken(token);
    
    public static TelegramBotCoreModels.TelegramBotSecret ToTgSecret(this string secret)
        => TelegramBotCoreModels.TelegramBotSecret.NewTelegramBotSecret(secret);
}