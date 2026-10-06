using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Integration.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFailedMessageAttemptCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "FailedMessages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "FailedMessages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
