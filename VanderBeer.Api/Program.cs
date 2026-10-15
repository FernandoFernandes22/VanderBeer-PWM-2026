using Microsoft.EntityFrameworkCore;
using VanderBeer.Api.Data;
using VanderBeer.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
if (builder.Environment.IsDevelopment()) builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddHttpClient<CepService>(http =>
{
    http.BaseAddress = new Uri("https://viacep.com.br/");
    http.Timeout = TimeSpan.FromSeconds(8);
});

var connection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection.");

builder.Services.AddDbContext<VanderBeerDbContext>(options => options.UseMySQL(connection));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Ok(new
{
    sistema = "VanderBeer API",
    status = "online",
    documentacao = "/openapi/v1.json"
}));

app.Run();

public partial class Program;
