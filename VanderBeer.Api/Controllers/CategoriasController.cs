using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore; using VanderBeer.Api.Data; using VanderBeer.Api.DTOs; using VanderBeer.Api.Models;
namespace VanderBeer.Api.Controllers;
[ApiController,Route("api/[controller]")] public class CategoriasController(VanderBeerDbContext db):ControllerBase {
 [HttpGet] public async Task<ActionResult> Listar()=>Ok(await db.Categorias.AsNoTracking().OrderBy(x=>x.Nome).ToListAsync());
 [HttpGet("{id:int}")] public async Task<ActionResult> Obter(int id){var x=await db.Categorias.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id);return x is null?NotFound():Ok(x);}
 [HttpPost] public async Task<ActionResult> Criar(CategoriaDto dto){var nome=dto.Nome.Trim();if(await db.Categorias.AnyAsync(x=>x.Nome==nome))return Conflict(new{mensagem="Categoria já cadastrada."});var x=new Categoria{Nome=nome,Descricao=dto.Descricao?.Trim(),Ativa=dto.Ativa};db.Add(x);await db.SaveChangesAsync();return CreatedAtAction(nameof(Obter),new{id=x.Id},x);}
 [HttpPut("{id:int}")] public async Task<IActionResult> Atualizar(int id,CategoriaDto dto){var x=await db.Categorias.FindAsync(id);if(x is null)return NotFound();x.Nome=dto.Nome.Trim();x.Descricao=dto.Descricao?.Trim();x.Ativa=dto.Ativa;await db.SaveChangesAsync();return NoContent();}
 [HttpDelete("{id:int}")] public async Task<IActionResult> Excluir(int id){var x=await db.Categorias.Include(x=>x.Produtos).FirstOrDefaultAsync(x=>x.Id==id);if(x is null)return NotFound();if(x.Produtos.Count>0)return Conflict(new{mensagem="A categoria possui produtos."});db.Remove(x);await db.SaveChangesAsync();return NoContent();}
}
