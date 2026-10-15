using VanderBeer.Api.Services;
using Xunit;

namespace VanderBeer.Api.Tests;

public class DocumentoClienteTests
{
    [Theory]
    [InlineData("52998224725", true)]
    [InlineData("529.982.247-25", true)]
    [InlineData("11222333000181", true)]
    [InlineData("11.222.333/0001-81", true)]
    [InlineData("11111111111", false)]
    [InlineData("00000000000000", false)]
    [InlineData("11222333000180", false)]
    [InlineData("52998224724", false)]
    [InlineData("abc52998224725", false)]
    public void Validar_documento(string documento, bool esperado) =>
        Assert.Equal(esperado, DocumentoCliente.Valido(documento));
}
