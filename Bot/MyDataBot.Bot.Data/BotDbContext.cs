using Microsoft.EntityFrameworkCore;
using MyDataBot.Bot.Data.Models;

namespace MyDataBot.Bot.Data;

public class BotDbContext : DbContext
{
#pragma warning disable CS8618
    public BotDbContext(DbContextOptions<DbContext> options) : base(options)
    {
    }

    public BotDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<BotDbo> Bots { get; set; }
    public DbSet<IncomingMessageDbo> Messages { get; set; }
    public DbSet<QuotaDbo> Quotas { get; set; }
    public DbSet<QuotaUserDbo> QuotaUsers { get; set; }
#pragma warning restore CS8618

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new BotDboConfiguration());
        modelBuilder.ApplyConfiguration(new MessageDboConfiguration());
        modelBuilder.ApplyConfiguration(new QuotaDboConfiguration());
        modelBuilder.ApplyConfiguration(new QuotaUserDboConfiguration());
        modelBuilder.ApplyConfiguration(new IncomingMessageResponseConfiguration());
    }
}