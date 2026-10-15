# AGENTS.md — VanderBeer

Estas instruções valem para todo o repositório.

## Fonte de verdade

- Leia `README.md` para preparar e executar o projeto.
- Leia `docs/ESPECIFICACAO.md` antes de alterar domínio, API, banco, telas ou testes.
- Preserve a arquitetura MAUI -> API REST -> MySQL. O MAUI não acessa o banco.
- Quando código e especificação divergirem, não esconda a diferença: ajuste ambos
  na mesma mudança ou registre claramente a decisão pendente.

## Convenções

- Use C# com nullable habilitado, nomes de domínio em português e nomes técnicos
  idiomáticos do .NET.
- Prefira um tipo por arquivo para código novo.
- Controllers devem ser finos; regras de negócio pertencem a serviços testáveis.
- Use DTOs de entrada e saída. Não devolva grafos de entidades EF diretamente.
- Toda operação de I/O deve ser assíncrona e aceitar `CancellationToken` quando
  fizer sentido.
- Valores monetários usam `decimal`; datas persistidas usam UTC.
- A API é responsável por preço, total, estoque e transições de status.
- Retorne erros no padrão `ProblemDetails`/`ValidationProblemDetails`.
- Nunca grave senhas, tokens ou connection strings reais no Git.

## Banco e migrations

- Banco oficial: MySQL 8+ com `utf8mb4`.
- Não edite manualmente migrations já aplicadas. Altere o modelo e gere uma nova.
- Índices, unicidade, tamanho e nulabilidade devem reforçar as regras do domínio.
- Mudanças em pedido e estoque devem usar transação.
- Exclusões com histórico devem ser lógicas quando a remoção física quebraria a
  rastreabilidade.

## Aplicativo MAUI

- Use MVVM, Shell, injeção de dependência e um serviço HTTP central.
- Não coloque regra de negócio crítica em code-behind.
- Trate carregamento, coleção vazia, falha de rede e nova tentativa.
- Não fixe uma única URL local no código; use configuração por ambiente/plataforma.
- Considere `10.0.2.2` para Android Emulator e `localhost` para Windows.

## Verificação obrigatória

Antes de considerar uma alteração concluída:

```powershell
dotnet restore .\VanderBeer.slnx
dotnet build .\VanderBeer.slnx --no-restore
dotnet test .\VanderBeer.slnx --no-build
```

Se ainda não houver projeto de testes, registre isso no resultado e crie-o quando
a tarefa envolver implementação de regras. Para alterações no banco, valide
também a criação/atualização das migrations contra um MySQL de desenvolvimento.

## Escopo das mudanças

- Preserve mudanças existentes que não pertençam à tarefa.
- Não faça refatorações amplas sem necessidade.
- Atualize a especificação ao introduzir ou alterar requisitos e regras.
- Uma funcionalidade está pronta somente com validação, tratamento de erro e
  testes proporcionais ao risco.
