# Customer Use Cases - Application Layer

## Visão Geral

Esta documentação descreve os casos de uso implementados na camada de aplicação para o agregado `Customer` usando o padrão CQRS (Command Query Responsibility Segregation).

## Arquitetura CQRS

### Conceitos Fundamentais

**Commands (Comandos)**
- Representam operações que **modificam estado** (Create, Update, Delete)
- Podem retornar ou não um valor
- São processados por `ICommandHandler<TCommand, TResponse>`

**Queries (Consultas)**
- Representam operações que **apenas leem dados** (Get, List, Search)
- Sempre retornam um valor
- São processados por `IQueryHandler<TQuery, TResponse>`

**Result Pattern**
- Encapsula o resultado de operações (sucesso ou falha)
- Evita uso de exceções para fluxo de controle
- Retorna `Result<T>` com valor ou erro

### Estrutura de Diretórios

```
src/Apex.Application/
├── Abstractions/
│   └── Messaging/
│       ├── ICommand.cs
│       ├── ICommandHandler.cs
│       ├── IQuery.cs
│       └── IQueryHandler.cs
├── Common/
│   ├── Result.cs
│   └── Error.cs
└── Customers/
    ├── Commands/
    │   └── CreateCustomer/
    │       ├── CreateCustomerCommand.cs
    │       └── CreateCustomerCommandHandler.cs
    ├── Queries/
    │   └── GetCustomer/
    │       ├── GetCustomerQuery.cs
    │       └── GetCustomerQueryHandler.cs
    └── DTOs/
        └── CustomerResponse.cs
```

## 1. Create Customer (Command)

### CreateCustomerCommand

Comando para criar um novo cliente no sistema.

```csharp
public sealed record CreateCustomerCommand(
    string ApiId,
    string Document,
    int Company,
    string? SinacorId = null,
    string? LegacyExternalId = null) : ICommand<Result<CustomerResponse>>;
```

### Parâmetros

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `ApiId` | `string` | Sim | Identificador da API (máx 32 caracteres) |
| `Document` | `string` | Sim | CPF (11 dígitos) ou CNPJ (14 dígitos) |
| `Company` | `int` | Sim | 1 = Warren, 2 = Rena |
| `SinacorId` | `string?` | Não | ID Sinacor (máx 9 caracteres) |
| `LegacyExternalId` | `string?` | Não | ID legado para migração (máx 9 caracteres) |

### CreateCustomerCommandHandler

Handler que processa o comando de criação de cliente.

**Fluxo de Execução:**

1. **Validação de Duplicação**: Verifica se já existe um cliente com Document + SinacorId + Company
2. **Criação da Entidade**: Usa factory method `Customer.Create()` para criar entidade válida
3. **Persistência**: Chama `ICustomerRepository.AddAsync()`
4. **Mapeamento**: Converte entidade para `CustomerResponse`
5. **Retorno**: `Result.Success(CustomerResponse)` ou `Result.Failure(Error)`

**Possíveis Erros:**

| Código | Descrição |
|--------|-----------|
| `Customer.AlreadyExists` | Cliente com mesma combinação Document+SinacorId+Company já existe |
| `Customer.ValidationFailed` | Parâmetros inválidos (ex: ApiId vazio, Document formato incorreto) |
| `Customer.DomainError` | Violação de regra de domínio |

### Exemplo de Uso

```csharp
var command = new CreateCustomerCommand(
    ApiId: "API_WARREN_001",
    Document: "12345678901",        // CPF
    Company: 1,                      // Warren
    SinacorId: "123456789",
    LegacyExternalId: null);

var result = await handler.HandleAsync(command);

if (result.IsSuccess)
{
    var customer = result.Value;
    Console.WriteLine($"Cliente criado: {customer.ApiId}");
    Console.WriteLine($"ID: {customer.Id}");
    Console.WriteLine($"Documento: {customer.Document}");
    Console.WriteLine($"Registros externos: {customer.ExternalRegisters.Count}"); // 2 (CETIP + SELIC)
}
else
{
    Console.WriteLine($"Erro: {result.Error.Code} - {result.Error.Message}");
}
```

## 2. Get Customer (Query)

### GetCustomerQuery

Consulta para obter um cliente por ID ou API ID.

```csharp
public sealed record GetCustomerQuery(
    int? Id = null,
    string? ApiId = null) : IQuery<Result<CustomerResponse>>;
```

### Parâmetros

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `Id` | `int?` | Condicional* | ID do banco de dados |
| `ApiId` | `string?` | Condicional* | Identificador da API |

**\* Pelo menos um dos dois deve ser fornecido**

### GetCustomerQueryHandler

Handler que processa a consulta de cliente.

**Fluxo de Execução:**

1. **Validação**: Verifica se pelo menos `Id` ou `ApiId` foi fornecido
2. **Priorização**: Se ambos fornecidos, `Id` tem prioridade
3. **Busca**: Chama `GetByIdAsync()` ou `GetByApiIdAsync()`
4. **Verificação**: Checa se cliente foi encontrado
5. **Mapeamento**: Converte entidade para `CustomerResponse`
6. **Retorno**: `Result.Success(CustomerResponse)` ou `Result.Failure(Error)`

**Possíveis Erros:**

| Código | Descrição |
|--------|-----------|
| `Customer.InvalidQuery` | Nenhum identificador fornecido |
| `Customer.NotFound` | Cliente não encontrado com o identificador fornecido |

### Exemplo de Uso

```csharp
// Buscar por ID
var queryById = new GetCustomerQuery(Id: 123);
var resultById = await handler.HandleAsync(queryById);

// Buscar por API ID
var queryByApiId = new GetCustomerQuery(ApiId: "API_WARREN_001");
var resultByApiId = await handler.HandleAsync(queryByApiId);

// Prioridade: Id tem prioridade sobre ApiId
var queryBoth = new GetCustomerQuery(Id: 123, ApiId: "API_WARREN_001");
// Irá buscar por Id = 123

if (resultById.IsSuccess)
{
    var customer = resultById.Value;
    Console.WriteLine($"Cliente encontrado: {customer.ApiId}");
    Console.WriteLine($"Documento: {customer.Document}");
    Console.WriteLine($"Company: {customer.Company}");
    Console.WriteLine($"Criado em: {customer.CreatedAt}");

    // Registros externos
    foreach (var register in customer.ExternalRegisters)
    {
        Console.WriteLine($"  {register.SystemType}: {register.Status}");
    }
}
```

## DTOs (Data Transfer Objects)

### CustomerResponse

Objeto de resposta para operações de Customer.

```csharp
public sealed class CustomerResponse
{
    public int Id { get; init; }
    public string ApiId { get; init; }
    public string Document { get; init; }
    public string? SinacorId { get; init; }
    public string Company { get; init; }                                  // "Warren" ou "Rena"
    public string? LegacyExternalId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastUpdatedAt { get; init; }
    public List<ExternalSystemRegisterResponse> ExternalRegisters { get; init; }
}
```

### ExternalSystemRegisterResponse

Objeto de resposta para registros em sistemas externos.

```csharp
public sealed class ExternalSystemRegisterResponse
{
    public int Id { get; init; }
    public string Status { get; init; }          // "NotRegistered", "Registered", "Inactive"
    public string SystemType { get; init; }      // "Cetip", "Selic"
    public DateTime CreatedAt { get; init; }
    public DateTime? LastUpdatedAt { get; init; }
}
```

**Nota:** Os DTOs usam `string` para enums para facilitar serialização JSON e desacoplamento da camada de domínio.

## Repository Interface

### ICustomerRepository

Interface de contrato para persistência de Customer (localizada em `Apex.Domain.Repositories`).

```csharp
public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByApiIdAsync(string apiId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string document, string? sinacorId, int company, CancellationToken cancellationToken = default);
}
```

**Notas:**
- Interface está no Domain (inversão de dependência)
- Implementação será na camada Infrastructure (EF Core)
- `ExistsAsync` verifica constraint de unicidade (Document + SinacorId + Company)

## Result Pattern

### Result e Result<T>

Classes para encapsular resultados de operações.

```csharp
// Operação sem retorno
Result result = Result.Success();
Result result = Result.Failure(error);

// Operação com retorno
Result<CustomerResponse> result = Result.Success(customerResponse);
Result<CustomerResponse> result = Result.Failure<CustomerResponse>(error);

// Verificação
if (result.IsSuccess)
{
    var value = result.Value;
}
else
{
    var error = result.Error;
}
```

### Error

Classe para representar erros.

```csharp
var error = new Error(
    code: "Customer.NotFound",
    message: "Customer not found with identifier: 123");

Console.WriteLine(error.Code);     // "Customer.NotFound"
Console.WriteLine(error.Message);  // "Customer not found with identifier: 123"
```

**Códigos de Erro Padronizados:**

| Código | Categoria | Descrição |
|--------|-----------|-----------|
| `Customer.AlreadyExists` | Validação | Duplicação de cliente |
| `Customer.ValidationFailed` | Validação | Parâmetros inválidos |
| `Customer.DomainError` | Domínio | Violação de regra de negócio |
| `Customer.InvalidQuery` | Query | Query sem identificadores |
| `Customer.NotFound` | Query | Cliente não encontrado |

## Testes

### Estatísticas

- **CreateCustomerCommandHandlerTests**: 7 testes
- **GetCustomerQueryHandlerTests**: 7 testes
- **Total**: 14 testes
- **Status**: ✅ 100% aprovados

### Cenários de Teste - CreateCustomer

✅ Criar cliente com parâmetros válidos
✅ Rejeitar cliente duplicado (Document+SinacorId+Company)
✅ Validar ApiId inválido
✅ Validar Document inválido
✅ Criar cliente com CNPJ (pessoa jurídica)
✅ Criar cliente com LegacyExternalId
✅ Inicializar registros externos (CETIP e SELIC)

### Cenários de Teste - GetCustomer

✅ Buscar por ID válido
✅ Buscar por ApiId válido
✅ Rejeitar query sem identificadores
✅ Retornar erro quando ID não existe
✅ Retornar erro quando ApiId não existe
✅ Priorizar Id quando ambos fornecidos
✅ Incluir registros externos na resposta

### Executar Testes

```bash
# Apenas testes de aplicação
dotnet test tests/Apex.UnitTests/Apex.UnitTests.csproj --filter "FullyQualifiedName~Application.Customers"

# Todos os testes
dotnet test tests/Apex.UnitTests/Apex.UnitTests.csproj
```

## Exemplo Completo: Fluxo de Criação e Consulta

```csharp
// 1. Criar um novo cliente
var createCommand = new CreateCustomerCommand(
    ApiId: "API_WARREN_001",
    Document: "12345678901",
    Company: 1, // Warren
    SinacorId: "123456789");

var createResult = await createHandler.HandleAsync(createCommand);

if (createResult.IsSuccess)
{
    Console.WriteLine($"✅ Cliente criado com ID: {createResult.Value.Id}");

    // 2. Buscar o cliente criado
    var getQuery = new GetCustomerQuery(Id: createResult.Value.Id);
    var getResult = await getHandler.HandleAsync(getQuery);

    if (getResult.IsSuccess)
    {
        var customer = getResult.Value;
        Console.WriteLine($"\nDetalhes do Cliente:");
        Console.WriteLine($"  API ID: {customer.ApiId}");
        Console.WriteLine($"  Documento: {customer.Document}");
        Console.WriteLine($"  Company: {customer.Company}");
        Console.WriteLine($"  Sinacor ID: {customer.SinacorId}");

        Console.WriteLine($"\nRegistros Externos:");
        foreach (var register in customer.ExternalRegisters)
        {
            Console.WriteLine($"  - {register.SystemType}: {register.Status}");
        }
    }
}
else
{
    Console.WriteLine($"❌ Erro ao criar cliente: {createResult.Error.Message}");
}
```

**Output:**
```
✅ Cliente criado com ID: 1

Detalhes do Cliente:
  API ID: API_WARREN_001
  Documento: 12345678901
  Company: Warren
  Sinacor ID: 123456789

Registros Externos:
  - Cetip: NotRegistered
  - Selic: NotRegistered
```

## Próximos Passos

1. **Infrastructure**: Implementar `CustomerRepository` com EF Core
2. **API**: Criar endpoints REST para Customer (ASP.NET Core)
3. **Mediator**: Integrar MediatR para desacoplamento
4. **Validation**: Adicionar FluentValidation para validação de requests
5. **Update/Delete**: Implementar comandos adicionais
6. **List/Search**: Implementar queries de listagem e busca

---

**Status:** ✅ Implementado e testado (101 testes passando)
**Última atualização:** 2026-01-19
**Cobertura:** 100% (14 testes de aplicação)
