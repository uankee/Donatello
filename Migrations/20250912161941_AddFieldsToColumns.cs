using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Donatello.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsToColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "Columns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Columns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AttachmentPath", "Description" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AttachmentPath", "Description" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AttachmentPath", "Description" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Columns");
        }
    }
}
