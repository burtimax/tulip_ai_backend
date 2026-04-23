using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Db.App.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConfirmedProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "confirmed_policy_and_documents",
                schema: "app",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "confirmed_policy_and_documents",
                schema: "app",
                table: "users",
                type: "boolean",
                nullable: true,
                comment: "Пользователь подтвердил ознакомление с политикой и документами.");
        }
    }
}
