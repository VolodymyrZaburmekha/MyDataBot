using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyDataBot.Bot.Core.Common;

namespace MyDataBot.Bot.Data.Models;

public class IncomingMessageDbo
{
    private IncomingMessageDbo()
    {
    }

    public Ulid Id { get; set; }
    public Ulid BotId { get; set; }

    public bool IsAiQuestion { get; private set; }
    public string Text { get; set; }
    public DateTime DateTimeUtc { get; set; }

    #region TelegramSpecific

    public long? TelegramChatId { get; set; }
    public string? TelegramUserName { get; set; }

    #endregion


    public BotDbo Bot { get; set; }
    public IncomingMessageResponseDbo? Response { get; set; }

    public static IncomingMessageDbo FromIncomingMessage(Message.IncomingBotMessage msg, bool isForAi)
    {
        return new IncomingMessageDbo
        {
            Id = msg.Id.Value,
            DateTimeUtc = msg.ReceivedAtUtc,
            TelegramChatId = msg.SpecificMetadata.Item.ChatId.Value,
            TelegramUserName = msg.SpecificMetadata.Item.UserName.Value,
            Text = msg.Text,
            BotId = msg.SpecificMetadata.Item.BotId.Value,
            IsAiQuestion = isForAi
        };
    }
}

public class MessageDboConfiguration : IEntityTypeConfiguration<IncomingMessageDbo>
{
    public const string TableName = "IncomingMessages";

    public void Configure(EntityTypeBuilder<IncomingMessageDbo> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ConfigureUlid();

        builder.Property(m => m.BotId).ConfigureUlid();

        builder.Property(m => m.Text).HasMaxLength(4096);

        builder.Property(m => m.BotId).ConfigureUlid();
        builder.Property(m => m.TelegramUserName)
            .HasMaxLength(QuotaUserDboConfiguration.TelegramUserNameLength);

        builder.HasOne(m => m.Bot)
            .WithMany(b => b.Messages)
            .HasForeignKey(m => m.BotId);
    }
}

public enum ResponseType
{
    NotAiOk = 0,
    NotAiGenericError = 1,
    NotAiAccessError = 2,
    NotAiQuotaError = 3,
    AiOk = 100,
    AiRateLimitError = 101,
    AiUnknownError = 199
}

public class IncomingMessageResponseDbo
{
    private IncomingMessageResponseDbo()
    {
    }

    public Ulid Id { get; private set; }

    public Ulid IncomingMessageId { get; private set; }

    public IncomingMessageDbo IncomingMessage { get; private set; }

    public string Text { get; private set; }
    public bool Delivered { get; private set; }

    public ResponseType ResponseType { get; private set; }
    public DateTime DateTimeUtc { get; private set; }

    public static IncomingMessageResponseDbo FromIncomingMessageResponse(Message.BotMessageResponse response)
    {
        return new IncomingMessageResponseDbo
        {
            Id = response.Id.Value,
            DateTimeUtc = response.DateTimeUtc,
            Delivered = response.Delivered,
            Text = response.Text,
            IncomingMessageId = response.IncomingMessageId.Value,
            ResponseType = response.ResponseType switch
            {
                { IsNotAiOk: true } => ResponseType.NotAiOk,
                { IsNotAiGenericError: true } => ResponseType.NotAiGenericError,
                { IsNotAiAccessError: true } => ResponseType.NotAiAccessError,
                { IsNotAiQuotaError: true } => ResponseType.NotAiQuotaError,

                { IsAiOk: true } => ResponseType.AiOk,
                { IsAiRateLimitError: true } => ResponseType.AiRateLimitError,
                { IsAiUnknownError: true } => ResponseType.AiUnknownError,
                _ => throw new UnreachableException("wrong response type")
            }
        };
    }
}

public class IncomingMessageResponseConfiguration : IEntityTypeConfiguration<IncomingMessageResponseDbo>
{
    public const string TableName = "MessageResponses";

    public void Configure(EntityTypeBuilder<IncomingMessageResponseDbo> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ConfigureUlid();
        builder.Property(r => r.IncomingMessageId).ConfigureUlid();
        builder.Property(r => r.Text).HasMaxLength(4096);

        builder.HasOne(r => r.IncomingMessage)
            .WithOne(m => m.Response)
            .HasForeignKey<IncomingMessageResponseDbo>(r => r.IncomingMessageId)
            .IsRequired();
    }
}