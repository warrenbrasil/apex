# Bond Aggregate - Documentação do Domínio

## Visão Geral

O agregado `Bond` representa um título de renda fixa no sistema. Esta é a raiz do agregado que encapsula as regras de negócio relacionadas a títulos financeiros.

## Estrutura do Domínio

### Entidade Principal: Bond

A entidade `Bond` é mapeada para a tabela `Bonds` no banco de dados e contém as seguintes propriedades:

```csharp
public sealed class Bond : Entity<int>, IAuditable
{
    public int Id { get; }                       // int (PK, auto-increment) - Id
    public BondSymbol Symbol { get; }            // varchar(64) - BondSymbol
    public Isin Isin { get; }                    // varchar(12) - Isin
    public DateTime IssuanceAt { get; }          // date - IssuanceAt
    public DateTime ExpirationAt { get; }        // date - ExpirationAt
    public int? BondDetailId { get; }            // int (nullable) - BondDetailId
    public bool IsCetipVerified { get; }         // tinyint(1) - IsCetipVerified
    public Guid ApiId { get; }                   // varchar(36) - ApiId
    public DateTime CreatedAt { get; }           // datetime(6) - CreatedAt
    public DateTime? LastUpdatedAt { get; }      // datetime(6) - LastUpdatedAt

    // Computed properties
    public bool ExistsInDatabase => Id > 0;      // True when bond is persisted
}
```

**Observação sobre ID:** O ID é do tipo `int` com auto-increment no banco de dados. Quando um Bond é criado via `Bond.Create()`, o ID é inicialmente `0` e será definido pelo banco de dados após a inserção.

### Value Objects

#### 1. BondSymbol
Representa o símbolo de negociação do título.

**Regras:**
- Não pode ser nulo ou vazio
- Máximo de 50 caracteres
- Espaços são removidos (trim)

#### 2. Isin (International Securities Identification Number)
Código alfanumérico de 12 caracteres que identifica unicamente um título financeiro.

**Formato:** 2 letras (código do país) + 9 alfanuméricos + 1 dígito de verificação

**Exemplo:** `BRXYZ1234567`

**Regras:**
- Exatamente 12 caracteres
- Primeiros 2 caracteres devem ser letras (país)
- Restante deve ser alfanumérico
- Case insensitive (convertido para uppercase)

#### 3. Rate
Representa uma taxa de juros percentual.

**Regras:**
- Não pode ser negativa
- Não pode exceder 1000%
- Armazenada como percentual (ex: 10.5 para 10.5%)

**Métodos:**
- `FormatForDisplay(int decimalPlaces = 2)` - Formata como "10.50%"
- `IsZero` - Verifica se a taxa é zero

#### 4. Money
Representa um valor monetário em BRL (Real Brasileiro).

**Operações suportadas:**
- Soma: `money1 + money2`
- Subtração: `money1 - money2`
- Multiplicação: `money * factor`
- Divisão: `money / divisor`

**Métodos:**
- `FormatForDisplay()` - Formata como "R$ 1.234,56"
- `IsZero`, `IsPositive`, `IsNegative` - Verificações de estado

#### 5. DateRange
Representa um intervalo de datas com lógica de negócio.

**Propriedades calculadas:**
- `DurationInDays` - Duração em dias
- `DurationInYears` - Duração em anos (convenção de 360 dias)
- `HasExpired` - Verifica se expirou
- `IsActive` - Verifica se está ativo

### Enums

#### CustodyChamberType
```csharp
public enum CustodyChamberType
{
    Cetip = 1,  // Títulos privados (CDB, CRI, CRA, Debêntures)
    Selic = 2   // Títulos públicos (Tesouro Direto)
}
```

#### MarketIndexType
```csharp
public enum MarketIndexType
{
    Pre = 0,        // Pré-fixado
    Cdi = 10,       // CDI
    Ipca = 20,      // Inflação (IPCA)
    Savings = 30,   // Poupança
    Selic = 40,     // SELIC
    IgpM = 50,      // IGP-M
    Ibovespa = 60,  // Ibovespa
    Sp500 = 70,     // S&P 500
    NoIndex = 200   // Sem índice
}
```

#### MarketType
```csharp
public enum MarketType
{
    Primary = 1,   // Mercado primário (novas emissões)
    Secondary = 2, // Mercado secundário (negociação entre investidores)
    Ipo = 3        // IPO
}
```

#### CreditRating
```csharp
public enum CreditRating
{
    Low = 10,     // BB+ e menor (alto risco)
    Medium = 20,  // Até A+ (risco moderado)
    High = 30     // Melhor que A+ (baixo risco)
}
```

#### EmitterType
```csharp
public enum EmitterType
{
    FinancialInstitution = 10, // Bancos, cooperativas
    Company = 20,              // Empresas privadas
    Individual = 30,           // Pessoa física
    Union = 40,                // Governo federal
    State = 50,                // Governo estadual
    City = 60                  // Governo municipal
}
```

## Regras de Negócio

### 1. Criação de Bonds

```csharp
var bond = Bond.Create(
    symbol: "CDB123",
    isin: "BRXYZ1234567",
    issuanceAt: DateTime.UtcNow,
    expirationAt: DateTime.UtcNow.AddYears(2),
    bondDetailId: null,      // Opcional
    isCetipVerified: false,  // Default
    apiId: null              // Opcional - será gerado automaticamente se não fornecido
);

// O ID será 0 até que o bond seja persistido no banco de dados
Console.WriteLine(bond.Id);                  // 0
Console.WriteLine(bond.ExistsInDatabase);    // false
```

**Validações:**
- Symbol deve ser válido
- ISIN deve ter formato correto
- Data de expiração deve ser posterior à data de emissão
- Permite criação de bonds expirados (para dados históricos)
- ApiId é gerado automaticamente se não fornecido

### 2. Propriedades Calculadas

#### HasExpired
Verifica se o título expirou (data de expiração no passado).

```csharp
bool expired = bond.HasExpired;
```

#### IsActive
Verifica se o título está ativo (dentro do período de validade e emissão já ocorreu).

```csharp
bool active = bond.IsActive; // true se entre IssuanceAt e ExpirationAt
```

#### RemainingDays
Retorna dias restantes até expiração (0 se já expirou).

```csharp
int days = bond.RemainingDays;
```

#### DurationInCalendarDays / DurationInYears
Calcula a duração total do título.

```csharp
int days = bond.DurationInCalendarDays;
decimal years = bond.DurationInYears; // Usa convenção de 360 dias
```

### 3. Operações de Atualização

#### Atualizar Verificação CETIP
```csharp
bond.UpdateCetipVerification(isVerified: true);
```

#### Vincular a BondDetail
```csharp
bond.LinkToBondDetail(bondDetailId: 123);
```

**Validação:** ID deve ser maior que zero.

#### Desvincular de BondDetail
```csharp
bond.UnlinkFromBondDetail();
```

#### Estender Expiração
```csharp
bond.ExtendExpiration(newExpirationAt: DateTime.UtcNow.AddYears(3));
```

**Validações:**
- Nova data deve ser posterior à expiração atual
- Nova data deve ser posterior à emissão

#### Atualizar Symbol/ISIN/ApiId
```csharp
bond.UpdateSymbol("NEW123");
bond.UpdateIsin("BRABC9876543");
bond.UpdateApiId(Guid.NewGuid());
```

**Validação ApiId:** Não pode ser `Guid.Empty`.

### 4. Métodos de Garantia (Guard Methods)

#### EnsureIsActive()
Garante que o bond está ativo antes de operações críticas.

```csharp
bond.EnsureIsActive(); // Lança BondNotActiveException se inativo
```

#### EnsureNotExpired()
Garante que o bond não expirou.

```csharp
bond.EnsureNotExpired(); // Lança BondExpiredException se expirado
```

#### EnsureCetipVerified()
Garante que a verificação CETIP foi concluída.

```csharp
bond.EnsureCetipVerified(); // Lança CetipVerificationException se não verificado
```

**Uso típico:**
```csharp
public void ProcessInvestment(Bond bond)
{
    bond.EnsureIsActive();
    bond.EnsureNotExpired();
    bond.EnsureCetipVerified();

    // Processar investimento...
}
```

## Exceções de Domínio

### InvalidBondException
Lançada quando dados do bond são inválidos.

**Exemplos:**
- Datas inválidas
- BondDetailId inválido
- Tentativa de operação inválida

### BondExpiredException
Lançada quando o bond expirou.

**Propriedades:**
- `BondSymbol` - Symbol do bond
- `ExpirationDate` - Data de expiração

### BondNotActiveException
Lançada quando o bond não está ativo.

**Propriedades:**
- `BondSymbol` - Symbol do bond

### CetipVerificationException
Lançada quando verificação CETIP falha ou é necessária.

**Propriedades:**
- `BondSymbol` - Symbol do bond
- `Reason` - Motivo da falha

## Padrões de Uso

### 1. Criação com Factory Method
```csharp
// Para novos bonds
var bond = Bond.Create(symbol, isin, issuanceAt, expirationAt);

// Para reconstituir do banco de dados
var bond = Bond.Reconstitute(
    id, symbol, isin, issuanceAt, expirationAt,
    bondDetailId, isCetipVerified, apiId,
    createdAt, lastUpdatedAt
);
```

### 2. Validação antes de Operações
```csharp
public async Task<InvestmentResult> InvestAsync(Bond bond, Money amount)
{
    // Validações de negócio
    bond.EnsureIsActive();
    bond.EnsureNotExpired();
    bond.EnsureCetipVerified();

    // Processar investimento
    // ...
}
```

### 3. Auditoria Automática
```csharp
// CreatedAt é definido automaticamente na criação
var bond = Bond.Create(...);
Console.WriteLine(bond.CreatedAt); // DateTime.UtcNow

// LastUpdatedAt é atualizado automaticamente em operações de modificação
bond.UpdateCetipVerification(true);
Console.WriteLine(bond.LastUpdatedAt); // DateTime.UtcNow
```

## Princípios de Design

### 1. Encapsulamento
- Setters são privados
- Modificações apenas através de métodos de negócio
- Value objects são imutáveis

### 2. Validação
- Validação acontece na criação e em operações
- Exceções de domínio específicas
- Mensagens de erro claras

### 3. Imutabilidade Seletiva
- ID e ApiId são imutáveis (private init)
- CreatedAt é imutável
- Outras propriedades modificáveis via métodos controlados

### 4. Auditoria
- Implementa `IAuditable`
- Rastreamento automático de criação e modificação
- Timezone UTC para todas as datas

### 5. Rich Domain Model
- Lógica de negócio no domínio
- Propriedades calculadas
- Métodos que expressam intenção de negócio

## Testes

A entidade Bond possui cobertura completa de testes unitários em `BondTests.cs`:

- ✅ 46 testes passando
- ✅ Cobertura de cenários positivos e negativos
- ✅ Validação de regras de negócio
- ✅ Testes de exceções
- ✅ Testes de propriedades calculadas
- ✅ Testes de ID e persistência

### Executar Testes
```bash
dotnet test tests/Apex.UnitTests/Apex.UnitTests.csproj
```

## Próximos Passos

1. Implementar agregado BondDetail
2. Implementar agregado BondEmitter
3. Implementar agregado MarketIndex
4. Criar repositórios
5. Implementar casos de uso na camada de aplicação
6. Integrar com Entity Framework Core

## Referências

- Tabela de banco de dados: `Bonds`
- Baseado no sistema legado: `fixed-income-investment-service`
- Padrões: Domain-Driven Design (DDD), Clean Architecture
