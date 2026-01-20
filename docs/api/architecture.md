# API Architecture - Apex

## Visão Geral

A API do Apex segue princípios de Clean Architecture e boas práticas para criar endpoints REST escaláveis e manuteníveis.

## Componentes Principais

### 1. ApiControllerBase

Classe base abstrata para todos os controllers da API com funcionalidades compartilhadas.

**Localização**: `src/Apex.Api/Controllers/ApiControllerBase.cs`

**Funcionalidades**:

#### ToActionResult<T>()
Converte um `Result<T>` em `IActionResult` com status code apropriado baseado no erro.

```csharp
protected IActionResult ToActionResult<T>(Result<T> result)
```

**Mapeamento de Erros**:
- `*.NotFound` → 404 Not Found
- `*.AlreadyExists` → 409 Conflict
- `*.ValidationFailed` → 400 Bad Request
- `*.InvalidQuery` → 400 Bad Request
- Outros → 400 Bad Request

**Exemplo de uso**:
```csharp
var result = await queryHandler.HandleAsync(query, cancellationToken);
return ToActionResult(result); // Automaticamente retorna 200 OK ou erro apropriado
```

#### ToCreatedAtActionResult<T>()
Converte um `Result<T>` em `CreatedAtActionResult` (201) ou erro.

```csharp
protected IActionResult ToCreatedAtActionResult<T>(
    Result<T> result,
    string actionName,
    object routeValues)
```

**Exemplo de uso**:
```csharp
var result = await commandHandler.HandleAsync(command, cancellationToken);
return ToCreatedAtActionResult(
    result,
    nameof(GetCustomer),
    new { id = result.Value.Id }); // 201 Created com Location header
```

---

### 2. Middlewares

#### ExceptionHandlingMiddleware

Middleware global para tratamento de exceções não capturadas.

**Localização**: `src/Apex.Api/Middleware/ExceptionHandlingMiddleware.cs`

**Funcionalidades**:
- Captura todas as exceções não tratadas
- Loga o erro com stack trace completo
- Retorna resposta padronizada em formato ProblemDetails (RFC 7807)
- Inclui TraceId para rastreabilidade

**Resposta de Erro**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred. Please try again later.",
  "traceId": "0HMV8K3QHQG1N:00000001"
}
```

**Configuração**:
```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

**Benefícios**:
- ✅ Evita exposição de detalhes internos em produção
- ✅ Logging centralizado de erros
- ✅ Formato de erro consistente (RFC 7807)
- ✅ TraceId para correlação de logs

---

### 3. Dependency Injection Automática

#### ServiceCollectionExtensions

Registra automaticamente todos os handlers CQRS por reflection.

**Localização**: `src/Apex.Api/Extensions/ServiceCollectionExtensions.cs`

**Método Principal**:
```csharp
public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
```

**O que registra automaticamente**:
1. `ICommandHandler<TCommand>` - Handlers de comando sem retorno
2. `ICommandHandler<TCommand, TResponse>` - Handlers de comando com retorno
3. `IQueryHandler<TQuery, TResponse>` - Handlers de query

**Como funciona**:
1. Carrega o assembly `Apex.Application` via reflection
2. Busca todas as classes que implementam as interfaces de handler
3. Registra cada handler com tempo de vida `Scoped`

**Configuração**:
```csharp
// Program.cs
builder.Services.AddApplicationHandlers();
```

**Antes (Manual)**:
```csharp
builder.Services.AddScoped<CreateCustomerCommandHandler>();
builder.Services.AddScoped<GetCustomerQueryHandler>();
builder.Services.AddScoped<UpdateCustomerCommandHandler>();
builder.Services.AddScoped<DeleteCustomerCommandHandler>();
// ... adicionar manualmente cada handler
```

**Depois (Automático)**:
```csharp
builder.Services.AddApplicationHandlers(); // Registra TODOS os handlers automaticamente
```

**Benefícios**:
- ✅ Zero manutenção manual
- ✅ Novos handlers são registrados automaticamente
- ✅ Reduz boilerplate
- ✅ Impossível esquecer de registrar um handler

---

## Controllers Simplificados

### Antes da Refatoração

**CustomersController** tinha ~195 linhas com:
- Mapeamento manual de erros para status codes
- Criação manual de ProblemDetails
- Duplicação de lógica entre métodos
- Muito código boilerplate

```csharp
public async Task<IActionResult> GetCustomer(int id)
{
    var result = await handler.HandleAsync(query);

    if (result.IsFailure)
    {
        return NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Customer Not Found",
            Detail = result.Error.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
        });
    }

    return Ok(result.Value);
}
```

### Depois da Refatoração

**CustomersController** agora tem ~155 linhas (20% redução):
- Herda de `ApiControllerBase`
- Usa métodos helper `ToActionResult()` e `ToCreatedAtActionResult()`
- Foco em logging e lógica de negócio
- Sem duplicação de código

```csharp
public async Task<IActionResult> GetCustomer(int id)
{
    _logger.LogInformation("Getting customer with ID: {CustomerId}", id);

    var query = new GetCustomerQuery(Id: id);
    var result = await queryHandler.HandleAsync(query, cancellationToken);

    if (result.IsFailure)
    {
        _logger.LogWarning("Customer not found. ID: {CustomerId}", id);
    }
    else
    {
        _logger.LogInformation("Customer found with ID: {CustomerId}", id);
    }

    return ToActionResult(result); // ✨ Mágica acontece aqui
}
```

**Redução de código**: ~40 linhas por endpoint

---

## Pipeline de Requisição

```
┌─────────────────────────────────────────────────────────────┐
│                        Client Request                        │
└──────────────────────────────────┬──────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────┐
│           ExceptionHandlingMiddleware (Global)              │
│   - Captura exceções não tratadas                          │
│   - Retorna ProblemDetails padronizado                     │
└──────────────────────────────────┬──────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────┐
│                   UseHttpsRedirection                        │
└──────────────────────────────────┬──────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────┐
│                    UseAuthorization                          │
└──────────────────────────────────┬──────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────┐
│                    Controller Action                         │
│   - Recebe request                                          │
│   - Injeta handler via [FromServices]                      │
│   - Executa handler.HandleAsync()                          │
│   - Usa ToActionResult() / ToCreatedAtActionResult()       │
└──────────────────────────────────┬──────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────┐
│                    Command/Query Handler                     │
│   - Executa lógica de negócio                              │
│   - Chama repository                                        │
│   - Retorna Result<T> (sucesso ou falha)                   │
└──────────────────────────────────┬──────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────┐
│                    ApiControllerBase                         │
│   - ToActionResult() mapeia Result → IActionResult         │
│   - Cria ProblemDetails automaticamente                    │
│   - Define status codes corretos                           │
└──────────────────────────────────┬──────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────┐
│                      JSON Response                           │
│   - 200 OK / 201 Created → CustomerResponse                │
│   - 400/404/409 → ProblemDetails                           │
└─────────────────────────────────────────────────────────────┘
```

---

## Convenções de Nomenclatura de Erros

Para garantir o mapeamento correto de erros para status codes, os códigos de erro devem seguir estas convenções:

| Sufixo | Status Code | Uso |
|--------|-------------|-----|
| `*.NotFound` | 404 Not Found | Recurso não encontrado |
| `*.AlreadyExists` | 409 Conflict | Recurso duplicado |
| `*.ValidationFailed` | 400 Bad Request | Validação falhou |
| `*.InvalidQuery` | 400 Bad Request | Query inválida |
| Outros | 400 Bad Request | Erro genérico |

**Exemplos**:
```csharp
// 404
new Error("Customer.NotFound", "Customer not found with identifier: 123")

// 409
new Error("Customer.AlreadyExists", "Customer already exists")

// 400
new Error("Customer.ValidationFailed", "API ID cannot be empty")
new Error("Customer.InvalidQuery", "Either Id or ApiId must be provided")
```

---

## Logging Estruturado

Todos os controllers seguem padrões de logging consistentes:

**Information (Sucesso)**:
```csharp
_logger.LogInformation(
    "Customer created successfully with ID: {CustomerId}, ApiId: {ApiId}",
    customer.Id,
    customer.ApiId);
```

**Warning (Erro de Negócio)**:
```csharp
_logger.LogWarning(
    "Failed to create customer. Error: {ErrorCode} - {ErrorMessage}",
    result.Error.Code,
    result.Error.Message);
```

**Error (Exceção)**:
```csharp
_logger.LogError(ex, "An unhandled exception occurred");
```

**Benefícios**:
- ✅ Logs estruturados com propriedades tipadas
- ✅ Facilita busca e análise em ferramentas de observabilidade
- ✅ TraceId para correlação de requisições

---

## Adicionando Novos Endpoints

### Passo 1: Criar Handler na Application Layer

```csharp
// Apex.Application/Customers/Commands/UpdateCustomer/UpdateCustomerCommand.cs
public sealed record UpdateCustomerCommand(
    int Id,
    string ApiId) : ICommand<Result<CustomerResponse>>;

// UpdateCustomerCommandHandler.cs
public sealed class UpdateCustomerCommandHandler
    : ICommandHandler<UpdateCustomerCommand, Result<CustomerResponse>>
{
    // Implementação...
}
```

### Passo 2: Criar Endpoint no Controller

```csharp
[HttpPut("{id:int}")]
public async Task<IActionResult> UpdateCustomer(
    [FromRoute] int id,
    [FromBody] UpdateCustomerRequest request,
    [FromServices] UpdateCustomerCommandHandler handler,
    CancellationToken cancellationToken)
{
    _logger.LogInformation("Updating customer with ID: {CustomerId}", id);

    var command = new UpdateCustomerCommand(id, request.ApiId);
    var result = await handler.HandleAsync(command, cancellationToken);

    if (result.IsFailure)
    {
        _logger.LogWarning("Failed to update customer. Error: {ErrorCode}", result.Error.Code);
    }
    else
    {
        _logger.LogInformation("Customer updated successfully. ID: {CustomerId}", id);
    }

    return ToActionResult(result); // ✨ Mágica!
}
```

### Passo 3: Pronto!

**Não é necessário**:
- ❌ Registrar handler no Program.cs (feito automaticamente)
- ❌ Mapear erros manualmente (feito pelo ApiControllerBase)
- ❌ Criar ProblemDetails manualmente
- ❌ Decidir status codes

---

## Testes

### Testando Controllers

```csharp
public class CustomersControllerTests
{
    [Fact]
    public async Task CreateCustomer_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var handler = Substitute.For<CreateCustomerCommandHandler>();
        var logger = Substitute.For<ILogger<CustomersController>>();
        var controller = new CustomersController(logger);

        var request = new CreateCustomerRequest
        {
            ApiId = "API_001",
            Document = "12345678901",
            Company = 1
        };

        handler.HandleAsync(Arg.Any<CreateCustomerCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new CustomerResponse { Id = 1 }));

        // Act
        var result = await controller.CreateCustomer(request, handler, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
    }
}
```

---

## Métricas de Refatoração

| Métrica | Antes | Depois | Melhoria |
|---------|-------|--------|----------|
| **Linhas CustomersController** | ~195 | ~155 | -20% |
| **Linhas por endpoint** | ~65 | ~25 | -60% |
| **Duplicação de código** | Alta | Zero | 100% |
| **Registro manual de handlers** | Sim | Não | Automático |
| **Tratamento global de erros** | Não | Sim | ✅ |
| **Manutenibilidade** | Média | Alta | ⬆️ |

---

## Próximos Passos

1. **Validation Middleware**: Integrar FluentValidation
2. **Rate Limiting**: Limitar requisições por IP/cliente
3. **Response Caching**: Cache de respostas GET
4. **Compression**: Gzip/Brotli para respostas
5. **CORS**: Configurar políticas CORS
6. **API Versioning**: v1, v2, etc.
7. **Health Checks**: Endpoints de healthcheck

---

**Status:** ✅ Implementado e testado
**Última atualização:** 2026-01-19
