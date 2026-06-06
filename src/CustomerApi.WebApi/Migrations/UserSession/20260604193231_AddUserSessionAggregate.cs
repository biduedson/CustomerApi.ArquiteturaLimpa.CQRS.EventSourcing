using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerApi.WebApi.Migrations.UserSession
{
    /// <inheritdoc />
    public partial class AddUserSessionAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    UserAgent = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: false),
                    IpAddress = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "DATETIME2", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "DATETIME2", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "DATETIME2", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    RevocationReason = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_RefreshTokenHash",
                table: "UserSessions",
                column: "RefreshTokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSessions");
        }
    }
}
