using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IncedentsAPI.Migrations
{
    /// <inheritdoc />
    public partial class fixAccountFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Incedents_Name",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_IncedentName",
                table: "Accounts",
                column: "IncedentName");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Incedents_IncedentName",
                table: "Accounts",
                column: "IncedentName",
                principalTable: "Incedents",
                principalColumn: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Incedents_IncedentName",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_IncedentName",
                table: "Accounts");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Incedents_Name",
                table: "Accounts",
                column: "Name",
                principalTable: "Incedents",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
