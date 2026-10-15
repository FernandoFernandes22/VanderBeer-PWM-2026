namespace VanderBeer.App.Models;

public sealed class ItemPedidoModel
{
    public string Produto { get; set; } = "";
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string Descricao => $"{Quantidade} × {Produto}";
    public string ValorUnitario => $"Preço unitário: R$ {PrecoUnitario:F2}";
}
