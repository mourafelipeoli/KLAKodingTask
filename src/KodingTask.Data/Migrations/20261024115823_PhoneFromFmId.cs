using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KodingTask.Data.Migrations
{
    /// <inheritdoc />
    public partial class PhoneFromFmId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhoneChallenges");

            migrationBuilder.AddColumn<DateTime>(
                name: "PhoneCheckedAtUtc",
                table: "UserProfiles",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneCheckedAtUtc",
                table: "UserProfiles");

            migrationBuilder.CreateTable(
                name: "PhoneChallenges",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodeHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FailedAttempts = table.Column<int>(type: "int", nullable: false),
                    PhoneE164 = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SentAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneChallenges", x => x.UserId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
