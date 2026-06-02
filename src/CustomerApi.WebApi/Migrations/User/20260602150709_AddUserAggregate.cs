using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerApi.WebApi.Migrations.User
{
    /// <inheritdoc />
    public partial class AddUserAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "varchar(254)", unicode: false, maxLength: 254, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Profile_FullName = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Profile_DateOfBirth = table.Column<DateTime>(type: "DATE", nullable: false),
                    Profile_JobTitle = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
