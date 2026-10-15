using System.ComponentModel.DataAnnotations;

namespace VanderBeer.Api.DTOs;

public record ItemPedidoEntradaDto(
    [Range(1, int.MaxValue)] int ProdutoId,
    [Range(1, int.MaxValue)] int Quantidade);

public record CriarPedidoDto(
    [Range(1, int.MaxValue)] int ClienteId,
    [Required, StringLength(300)] string EnderecoEntrega,
    [Required] string FormaPagamento,
    decimal? TrocoPara,
    [Required, MinLength(1)] IReadOnlyCollection<ItemPedidoEntradaDto> Itens);

public record AtualizarStatusPedidoDto([Required] string Status);

public record ItemPedidoSaidaDto(
    int ProdutoId,
    string Produto,
    int Quantidade,
    decimal PrecoUnitario,
    decimal Subtotal);

public record PedidoSaidaDto(
    Guid CodigoPedido,
    int ClienteId,
    DateTime DataPedido,
    string FormaPagamento,
    string Status,
    string EnderecoEntrega,
    decimal Total,
    decimal? TrocoPara,
    IReadOnlyCollection<ItemPedidoSaidaDto> Itens);
