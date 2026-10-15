using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VanderBeer.App.Models;

public sealed class ItemCarrinho(ProdutoModel produto) : INotifyPropertyChanged
{
    private int _quantidade = 1;
    public ProdutoModel Produto { get; } = produto;
    public int Quantidade
    {
        get => _quantidade;
        set { _quantidade = Math.Clamp(value, 1, Math.Max(1, Produto.Estoque)); OnPropertyChanged(); OnPropertyChanged(nameof(Subtotal)); }
    }
    public decimal Subtotal => Produto.Preco * Quantidade;
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? nome = null) => PropertyChanged?.Invoke(this, new(nome));
}
