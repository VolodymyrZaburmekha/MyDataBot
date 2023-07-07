using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyDataBot.Bot.Data.Models;

public enum QuotaAccessRule
{
    AllowAll,
    AllowNoted,
    ForbidNoted
}

public class QuotaDbo
{
    public Ulid Id { get; set; }
    public DateTime FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public string? DataFolder { get; set; }
    public string? Reason { get; set; }
    public int MessagesAllowed { get; set; }
    public bool Active { get; set; }
    public QuotaAccessRule AccessRule { get; set; }

    public ICollection<BotDbo> Bots { get; set; }
    public ICollection<QuotaUserDbo> Users { get; set; }
}

public class QuotaUserDbo
{
    public Ulid Id { get; set; }
    public BotType BotType { get; set; }
    public string? TelegramUserName { get; set; }
    public Ulid QuotaId { get; set; }

    public QuotaDbo Quota { get; set; }
    // add metadata for other messangers later
}

public class QuotaDboConfiguration : IEntityTypeConfiguration<QuotaDbo>
{
    public const string TableName = "Quotas";

    public void Configure(EntityTypeBuilder<QuotaDbo> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).ConfigureUlid();
        builder.Property(q => q.DataFolder).HasMaxLength(255);

        builder.Property(q => q.Reason).HasMaxLength(255);
    }
}

public class QuotaUserDboConfiguration : IEntityTypeConfiguration<QuotaUserDbo>
{
    public const string TableName = "QuotaUsers";
    public const int TelegramUserNameLength = 32;

    public void Configure(EntityTypeBuilder<QuotaUserDbo> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ConfigureUlid();

        builder.Property(u => u.TelegramUserName).HasMaxLength(TelegramUserNameLength);
        builder.Property(u => u.QuotaId).ConfigureUlid();

        builder.HasOne(u => u.Quota)
            .WithMany(q => q.Users)
            .HasForeignKey(u => u.QuotaId);
    }
}