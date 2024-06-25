using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IncedentsAPI.Migrations
{
    /// <inheritdoc />
    public partial class testNulable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Accounts_AccountName",
                table: "Contacts");

            migrationBuilder.AlterColumn<string>(
                name: "AccountName",
                table: "Contacts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Accounts_AccountName",
                table: "Contacts",
                column: "AccountName",
                principalTable: "Accounts",
                principalColumn: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Accounts_AccountName",
                table: "Contacts");

            migrationBuilder.AlterColumn<string>(
                name: "AccountName",
                table: "Contacts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Accounts_AccountName",
                table: "Contacts",
                column: "AccountName",
                principalTable: "Accounts",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
