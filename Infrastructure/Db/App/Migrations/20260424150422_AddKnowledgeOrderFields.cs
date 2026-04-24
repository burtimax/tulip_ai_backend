using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Db.App.Migrations
{
    /// <inheritdoc />
    public partial class AddKnowledgeOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "order",
                schema: "knowledge",
                table: "knowledge_titles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Порядок сортировки статьи внутри категории");

            migrationBuilder.AddColumn<int>(
                name: "order",
                schema: "knowledge",
                table: "knowledge_categories",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Порядок сортировки категории");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "order",
                schema: "knowledge",
                table: "knowledge_titles");

            migrationBuilder.DropColumn(
                name: "order",
                schema: "knowledge",
                table: "knowledge_categories");
        }
    }
}
