using System.ComponentModel.DataAnnotations;

namespace VanderBeer.Api.DTOs;

public record ClienteDto(
    [Required, StringLength(18, MinimumLength = 11)] string Cpf,
    [Required, StringLength(150)] string Nome,
    [Required, EmailAddress, StringLength(150)] string? Email,
    [Required, StringLength(20)] string Telefone,
    [Required, StringLength(9, MinimumLength = 8)] string? Cep,
    [Required, StringLength(200)] string? Endereco,
    [Required, StringLength(20)] string? Numero,
    [Required, StringLength(100)] string? Bairro,
    [StringLength(100)] string? Cidade,
    [Required, StringLength(2, MinimumLength = 2)] string? Uf);
