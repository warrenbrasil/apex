# Customer Aggregate - Documentação do Domínio

## Visão Geral

O agregado `Customer` representa um cliente no sistema de investimentos em renda fixa. Este é um aggregate root que gerencia a identificação do cliente e seus registros em sistemas externos de liquidação/custódia (CETIP e SELIC).

## Estrutura do Domínio

### Entidade Principal: Customer

A entidade `Customer` é mapeada para a tabela `Customers` no banco de dados.

```csharp
public sealed class Customer : Entity<int>, IAuditable
{
    public int Id { get; }                                          // int (PK, auto-increment)
    public string ApiId { get; }                                    // varchar(32) - API identifier
    public BusinessDocument Document { get; }                       // varchar(14) - CPF/CNPJ
    public string? SinacorId { get; }                              // varchar(9) - Sinacor ID
    public Company Company { get; }                                 // int - Warren or Rena
    public string? LegacyExternalId { get; }                       // varchar(9) - Legacy ID
    public DateTime CreatedAt { get; }                              // datetime(6)
    public DateTime? LastUpdatedAt { get; }                         // datetime(6)
    public IReadOnlyList<CustomerExternalSystemRegister> ExternalRegisters { get; }  // Collection
}
```

### Entidade Filha: CustomerExternalSystemRegister

Representa o status de registro do cliente em um sistema externo (CETIP ou SELIC).

```csharp
public sealed class CustomerExternalSystemRegister : Entity<int>, IAuditable
{
    public int Id { get; }                                          // int (PK)
    public CustomerExternalSystemStatus Status { get; }             // byte - Status enum
    public CustomerExternalSystemType SystemType { get; }           // string - CETIP or SELIC
    public int CustomerId { get; }                                  // int (FK)
    public DateTime CreatedAt { get; }                              // datetime(6)
    public DateTime? LastUpdatedAt { get; }                         // datetime(6)
}
```

## Diagrama de Relacionamento

```
┌─────────────────────────────────┐
│         Customer                │
│    (Aggregate Root)             │
├─────────────────────────────────┤
│ + ApiId: string                 │
│ + Document: BusinessDocument    │
│ + SinacorId: string?            │
│ + Company: Company              │
│ + LegacyExternalId: string?     │
└────────────┬────────────────────┘
             │ 1
             │
             │ owns
             │
             │ 2 (CETIP + SELIC)
             ▼
┌─────────────────────────────────┐
│ CustomerExternalSystemRegister  │
├─────────────────────────────────┤
│ + Status: enum                  │
│ + SystemType: enum              │
│ + CustomerId: int (FK)          │
└─────────────────────────────────┘
```

## Value Objects

### BusinessDocument

Representa um documento empresarial brasileiro (CPF ou CNPJ).

**Formato:**
- **CPF**: 11 dígitos (pessoa física - individual)
- **CNPJ**: 14 dígitos (pessoa jurídica - legal entity)

**Funcionalidades:**
- Detecção automática do tipo baseado no comprimento
- Validação de formato (apenas dígitos)
- Formatação com máscara:
  - CPF: `000.000.000-00`
  - CNPJ: `00.000.000/0000-00`

**Exemplo:**
```csharp
var cpf = BusinessDocument.Create("12345678901");
Console.WriteLine(cpf.IsCpf);              // true
Console.WriteLine(cpf.FormatWithMask());   // "123.456.789-01"

var cnpj = BusinessDocument.Create("12345678000190");
Console.WriteLine(cnpj.IsCnpj);            // true
Console.WriteLine(cnpj.FormatWithMask());  // "12.345.678/0001-90"
```

## Enumerações

### Company

```csharp
public enum Company
{
    Warren = 1,  // Plataforma principal de investimentos
    Rena = 2     // Plataforma alternativa de investimentos
}
```

### CustomerExternalSystemStatus

```csharp
public enum CustomerExternalSystemStatus : byte
{
    NotRegistered = 0,  // Não registrado no sistema externo
    Registered = 1,     // Registrado e ativo
    Inactive = 2        // Registrado mas inativo
}
```

### CustomerExternalSystemType

```csharp
public enum CustomerExternalSystemType
{
    Cetip = 0,  // Sistema de custódia para títulos privados
    Selic = 1   // Sistema de custódia para títulos públicos
}
```

## Regras de Negócio

### 1. Criação de Customer

```csharp
var customer = Customer.Create(
    apiId: "API_12345",
    document: "12345678901",        // CPF ou CNPJ
    company: Company.Warren,
    sinacorId: "123456789",         // Opcional
    legacyExternalId: "LEGACY123"); // Opcional
```

**Validações:**
- ✅ `apiId` é obrigatório e máximo 32 caracteres
- ✅ `document` deve ser CPF (11 dígitos) ou CNPJ (14 dígitos) válido
- ✅ `company` é obrigatório
- ✅ `sinacorId` é opcional, máximo 9 caracteres
- ✅ `legacyExternalId` é opcional, máximo 9 caracteres

**Inicialização Automática:**
- ✅ Cria automaticamente 2 registros externos:
  - Um para CETIP (status: NotRegistered)
  - Um para SELIC (status: NotRegistered)

### 2. Propriedades Calculadas

```csharp
// Tipo de cliente
bool isIndividual = customer.IsIndividual;      // true se CPF
bool isLegalEntity = customer.IsLegalEntity;    // true se CNPJ

// Company
bool isWarren = customer.IsWarrenCustomer;      // true se Warren
bool isRena = customer.IsRenaCustomer;          // true se Rena

// IDs opcionais
bool hasSinacor = customer.HasSinacorId;        // true se tem SinacorId
bool hasLegacy = customer.HasLegacyExternalId;  // true se tem LegacyExternalId
```

### 3. Gerenciamento de Registros Externos

#### Obter Registros

```csharp
// Obter registro específico
var cetipRegister = customer.GetCetipRegister();
var selicRegister = customer.GetSelicRegister();

// Ou por tipo
var register = customer.GetRegisterForSystem(CustomerExternalSystemType.Cetip);
```

#### Verificar Status

```csharp
// Verificar se está registrado
bool isRegisteredCetip = customer.IsRegisteredInCetip();
bool isRegisteredSelic = customer.IsRegisteredInSelic();

// Ou por tipo
bool isRegistered = customer.IsRegisteredIn(CustomerExternalSystemType.Cetip);
```

#### Atualizar Status

```csharp
// Marcar como registrado
customer.MarkAsRegisteredIn(CustomerExternalSystemType.Cetip);

// Marcar como inativo
customer.MarkAsInactiveIn(CustomerExternalSystemType.Selic);
```

### 4. Atualizações

```csharp
// Atualizar API ID
customer.UpdateApiId("NEW_API_ID");

// Atualizar Sinacor ID
customer.UpdateSinacorId("987654321");

// Atualizar Company
customer.UpdateCompany(Company.Rena);

// Atualizar Document
customer.UpdateDocument("98765432000100");  // Muda de CPF para CNPJ

// Atualizar Legacy ID
customer.UpdateLegacyExternalId("NEW_LEGACY");
```

**Auditoria Automática:**
- ✅ Toda atualização define `LastUpdatedAt = DateTime.UtcNow`

## Restrições de Banco de Dados

### Chave Única Composta

A tabela `Customers` possui uma restrição de unicidade composta:

```sql
UNIQUE KEY (Document, SinacorId, Company)
```

**Significado:**
- Um cliente é único pela combinação de documento + SinacorId + Company
- Permite o mesmo CPF/CNPJ em diferentes companies (Warren e Rena)
- Permite múltiplos SinacorIds para o mesmo documento

### Relacionamento com CustomerExternalSystemRegister

```sql
FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
ON DELETE RESTRICT
```

**Significado:**
- Um cliente não pode ser excluído se tiver registros externos associados
- Garante integridade referencial

## Exceções de Domínio

### InvalidCustomerOperationException

Lançada quando uma operação inválida é tentada no customer.

```csharp
try
{
    customer.MarkAsRegisteredIn(CustomerExternalSystemType.Cetip);
}
catch (InvalidCustomerOperationException ex)
{
    Console.WriteLine(ex.Message);
}
```

### CustomerNotFoundException

Lançada quando um customer não é encontrado.

```csharp
public sealed class CustomerNotFoundException : DomainException
{
    public string Identifier { get; }
}
```

### DuplicateCustomerException

Lançada quando tenta-se criar um customer duplicado.

```csharp
public sealed class DuplicateCustomerException : DomainException
{
    public string Document { get; }
    public string SinacorId { get; }
    public string Company { get; }
}
```

## Casos de Uso Comuns

### Caso 1: Criar novo cliente Warren

```csharp
var customer = Customer.Create(
    apiId: "WARREN_API_001",
    document: "12345678901",
    company: Company.Warren,
    sinacorId: "123456");

// Verifica inicialização automática
Console.WriteLine(customer.ExternalRegisters.Count);  // 2 (CETIP + SELIC)
Console.WriteLine(customer.IsWarrenCustomer);         // true
Console.WriteLine(customer.IsIndividual);             // true
```

### Caso 2: Registrar cliente na CETIP

```csharp
// Verificar se já está registrado
if (!customer.IsRegisteredInCetip())
{
    // Realizar registro externo (API CETIP)
    // ...

    // Atualizar status no domínio
    customer.MarkAsRegisteredIn(CustomerExternalSystemType.Cetip);

    // Persistir via repository
    await customerRepository.UpdateAsync(customer);
}
```

### Caso 3: Migrar cliente de Warren para Rena

```csharp
// Criar novo customer na Rena com mesmo documento
var renaCustomer = Customer.Create(
    apiId: "RENA_API_001",
    document: customer.Document.Value,  // Mesmo CPF/CNPJ
    company: Company.Rena,
    sinacorId: "999888",               // Novo Sinacor
    legacyExternalId: customer.ApiId); // Referência ao Warren

// A restrição UNIQUE permite porque a Company é diferente
```

### Caso 4: Empresa com múltiplos Sinacors

```csharp
// Cliente pessoa jurídica (CNPJ) pode ter múltiplos SinacorIds
var customer1 = Customer.Create(
    apiId: "API_COMPANY_001",
    document: "12345678000190",    // CNPJ
    company: Company.Warren,
    sinacorId: "111111");

var customer2 = Customer.Create(
    apiId: "API_COMPANY_002",
    document: "12345678000190",    // Mesmo CNPJ
    company: Company.Warren,
    sinacorId: "222222");          // Diferente Sinacor

// Permitido: mesmo document + company, mas SinacorId diferente
Console.WriteLine(customer1.IsLegalEntity);  // true
Console.WriteLine(customer2.IsLegalEntity);  // true
```

### Caso 5: Verificar status de registros externos

```csharp
// Obter todos os registros
foreach (var register in customer.ExternalRegisters)
{
    Console.WriteLine($"{register.SystemType}: {register.Status}");

    if (register.IsRegistered)
    {
        Console.WriteLine($"  Registrado em: {register.CreatedAt}");
    }
}

// Output:
// Cetip: Registered
//   Registrado em: 2025-01-15 10:30:00
// Selic: NotRegistered
```

## Testes

### Estatísticas de Testes

- **CustomerTests**: 23 testes
- **Todos passando**: ✅ 100%
- **Cobertura**: Completa de regras de negócio

### Cenários Testados

✅ Criação com parâmetros válidos
✅ Inicialização automática de registros externos
✅ Validação de CPF e CNPJ
✅ Validação de tamanhos máximos (ApiId, SinacorId, LegacyId)
✅ Propriedades calculadas (IsIndividual, IsWarren, etc.)
✅ Atualizações de dados
✅ Gerenciamento de registros externos
✅ Mudança de status de registros
✅ Reconstitute from persistence

### Executar Testes

```bash
dotnet test tests/Apex.UnitTests/Apex.UnitTests.csproj --filter "FullyQualifiedName~CustomerTests"
```

## Integração com Sistemas Externos

### CETIP (Central de Custódia e Liquidação Financeira de Títulos)

- **Propósito**: Custódia e liquidação de títulos privados (CDB, CRI, CRA, Debêntures)
- **Registro**: Necessário para negociar títulos privados
- **Status**: Gerenciado via `CustomerExternalSystemRegister`

### SELIC (Sistema Especial de Liquidação e Custódia)

- **Propósito**: Custódia e liquidação de títulos públicos (Tesouro Direto)
- **Registro**: Necessário para negociar títulos públicos
- **Status**: Gerenciado via `CustomerExternalSystemRegister`

### Sinacor (Sistema Nacional de Registro de Corretoras)

- **Propósito**: Identificação única do cliente na bolsa de valores
- **Formato**: Até 9 caracteres
- **Uso**: Integração com sistemas de corretagem

## Princípios de Design

### 1. Aggregate Root

✅ Customer é o aggregate root
✅ Controla acesso aos CustomerExternalSystemRegisters
✅ Garante consistência das regras de negócio
✅ Expõe apenas IReadOnlyList para prevenir modificações diretas

### 2. Encapsulamento

✅ Setters privados
✅ Modificações apenas via métodos de negócio
✅ Validações centralizadas
✅ Factory methods (Create, Reconstitute)

### 3. Auditoria

✅ Implementa `IAuditable`
✅ `CreatedAt` imutável (set once)
✅ `LastUpdatedAt` atualizado automaticamente
✅ Timezone UTC para todas as datas

### 4. Value Objects

✅ BusinessDocument encapsula lógica de CPF/CNPJ
✅ Imutabilidade
✅ Validação no construtor
✅ Formatação com máscara

## Próximos Passos

1. **Repository**: Implementar `ICustomerRepository` com EF Core
2. **Queries**: Criar queries CQRS para consultas eficientes
3. **Application Services**: Casos de uso de Customer
4. **DTOs**: Objetos de transferência para API
5. **Integrations**: Clientes HTTP para CETIP/SELIC
6. **Validators**: FluentValidation para requests

---

**Status:** ✅ Completamente implementado e testado
**Última atualização:** 2026-01-19
**Testes:** 87 testes passando (23 específicos de Customer)
