using Microsoft.EntityFrameworkCore;
using VanderBeer.Api.Data;
using VanderBeer.Api.DTOs;
using VanderBeer.Api.Models;

namespace VanderBeer.Api.Services;

public sealed class PedidoService(VanderBeerDbContext db)
{
    private static readonly string[] FormasPagamento = ["Pix", "Cartao", "Dinheiro"];
    private static readonly string[] FluxoStatus =
        ["Confirmado", "EmPreparacao", "SaiuParaEntrega", "Entregue"];

    public async Task<Pedido> CriarAsync(CriarPedidoDto dto, CancellationToken cancellationToken)
    {
        if (!await db.Clientes.AnyAsync(x => x.Id == dto.ClienteId, cancellationToken))
            throw new RegraNegocioException("Cliente não encontrado.");

        var formaPagamento = NormalizarOpcao(dto.FormaPagamento, FormasPagamento)
            ?? throw new RegraNegocioException("Forma de pagamento inválida.");

        if (formaPagamento != "Dinheiro" && dto.TrocoPara is not null)
            throw new RegraNegocioException("Troco somente pode ser informado para pagamento em dinheiro.");

        var itensAgrupados = dto.Itens
            .GroupBy(x => x.ProdutoId)
            .Select(x => new { ProdutoId = x.Key, Quantidade = x.Sum(i => i.Quantidade) })
            .ToList();

        if (itensAgrupados.Count == 0)
            throw new RegraNegocioException("O pedido deve possuir pelo menos um item.");

        var ids = itensAgrupados.Select(x => x.ProdutoId).ToList();
        var produtos = await db.Produtos
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var pedido = new Pedido
        {
            ClienteId = dto.ClienteId,
            EnderecoEntrega = dto.EnderecoEntrega.Trim(),
            FormaPagamento = formaPagamento,
            TrocoPara = dto.TrocoPara,
            Status = "Confirmado"
        };

        foreach (var entrada in itensAgrupados)
        {
            if (!produtos.TryGetValue(entrada.ProdutoId, out var produto) || !produto.Ativo)
                throw new RegraNegocioException($"Produto {entrada.ProdutoId} não está disponível.");
            if (entrada.Quantidade <= 0)
                throw new RegraNegocioException("A quantidade deve ser maior que zero.");
            if (produto.Estoque < entrada.Quantidade)
                throw new RegraNegocioException($"Estoque insuficiente para {produto.Nome}. Disponível: {produto.Estoque}.");

            var subtotal = produto.Preco * entrada.Quantidade;
            produto.Estoque -= entrada.Quantidade;
            pedido.Itens.Add(new ItemPedido
            {
                ProdutoId = produto.Id,
                Quantidade = entrada.Quantidade,
                PrecoUnitario = produto.Preco,
                Subtotal = subtotal
            });
            pedido.Total += subtotal;
        }

        if (pedido.TrocoPara is not null && pedido.TrocoPara < pedido.Total)
            throw new RegraNegocioException("O valor para troco não pode ser menor que o total.");

        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(cancellationToken)
            : null;

        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync(cancellationToken);
        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);

        return pedido;
    }

    public async Task<Pedido> AtualizarStatusAsync(Guid codigo, string novoStatus, CancellationToken cancellationToken)
    {
        var pedido = await db.Pedidos.Include(x => x.Itens)
            .FirstOrDefaultAsync(x => x.CodigoPedido == codigo, cancellationToken)
            ?? throw new KeyNotFoundException("Pedido não encontrado.");

        var status = novoStatus.Equals("Cancelado", StringComparison.OrdinalIgnoreCase)
            ? "Cancelado"
            : NormalizarOpcao(novoStatus, FluxoStatus)
              ?? throw new RegraNegocioException("Status inválido.");

        if (pedido.Status is "Entregue" or "Cancelado")
            throw new RegraNegocioException("Um pedido finalizado não pode mudar de status.");

        if (status == "Cancelado")
        {
            if (!pedido.EstoqueDevolvido)
            {
                var ids = pedido.Itens.Select(x => x.ProdutoId).ToList();
                var produtos = await db.Produtos.Where(x => ids.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, cancellationToken);
                foreach (var item in pedido.Itens)
                    produtos[item.ProdutoId].Estoque += item.Quantidade;
                pedido.EstoqueDevolvido = true;
            }
        }
        else
        {
            var atual = Array.IndexOf(FluxoStatus, pedido.Status);
            var proximo = Array.IndexOf(FluxoStatus, status);
            if (proximo != atual + 1)
                throw new RegraNegocioException("A alteração não segue o fluxo de status do pedido.");
        }

        pedido.Status = status;
        await db.SaveChangesAsync(cancellationToken);
        return pedido;
    }

    private static string? NormalizarOpcao(string valor, IEnumerable<string> opcoes) =>
        opcoes.FirstOrDefault(x => x.Equals(valor.Trim(), StringComparison.OrdinalIgnoreCase));
}
