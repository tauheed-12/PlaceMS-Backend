using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentService.Migrations
{
    /// <inheritdoc />
    public partial class InititalCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "student");

            migrationBuilder.CreateTable(
                name: "student_profiles",
                schema: "student",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    college_id = table.Column<Guid>(type: "uuid", nullable: false),
                    college_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    college_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    branch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    batch_year = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    cgpa = table.Column<double>(type: "double precision", precision: 4, scale: 2, nullable: false, defaultValue: 0.0),
                    about_me = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    profile_completion_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    active_resume_file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "certifications",
                schema: "student",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    issuing_organization = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    issue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    credential_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_certifications_student_profiles_student_profile_id",
                        column: x => x.student_profile_id,
                        principalSchema: "student",
                        principalTable: "student_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "educations",
                schema: "student",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    degree = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    institution = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    start_year = table.Column<int>(type: "integer", nullable: false),
                    end_year = table.Column<int>(type: "integer", nullable: true),
                    score = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_educations", x => x.id);
                    table.ForeignKey(
                        name: "FK_educations_student_profiles_student_profile_id",
                        column: x => x.student_profile_id,
                        principalSchema: "student",
                        principalTable: "student_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                schema: "student",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    tech_stack = table.Column<string>(type: "jsonb", nullable: false),
                    project_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_projects_student_profiles_student_profile_id",
                        column: x => x.student_profile_id,
                        principalSchema: "student",
                        principalTable: "student_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resume_files",
                schema: "student",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    stored_object_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    bucket_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resume_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_resume_files_student_profiles_student_profile_id",
                        column: x => x.student_profile_id,
                        principalSchema: "student",
                        principalTable: "student_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "skills",
                schema: "student",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skills", x => x.id);
                    table.ForeignKey(
                        name: "FK_skills_student_profiles_student_profile_id",
                        column: x => x.student_profile_id,
                        principalSchema: "student",
                        principalTable: "student_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_certifications_student_profile_id",
                schema: "student",
                table: "certifications",
                column: "student_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_educations_student_profile_id",
                schema: "student",
                table: "educations",
                column: "student_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_student_profile_id",
                schema: "student",
                table: "projects",
                column: "student_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_resume_files_profile_active",
                schema: "student",
                table: "resume_files",
                columns: new[] { "student_profile_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_skills_profile_name",
                schema: "student",
                table: "skills",
                columns: new[] { "student_profile_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_student_profiles_college_id",
                schema: "student",
                table: "student_profiles",
                column: "college_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_profiles_email",
                schema: "student",
                table: "student_profiles",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_student_profiles_user_id",
                schema: "student",
                table: "student_profiles",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "certifications",
                schema: "student");

            migrationBuilder.DropTable(
                name: "educations",
                schema: "student");

            migrationBuilder.DropTable(
                name: "projects",
                schema: "student");

            migrationBuilder.DropTable(
                name: "resume_files",
                schema: "student");

            migrationBuilder.DropTable(
                name: "skills",
                schema: "student");

            migrationBuilder.DropTable(
                name: "student_profiles",
                schema: "student");
        }
    }
}
