using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanderBeer.Api.Migrations
{
    /// <inheritdoc />
    public partial class PedidosTrocoEEstoqueDevolvido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EstoqueDevolvido",
                table: "Pedidos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TrocoPara",
                table: "Pedidos",
                type: "decimal(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstoqueDevolvido",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "TrocoPara",
                table: "Pedidos");
        }
    }
}
