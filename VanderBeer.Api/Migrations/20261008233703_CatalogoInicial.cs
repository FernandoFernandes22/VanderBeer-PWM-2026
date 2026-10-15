using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VanderBeer.Api.Migrations
{
    /// <inheritdoc />
    public partial class CatalogoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Produtos",
                columns: new[] { "Id", "Ativo", "CategoriaId", "DataCadastro", "Descricao", "Estoque", "ImagemUrl", "Nome", "Preco" },
                values: new object[,]
                {
                    { 1, true, 1, new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Cerveja leve e refrescante.", 50, null, "Cerveja Pilsen 350 ml", 5.90m },
                    { 2, true, 1, new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Cerveja artesanal de amargor marcante.", 25, null, "Cerveja IPA 500 ml", 14.90m },
                    { 3, true, 2, new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Refrigerante sabor cola.", 40, null, "Refrigerante Cola 2 L", 10.50m },
                    { 4, true, 2, new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Refrigerante sabor guaraná.", 40, null, "Refrigerante Guaraná 2 L", 9.90m },
                    { 5, true, 3, new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Água mineral sem gás.", 100, null, "Água mineral 500 ml", 3.00m },
                    { 6, true, 3, new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Água mineral gaseificada.", 80, null, "Água com gás 500 ml", 3.50m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
