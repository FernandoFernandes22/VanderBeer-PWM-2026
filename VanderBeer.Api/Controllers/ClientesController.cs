using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanderBeer.Api.Data;
using VanderBeer.Api.DTOs;
using VanderBeer.Api.Models;
using VanderBeer.Api.Services;

namespace VanderBeer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ClientesController(VanderBeerDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> BuscarPorCpf([FromQuery] string cpf, CancellationToken cancellationToken)
    {
        var normalizado = SomenteDigitos(cpf);
        var cliente = await db.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Cpf == normalizado, cancellationToken);
        return cliente is null ? NotFound() : Ok(new ClienteSaidaDto(cliente.Id, cliente.Verificador, cliente.Nome));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> Obter(int id, CancellationToken cancellationToken)
    {
        var cliente = await db.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return cliente is null ? NotFound() : Ok(new ClienteSaidaDto(cliente.Id, cliente.Verificador, cliente.Nome));
    }

    [HttpPost]
    public async Task<ActionResult> Criar(ClienteDto dto, CancellationToken cancellationToken)
    {
        var cpf = SomenteDigitos(dto.Cpf);
        if (!DocumentoCliente.Valido(dto.Cpf))
            return BadRequest(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["cpf"] = ["CPF ou CNPJ inválido."] }));
        if (await db.Clientes.AnyAsync(x => x.Cpf == cpf, cancellationToken))
            return Conflict(new ProblemDetails { Title = "Cliente já cadastrado", Detail = "Já existe um cliente com este CPF ou CNPJ.", Status = 409 });

        var cliente = Mapear(new Cliente(), dto, cpf);
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = cliente.Id }, new ClienteSaidaDto(cliente.Id, cliente.Verificador, cliente.Nome));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, ClienteDto dto, CancellationToken cancellationToken)
    {
        var cliente = await db.Clientes.FindAsync([id], cancellationToken);
        if (cliente is null) return NotFound();

        var cpf = SomenteDigitos(dto.Cpf);
        if (!DocumentoCliente.Valido(dto.Cpf))
            return BadRequest(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["cpf"] = ["CPF ou CNPJ inválido."] }));
        if (await db.Clientes.AnyAsync(x => x.Cpf == cpf && x.Id != id, cancellationToken))
            return Conflict(new ProblemDetails { Title = "CPF em uso", Status = 409 });

        Mapear(cliente, dto, cpf);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:int}/pedidos")]
    public async Task<ActionResult> ListarPedidos(int id, CancellationToken cancellationToken)
    {
        if (!await db.Clientes.AnyAsync(x => x.Id == id, cancellationToken)) return NotFound();
        var pedidos = await db.Pedidos.AsNoTracking()
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.DataPedido)
            .ThenByDescending(x => x.Id)
            .Select(x => new PedidoSaidaDto(x.CodigoPedido, x.ClienteId, x.DataPedido,
                x.FormaPagamento, x.Status, x.EnderecoEntrega, x.Total, x.TrocoPara,
                x.Itens.Select(i => new ItemPedidoSaidaDto(i.ProdutoId,
                    i.Produto!.Nome, i.Quantidade, i.PrecoUnitario, i.Subtotal)).ToList()))
            .ToListAsync(cancellationToken);
        return Ok(pedidos);
    }

    private static Cliente Mapear(Cliente cliente, ClienteDto dto, string cpf)
    {
        cliente.Cpf = cpf;
        cliente.Nome = dto.Nome.Trim();
        cliente.Email = dto.Email?.Trim().ToLowerInvariant();
        cliente.Telefone = SomenteDigitos(dto.Telefone);
        cliente.Cep = dto.Cep is null ? null : SomenteDigitos(dto.Cep);
        cliente.Endereco = dto.Endereco?.Trim();
        cliente.Numero = dto.Numero?.Trim();
        cliente.Bairro = dto.Bairro?.Trim();
        cliente.Cidade = dto.Cidade?.Trim();
        cliente.Uf = dto.Uf?.Trim().ToUpperInvariant();
        return cliente;
    }

    private static string SomenteDigitos(string valor) => new(valor.Where(char.IsDigit).ToArray());

    internal static bool CpfValido(string cpf)
    {
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return false;
        for (var tamanho = 9; tamanho <= 10; tamanho++)
        {
            var soma = 0;
            for (var i = 0; i < tamanho; i++) soma += (cpf[i] - '0') * (tamanho + 1 - i);
            var digito = 11 - soma % 11;
            if (digito >= 10) digito = 0;
            if (digito != cpf[tamanho] - '0') return false;
        }
        return true;
    }
}
