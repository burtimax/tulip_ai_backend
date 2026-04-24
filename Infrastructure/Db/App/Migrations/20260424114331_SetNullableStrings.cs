using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Db.App.Migrations
{
    /// <inheritdoc />
    public partial class SetNullableStrings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "size_bytes",
                schema: "app",
                table: "message_images",
                type: "bigint",
                nullable: true,
                comment: "Размер изображения в байтах",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Размер изображения в байтах");

            migrationBuilder.AlterColumn<string>(
                name: "mime_type",
                schema: "app",
                table: "message_images",
                type: "text",
                nullable: true,
                comment: "MIME-тип изображения",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "MIME-тип изображения");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "size_bytes",
                schema: "app",
                table: "message_images",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Размер изображения в байтах",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "Размер изображения в байтах");

            migrationBuilder.AlterColumn<string>(
                name: "mime_type",
                schema: "app",
                table: "message_images",
                type: "text",
                nullable: false,
                defaultValue: "",
                comment: "MIME-тип изображения",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "MIME-тип изображения");
        }
    }
}
