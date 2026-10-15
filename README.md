# VanderBeer — PWM 2026

Projeto acadêmico de pedidos de bebidas em **C# / .NET 10**, com aplicativo
**.NET MAUI**, **ASP.NET Core Web API** e **MySQL 8**.

```text
Aplicativo MAUI → API REST (HTTP/JSON local) → Entity Framework Core → MySQL
```

O aplicativo não acessa o banco diretamente. O MySQL roda na máquina de quem
executa o projeto. Clonar o repositório não copia os clientes e pedidos da máquina
do autor: as migrations criam as tabelas e seis produtos de demonstração.

## O que está implementado

- Cadastro obrigatório com nome, CPF ou CNPJ numérico, telefone, e-mail e endereço.
- Validação dos dígitos de CPF/CNPJ e documento único no MySQL.
- Verificador UUID gerado pela API, persistido antes da liberação do catálogo.
- Consulta de CEP pela API usando ViaCEP, com preenchimento manual alternativo.
- Catálogo, carrinho, quantidades e pagamento simulado.
- Pedido com preços calculados no servidor e baixa de estoque.
- Aba Meus pedidos com itens, quantidades, valores, data/hora local e status.
- Histórico persistido no MySQL e consulta/alteração de status pela API.
- Testes automatizados de documentos, cadastro, pedidos, histórico e CEP.

Esta é uma **versão acadêmica demonstrável**, não uma loja pronta para produção.
O pagamento não realiza cobrança. O verificador não substitui autenticação.
Ainda não há painel administrativo, login, proteção contra disputa simultânea
de estoque nem integração com transportadora. O Android é opcional e ainda não
foi validado nesta entrega; a plataforma de apresentação é **Windows**.

## 1. Pré-requisitos do professor

- Windows 10 atualizado (build 19041 ou superior) ou Windows 11.
- SDK **.NET 10**, incluindo SDK e runtime ASP.NET Core.
- Carga de trabalho **.NET MAUI para Windows** e Windows SDK.
  Pode ser instalada pelo Visual Studio compatível com .NET 10, na carga
  de desenvolvimento .NET MAUI.
- MySQL Server 8 em execução; MySQL Workbench é opcional.
- Git e internet para restaurar os pacotes. A busca de CEP também usa internet.

Verifique no PowerShell:

```powershell
dotnet --info
dotnet workload list
Get-Service MySQL80
```

Se a carga MAUI Windows estiver ausente, instale pelo Visual Studio Installer
ou execute em um terminal de administrador:

```powershell
dotnet workload install maui-windows
```

O nome do serviço MySQL pode variar conforme a instalação.

## 2. Clonar e preparar

Execute cada comando separadamente:

```powershell
git clone https://github.com/FernandoFernandes22/VanderBeer-PWM-2026.git
cd VanderBeer-PWM-2026
dotnet tool restore
dotnet restore .\VanderBeer.slnx
```

No Windows, a solução usa somente o alvo Windows por padrão para não exigir
as ferramentas de iOS/macOS/Android.

## 3. Preparar o MySQL

Abra o MySQL Workbench e conecte com um usuário administrador do seu próprio
servidor (por exemplo, root, com a senha definida na sua instalação).
Execute este SQL para criar o banco e o usuário de demonstração:

```sql
CREATE DATABASE IF NOT EXISTS vanderbeer
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER IF NOT EXISTS 'admin'@'localhost' IDENTIFIED BY 'admin';
GRANT ALL PRIVILEGES ON vanderbeer.* TO 'admin'@'localhost';
```

O usuário recebe permissões somente para o banco vanderbeer. Se ele já existir
com outra senha, escolha outro usuário para o projeto ou ajuste sua conexão.
O par admin/admin é uma convenção somente de desenvolvimento local, não a
senha obrigatória do root.

Configure a conexão em **User Secrets**, fora do repositório:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=vanderbeer;User=admin;Password=admin;SslMode=Disabled" --project .\VanderBeer.Api
```

Se escolher outras credenciais, use os valores reais nesse comando. Não copie
marcadores como SEU_USUARIO ou SUA_SENHA literalmente.
O SSL desabilitado é exclusivo da conexão com o banco local de demonstração.

Crie as tabelas e carregue os produtos iniciais:

```powershell
dotnet tool run dotnet-ef database update --project .\VanderBeer.Api --startup-project .\VanderBeer.Api
```

As migrations são incrementais: não é necessário apagar o banco nem reaplicar
cadastros a cada execução. Rode database update novamente quando houver novas
migrations.

## 4. Executar — dois terminais

No **terminal 1**, na pasta clonada, inicie a API e mantenha-o aberto:

```powershell
dotnet run --project .\VanderBeer.Api --launch-profile http
```

Deve aparecer: Now listening on: http://localhost:5162.

- [Estado da API](http://localhost:5162/)
- [Produtos no MySQL](http://localhost:5162/api/produtos)
- [Contrato OpenAPI em desenvolvimento](http://localhost:5162/openapi/v1.json)

O JSON no navegador é a resposta da API. A interface visual abre no aplicativo.

No **terminal 2**, também na pasta clonada:

```powershell
dotnet run --project .\VanderBeer.App --framework net10.0-windows10.0.19041.0
```

Se precisar de outra URL, defina VANDERBEER_API_URL no terminal que inicia o
aplicativo, incluindo a barra final:

```powershell
$env:VANDERBEER_API_URL = "http://localhost:5162/"
```

Para encerrar, feche o aplicativo e use Ctrl+C no terminal da API.

## 5. Roteiro de demonstração

1. Abra o aplicativo no formulário de cadastro.
2. Use dados fictícios. CPF de teste: 52998224725.
   CNPJ numérico de teste: 11222333000181.
3. Informe um e-mail com formato válido e o CEP 01001000.
   Confira o endereço retornado e informe o número. Sem internet, preencha
   rua, bairro, cidade e UF manualmente.
4. Confirme o cadastro e observe o verificador retornado pela API.
5. Adicione bebidas ao carrinho, ajuste quantidades e escolha o pagamento.
6. Confirme o pedido: a aba Meus pedidos abre com os detalhes da compra.
7. Use Atualizar para consultar o histórico salvo no MySQL.
8. No Workbench, confira os dados:

```sql
USE vanderbeer;
SELECT Id, Nome, Estoque FROM Produtos;
SELECT Id, Nome, Verificador FROM Clientes;
SELECT CodigoPedido, DataPedido, Status, Total FROM Pedidos;
SELECT PedidoId, ProdutoId, Quantidade, PrecoUnitario, Subtotal FROM ItensPedido;
```

DataPedido é armazenada em UTC; o app mostra o horário local.
Ao reabrir o aplicativo, informe novamente o cadastro: documentos já registrados
reutilizam o mesmo cliente/verificador e atualizam seus dados. Não há login real.

## 6. Compilação e testes

Feche as instâncias antigas do app e da API antes de compilar.

```powershell
dotnet restore .\VanderBeer.slnx
dotnet build .\VanderBeer.slnx --no-restore
dotnet test .\VanderBeer.slnx --no-build
```

Os testes automatizados usam respostas HTTP simuladas para CEP e EF InMemory
para regras de banco. Não dependem de MySQL nem de ViaCEP online; não substituem
testes de concorrência/transação com MySQL ou testes de interface no dispositivo.

Para executar apenas os testes da API:

```powershell
dotnet test .\VanderBeer.Api.Tests\VanderBeer.Api.Tests.csproj
```

## 7. Problemas comuns

| Sintoma | Como resolver |
|---|---|
| Access denied no MySQL | Confira usuário e senha no User Secrets e as permissões no banco |
| Configure ConnectionStrings:DefaultConnection | Configure User Secrets e use o profile http (Development) |
| Connection refused | Verifique o serviço MySQL ou se a API está aberta no terminal 1 |
| Porta 5162 ocupada | Encerre a instância anterior da API com Ctrl+C |
| Arquivo exe em uso na compilação | Feche o aplicativo e a API antes de recompilar |
| Workload/Windows SDK ausente | Instale MAUI Windows e Windows SDK pelo Visual Studio Installer |
| Consulta de CEP indisponível | Preencha o endereço manualmente; não impede o cadastro |
| PowerShell mostra >> | Use Ctrl+C e cole o comando completo em uma linha |

## Android (opcional)

Exige Android SDK, workload MAUI Android e emulador. No Windows, habilite o alvo
explicitamente ao restaurar/compilar:

```powershell
dotnet build .\VanderBeer.App\VanderBeer.App.csproj -p:BuildAndroid=true -f net10.0-android
```

O emulador usa http://10.0.2.2:5162/. Celular físico exige IP acessível, URL
configurada e regras de rede apropriadas. Esta entrega foi validada no Windows.

## Documentação

- [Requisitos, atributos, regras e fluxos](docs/ESPECIFICACAO.md)
- [Instruções para colaboradores](AGENTS.md)
