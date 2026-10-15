using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanderBeer.Api.Controllers;
using VanderBeer.Api.Data;
using VanderBeer.Api.DTOs;
using Xunit;

namespace VanderBeer.Api.Tests;

public class CadastroClienteTests
{
    [Fact]
    public async Task Cadastro_gera_verificador_unico_e_persiste_documento_normalizado()
    {
        await using var db = new VanderBeerDbContext(new DbContextOptionsBuilder<VanderBeerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var controller = new ClientesController(db);
        var dto = new ClienteDto("11.222.333/0001-81", "Empresa teste", "teste@example.com",
            "11999999999", "01001000", "Rua Teste", "10", "Centro", "São Paulo", "SP");
        var resultado = Assert.IsType<CreatedAtActionResult>(await controller.Criar(dto, default));
        var cliente = Assert.IsType<ClienteSaidaDto>(resultado.Value);
        Assert.NotEqual(Guid.Empty, cliente.Verificador);
        Assert.Equal(cliente.Verificador, (await db.Clientes.SingleAsync()).Verificador);
        Assert.Equal("11222333000181", (await db.Clientes.SingleAsync()).Cpf);
        Assert.IsType<ConflictObjectResult>(await controller.Criar(dto, default));
        Assert.Equal(1, await db.Clientes.CountAsync());
    }
}
