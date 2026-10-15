using System.ComponentModel.DataAnnotations;
namespace VanderBeer.Api.DTOs;
public record CategoriaDto([Required,StringLength(100)] string Nome,[StringLength(300)] string? Descricao,bool Ativa=true);
public record ProdutoDto([Required,StringLength(150)] string Nome,[StringLength(500)] string? Descricao,[Range(0.01,999999.99)] decimal Preco,[Range(0,int.MaxValue)] int Estoque,[StringLength(500)] string? ImagemUrl,bool Ativo,[Range(1,int.MaxValue)] int CategoriaId);
