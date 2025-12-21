using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FocusFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyIdentitySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_tasks_projects_project_id",
                schema: "focus_flow",
                table: "Tasks");

            migrationBuilder.DropPrimaryKey(
                name: "pk_tasks",
                schema: "focus_flow",
                table: "Tasks");

            migrationBuilder.DropPrimaryKey(
                name: "pk_projects",
                schema: "focus_flow",
                table: "Projects");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameColumn(
                name: "title",
                schema: "focus_flow",
                table: "Tasks",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "focus_flow",
                table: "Tasks",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "priority",
                schema: "focus_flow",
                table: "Tasks",
                newName: "Priority");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "focus_flow",
                table: "Tasks",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "focus_flow",
                table: "Tasks",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "focus_flow",
                table: "Tasks",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "project_id",
                schema: "focus_flow",
                table: "Tasks",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "due_date",
                schema: "focus_flow",
                table: "Tasks",
                newName: "DueDate");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "focus_flow",
                table: "Tasks",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "completed_at",
                schema: "focus_flow",
                table: "Tasks",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "assigned_user_id",
                schema: "focus_flow",
                table: "Tasks",
                newName: "AssignedUserId");

            migrationBuilder.RenameIndex(
                name: "ix_tasks_status",
                schema: "focus_flow",
                table: "Tasks",
                newName: "ix_tasks_status");

            migrationBuilder.RenameIndex(
                name: "ix_tasks_project_id",
                schema: "focus_flow",
                table: "Tasks",
                newName: "ix_tasks_project_id");

            migrationBuilder.RenameIndex(
                name: "ix_tasks_due_date",
                schema: "focus_flow",
                table: "Tasks",
                newName: "ix_tasks_due_date");

            migrationBuilder.RenameIndex(
                name: "ix_tasks_created_at",
                schema: "focus_flow",
                table: "Tasks",
                newName: "ix_tasks_created_at");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "focus_flow",
                table: "Projects",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "focus_flow",
                table: "Projects",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "focus_flow",
                table: "Projects",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "focus_flow",
                table: "Projects",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                schema: "focus_flow",
                table: "Projects",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "focus_flow",
                table: "Projects",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_projects_owner_id",
                schema: "focus_flow",
                table: "Projects",
                newName: "ix_projects_owner_id");

            migrationBuilder.RenameIndex(
                name: "ix_projects_created_at",
                schema: "focus_flow",
                table: "Projects",
                newName: "ix_projects_created_at");

            migrationBuilder.AddPrimaryKey(
                name: "pk_tasks",
                schema: "focus_flow",
                table: "Tasks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_projects",
                schema: "focus_flow",
                table: "Projects",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "asp_net_users", 
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_users_normalized_email", 
                schema: "public",
                table: "asp_net_users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_users_normalized_user_name",
                schema: "public",
                table: "asp_net_users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_users_created_at",
                schema: "public",
                table: "asp_net_users",
                column: "CreatedAt");

            migrationBuilder.AddForeignKey(
                name: "fk_tasks_projects_project_id",
                schema: "focus_flow",
                table: "Tasks",
                column: "ProjectId",
                principalSchema: "focus_flow",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_tasks_projects_project_id",
                schema: "focus_flow",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "asp_net_users",
                schema: "public");

            migrationBuilder.DropPrimaryKey(
                name: "pk_tasks",
                schema: "focus_flow",
                table: "Tasks");

            migrationBuilder.DropPrimaryKey(
                name: "pk_projects",
                schema: "focus_flow",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "Title",
                schema: "focus_flow",
                table: "Tasks",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "focus_flow",
                table: "Tasks",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Priority",
                schema: "focus_flow",
                table: "Tasks",
                newName: "priority");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "focus_flow",
                table: "Tasks",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "focus_flow",
                table: "Tasks",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "focus_flow",
                table: "Tasks",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                schema: "focus_flow",
                table: "Tasks",
                newName: "project_id");

            migrationBuilder.RenameColumn(
                name: "DueDate",
                schema: "focus_flow",
                table: "Tasks",
                newName: "due_date");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "focus_flow",
                table: "Tasks",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                schema: "focus_flow",
                table: "Tasks",
                newName: "completed_at");

            migrationBuilder.RenameColumn(
                name: "AssignedUserId",
                schema: "focus_flow",
                table: "Tasks",
                newName: "assigned_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_tasks_status",
                schema: "focus_flow",
                table: "Tasks",
                newName: "ix_tasks_status");

            migrationBuilder.RenameIndex(
                name: "ix_tasks_project_id",
                schema: "focus_flow",
                table: "Tasks",
                newName: "ix_tasks_project_id");

            migrationBuilder.RenameIndex(
                name: "ix_tasks_due_date",
                schema: "focus_flow",
                table: "Tasks",
                newName: "ix_tasks_due_date");

            migrationBuilder.RenameIndex(
                name: "ix_tasks_created_at",
                schema: "focus_flow",
                table: "Tasks",
                newName: "ix_tasks_created_at");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "focus_flow",
                table: "Projects",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "focus_flow",
                table: "Projects",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "focus_flow",
                table: "Projects",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "focus_flow",
                table: "Projects",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                schema: "focus_flow",
                table: "Projects",
                newName: "owner_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "focus_flow",
                table: "Projects",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "ix_projects_owner_id",
                schema: "focus_flow",
                table: "Projects",
                newName: "ix_projects_owner_id");

            migrationBuilder.RenameIndex(
                name: "ix_projects_created_at",
                schema: "focus_flow",
                table: "Projects",
                newName: "ix_projects_created_at");

            migrationBuilder.AddPrimaryKey(
                name: "pk_tasks",
                schema: "focus_flow",
                table: "Tasks",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_projects",
                schema: "focus_flow",
                table: "Projects",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_tasks_projects_project_id",
                schema: "focus_flow",
                table: "Tasks",
                column: "project_id",
                principalSchema: "focus_flow",
                principalTable: "Projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
