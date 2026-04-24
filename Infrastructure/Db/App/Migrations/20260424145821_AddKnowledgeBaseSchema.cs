using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Db.App.Migrations
{
    /// <inheritdoc />
    public partial class AddKnowledgeBaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "knowledge");

            migrationBuilder.CreateTable(
                name: "knowledge_categories",
                schema: "knowledge",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ИД сущности."),
                    name = table.Column<string>(type: "text", nullable: false, comment: "Название категории базы знаний"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Когда сущность была создана."),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто создал сущность."),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была в последний раз обновлена."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто обновил сущность."),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была удалена."),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто удалил сущность.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_knowledge_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_knowledge_categories_users_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_knowledge_categories_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_knowledge_categories_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "knowledge_titles",
                schema: "knowledge",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ИД сущности."),
                    knowledge_category_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор категории"),
                    title = table.Column<string>(type: "text", nullable: false, comment: "Заголовок статьи"),
                    excerpt = table.Column<string>(type: "text", nullable: true, comment: "Краткое описание статьи"),
                    images = table.Column<string>(type: "jsonb", nullable: false, comment: "Список изображений статьи"),
                    content = table.Column<string>(type: "text", nullable: false, comment: "HTML-контент статьи"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Когда сущность была создана."),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто создал сущность."),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была в последний раз обновлена."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто обновил сущность."),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была удалена."),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто удалил сущность.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_knowledge_titles", x => x.id);
                    table.ForeignKey(
                        name: "fk_knowledge_titles_knowledge_categories_category_id",
                        column: x => x.knowledge_category_id,
                        principalSchema: "knowledge",
                        principalTable: "knowledge_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_knowledge_titles_users_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_knowledge_titles_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_knowledge_titles_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_knowledge_categories_created_by_id",
                schema: "knowledge",
                table: "knowledge_categories",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_knowledge_categories_deleted_by_id",
                schema: "knowledge",
                table: "knowledge_categories",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_categories_name",
                schema: "knowledge",
                table: "knowledge_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_knowledge_categories_updated_by_id",
                schema: "knowledge",
                table: "knowledge_categories",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_knowledge_titles_created_by_id",
                schema: "knowledge",
                table: "knowledge_titles",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_knowledge_titles_deleted_by_id",
                schema: "knowledge",
                table: "knowledge_titles",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_titles_knowledge_category_id_title",
                schema: "knowledge",
                table: "knowledge_titles",
                columns: new[] { "knowledge_category_id", "title" });

            migrationBuilder.CreateIndex(
                name: "ix_knowledge_titles_updated_by_id",
                schema: "knowledge",
                table: "knowledge_titles",
                column: "updated_by_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "knowledge_titles",
                schema: "knowledge");

            migrationBuilder.DropTable(
                name: "knowledge_categories",
                schema: "knowledge");
        }
    }
}
