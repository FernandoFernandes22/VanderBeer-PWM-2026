using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanderBeer.Api.Data;
using VanderBeer.Api.DTOs;
using VanderBeer.Api.Models;
using VanderBeer.Api.Services;

namespace VanderBeer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PedidosController(VanderBeerDbContext db, PedidoService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PedidoSaidaDto>> Criar(CriarPedidoDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var pedido = await service.CriarAsync(dto, cancellationToken);
            var salvo = await ConsultaCompleta().FirstAsync(x => x.Id == pedido.Id, cancellationToken);
            return CreatedAtAction(nameof(Obter), new { codigo = pedido.CodigoPedido }, Mapear(salvo));
        }
        catch (RegraNegocioException ex)
        {
            return Conflict(new ProblemDetails { Title = "Pedido não pôde ser criado", Detail = ex.Message, Status = 409 });
        }
    }

    [HttpGet("{codigo:guid}")]
    public async Task<ActionResult<PedidoSaidaDto>> Obter(Guid codigo, CancellationToken cancellationToken)
    {
        var pedido = await ConsultaCompleta().FirstOrDefaultAsync(x => x.CodigoPedido == codigo, cancellationToken);
        return pedido is null ? NotFound() : Ok(Mapear(pedido));
    }

    [HttpPatch("{codigo:guid}/status")]
    public async Task<ActionResult<PedidoSaidaDto>> AtualizarStatus(Guid codigo, AtualizarStatusPedidoDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await service.AtualizarStatusAsync(codigo, dto.Status, cancellationToken);
            var pedido = await ConsultaCompleta().FirstAsync(x => x.CodigoPedido == codigo, cancellationToken);
            return Ok(Mapear(pedido));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (RegraNegocioException ex)
        {
            return Conflict(new ProblemDetails { Title = "Status não alterado", Detail = ex.Message, Status = 409 });
        }
    }

    private IQueryable<Pedido> ConsultaCompleta() => db.Pedidos.AsNoTracking()
        .Include(x => x.Itens).ThenInclude(x => x.Produto);

    private static PedidoSaidaDto Mapear(Pedido salvo) => new(
        salvo.CodigoPedido,
        salvo.ClienteId,
        salvo.DataPedido,
        salvo.FormaPagamento,
        salvo.Status,
        salvo.EnderecoEntrega,
        salvo.Total,
        salvo.TrocoPara,
        salvo.Itens.Select(x => new ItemPedidoSaidaDto(
            x.ProdutoId, x.Produto?.Nome ?? string.Empty, x.Quantidade, x.PrecoUnitario, x.Subtotal)).ToList());
}
