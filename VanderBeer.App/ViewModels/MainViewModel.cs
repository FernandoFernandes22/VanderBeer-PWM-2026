using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using VanderBeer.App.Models;
using VanderBeer.App.Services;

namespace VanderBeer.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly VanderBeerApi _api;
    private CancellationTokenSource? _consultaCep;
    private string _cep = "";
    private string _endereco = "", _bairro = "", _cidade = "", _uf = "";
    private string _mensagemCep = "Digite o CEP para buscar o endereço automaticamente.";
    private bool _buscandoCep;
    public string MensagemCep { get => _mensagemCep; private set => Definir(ref _mensagemCep, value); }
    public bool BuscandoCep { get => _buscandoCep; private set => Definir(ref _buscandoCep, value); }
    public ICommand BuscarCepCommand { get; }
    private bool _carregando;
    private string _mensagem = "Cadastre seus dados para acessar o catálogo.";
    private ClienteModel? _cliente;
    private bool _abaPedidos;
    public bool MostrarPedidos => CatalogoLiberado && _abaPedidos;
    public bool MostrarLoja => CatalogoLiberado && !_abaPedidos;
    public ObservableCollection<PedidoModel> Pedidos { get; } = [];
    public ICommand AbrirPedidosCommand { get; }
    public ICommand AbrirLojaCommand { get; }
    public ICommand AtualizarPedidosCommand { get; }
    public bool CadastroPendente => _cliente is null;
    public bool CatalogoLiberado => _cliente is not null;
    public string Identificacao => _cliente is null ? "" : $"Cliente: {_cliente.Nome} | Verificador: {_cliente.Verificador}";
    public ICommand CadastrarCommand { get; }

    public MainViewModel(VanderBeerApi api)
    {
        _api = api;
        BuscarCepCommand = new Command(async () => await ConsultarCepAsync());
        AbrirPedidosCommand = new Command(async () => { SelecionarAba(true); await CarregarPedidosAsync(); }, () => !Carregando && CatalogoLiberado);
        AbrirLojaCommand = new Command(async () => { SelecionarAba(false); await CarregarAsync(); }, () => !Carregando && CatalogoLiberado);
        AtualizarPedidosCommand = new Command(async () => await CarregarPedidosAsync(), () => !Carregando && CatalogoLiberado);
        CadastrarCommand = new Command(async () => await CadastrarAsync(), () => !Carregando && CadastroPendente);
        CarregarCommand = new Command(async () => await CarregarAsync(), () => !Carregando && CatalogoLiberado);
        AdicionarCommand = new Command<ProdutoModel>(Adicionar);
        AumentarCommand = new Command<ItemCarrinho>(item => AlterarQuantidade(item, 1));
        DiminuirCommand = new Command<ItemCarrinho>(item => AlterarQuantidade(item, -1));
        RemoverCommand = new Command<ItemCarrinho>(item => Carrinho.Remove(item));
        FinalizarCommand = new Command(async () => await FinalizarAsync(), () => !Carregando && CatalogoLiberado && Carrinho.Count > 0);
        Carrinho.CollectionChanged += (_, _) => AtualizarCarrinho();
    }

    public ObservableCollection<ProdutoModel> Produtos { get; } = [];
    public ObservableCollection<ItemCarrinho> Carrinho { get; } = [];
    public ICommand CarregarCommand { get; }
    public ICommand AdicionarCommand { get; }
    public ICommand AumentarCommand { get; }
    public ICommand DiminuirCommand { get; }
    public ICommand RemoverCommand { get; }
    public ICommand FinalizarCommand { get; }
    public string Cpf { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Cep
    {
        get => _cep;
        set
        {
            if (_cep == value) return;
            Definir(ref _cep, value);
            _ = ConsultarCepAsync();
        }
    }
    public string Endereco { get => _endereco; set => Definir(ref _endereco, value); }
    public string Numero { get; set; } = "";
    public string Bairro { get => _bairro; set => Definir(ref _bairro, value); }
    public string Cidade { get => _cidade; set => Definir(ref _cidade, value); }
    public string Uf { get => _uf; set => Definir(ref _uf, value); }
    public string FormaPagamento { get; set; } = "Pix";
    public IReadOnlyList<string> FormasPagamento { get; } = ["Pix", "Cartao", "Dinheiro"];
    public decimal Total => Carrinho.Sum(x => x.Subtotal);
    public string TotalFormatado => $"Total: {Total:C}";
    public bool Carregando { get => _carregando; private set => Definir(ref _carregando, value); }
    public string Mensagem { get => _mensagem; private set => Definir(ref _mensagem, value); }

    private async Task ConsultarCepAsync()
    {
        _consultaCep?.Cancel();
        var consulta = new CancellationTokenSource();
        _consultaCep = consulta;
        var cep = Cep.Replace("-", "").Trim();
        BuscandoCep = false;
        if (cep.Length != 8 || !cep.All(char.IsAsciiDigit))
        {
            MensagemCep = "Informe os oito dígitos do CEP ou preencha o endereço manualmente.";
            _consultaCep = null;
            consulta.Dispose();
            return;
        }
        var ruaAnterior = Endereco;
        var bairroAnterior = Bairro;
        var cidadeAnterior = Cidade;
        var ufAnterior = Uf;
        try
        {
            await Task.Delay(350, consulta.Token);
            BuscandoCep = true;
            MensagemCep = "Buscando seu endereço…";
            var endereco = await _api.ConsultarCepAsync(cep, consulta.Token);
            if (consulta.IsCancellationRequested || !ReferenceEquals(_consultaCep, consulta) || !CadastroPendente) return;
            if (Endereco == ruaAnterior) Endereco = endereco.Rua;
            if (Bairro == bairroAnterior) Bairro = endereco.Bairro;
            if (Cidade == cidadeAnterior) Cidade = endereco.Cidade;
            if (Uf == ufAnterior) Uf = endereco.Uf;
            MensagemCep = string.IsNullOrWhiteSpace(endereco.Rua) || string.IsNullOrWhiteSpace(endereco.Bairro)
                ? "CEP encontrado. Complete a rua, o bairro e o número que faltarem."
                : "Endereço encontrado. Confira os dados e informe o número.";
        }
        catch (OperationCanceledException) when (consulta.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (ReferenceEquals(_consultaCep, consulta))
                MensagemCep = $"Não foi possível consultar: {ex.Message} Você pode preencher manualmente.";
        }
        finally
        {
            if (ReferenceEquals(_consultaCep, consulta))
            {
                BuscandoCep = false;
                _consultaCep = null;
            }
            consulta.Dispose();
        }
    }

    private async Task CarregarAsync()
    {
        if (!CatalogoLiberado || Carregando) return;
        try
        {
            Carregando = true; Produtos.Clear();
            foreach (var produto in await _api.ListarProdutosAsync()) Produtos.Add(produto);
            Mensagem = Produtos.Count == 0 ? "Nenhum produto disponível." : $"{Produtos.Count} produto(s) disponível(is).";
        }
        catch (Exception ex) { Mensagem = $"Não foi possível acessar a API: {ex.Message}"; }
        finally { Carregando = false; AtualizarComandos(); }
    }

    private void Adicionar(ProdutoModel produto)
    {
        if (!CatalogoLiberado || Carregando) return;
        if (produto.Estoque <= 0) { Mensagem = $"{produto.Nome} está sem estoque."; return; }
        var item = Carrinho.FirstOrDefault(x => x.Produto.Id == produto.Id);
        if (item is null) { item = new(produto); item.PropertyChanged += (_, _) => AtualizarCarrinho(); Carrinho.Add(item); }
        else item.Quantidade++;
        Mensagem = $"{produto.Nome} adicionado."; AtualizarCarrinho();
    }

    private void AlterarQuantidade(ItemCarrinho item, int diferenca)
    {
        if (Carregando) return;
        if (diferenca < 0 && item.Quantidade == 1) Carrinho.Remove(item); else item.Quantidade += diferenca;
        AtualizarCarrinho();
    }

    private async Task FinalizarAsync()
    {
        if (_cliente is null || Carregando || Carrinho.Count == 0) return;
        try
        {
            Carregando = true;
            var enderecoCompleto = $"{Endereco}, {Numero} - {Bairro}, {Cidade}/{Uf} - CEP {Cep}";
            var pedido = await _api.CriarPedidoAsync(new(_cliente.Id, enderecoCompleto, FormaPagamento, null,
                Carrinho.Select(x => new ItemPedidoEntrada(x.Produto.Id, x.Quantidade)).ToList()));
            Carrinho.Clear();
            Pedidos.Insert(0, pedido);
            SelecionarAba(true);
            Mensagem = $"Pedido {pedido.CodigoPedido} confirmado. Total {pedido.Total:C}.";

        }
        catch (Exception ex) { Mensagem = $"Não foi possível finalizar: {ex.Message}"; }
        finally { Carregando = false; AtualizarComandos(); }
    }

    private async Task CadastrarAsync()
    {
        if (Carregando || CatalogoLiberado) return;
        if (new[] { Cpf, Nome, Email, Telefone, Cep, Endereco, Numero, Bairro, Cidade, Uf }.Any(string.IsNullOrWhiteSpace))
        { Mensagem = "Preencha todos os campos do cadastro e da entrega."; return; }
        try
        {
            Carregando = true;
            var cliente = await _api.ObterOuCriarClienteAsync(new(Cpf, Nome, Email, Telefone, Cep, Endereco, Numero, Bairro, Cidade, Uf));
            if (cliente.Id <= 0 || cliente.Verificador == Guid.Empty)
                throw new InvalidOperationException("A API não retornou a confirmação do cadastro.");
            _cliente = cliente;
            OnPropertyChanged(nameof(CadastroPendente));
            OnPropertyChanged(nameof(CatalogoLiberado));
            OnPropertyChanged(nameof(MostrarLoja));
            OnPropertyChanged(nameof(MostrarPedidos));
            OnPropertyChanged(nameof(Identificacao));
            Mensagem = "Cadastro confirmado. Escolha seus produtos.";
        }
        catch (Exception ex) { Mensagem = $"Não foi possível confirmar o cadastro: {ex.Message}"; }
        finally { Carregando = false; AtualizarComandos(); }
        if (CatalogoLiberado) await CarregarAsync();
    }

    private void SelecionarAba(bool pedidos)
    {
        _abaPedidos = pedidos;
        OnPropertyChanged(nameof(MostrarPedidos));
        OnPropertyChanged(nameof(MostrarLoja));
    }

    private async Task CarregarPedidosAsync()
    {
        if (_cliente is null || Carregando) return;
        try
        {
            Carregando = true;
            var pedidos = await _api.ListarPedidosAsync(_cliente.Id);
            Pedidos.Clear();
            foreach (var pedido in pedidos) Pedidos.Add(pedido);
            Mensagem = Pedidos.Count == 0 ? "Você ainda não fez pedidos." : "Seus pedidos, do mais recente ao mais antigo.";
        }
        catch (Exception ex) { Mensagem = $"Não foi possível carregar os pedidos. Tente atualizar novamente: {ex.Message}"; }
        finally { Carregando = false; AtualizarComandos(); }
    }

    private void AtualizarCarrinho() { OnPropertyChanged(nameof(Total)); OnPropertyChanged(nameof(TotalFormatado)); AtualizarComandos(); }
    private void AtualizarComandos() { ((Command)AbrirPedidosCommand).ChangeCanExecute(); ((Command)AbrirLojaCommand).ChangeCanExecute(); ((Command)AtualizarPedidosCommand).ChangeCanExecute(); ((Command)CadastrarCommand).ChangeCanExecute(); ((Command)CarregarCommand).ChangeCanExecute(); ((Command)FinalizarCommand).ChangeCanExecute(); }
    private void Definir<T>(ref T campo, T valor, [CallerMemberName] string? nome = null) { if (EqualityComparer<T>.Default.Equals(campo, valor)) return; campo = valor; OnPropertyChanged(nome); }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? nome = null) => PropertyChanged?.Invoke(this, new(nome));
}
