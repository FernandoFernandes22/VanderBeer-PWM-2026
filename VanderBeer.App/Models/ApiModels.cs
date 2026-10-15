namespace VanderBeer.App.Models;

public sealed class ProdutoModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
    public string Categoria { get; set; } = string.Empty;
}

public sealed class ClienteModel { public int Id { get; set; } public Guid Verificador { get; set; } public string Nome { get; set; } = ""; }
public sealed class PedidoModel
{
    public Guid CodigoPedido { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataPedido { get; set; }
    public string FormaPagamento { get; set; } = "";
    public string EnderecoEntrega { get; set; } = "";
    public List<ItemPedidoModel> Itens { get; set; } = [];
    public string CodigoExibicao => $"Pedido #{CodigoPedido.ToString()[..8].ToUpperInvariant()}";
    public string DataHora => DateTime.SpecifyKind(DataPedido, DateTimeKind.Utc).ToLocalTime().ToString("dd/MM/yyyy 'às' HH:mm");
    public string Pagamento => $"Pagamento: {(FormaPagamento == "Cartao" ? "Cartão" : FormaPagamento)} (simulado)";
    public string StatusExibicao => Status switch
    {
        "EmPreparacao" => "Em preparação", "SaiuParaEntrega" => "Saiu para entrega", _ => Status
    };
}
public sealed record ClienteEntrada(string Cpf, string Nome, string? Email, string Telefone, string? Cep, string? Endereco, string? Numero, string? Bairro, string? Cidade, string? Uf);
public sealed record ItemPedidoEntrada(int ProdutoId, int Quantidade);
public sealed record PedidoEntrada(int ClienteId, string EnderecoEntrega, string FormaPagamento, decimal? TrocoPara, IReadOnlyCollection<ItemPedidoEntrada> Itens);
