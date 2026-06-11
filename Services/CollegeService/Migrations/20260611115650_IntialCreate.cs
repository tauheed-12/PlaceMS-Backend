using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollegeService.Migrations
{
    /// <inheritdoc />
    public partial class IntialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "college");

            migrationBuilder.CreateTable(
                name: "admin_colleges",
                schema: "college",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    college_id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollegeName = table.Column<string>(type: "text", nullable: false),
                    CollegeCode = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_colleges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "college_tpos",
                schema: "college",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    college_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tpo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    CollegeName = table.Column<string>(type: "text", nullable: false),
                    CollegeCode = table.Column<string>(type: "text", nullable: false),
                    tpo_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_college_tpos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "colleges",
                schema: "college",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Website = table.Column<string>(type: "text", nullable: false),
                    affiliated_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    city = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    state = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Pincode = table.Column<string>(type: "text", nullable: false),
                    registered_by = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AccountStatus = table.Column<int>(type: "integer", nullable: false),
                    verification_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_colleges", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_colleges_code",
                schema: "college",
                table: "colleges",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_colleges_email",
                schema: "college",
                table: "colleges",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_colleges",
                schema: "college");

            migrationBuilder.DropTable(
                name: "college_tpos",
                schema: "college");

            migrationBuilder.DropTable(
                name: "colleges",
                schema: "college");
        }
    }
}
