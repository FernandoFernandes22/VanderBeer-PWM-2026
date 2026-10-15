using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanderBeer.Api.Controllers;
using VanderBeer.Api.Data;
using VanderBeer.Api.DTOs;
using VanderBeer.Api.Models;
using Xunit;

namespace VanderBeer.Api.Tests;

public class HistoricoPedidosTests
{
    [Fact]
    public async Task Historico_retorna_itens_valores_e_apenas_pedidos_do_cliente()
    {
        await using var db = new VanderBeerDbContext(new DbContextOptionsBuilder<VanderBeerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Clientes.AddRange(new Cliente { Id = 1 }, new Cliente { Id = 2 });
        var produto = new Produto { Id = 10, Nome = "Água", Preco = 9m };
        var pedido = new Pedido { ClienteId = 1, Total = 6m, FormaPagamento = "Pix", EnderecoEntrega = "Rua A, 10" };
        pedido.Itens.Add(new ItemPedido { Produto = produto, Quantidade = 2, PrecoUnitario = 3m, Subtotal = 6m });
        db.Pedidos.AddRange(pedido, new Pedido { ClienteId = 2 });
        await db.SaveChangesAsync();

        var resposta = Assert.IsType<OkObjectResult>(await new ClientesController(db).ListarPedidos(1, default));
        var historico = Assert.IsType<List<PedidoSaidaDto>>(resposta.Value);
        var salvo = Assert.Single(historico);
        Assert.Equal(pedido.CodigoPedido, salvo.CodigoPedido);
        Assert.Equal(6m, salvo.Total);
        Assert.Equal(pedido.DataPedido, salvo.DataPedido);
        Assert.Equal("Rua A, 10", salvo.EnderecoEntrega);
        var item = Assert.Single(salvo.Itens);
        Assert.Equal("Água", item.Produto);
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(3m, item.PrecoUnitario);
    }
}
