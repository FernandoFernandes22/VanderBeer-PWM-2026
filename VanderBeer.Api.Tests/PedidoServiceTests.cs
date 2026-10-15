using Microsoft.EntityFrameworkCore;
using VanderBeer.Api.Data;
using VanderBeer.Api.DTOs;
using VanderBeer.Api.Models;
using VanderBeer.Api.Services;
using Xunit;

namespace VanderBeer.Api.Tests;

public sealed class PedidoServiceTests
{
    [Fact]
    public async Task Criar_deve_calcular_total_e_baixar_estoque()
    {
        await using var db = CriarBanco();
        db.Clientes.Add(new Cliente { Id = 1, Cpf = "52998224725", Nome = "Cliente", Telefone = "11999999999" });
        db.Produtos.Add(new Produto { Id = 10, Nome = "Cerveja", Preco = 7.50m, Estoque = 10, CategoriaId = 1 });
        await db.SaveChangesAsync();

        var pedido = await new PedidoService(db).CriarAsync(new CriarPedidoDto(
            1, "Rua A, 10", "Pix", null, [new ItemPedidoEntradaDto(10, 3)]), default);

        Assert.Equal(22.50m, pedido.Total);
        Assert.Equal(7, (await db.Produtos.FindAsync(10))!.Estoque);
        Assert.Single(pedido.Itens);
        Assert.Equal(7.50m, pedido.Itens.Single().PrecoUnitario);
    }

    [Fact]
    public async Task Criar_deve_rejeitar_quantidade_maior_que_estoque()
    {
        await using var db = CriarBanco();
        db.Clientes.Add(new Cliente { Id = 1, Cpf = "52998224725", Nome = "Cliente", Telefone = "11999999999" });
        db.Produtos.Add(new Produto { Id = 10, Nome = "Cerveja", Preco = 7.50m, Estoque = 2, CategoriaId = 1 });
        await db.SaveChangesAsync();

        var erro = await Assert.ThrowsAsync<RegraNegocioException>(() =>
            new PedidoService(db).CriarAsync(new CriarPedidoDto(
                1, "Rua A, 10", "Pix", null, [new ItemPedidoEntradaDto(10, 3)]), default));

        Assert.Contains("Estoque insuficiente", erro.Message);
        Assert.Empty(db.Pedidos);
    }

    [Fact]
    public async Task Cancelar_deve_devolver_estoque_uma_unica_vez()
    {
        await using var db = CriarBanco();
        var produto = new Produto { Id = 10, Nome = "Cerveja", Preco = 7.50m, Estoque = 7, CategoriaId = 1 };
        var pedido = new Pedido { ClienteId = 1, EnderecoEntrega = "Rua A", FormaPagamento = "Pix", Status = "Confirmado", Total = 22.50m };
        pedido.Itens.Add(new ItemPedido { Produto = produto, ProdutoId = 10, Quantidade = 3, PrecoUnitario = 7.50m, Subtotal = 22.50m });
        db.Clientes.Add(new Cliente { Id = 1, Cpf = "52998224725", Nome = "Cliente", Telefone = "11999999999" });
        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();

        await new PedidoService(db).AtualizarStatusAsync(pedido.CodigoPedido, "Cancelado", default);

        Assert.Equal(10, produto.Estoque);
        Assert.True(pedido.EstoqueDevolvido);
        await Assert.ThrowsAsync<RegraNegocioException>(() =>
            new PedidoService(db).AtualizarStatusAsync(pedido.CodigoPedido, "Cancelado", default));
        Assert.Equal(10, produto.Estoque);
    }

    private static VanderBeerDbContext CriarBanco()
    {
        var options = new DbContextOptionsBuilder<VanderBeerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new VanderBeerDbContext(options);
    }
}
