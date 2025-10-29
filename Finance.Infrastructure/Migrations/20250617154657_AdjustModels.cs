using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.Web.Migrations
{
    /// <inheritdoc />
    public partial class AdjustModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Moviments_Categories_CategoryId",
                table: "Moviments");

            migrationBuilder.DropColumn(
                name: "typeExpense",
                table: "Moviments");

            migrationBuilder.DropColumn(
                name: "typeIncome",
                table: "Moviments");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Moviments",
                newName: "categoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Moviments_CategoryId",
                table: "Moviments",
                newName: "IX_Moviments_categoryId");

            migrationBuilder.AddColumn<decimal>(
                name: "value",
                table: "Moviments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Moviments_Categories_categoryId",
                table: "Moviments",
                column: "categoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Moviments_Categories_categoryId",
                table: "Moviments");

            migrationBuilder.DropColumn(
                name: "value",
                table: "Moviments");

            migrationBuilder.RenameColumn(
                name: "categoryId",
                table: "Moviments",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Moviments_categoryId",
                table: "Moviments",
                newName: "IX_Moviments_CategoryId");

            migrationBuilder.AddColumn<int>(
                name: "typeExpense",
                table: "Moviments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "typeIncome",
                table: "Moviments",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Moviments_Categories_CategoryId",
                table: "Moviments",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
