using System.Net;
using System.Text;
using System.Text.Json;
using VanderBeer.Api.Services;
using Xunit;

namespace VanderBeer.Api.Tests;

public class CepServiceTests
{
    [Fact]
    public async Task Consulta_mapeia_endereco_retornado()
    {
        var service = Criar("""{"logradouro":"Praça da Sé","bairro":"Sé","localidade":"São Paulo","uf":"SP"}""");
        var endereco = await service.ConsultarAsync("01001000", default);
        Assert.Equal("Praça da Sé", endereco!.Rua);
        Assert.Equal("Sé", endereco.Bairro);
        Assert.Equal("São Paulo", endereco.Cidade);
        Assert.Equal("SP", endereco.Uf);
    }

    [Theory]
    [InlineData("{\"erro\":true}")]
    [InlineData("{\"erro\":\"true\"}")]
    public async Task Cep_inexistente_retorna_nulo(string json) =>
        Assert.Null(await Criar(json).ConsultarAsync("99999999", default));

    [Theory]
    [InlineData("123")]
    [InlineData("1234567A")]
    public async Task Formato_invalido_nao_consulta_provedor(string cep) =>
        await Assert.ThrowsAsync<ArgumentException>(() => Criar("{}").ConsultarAsync(cep, default));

    [Fact]
    public async Task Cep_generico_permite_rua_e_bairro_vazios()
    {
        var endereco = await Criar("""{"logradouro":"","bairro":"","localidade":"Cidade","uf":"PR"}""")
            .ConsultarAsync("01001000", default);
        Assert.Equal("", endereco!.Rua);
        Assert.Equal("Cidade", endereco.Cidade);
    }

    [Fact]
    public async Task Falha_do_servico_e_propagada() =>
        await Assert.ThrowsAsync<HttpRequestException>(() => Criar("{}", HttpStatusCode.ServiceUnavailable).ConsultarAsync("01001000", default));

    [Fact]
    public async Task Resposta_incompleta_nao_vira_endereco_valido() =>
        await Assert.ThrowsAsync<JsonException>(() => Criar("{}").ConsultarAsync("01001000", default));

    private static CepService Criar(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        new(new HttpClient(new RespostaHandler(json, status)) { BaseAddress = new Uri("https://viacep.com.br/") });

    private sealed class RespostaHandler(string json, HttpStatusCode status) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
    }
}
