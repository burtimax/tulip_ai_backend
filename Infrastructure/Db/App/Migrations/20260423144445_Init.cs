using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Db.App.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "stat");

            migrationBuilder.EnsureSchema(
                name: "app");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    confirmed_policy_and_documents = table.Column<bool>(type: "boolean", nullable: true, comment: "Пользователь подтвердил ознакомление с политикой и документами."),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "llm_usages",
                schema: "stat",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ИД сущности."),
                    input_json = table.Column<string>(type: "jsonb", nullable: false, comment: "Входящий запрос к LLM в формате JSON"),
                    duration_ms = table.Column<long>(type: "bigint", nullable: false, comment: "Длительность HTTP-запроса, мс"),
                    is_success = table.Column<bool>(type: "boolean", nullable: false, comment: "Успешное завершение попытки (HTTP и разбор ответа)"),
                    llm_response = table.Column<string>(type: "text", nullable: true, comment: "Текст ответа ассистента или сырое тело ответа при сбое разбора"),
                    error_message = table.Column<string>(type: "text", nullable: true, comment: "Сообщение об ошибке при неуспехе"),
                    prompt_tokens = table.Column<int>(type: "integer", nullable: true, comment: "Число токенов на входе"),
                    completion_tokens = table.Column<int>(type: "integer", nullable: true, comment: "Число токенов на выходе"),
                    llm_model = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, comment: "Имя модели LLM"),
                    cost = table.Column<decimal>(type: "numeric", nullable: true, comment: "Стоимость запроса (если провайдер вернул)"),
                    llm_request_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "Идентификатор запроса/ответа у провайдера LLM"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Когда сущность была создана."),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто создал сущность."),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была в последний раз обновлена."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто обновил сущность."),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была удалена."),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто удалил сущность.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_llm_usages", x => x.id);
                    table.ForeignKey(
                        name: "fk_llm_usages_users_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_llm_usages_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_llm_usages_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ИД сущности."),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор пользователя (если известен)"),
                    log_source = table.Column<int>(type: "integer", nullable: true, comment: "Источник лога: бэкенд или фронтенд"),
                    log_type = table.Column<string>(type: "text", nullable: true, comment: "Тип/категория лога"),
                    log = table.Column<string>(type: "jsonb", nullable: true, comment: "Структурированные данные лога в формате JSON"),
                    log_message = table.Column<string>(type: "text", nullable: true, comment: "Текстовое сообщение лога"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Когда сущность была создана."),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто создал сущность."),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была в последний раз обновлена."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто обновил сущность."),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была удалена."),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто удалил сущность.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_logs_users_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_logs_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_logs_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_logs_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "stat_events",
                schema: "stat",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ИД сущности."),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор пользователя"),
                    session_id = table.Column<long>(type: "bigint", nullable: true, comment: "Идентификатор сессии"),
                    type = table.Column<string>(type: "text", nullable: true, comment: "Тип события"),
                    data = table.Column<string>(type: "text", nullable: true, comment: "Данные о событии"),
                    duration_seconds = table.Column<double>(type: "double precision", nullable: true, comment: "Длительность выполняемого события"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Когда сущность была создана."),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто создал сущность."),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была в последний раз обновлена."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто обновил сущность."),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Когда сущность была удалена."),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Кто удалил сущность.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stat_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_stat_events_users_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_stat_events_users_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_stat_events_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_stat_events_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_llm_usages_created_by_id",
                schema: "stat",
                table: "llm_usages",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_llm_usages_deleted_by_id",
                schema: "stat",
                table: "llm_usages",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_llm_usages_updated_by_id",
                schema: "stat",
                table: "llm_usages",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_logs_created_by_id",
                table: "logs",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_logs_deleted_by_id",
                table: "logs",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_logs_updated_by_id",
                table: "logs",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_logs_user_id",
                table: "logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_stat_events_created_by_id",
                schema: "stat",
                table: "stat_events",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_stat_events_deleted_by_id",
                schema: "stat",
                table: "stat_events",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_stat_events_updated_by_id",
                schema: "stat",
                table: "stat_events",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_stat_events_user_id",
                schema: "stat",
                table: "stat_events",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "llm_usages",
                schema: "stat");

            migrationBuilder.DropTable(
                name: "logs");

            migrationBuilder.DropTable(
                name: "stat_events",
                schema: "stat");

            migrationBuilder.DropTable(
                name: "users",
                schema: "app");
        }
    }
}
