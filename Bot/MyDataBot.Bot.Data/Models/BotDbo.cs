using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyDataBot.Bot.Data.Models;

public enum BotType
{
    Telegram,
    // add viber, whatsapp later
}

public class BotDbo
{
    public Ulid Id { get; set; }
    public Ulid QuotaId { get; set; }
    public BotType BotType { get; set; }

    #region TelegramSpecific

    public string? TelegramSecret { get; set; }
    public string? TelegramToken { get; set; }
    public int? TelegramOffset { get; set; }
    public int? TelegramLimit { get; set; }
    #endregion


    public QuotaDbo Quota { get; set; }
    public ICollection<IncomingMessageDbo> Messages { get; set; }
}

public class BotDboConfiguration : IEntityTypeConfiguration<BotDbo>
{
    public const string TableName = "Bots";

    public void Configure(EntityTypeBuilder<BotDbo> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ConfigureUlid();

        builder.Property(b => b.QuotaId).ConfigureUlid();

        builder.Property(b => b.TelegramToken).HasMaxLength(50);
        builder.Property(b => b.TelegramSecret).HasMaxLength(256);

        builder.HasOne(b => b.Quota)
            .WithMany(q => q.Bots)
            .HasForeignKey(b => b.QuotaId);
    }
}