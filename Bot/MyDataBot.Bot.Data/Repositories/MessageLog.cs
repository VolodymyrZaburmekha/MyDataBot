using MyDataBot.Bot.Core.Common;
using MyDataBot.Bot.Data.Models;
using MyDataBot.Bot.Dependencies.Data;

namespace MyDataBot.Bot.Data.Repositories;

public class MessageLog : IMessageLog
{
    private readonly BotDbContext _dbContext;

    public MessageLog(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MessangerMetadata> GetMetadataForResponse(CancellationToken ct,
        CommonBotModels.MessageId messageId)
    {
        var msg = await _dbContext.Messages.FindAsync(messageId.Value);
        if (msg.TelegramChatId is { } tgChatId)
        {
            return MessangerMetadata.NewTelegram(TelegramBotCoreModels.TelegramBotId.NewTelegramBotId(msg.BotId),
                TelegramBotCoreModels.TelegramChatId.NewTelegramChatId(tgChatId));
        }

        // add more messangers here 
        throw new InvalidDataException("Invalid message messanger");
    }

    public async Task SaveMessage(CancellationToken ct, bool isForAi, Message.IncomingBotMessage message)
    {
        var dbo = IncomingMessageDbo.FromIncomingMessage(message, isForAi);
        await _dbContext.AddAsync(dbo, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task SaveResponse(CancellationToken ct, Message.BotMessageResponse response)
    {
        var dbo = IncomingMessageResponseDbo.FromIncomingMessageResponse(response);
        await _dbContext.AddAsync(dbo, ct);
        await _dbContext.SaveChangesAsync(ct);
    }
}