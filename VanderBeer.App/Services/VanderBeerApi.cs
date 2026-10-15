using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VanderBeer.App.Models;

namespace VanderBeer.App.Services;

public sealed class VanderBeerApi(HttpClient http)
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public async Task<EnderecoCepModel> ConsultarCepAsync(string cep, CancellationToken ct = default)
    {
        using var response = await http.GetAsync($"api/enderecos/cep/{Uri.EscapeDataString(cep)}", ct);
        await GarantirSucessoAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<EnderecoCepModel>(Json, ct)
            ?? throw new InvalidOperationException("Endereço não retornado. Preencha manualmente.");
    }

    public async Task<IReadOnlyCollection<ProdutoModel>> ListarProdutosAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<ProdutoModel>>("api/produtos", Json, ct) ?? [];

    public async Task<IReadOnlyCollection<PedidoModel>> ListarPedidosAsync(int clienteId, CancellationToken ct = default)
    {
        using var response = await http.GetAsync($"api/clientes/{clienteId}/pedidos", ct);
        await GarantirSucessoAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<List<PedidoModel>>(Json, ct) ?? [];
    }

    public async Task<ClienteModel> ObterOuCriarClienteAsync(ClienteEntrada entrada, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/clientes", entrada, Json, ct);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var cpf = new string(entrada.Cpf.Where(char.IsDigit).ToArray());
            var existente = await http.GetFromJsonAsync<ClienteModel>($"api/clientes?cpf={cpf}", Json, ct)
                ?? throw new InvalidOperationException("Cliente não localizado.");
            using var atualizacao = await http.PutAsJsonAsync($"api/clientes/{existente.Id}", entrada, Json, ct);
            await GarantirSucessoAsync(atualizacao, ct);
            return existente;
        }
        await GarantirSucessoAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<ClienteModel>(Json, ct))!;
    }

    public async Task<PedidoModel> CriarPedidoAsync(PedidoEntrada entrada, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/pedidos", entrada, Json, ct);
        await GarantirSucessoAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<PedidoModel>(Json, ct))!;
    }

    private static async Task GarantirSucessoAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        var problema = await response.Content.ReadFromJsonAsync<ProblemResponse>(Json, ct);
        var erros = problema?.Errors?.Values.SelectMany(x => x).ToArray();
        throw new InvalidOperationException(erros is { Length: > 0 } ? string.Join(" ", erros) : problema?.Detail ?? problema?.Title ?? "A API não concluiu a operação.");
    }

    private sealed record ProblemResponse(string? Title, string? Detail, Dictionary<string, string[]>? Errors);
}
