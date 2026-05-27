using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerApi.WebApi.Migrations.EventStore
{
    /// <inheritdoc />
    public partial class InitialEventStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventStores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false, comment: "Evento serializado em JSON"),
                    MessageType = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    AggregateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurreedOn = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "CreatedAt")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventStores", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventStores");
        }
    }
}
