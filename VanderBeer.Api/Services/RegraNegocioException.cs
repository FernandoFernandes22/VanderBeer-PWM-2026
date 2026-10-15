namespace VanderBeer.Api.Services;

public sealed class RegraNegocioException(string mensagem) : Exception(mensagem);
