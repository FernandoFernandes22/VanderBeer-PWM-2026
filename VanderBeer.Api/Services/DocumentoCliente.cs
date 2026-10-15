namespace VanderBeer.Api.Services;

public static class DocumentoCliente
{
    public static string Normalizar(string valor) => new(valor.Where(char.IsAsciiDigit).ToArray());

    public static bool Valido(string valor)
    {
        if (valor.Any(c => !char.IsAsciiDigit(c) && c is not '.' and not '-' and not '/' and not ' ')) return false;
        var documento = Normalizar(valor);
        if (documento.Length is not (11 or 14) || documento.Distinct().Count() == 1) return false;
        var baseTamanho = documento.Length - 2;
        for (var tamanho = baseTamanho; tamanho < documento.Length; tamanho++)
        {
            var soma = 0;
            for (var i = 0; i < tamanho; i++)
            {
                var peso = documento.Length == 11 ? tamanho + 1 - i : (tamanho - 1 - i) % 8 + 2;
                soma += (documento[i] - '0') * peso;
            }
            var resto = soma % 11;
            var digito = resto < 2 ? 0 : 11 - resto;
            if (documento[tamanho] - '0' != digito) return false;
        }
        return true;
    }
}
