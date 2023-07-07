using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyDataBot.Bot.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quotas",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    FromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataFolder = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MessagesAllowed = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    AccessRule = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bots",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    QuotaId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    BotType = table.Column<int>(type: "integer", nullable: false),
                    TelegramSecret = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TelegramToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TelegramOffset = table.Column<int>(type: "integer", nullable: true),
                    TelegramLimit = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bots_Quotas_QuotaId",
                        column: x => x.QuotaId,
                        principalTable: "Quotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuotaUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    BotType = table.Column<int>(type: "integer", nullable: false),
                    TelegramUserName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    QuotaId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotaUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotaUsers_Quotas_QuotaId",
                        column: x => x.QuotaId,
                        principalTable: "Quotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncomingMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    BotId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    IsAiQuestion = table.Column<bool>(type: "boolean", nullable: false),
                    Text = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    DateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TelegramChatId = table.Column<long>(type: "bigint", nullable: true),
                    TelegramUserName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingMessages_Bots_BotId",
                        column: x => x.BotId,
                        principalTable: "Bots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageResponses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    IncomingMessageId = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    Text = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    Delivered = table.Column<bool>(type: "boolean", nullable: false),
                    ResponseType = table.Column<int>(type: "integer", nullable: false),
                    DateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageResponses_IncomingMessages_IncomingMessageId",
                        column: x => x.IncomingMessageId,
                        principalTable: "IncomingMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bots_QuotaId",
                table: "Bots",
                column: "QuotaId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMessages_BotId",
                table: "IncomingMessages",
                column: "BotId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageResponses_IncomingMessageId",
                table: "MessageResponses",
                column: "IncomingMessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotaUsers_QuotaId",
                table: "QuotaUsers",
                column: "QuotaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageResponses");

            migrationBuilder.DropTable(
                name: "QuotaUsers");

            migrationBuilder.DropTable(
                name: "IncomingMessages");

            migrationBuilder.DropTable(
                name: "Bots");

            migrationBuilder.DropTable(
                name: "Quotas");
        }
    }
}
