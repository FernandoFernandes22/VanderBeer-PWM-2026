# Especificação do sistema VanderBeer

## 1. Objetivo

Documento de requisitos e planejamento. Nem todo requisito abaixo está
implementado. Para instalação e demonstração da entrega, siga o README.
A plataforma validada é Windows; Android/iOS/macOS não foram validados.

### Estado real da entrega

| Área | Situação |
|---|---|
| Cadastro CPF/CNPJ e verificador | Implementado |
| Consulta CEP com alternativa manual | Implementado |
| Catálogo, carrinho e pedido | Implementado |
| Histórico com itens, valores e data/hora | Implementado |
| Categorias/produtos e status | Endpoints implementados; sem painel administrativo |
| Pesquisa e filtro visual no catálogo | Planejados; filtro por categoria já existe na API |
| Autenticação e autorização | Pendentes; verificador é identificador, não login |
| Concorrência de estoque | Proteção adicional e testes MySQL pendentes |
| Pagamento e entrega | Pagamento simulado; sem cobrança ou transportadora |
| Testes | xUnit, EF InMemory e HTTP simulado; sem automação visual |

O VanderBeer permitirá que um cliente consulte bebidas por categoria, monte um
carrinho, informe seus dados e endereço, escolha a forma de pagamento e realize
um pedido. Os dados serão enviados pelo aplicativo .NET MAUI a uma API REST em
ASP.NET Core e persistidos em MySQL.

Esta primeira versão é acadêmica. O pagamento será apenas registrado; não haverá
integração real com cartão, Pix ou instituição financeira.

## 2. Atores

- **Cliente:** cadastra-se, consulta produtos, monta o carrinho e acompanha seus
  pedidos.
- **Administrador:** mantém categorias e produtos e consulta/atualiza pedidos.
  A interface administrativa pode ser entregue depois do fluxo do cliente.

## 3. Requisitos funcionais

| Código | Requisito | Prioridade |
|---|---|---|
| RF01 | Cadastrar e atualizar cliente | Essencial |
| RF02 | Consultar categorias ativas | Essencial |
| RF03 | Listar e pesquisar produtos ativos | Essencial |
| RF04 | Filtrar produtos por categoria | Essencial |
| RF05 | Adicionar, alterar e remover itens do carrinho | Essencial |
| RF06 | Calcular subtotal e total do carrinho | Essencial |
| RF07 | Criar pedido com endereço e forma de pagamento | Essencial |
| RF08 | Consultar pedido por código e histórico do cliente | Essencial |
| RF09 | Manter categorias | Essencial |
| RF10 | Manter produtos, preço, estoque e disponibilidade | Essencial |
| RF11 | Atualizar o status do pedido | Desejável |
| RF12 | Autenticar cliente e administrador | Desejável para a primeira entrega; obrigatório antes de produção |

## 4. Regras de negócio

| Código | Regra |
|---|---|
| RN01 | CPF (11 dígitos) ou CNPJ numérico (14 dígitos) é obrigatório, deve ter dígitos verificadores válidos e não pode se repetir. |
| RN02 | Categoria deve ter nome obrigatório e único, ignorando diferenças de maiúsculas e espaços externos. |
| RN03 | Produto pertence a exatamente uma categoria existente. |
| RN04 | Preço do produto deve ser maior que zero e estoque não pode ser negativo. |
| RN05 | Apenas categoria e produto ativos aparecem no catálogo. |
| RN06 | Produto associado a pedido não é excluído fisicamente; é inativado. |
| RN07 | Um pedido exige cliente existente, endereço de entrega, forma de pagamento e pelo menos um item. |
| RN08 | Quantidade de cada item deve ser maior que zero e não pode superar o estoque disponível. |
| RN09 | O preço unitário do item é copiado do produto no momento da compra e não muda se o catálogo for alterado depois. |
| RN10 | Subtotal é `quantidade × preço unitário`; total é a soma dos subtotais. O servidor sempre recalcula esses valores. |
| RN11 | A criação do pedido e a baixa de estoque ocorrem na mesma transação. Se uma operação falhar, nenhuma alteração é confirmada. |
| RN12 | O código público do pedido é único e gerado pelo servidor. |
| RN13 | Formas de pagamento da primeira versão: Pix, cartão e dinheiro. Para dinheiro, pode ser informado troco. |
| RN14 | Estados do pedido: Confirmado, EmPreparacao, SaiuParaEntrega, Entregue e Cancelado. |
| RN15 | Transições normais seguem a ordem dos estados; Entregue não pode ser alterado. Cancelamento devolve o estoque uma única vez. |
| RN16 | Valores monetários usam `decimal`, duas casas decimais e nunca `double`. |
| RN17 | Datas são armazenadas em UTC e convertidas para o horário local apenas na exibição. |
| RN18 | Endpoints administrativos devem ser protegidos quando a autenticação for implementada. |

## 5. Dados e atributos

### Categoria

| Atributo | Tipo | Obrigatório | Observação |
|---|---|---|---|
| Id | int | Sim | Chave primária |
| Nome | varchar(100) | Sim | Único |
| Descricao | varchar(300) | Não | Texto livre |
| Ativa | boolean | Sim | Padrão `true` |

### Produto

| Atributo | Tipo | Obrigatório | Observação |
|---|---|---|---|
| Id | int | Sim | Chave primária |
| Nome | varchar(150) | Sim | Nome comercial |
| Descricao | varchar(500) | Não | Detalhes do produto |
| Preco | decimal(10,2) | Sim | Maior que zero |
| Estoque | int | Sim | Zero ou maior |
| ImagemUrl | varchar(500) | Não | URL da imagem |
| Ativo | boolean | Sim | Padrão `true` |
| DataCadastro | datetime | Sim | UTC |
| CategoriaId | int | Sim | Chave estrangeira |

### Cliente

| Atributo | Tipo | Obrigatório | Observação |
|---|---|---|---|
| Id | int | Sim | Chave primária |
| Cpf | varchar(14) | Sim | Campo legado armazena CPF ou CNPJ numérico, normalizado e único |
| Verificador | UUID | Sim | Gerado pela API; índice único; preservado em novos acessos |
| Nome | varchar(150) | Sim | Nome completo |
| Email | varchar(150) | Sim | Formato válido |
| Telefone | varchar(20) | Sim | Preferencialmente normalizado |
| Cep | char(8) | Sim no checkout | Somente dígitos |
| Endereco | varchar(200) | Sim no checkout | Logradouro |
| Numero | varchar(20) | Sim no checkout | Permite `S/N` |
| Bairro | varchar(100) | Sim no checkout |  |
| Cidade | varchar(100) | Sim no checkout |  |
| Uf | char(2) | Sim no checkout | Sigla brasileira |
| DataCadastro | datetime | Sim | UTC |

### Pedido

| Atributo | Tipo | Obrigatório | Observação |
|---|---|---|---|
| Id | int | Sim | Chave primária interna |
| CodigoPedido | UUID | Sim | Código público único |
| ClienteId | int | Sim | Chave estrangeira |
| DataPedido | datetime | Sim | UTC |
| FormaPagamento | enum/string | Sim | Pix, Cartao ou Dinheiro |
| Status | enum/string | Sim | Estado controlado |
| EnderecoEntrega | varchar(300) | Sim | Cópia imutável do endereço usado |
| Total | decimal(10,2) | Sim | Calculado pela API |
| TrocoPara | decimal(10,2) | Não | Apenas para dinheiro; sugestão para evolução do modelo atual |

### ItemPedido

| Atributo | Tipo | Obrigatório | Observação |
|---|---|---|---|
| Id | int | Sim | Chave primária |
| PedidoId | int | Sim | Chave estrangeira |
| ProdutoId | int | Sim | Chave estrangeira |
| Quantidade | int | Sim | Maior que zero |
| PrecoUnitario | decimal(10,2) | Sim | Fotografia do preço na compra |
| Subtotal | decimal(10,2) | Sim | Calculado pela API |

Relacionamentos: Categoria 1:N Produto, Cliente 1:N Pedido, Pedido 1:N
ItemPedido e Produto 1:N ItemPedido.

## 6. Fluxos principais

### Fluxo de compra

1. Cliente abre o aplicativo no cadastro obrigatório, com nome, CPF/CNPJ, telefone, e-mail e endereço completo.
2. App envia o cadastro à API; ela valida, salva no MySQL e retorna um verificador UUID único. Somente após a resposta válida o app libera e carrega o catálogo.
3. Cliente filtra o catálogo e adiciona produtos ao carrinho.
4. App valida quantidades localmente e exibe o total estimado.
5. Cliente escolhe o pagamento simulado; o pedido usa o cliente confirmado e o endereço cadastrado.
6. App envia somente cliente, endereço, forma de pagamento, produto e quantidade.
7. API revalida produto, disponibilidade e estoque; busca preços atuais.
8. API calcula valores, grava pedido e itens e baixa o estoque em transação.
9. API retorna o código do pedido e o status Confirmado.
10. App exibe a confirmação e permite consultar o histórico.

### Fluxos alternativos

- produto inativo ou sem estoque: API rejeita o item e o app atualiza o carrinho;
- quantidade insuficiente: API informa o produto e a quantidade disponível;
- CPF já cadastrado: API retorna conflito, sem duplicar o cliente;
- falha ao persistir: transação é revertida e nenhum estoque é baixado;
- cancelamento válido: status muda para Cancelado e as quantidades retornam ao estoque.

## 7. API planejada

Prefixo: `/api` e conteúdo JSON.

| Método e rota | Finalidade |
|---|---|
| GET `/categorias` | Listar categorias |
| GET `/categorias/{id}` | Obter categoria |
| POST `/categorias` | Criar categoria |
| PUT `/categorias/{id}` | Atualizar categoria |
| DELETE `/categorias/{id}` | Excluir/inativar categoria conforme vínculos |
| GET `/produtos?categoriaId=&somenteAtivos=` | Listar/filtrar produtos |
| GET `/produtos/{id}` | Obter produto |
| POST `/produtos` | Criar produto |
| PUT `/produtos/{id}` | Atualizar produto |
| DELETE `/produtos/{id}` | Inativar produto |
| GET `/clientes/{id}` | Obter cliente |
| POST `/clientes` | Cadastrar cliente |
| PUT `/clientes/{id}` | Atualizar cliente |
| POST `/pedidos` | Validar e criar pedido |
| GET `/pedidos/{codigo}` | Consultar pedido |
| GET `/clientes/{id}/pedidos` | Histórico do cliente |
| PATCH `/pedidos/{codigo}/status` | Alterar status |

Respostas devem usar códigos HTTP coerentes: 200/201/204 para sucesso, 400
para dados inválidos, 404 para recurso inexistente, 409 para conflito de regra
e 500 somente para falhas inesperadas. Erros devem seguir um único formato com
título, status, detalhe e erros por campo.

## 8. Tecnologias e ferramentas

| Camada | Tecnologia |
|---|---|
| Aplicativo | .NET 10 MAUI, XAML, Shell e padrão MVVM |
| Comunicação | `HttpClient` e JSON; HTTP local em desenvolvimento, HTTPS previsto para publicação |
| Servidor | ASP.NET Core Web API com Controllers |
| Acesso a dados | Entity Framework Core e migrations |
| Banco | MySQL 8+, charset `utf8mb4` |
| Contrato | OpenAPI |
| Testes | xUnit, EF InMemory e HttpMessageHandler simulado |
| Integração planejada | `WebApplicationFactory` e banco de teste MySQL/Testcontainers |
| Versionamento | Git e GitHub |

O pacote atual é `MySql.EntityFrameworkCore`. Antes de trocar de provedor, deve
ser verificada a compatibilidade com a versão do .NET/EF e recriadas/validadas
as migrations.

## 9. Arquitetura de implementação

Na API, separar progressivamente:

- Controllers: protocolo HTTP, códigos de resposta e DTOs;
- Services: regras de cliente, estoque, pedido e status;
- Data: `DbContext`, configurações e migrations;
- Models: entidades persistidas;
- DTOs: contratos de entrada e saída, sem expor entidades diretamente.

No MAUI, usar:

- Views (XAML) e ViewModels;
- Services para acesso à API;
- Models/DTOs próprios do cliente;
- `Shell` para navegação;
- injeção de dependência para páginas, ViewModels e serviços;
- carrinho em memória na primeira versão, com persistência local opcional.

Telas mínimas: catálogo, detalhe do produto, carrinho, identificação/cadastro,
checkout, confirmação e histórico. Administração pode ser feita inicialmente
pela API/OpenAPI e depois receber telas próprias.

A interface atual usa uma identidade verde-escura, creme e dourada. O cadastro
é dividido em dados pessoais e entrega, com rótulos permanentes. O catálogo
apresenta cartões com categoria, descrição, preço e estoque, seguido do carrinho
e resumo de pagamento. O conteúdo tem largura máxima no desktop e rolagem
vertical em telas menores; a mensagem da operação permanece próxima ao topo.

## 10. Requisitos não funcionais

- RNF01: nenhuma credencial deve ser versionada.
- RNF02: consultas de catálogo devem ser assíncronas e não bloquear a interface.
- RNF03: a API deve validar toda entrada; validação no app é apenas conveniência.
- RNF04: operações de pedido devem ser atômicas e resistentes a concorrência.
- RNF05: mensagens ao usuário devem ser claras e em português.
- RNF06: a interface deve indicar carregamento, vazio e erro.
- RNF07: dados pessoais não devem aparecer em logs.
- RNF08: o projeto deve compilar sem avisos relevantes e ter testes repetíveis.

## 11. Estratégia e conjunto de testes

### Testes unitários da API

- validação e normalização de CPF;
- rejeição de preço, estoque e quantidade inválidos;
- cálculo de subtotal e total;
- rejeição de produto inativo ou estoque insuficiente;
- transições permitidas e proibidas de status;
- devolução única de estoque no cancelamento.

### Testes de integração da API

- CRUD e unicidade de categoria;
- CRUD/inativação e filtro de produto;
- cadastro duplicado de CPF;
- criação completa de pedido;
- rollback quando algum item for inválido;
- concorrência de dois pedidos pelo último item;
- consulta do pedido e histórico;
- contrato e códigos HTTP dos erros.

### Testes do aplicativo MAUI

- ViewModel carrega e filtra catálogo;
- carrinho soma, altera e remove itens;
- comandos ficam indisponíveis durante requisições;
- erros da API viram mensagens compreensíveis;
- checkout não prossegue com campos obrigatórios ausentes;
- navegação chega à confirmação após sucesso.

### Testes manuais de aceite

1. cadastrar cliente válido;
2. cadastrar categoria e produto;
3. localizar produto no celular;
4. adicionar duas unidades e concluir pedido;
5. confirmar total e redução do estoque no MySQL;
6. consultar o pedido pelo código;
7. tentar comprar acima do estoque e conferir a rejeição;
8. cancelar pedido e confirmar reposição do estoque.

## 12. Critérios de conclusão da primeira versão

- API e MAUI compilam;
- schema pode ser criado do zero por migration;
- fluxo de compra funciona de ponta a ponta;
- regras críticas são aplicadas no servidor;
- suíte automatizada passa;
- configuração não contém senha versionada;
- documentação explica instalação, execução e testes;
- demonstração comprova persistência no MySQL.

## 13. Fluxo de cadastro confirmado (14/10/2026)

O aplicativo inicia no formulário de cadastro. Nome, CPF ou CNPJ numérico,
telefone, e-mail, CEP, rua, número, bairro, cidade e UF são obrigatórios.
A API gera e persiste um UUID exclusivo chamado `Verificador`, retornado com
o ID e o nome do cliente. O catálogo só é liberado pelo app após essa resposta.
O verificador identifica o cadastro; não é código de SMS/e-mail nem substitui
autenticação. Nesta versão acadêmica, um documento existente reaproveita o
cliente e seu verificador, atualizando os dados informados; não há login real.
Ao reiniciar o app, o formulário é exibido novamente. O pagamento é simulado,
sem cobrança real. O pedido confirmado segue para preparação e entrega; não
existe integração automática com transportadora.

## 14. Decisões pendentes

As regras de disponibilidade por categoria, validação completa de endereço,
padronização de todos os erros, autorização de acesso aos dados e controle de
concorrência ainda precisam ser consolidadas. Atualmente, informar um documento
existente permite reutilizar o cadastro e consultar seus pedidos, sem senha.
Esta limitação deve ser considerada na demonstração: use somente dados fictícios.
O endereço tem campos opcionais no modelo legado; o formulário exige todos.
Um pedido confirmado reserva/baixa estoque, mas não representa pagamento real.

O cadastro consulta automaticamente o CEP ao completar oito dígitos. O MAUI
chama GET `/api/enderecos/cep/{cep}`; a API consulta o ViaCEP por HTTPS, com
timeout. Rua, bairro, cidade e UF são preenchidos quando disponíveis. Número
permanece manual, e todos os campos de endereço continuam editáveis. CEP
inexistente, indisponibilidade e CEP genérico permitem preenchimento manual.
O aplicativo cancela consultas antigas ao mudar o CEP e oferece nova tentativa.
Não há mudança no schema do MySQL para esta funcionalidade.

O MAUI possui abas Loja e Meus pedidos após a confirmação do cadastro. Ao
concluir uma compra, seleciona Meus pedidos e exibe a resposta confirmada da
API. A consulta do histórico retorna também itens, quantidades, preços da
compra, subtotais, total, data/hora local, status, pagamento e endereço de entrega.
O botão Atualizar recarrega o histórico persistido, do mais recente ao mais antigo.


Antes da implementação de autenticação, definir com o professor:

- se haverá login real ou apenas identificação pelo CPF;
- se o administrador terá telas no MAUI ou usará apenas a API;
- se taxa de entrega, limite geográfico e pedido mínimo fazem parte da avaliação;
- se pagamento precisa ser simulado visualmente ou somente registrado;
- quais plataformas serão apresentadas: Windows, Android ou ambas.
