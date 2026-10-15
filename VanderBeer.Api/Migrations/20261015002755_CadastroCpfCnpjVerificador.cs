using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanderBeer.Api.Migrations
{
    /// <inheritdoc />
    public partial class CadastroCpfCnpjVerificador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "Clientes",
                type: "varchar(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(11)",
                oldMaxLength: 11);

            migrationBuilder.AddColumn<Guid>(
                name: "Verificador",
                table: "Clientes",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("UPDATE `Clientes` SET `Verificador` = UUID() WHERE `Verificador` = '00000000-0000-0000-0000-000000000000'");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Verificador",
                table: "Clientes",
                column: "Verificador",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_Verificador",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Verificador",
                table: "Clientes");

            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "Clientes",
                type: "varchar(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(14)",
                oldMaxLength: 14);
        }
    }
}
