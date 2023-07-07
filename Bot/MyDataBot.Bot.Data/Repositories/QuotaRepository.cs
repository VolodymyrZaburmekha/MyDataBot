using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.FSharp.Core;
using MyDataBot.Bot.Core.Common;
using MyDataBot.Bot.Core.UseCases.AskQuestion;
using MyDataBot.Bot.Data.Mappers;
using MyDataBot.Bot.Data.Models;
using MyDataBot.Bot.Dependencies.Data;

namespace MyDataBot.Bot.Data.Repositories;

public class QuotaRepository : IQuotaRepository
{
    private readonly BotDbContext _context;

    public QuotaRepository(BotDbContext context)
    {
        _context = context;
    }

    public async Task<FSharpOption<Quota>> TryGetQuotaByBotId(CancellationToken ct, CommonBotModels.BotId botId)
    {
        var nullableQuota = await _context.Quotas
            .Include(q => q.Bots).ThenInclude(b => b.Messages)
            .Include(q => q.Users)
            .Select(q =>
                new
                {
                    q.Id,
                    q.MessagesAllowed,
                    AiQuestionsCount = q.Bots.SelectMany(b => b.Messages).Count(m => m.IsAiQuestion),
                    q.AccessRule,
                    q.FromUtc,
                    q.ToUtc,
                    q.Reason,
                    q.DataFolder,
                    Bots = q.Bots.Select(b => new { BotId = b.Id, b.BotType }),
                    Users = q.Users.Select(u => new { u.BotType, u.TelegramUserName })
                })
            .FirstOrDefaultAsync(q => q.Bots.Any(b => b.BotId == botId.Value), ct);

        if (nullableQuota is { } quota)
        {
            var users = quota.Users.Select(u =>
            {
                if (u.TelegramUserName is { } tgUsername)
                {
                    return BotUser.NewTelegramUser(
                        TelegramBotCoreModels.TelegramUserName.NewTelegramUserName(tgUsername));
                }

                throw new UnreachableException("wrong usertype");
            }).ToFsharpList();

            return new Quota(
                id: QuotaId.NewQuotaId(quota.Id),
                dataFolder: Message.DataFolder.NewDataFolder(quota.DataFolder),
                allowedAiQuestionsCount: quota.MessagesAllowed,
                aiQuestionsCount: quota.AiQuestionsCount,
                accessRule: quota.AccessRule switch
                {
                    QuotaAccessRule.AllowAll => BotUserAccess.AllowAll,
                    QuotaAccessRule.AllowNoted => BotUserAccess.NewForbidAllExcept(users),
                    QuotaAccessRule.ForbidNoted => BotUserAccess.NewAllowAllExcept(users)
                },
                dateTimeFromUtc: quota.FromUtc,
                dateTimeToUtc: quota.ToUtc.ToFsharpOption(), //todo: place real reason
                reason: QuotaReason.Trial,
                bots: quota.Bots.Select(b =>
                {
                    if (b.BotType == BotType.Telegram)
                    {
                        return CommonBotModels.BotId.NewTelegramBotId(b.BotId.ToTgBotId());
                    }

                    throw new UnreachableException("wrong bot type");
                }).ToFsharpList()
            );
        }

        return default(Quota).ToFsharpOption();
    }
}