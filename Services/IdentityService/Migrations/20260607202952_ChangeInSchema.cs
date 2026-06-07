using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AccountStatus",
                schema: "identity",
                table: "users",
                newName: "account_status");

            migrationBuilder.AlterColumn<string>(
                name: "account_status",
                schema: "identity",
                table: "users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "account_status",
                schema: "identity",
                table: "users",
                newName: "AccountStatus");

            migrationBuilder.AlterColumn<int>(
                name: "AccountStatus",
                schema: "identity",
                table: "users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);
        }
    }
}
