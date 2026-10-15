using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using VanderBeer.Api.DTOs;
using VanderBeer.Api.Services;

namespace VanderBeer.Api.Controllers;

[ApiController]
[Route("api/enderecos")]
public sealed class EnderecosController(CepService service) : ControllerBase
{
    [HttpGet("cep/{cep}")]
    public async Task<ActionResult<EnderecoCepDto>> Consultar(string cep, CancellationToken ct)
    {
        var normalizado = cep.Replace("-", "").Trim();
        if (normalizado.Length != 8 || !normalizado.All(char.IsAsciiDigit))
            return Problem(statusCode: 400, title: "CEP inválido", detail: "Informe um CEP com oito dígitos.");
        try
        {
            var endereco = await service.ConsultarAsync(normalizado, ct);
            return endereco is null
                ? Problem(statusCode: 404, title: "CEP não encontrado", detail: "Confira o CEP ou preencha o endereço manualmente.")
                : Ok(endereco);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException || ex is OperationCanceledException && !ct.IsCancellationRequested)
        {
            return Problem(statusCode: 503, title: "Consulta de CEP indisponível", detail: "Tente novamente ou preencha o endereço manualmente.");
        }
    }
}
