using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInstruksVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop previous simple index so we can add the composite one with IsLatest
            migrationBuilder.DropIndex(
                name: "IX_InstruksTable_CategoryId",
                table: "InstruksTable");

            // Make existing columns nullable where intended
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "InstruksTable",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "InstruksTable",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "InstruksTable",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            // --- Versioning columns ---
            migrationBuilder.AddColumn<Guid>(
                name: "DocumentId",
                table: "InstruksTable",
                type: "TEXT",
                nullable: false); // no default: must be set by code

            migrationBuilder.AddColumn<bool>(
                name: "IsLatest",
                table: "InstruksTable",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreviousVersionId",
                table: "InstruksTable",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "InstruksTable",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "InstruksTable",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1); // safer default than 0

            // Indices
            migrationBuilder.CreateIndex(
                name: "IX_InstruksTable_CategoryId_IsLatest",
                table: "InstruksTable",
                columns: new[] { "CategoryId", "IsLatest" });

            migrationBuilder.CreateIndex(
                name: "IX_InstruksTable_DocumentId_VersionNumber",
                table: "InstruksTable",
                columns: new[] { "DocumentId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new indices
            migrationBuilder.DropIndex(
                name: "IX_InstruksTable_CategoryId_IsLatest",
                table: "InstruksTable");

            migrationBuilder.DropIndex(
                name: "IX_InstruksTable_DocumentId_VersionNumber",
                table: "InstruksTable");

            // Drop versioning columns
            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "InstruksTable");

            migrationBuilder.DropColumn(
                name: "IsLatest",
                table: "InstruksTable");

            migrationBuilder.DropColumn(
                name: "PreviousVersionId",
                table: "InstruksTable");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "InstruksTable");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "InstruksTable");

            // Revert nullability changes
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "InstruksTable",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "InstruksTable",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "InstruksTable",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            // Restore the original index on CategoryId
            migrationBuilder.CreateIndex(
                name: "IX_InstruksTable_CategoryId",
                table: "InstruksTable",
                column: "CategoryId");
        }
    }
}
