# Customers API - Documentação dos Endpoints

## Visão Geral

Esta documentação descreve os endpoints REST disponíveis para gerenciamento de clientes (Customers) na API Apex.

**Base URL**: `https://localhost:{port}/api/customers`

## Endpoints

### 1. Create Customer

Cria um novo cliente no sistema.

**Endpoint**: `POST /api/customers`

**Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "apiId": "API_WARREN_001",
  "document": "12345678901",
  "company": 1,
  "sinacorId": "123456789",
  "legacyExternalId": "LEGACY001"
}
```

**Parâmetros**:

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `apiId` | `string` | Sim | Identificador da API (máx 32 caracteres) |
| `document` | `string` | Sim | CPF (11 dígitos) ou CNPJ (14 dígitos) |
| `company` | `int` | Sim | 1 = Warren, 2 = Rena |
| `sinacorId` | `string` | Não | ID Sinacor (máx 9 caracteres) |
| `legacyExternalId` | `string` | Não | ID legado para migração (máx 9 caracteres) |

**Response 201 (Created)**:
```json
{
  "id": 1,
  "apiId": "API_WARREN_001",
  "document": "12345678901",
  "sinacorId": "123456789",
  "company": "Warren",
  "legacyExternalId": "LEGACY001",
  "createdAt": "2026-01-19T10:30:00Z",
  "lastUpdatedAt": null,
  "externalRegisters": [
    {
      "id": 1,
      "status": "NotRegistered",
      "systemType": "Cetip",
      "createdAt": "2026-01-19T10:30:00Z",
      "lastUpdatedAt": null
    },
    {
      "id": 2,
      "status": "NotRegistered",
      "systemType": "Selic",
      "createdAt": "2026-01-19T10:30:00Z",
      "lastUpdatedAt": null
    }
  ]
}
```

**Headers de Resposta**:
```
Location: /api/customers/1
```

**Response 400 (Bad Request)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "API ID cannot be null or empty."
}
```

**Response 409 (Conflict)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Customer Already Exists",
  "status": 409,
  "detail": "Customer with document '12345678901', Sinacor ID '123456789' and company '1' already exists."
}
```

**Exemplo cURL**:
```bash
curl -X POST https://localhost:7001/api/customers \
  -H "Content-Type: application/json" \
  -d '{
    "apiId": "API_WARREN_001",
    "document": "12345678901",
    "company": 1,
    "sinacorId": "123456789"
  }'
```

---

### 2. Get Customer by ID

Obtém um cliente pelo ID do banco de dados.

**Endpoint**: `GET /api/customers/{id}`

**Path Parameters**:

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `id` | `int` | ID do cliente no banco de dados |

**Response 200 (OK)**:
```json
{
  "id": 1,
  "apiId": "API_WARREN_001",
  "document": "12345678901",
  "sinacorId": "123456789",
  "company": "Warren",
  "legacyExternalId": "LEGACY001",
  "createdAt": "2026-01-19T10:30:00Z",
  "lastUpdatedAt": null,
  "externalRegisters": [
    {
      "id": 1,
      "status": "NotRegistered",
      "systemType": "Cetip",
      "createdAt": "2026-01-19T10:30:00Z",
      "lastUpdatedAt": null
    },
    {
      "id": 2,
      "status": "NotRegistered",
      "systemType": "Selic",
      "createdAt": "2026-01-19T10:30:00Z",
      "lastUpdatedAt": null
    }
  ]
}
```

**Response 404 (Not Found)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Customer Not Found",
  "status": 404,
  "detail": "Customer not found with identifier: 999"
}
```

**Exemplo cURL**:
```bash
curl -X GET https://localhost:7001/api/customers/1
```

---

### 3. Get Customer by API ID

Obtém um cliente pelo API ID (identificador externo).

**Endpoint**: `GET /api/customers/by-api-id/{apiId}`

**Path Parameters**:

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `apiId` | `string` | Identificador da API |

**Response 200 (OK)**:
```json
{
  "id": 1,
  "apiId": "API_WARREN_001",
  "document": "12345678901",
  "sinacorId": "123456789",
  "company": "Warren",
  "legacyExternalId": null,
  "createdAt": "2026-01-19T10:30:00Z",
  "lastUpdatedAt": null,
  "externalRegisters": [
    {
      "id": 1,
      "status": "Registered",
      "systemType": "Cetip",
      "createdAt": "2026-01-19T10:30:00Z",
      "lastUpdatedAt": "2026-01-19T11:00:00Z"
    },
    {
      "id": 2,
      "status": "NotRegistered",
      "systemType": "Selic",
      "createdAt": "2026-01-19T10:30:00Z",
      "lastUpdatedAt": null
    }
  ]
}
```

**Response 404 (Not Found)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Customer Not Found",
  "status": 404,
  "detail": "Customer not found with identifier: API_NONEXISTENT"
}
```

**Exemplo cURL**:
```bash
curl -X GET https://localhost:7001/api/customers/by-api-id/API_WARREN_001
```

---

## Códigos de Status HTTP

| Código | Descrição | Quando ocorre |
|--------|-----------|---------------|
| `200 OK` | Sucesso | GET retornou cliente |
| `201 Created` | Recurso criado | POST criou cliente com sucesso |
| `400 Bad Request` | Requisição inválida | Parâmetros inválidos, validação falhou |
| `404 Not Found` | Recurso não encontrado | Cliente não existe |
| `409 Conflict` | Conflito | Cliente duplicado (Document+SinacorId+Company) |
| `500 Internal Server Error` | Erro do servidor | Erro inesperado |

---

## Estrutura de Resposta

### CustomerResponse

```typescript
interface CustomerResponse {
  id: number;                                    // ID do banco de dados
  apiId: string;                                 // Identificador da API
  document: string;                              // CPF ou CNPJ
  sinacorId: string | null;                      // ID Sinacor (opcional)
  company: string;                               // "Warren" ou "Rena"
  legacyExternalId: string | null;               // ID legado (opcional)
  createdAt: string;                             // ISO 8601 format
  lastUpdatedAt: string | null;                  // ISO 8601 format
  externalRegisters: ExternalSystemRegisterResponse[];
}
```

### ExternalSystemRegisterResponse

```typescript
interface ExternalSystemRegisterResponse {
  id: number;                                    // ID do registro
  status: string;                                // "NotRegistered" | "Registered" | "Inactive"
  systemType: string;                            // "Cetip" | "Selic"
  createdAt: string;                             // ISO 8601 format
  lastUpdatedAt: string | null;                  // ISO 8601 format
}
```

### ProblemDetails (RFC 7807)

```typescript
interface ProblemDetails {
  type: string;                                  // URL para documentação do erro
  title: string;                                 // Título legível do erro
  status: number;                                // Código HTTP
  detail: string;                                // Descrição detalhada
}
```

---

## Exemplos de Uso

### Exemplo 1: Criar Cliente Individual (CPF) Warren

**Request**:
```bash
POST /api/customers
Content-Type: application/json

{
  "apiId": "API_WARREN_001",
  "document": "12345678901",
  "company": 1,
  "sinacorId": "123456789"
}
```

**Response**:
```json
201 Created
Location: /api/customers/1

{
  "id": 1,
  "apiId": "API_WARREN_001",
  "document": "12345678901",
  "sinacorId": "123456789",
  "company": "Warren",
  "legacyExternalId": null,
  "createdAt": "2026-01-19T10:30:00Z",
  "lastUpdatedAt": null,
  "externalRegisters": [...]
}
```

---

### Exemplo 2: Criar Cliente Pessoa Jurídica (CNPJ) Rena

**Request**:
```bash
POST /api/customers
Content-Type: application/json

{
  "apiId": "API_RENA_001",
  "document": "12345678000190",
  "company": 2
}
```

**Response**:
```json
201 Created
Location: /api/customers/2

{
  "id": 2,
  "apiId": "API_RENA_001",
  "document": "12345678000190",
  "sinacorId": null,
  "company": "Rena",
  "legacyExternalId": null,
  "createdAt": "2026-01-19T10:35:00Z",
  "lastUpdatedAt": null,
  "externalRegisters": [...]
}
```

---

### Exemplo 3: Buscar Cliente por ID

**Request**:
```bash
GET /api/customers/1
```

**Response**:
```json
200 OK

{
  "id": 1,
  "apiId": "API_WARREN_001",
  "document": "12345678901",
  "sinacorId": "123456789",
  "company": "Warren",
  "legacyExternalId": null,
  "createdAt": "2026-01-19T10:30:00Z",
  "lastUpdatedAt": null,
  "externalRegisters": [...]
}
```

---

### Exemplo 4: Buscar Cliente por API ID

**Request**:
```bash
GET /api/customers/by-api-id/API_WARREN_001
```

**Response**:
```json
200 OK

{
  "id": 1,
  "apiId": "API_WARREN_001",
  "document": "12345678901",
  "sinacorId": "123456789",
  "company": "Warren",
  "legacyExternalId": null,
  "createdAt": "2026-01-19T10:30:00Z",
  "lastUpdatedAt": null,
  "externalRegisters": [...]
}
```

---

### Exemplo 5: Erro - Cliente Duplicado

**Request**:
```bash
POST /api/customers
Content-Type: application/json

{
  "apiId": "API_WARREN_002",
  "document": "12345678901",
  "company": 1,
  "sinacorId": "123456789"
}
```

**Response**:
```json
409 Conflict

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Customer Already Exists",
  "status": 409,
  "detail": "Customer with document '12345678901', Sinacor ID '123456789' and company '1' already exists."
}
```

---

### Exemplo 6: Erro - Validação Falhou

**Request**:
```bash
POST /api/customers
Content-Type: application/json

{
  "apiId": "",
  "document": "123",
  "company": 1
}
```

**Response**:
```json
400 Bad Request

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "API ID cannot be null or empty."
}
```

---

### Exemplo 7: Erro - Cliente Não Encontrado

**Request**:
```bash
GET /api/customers/999
```

**Response**:
```json
404 Not Found

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Customer Not Found",
  "status": 404,
  "detail": "Customer not found with identifier: 999"
}
```

---

## Regras de Negócio

### Unicidade de Cliente

Um cliente é único pela combinação de:
- `Document` (CPF ou CNPJ)
- `SinacorId` (pode ser null)
- `Company` (Warren ou Rena)

**Exemplos válidos:**

✅ Mesmo CPF em empresas diferentes:
```json
// Cliente 1
{"document": "12345678901", "company": 1, "sinacorId": "123"}

// Cliente 2 (PERMITIDO)
{"document": "12345678901", "company": 2, "sinacorId": "123"}
```

✅ Mesmo CPF com Sinacor IDs diferentes:
```json
// Cliente 1
{"document": "12345678901", "company": 1, "sinacorId": "123"}

// Cliente 2 (PERMITIDO)
{"document": "12345678901", "company": 1, "sinacorId": "456"}
```

❌ Combinação duplicada:
```json
// Cliente 1
{"document": "12345678901", "company": 1, "sinacorId": "123"}

// Cliente 2 (REJEITADO - 409 Conflict)
{"document": "12345678901", "company": 1, "sinacorId": "123"}
```

### Inicialização Automática

Ao criar um cliente, o sistema automaticamente:
1. Cria 2 registros externos:
   - CETIP (status: NotRegistered)
   - SELIC (status: NotRegistered)
2. Define `CreatedAt` com timestamp UTC
3. Deixa `LastUpdatedAt` como `null`

### Validações

| Campo | Validação |
|-------|-----------|
| `apiId` | Não vazio, máximo 32 caracteres |
| `document` | CPF (11 dígitos) ou CNPJ (14 dígitos), apenas números |
| `company` | 1 (Warren) ou 2 (Rena) |
| `sinacorId` | Opcional, máximo 9 caracteres |
| `legacyExternalId` | Opcional, máximo 9 caracteres |

---

## Logs

O controller registra os seguintes eventos:

**Informação (Information)**:
- `Creating customer with ApiId: {ApiId}, Document: {Document}, Company: {Company}`
- `Customer created successfully with ID: {CustomerId}, ApiId: {ApiId}`
- `Getting customer with ID: {CustomerId}`
- `Getting customer with ApiId: {ApiId}`
- `Customer found with ID: {CustomerId}, ApiId: {ApiId}`

**Aviso (Warning)**:
- `Failed to create customer. Error: {ErrorCode} - {ErrorMessage}`
- `Customer not found. ID: {CustomerId}, Error: {ErrorCode}`
- `Customer not found. ApiId: {ApiId}, Error: {ErrorCode}`

---

## OpenAPI/Swagger

A API expõe documentação interativa via OpenAPI (Swagger) em ambiente de desenvolvimento:

**URL**: `https://localhost:{port}/openapi/v1.json`

Para acessar a UI do Swagger, adicione ao `Program.cs`:
```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Apex API v1");
    });
}
```

---

## Próximos Passos

1. **Autenticação/Autorização**: Adicionar JWT/OAuth2
2. **Rate Limiting**: Limitar requisições por cliente
3. **Paginação**: Implementar `GET /api/customers` com paginação
4. **Filtros**: Adicionar query parameters (company, document, etc.)
5. **Update/Delete**: Implementar endpoints PUT e DELETE
6. **Versionamento**: Adicionar versionamento de API (v1, v2)
7. **Cache**: Implementar cache de respostas

---

**Status:** ✅ Implementado
**Última atualização:** 2026-01-19
**Versão da API:** v1
