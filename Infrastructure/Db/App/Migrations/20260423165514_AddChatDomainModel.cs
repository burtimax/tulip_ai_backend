using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Db.App.Migrations
{
    /// <inheritdoc />
    public partial class AddChatDomainModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chats",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ИД сущности."),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор пользователя"),
                    title = table.Column<string>(type: "text", nullable: true, comment: "Краткий заголовок чата"),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, comment: "Агрегированный статус чата"),
                    last_message_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Время последнего сообщения в чате"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Когда сущность была создана."),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто создал сущность."),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была в последний раз обновлена."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто обновил сущность."),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была удалена."),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто удалил сущность.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chats", x => x.id);
                    table.ForeignKey(
                        name: "fk_chats_users_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_chats_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_chats_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_chats_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ИД сущности."),
                    chat_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор чата"),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, comment: "Роль отправителя"),
                    text_html = table.Column<string>(type: "text", nullable: true, comment: "Санитизированный HTML пользователя или ассистента"),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, comment: "Статус обработки сообщения"),
                    failure_code = table.Column<string>(type: "text", nullable: true, comment: "Код ошибки обработки"),
                    failure_reason = table.Column<string>(type: "text", nullable: true, comment: "Причина ошибки обработки"),
                    retry_count = table.Column<int>(type: "integer", nullable: false, comment: "Число попыток обработки"),
                    client_request_id = table.Column<string>(type: "text", nullable: true, comment: "Ключ идемпотентности клиентского запроса"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Когда сущность была создана."),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто создал сущность."),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была в последний раз обновлена."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто обновил сущность."),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была удалена."),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто удалил сущность.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_messages_chats_chat_id",
                        column: x => x.chat_id,
                        principalSchema: "app",
                        principalTable: "chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_messages_users_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_messages_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_messages_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "message_images",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ИД сущности."),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор сообщения"),
                    storage_url = table.Column<string>(type: "text", nullable: false, comment: "URL изображения в хранилище"),
                    mime_type = table.Column<string>(type: "text", nullable: false, comment: "MIME-тип изображения"),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false, comment: "Размер изображения в байтах"),
                    width = table.Column<int>(type: "integer", nullable: true, comment: "Ширина изображения"),
                    height = table.Column<int>(type: "integer", nullable: true, comment: "Высота изображения"),
                    sort_order = table.Column<int>(type: "integer", nullable: false, comment: "Порядок изображения в сообщении"),
                    plant_id_raw_json = table.Column<string>(type: "jsonb", nullable: true, comment: "Сырой ответ PlantId"),
                    plant_id_normalized_json = table.Column<string>(type: "jsonb", nullable: true, comment: "Нормализованный ответ PlantId"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Когда сущность была создана."),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто создал сущность."),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была в последний раз обновлена."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто обновил сущность."),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была удалена."),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто удалил сущность.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_message_images_messages_message_id",
                        column: x => x.message_id,
                        principalSchema: "app",
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_message_images_users_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_message_images_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_message_images_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "processing_jobs",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ИД сущности."),
                    chat_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор чата"),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор сообщения"),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, comment: "Статус задачи"),
                    attempt = table.Column<int>(type: "integer", nullable: false, comment: "Номер попытки"),
                    max_attempts = table.Column<int>(type: "integer", nullable: false, comment: "Максимальное число попыток"),
                    locked_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Время удержания lock на задаче"),
                    last_error = table.Column<string>(type: "text", nullable: true, comment: "Последняя ошибка обработки"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Когда сущность была создана."),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто создал сущность."),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была в последний раз обновлена."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто обновил сущность."),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была удалена."),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто удалил сущность.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processing_jobs", x => x.id);
                    table.ForeignKey(
                        name: "fk_processing_jobs_chats_chat_id",
                        column: x => x.chat_id,
                        principalSchema: "app",
                        principalTable: "chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_processing_jobs_messages_message_id",
                        column: x => x.message_id,
                        principalSchema: "app",
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_processing_jobs_users_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_processing_jobs_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_processing_jobs_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_chats_created_by_id",
                schema: "app",
                table: "chats",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_chats_deleted_by_id",
                schema: "app",
                table: "chats",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_chats_updated_by_id",
                schema: "app",
                table: "chats",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_chats_user_id_updated_at",
                schema: "app",
                table: "chats",
                columns: new[] { "user_id", "updated_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_message_images_created_by_id",
                schema: "app",
                table: "message_images",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_message_images_deleted_by_id",
                schema: "app",
                table: "message_images",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_message_images_message_id",
                schema: "app",
                table: "message_images",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "ix_message_images_updated_by_id",
                schema: "app",
                table: "message_images",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_chat_id_created_at",
                schema: "app",
                table: "messages",
                columns: new[] { "chat_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_messages_client_request_id",
                schema: "app",
                table: "messages",
                column: "client_request_id",
                unique: true,
                filter: "\"client_request_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_messages_created_by_id",
                schema: "app",
                table: "messages",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_deleted_by_id",
                schema: "app",
                table: "messages",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_updated_by_id",
                schema: "app",
                table: "messages",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_processing_jobs_chat_id",
                schema: "app",
                table: "processing_jobs",
                column: "chat_id");

            migrationBuilder.CreateIndex(
                name: "ix_processing_jobs_created_by_id",
                schema: "app",
                table: "processing_jobs",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_processing_jobs_deleted_by_id",
                schema: "app",
                table: "processing_jobs",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_processing_jobs_message_id",
                schema: "app",
                table: "processing_jobs",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "IX_processing_jobs_status_created_at",
                schema: "app",
                table: "processing_jobs",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_processing_jobs_updated_by_id",
                schema: "app",
                table: "processing_jobs",
                column: "updated_by_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "message_images",
                schema: "app");

            migrationBuilder.DropTable(
                name: "processing_jobs",
                schema: "app");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "app");

            migrationBuilder.DropTable(
                name: "chats",
                schema: "app");
        }
    }
}
