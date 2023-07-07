using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyDataBot.Bot.Data.Models;

public static class DbConfigurationHelper
{
    public static PropertyBuilder<Ulid> ConfigureUlid(this PropertyBuilder<Ulid> propertyBuilder)
        =>
            propertyBuilder
                .HasConversion(ulid => ulid.ToString(), str => Ulid.Parse(str))
                .HasMaxLength(26);

    public static PropertyBuilder<Ulid?> ConfigureNullableUlid(this PropertyBuilder<Ulid?> propertyBuilder)
        =>
            propertyBuilder
                .HasConversion(
                    nullableUlid => nullableUlid == null ? null : nullableUlid.Value.ToString(),
                    str => str == null ? null : Ulid.Parse(str))
                .HasMaxLength(26);
}