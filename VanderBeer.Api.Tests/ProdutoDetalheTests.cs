using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanderBeer.Api.Controllers;
using VanderBeer.Api.Data;
using VanderBeer.Api.Models;
using Xunit;

namespace VanderBeer.Api.Tests;

public class ProdutoDetalheTests
{
    [Fact]
    public async Task Detalhe_pode_ser_serializado_sem_ciclo_categoria_produtos()
    {
        await using var db = new VanderBeerDbContext(new DbContextOptionsBuilder<VanderBeerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var categoria = new Categoria { Id = 1, Nome = "Águas" };
        db.Produtos.Add(new Produto { Id = 1, Nome = "Água", Preco = 3m, Estoque = 10, Categoria = categoria });
        await db.SaveChangesAsync();
        var resposta = Assert.IsType<OkObjectResult>(await new ProdutosController(db).Obter(1));
        var json = JsonSerializer.Serialize(resposta.Value);
        using var resultado = JsonDocument.Parse(json);
        Assert.Equal("Águas", resultado.RootElement.GetProperty("Categoria").GetString());
        Assert.Equal(3m, resultado.RootElement.GetProperty("Preco").GetDecimal());
        Assert.IsType<NotFoundResult>(await new ProdutosController(db).Obter(999));
    }
}
