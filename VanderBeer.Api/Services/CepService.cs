using System.Text.Json;
using VanderBeer.Api.DTOs;

namespace VanderBeer.Api.Services;

public sealed class CepService(HttpClient http)
{
    public async Task<EnderecoCepDto?> ConsultarAsync(string cep, CancellationToken ct)
    {
        if (cep.Length != 8 || !cep.All(char.IsAsciiDigit))
            throw new ArgumentException("O CEP deve conter oito dígitos.");
        using var response = await http.GetAsync($"ws/{cep}/json/", ct);
        response.EnsureSuccessStatusCode();
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var root = json.RootElement;
        if (root.TryGetProperty("erro", out var erro) &&
            (erro.ValueKind == JsonValueKind.True || erro.ToString().Equals("true", StringComparison.OrdinalIgnoreCase))) return null;
        string Valor(string chave) => root.TryGetProperty(chave, out var valor) ? valor.GetString() ?? "" : "";
        if (string.IsNullOrWhiteSpace(Valor("localidade")) || string.IsNullOrWhiteSpace(Valor("uf")))
            throw new JsonException("Resposta de endereço incompleta.");
        return new(cep, Valor("logradouro"), Valor("bairro"), Valor("localidade"), Valor("uf"));
    }
}
