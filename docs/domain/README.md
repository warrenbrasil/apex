# Documentação do Domínio - Apex

Este diretório contém a documentação completa do modelo de domínio do sistema Apex.

## Visão Geral

O domínio do Apex foi desenvolvido seguindo os princípios de **Domain-Driven Design (DDD)** e **Clean Architecture**, garantindo:

- ✅ **Encapsulamento**: Lógica de negócio protegida dentro das entidades
- ✅ **Validações**: Regras de negócio aplicadas no momento correto
- ✅ **Testabilidade**: Cobertura completa de testes unitários
- ✅ **Imutabilidade**: Value Objects imutáveis para garantir consistência
- ✅ **Auditoria**: Rastreamento automático de criação e modificação

## Estrutura do Domínio

```
src/Apex.Domain/
├── Primitives/           # Infraestrutura base do domínio
│   ├── Entity.cs         # Classe base para entidades
│   ├── ValueObject.cs    # Classe base para value objects
│   ├── IAuditable.cs     # Interface de auditoria
│   └── DomainException.cs # Exceção base do domínio
│
├── Entities/             # Entidades do domínio (aggregate roots)
│   └── Bond.cs           # ✅ Implementado - Título de renda fixa
│
├── ValueObjects/         # Value Objects (objetos de valor)
│   ├── Isin.cs          # ISIN code (12 caracteres)
│   ├── BondSymbol.cs    # Símbolo do título
│   ├── Rate.cs          # Taxa de juros
│   ├── Money.cs         # Valor monetário em BRL
│   └── DateRange.cs     # Intervalo de datas
│
├── Enums/               # Enumerações fortemente tipadas
│   ├── CustodyChamberType.cs  # CETIP/SELIC
│   ├── MarketIndexType.cs     # PRE, CDI, IPCA, SELIC, etc.
│   ├── MarketType.cs          # Primary, Secondary, IPO
│   ├── CreditRating.cs        # Low, Medium, High
│   └── EmitterType.cs         # FinancialInstitution, Company, etc.
│
└── Exceptions/          # Exceções de domínio
    └── BondExceptions.cs # Exceções relacionadas a Bond
```

## Agregados Implementados

### 1. Bond Aggregate ✅

**Status:** Completamente implementado e testado

**Documentação:** [bond-aggregate.md](./bond-aggregate.md)

**Tabela do banco:** `Bonds`

**Responsabilidades:**
- Representar títulos de renda fixa
- Validar datas de emissão e expiração
- Gerenciar verificação CETIP
- Calcular propriedades derivadas (duração, dias restantes, status)
- Vincular a BondDetail

**Propriedades:**
- `Id` (int) - Chave primária, auto-increment
- `Symbol` (BondSymbol) - Símbolo de negociação
- `Isin` (Isin) - Código ISIN internacional
- `IssuanceAt` (DateTime) - Data de emissão
- `ExpirationAt` (DateTime) - Data de vencimento
- `BondDetailId` (int?) - FK para BondDetail
- `IsCetipVerified` (bool) - Status de verificação CETIP
- `ApiId` (Guid) - Identificador para API externa
- `CreatedAt` (DateTime) - Data de criação
- `LastUpdatedAt` (DateTime?) - Data de última atualização

**Testes:** 46 testes unitários passando (100% de cobertura)

## Primitives (Building Blocks)

### Entity&lt;TId&gt;

Classe base para todas as entidades do domínio.

**Características:**
- Identidade única (`Id`)
- Comparação por identidade
- Sobrecarga de operadores `==` e `!=`
- Implementação de `Equals` e `GetHashCode`

**Exemplo:**
```csharp
public sealed class Bond : Entity<int>
{
    // ...
}
```

### ValueObject

Classe base para todos os value objects.

**Características:**
- Imutabilidade
- Comparação por valor (não por identidade)
- Método abstrato `GetEqualityComponents()`
- Implementação de `Equals` e `GetHashCode`

**Exemplo:**
```csharp
public sealed class Isin : ValueObject
{
    public string Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### IAuditable

Interface para entidades que requerem auditoria.

```csharp
public interface IAuditable
{
    DateTime CreatedAt { get; }
    DateTime? LastUpdatedAt { get; }
}
```

### DomainException

Classe base para todas as exceções de domínio.

**Exceções implementadas:**
- `InvalidBondException` - Dados de bond inválidos
- `BondExpiredException` - Bond expirado
- `BondNotActiveException` - Bond não ativo
- `CetipVerificationException` - Falha na verificação CETIP

## Value Objects Implementados

### Isin
Código internacional de identificação de títulos (12 caracteres).

**Formato:** 2 letras (país) + 10 alfanuméricos

**Validações:**
- Exatamente 12 caracteres
- Primeiros 2 devem ser letras
- Restante alfanumérico

### BondSymbol
Símbolo de negociação do título.

**Validações:**
- Não pode ser vazio
- Máximo 50 caracteres

### Rate
Taxa de juros percentual.

**Validações:**
- Não pode ser negativa
- Máximo 1000%

**Recursos:**
- Formatação para display: `10.50%`
- Verificação de taxa zero

### Money
Valor monetário em BRL.

**Operações:**
- Soma, subtração, multiplicação, divisão
- Formatação: `R$ 1.234,56`
- Verificações: `IsZero`, `IsPositive`, `IsNegative`

### DateRange
Intervalo de datas com lógica de negócio.

**Recursos:**
- Cálculo de duração (dias e anos)
- Verificação de expiração
- Status de ativação
- Convenção de 360 dias/ano

## Padrões de Design Utilizados

### 1. Factory Methods
Criação controlada de entidades através de métodos estáticos.

```csharp
// Para novos bonds
var bond = Bond.Create(symbol, isin, issuanceAt, expirationAt);

// Para reconstituir do banco de dados
var bond = Bond.Reconstitute(id, symbol, isin, ...);
```

### 2. Guard Clauses
Validações no início dos métodos para garantir pré-condições.

```csharp
bond.EnsureIsActive();
bond.EnsureNotExpired();
bond.EnsureCetipVerified();
```

### 3. Rich Domain Model
Lógica de negócio dentro do domínio, não em serviços.

```csharp
// Propriedades calculadas
bool isExpired = bond.HasExpired;
int daysLeft = bond.RemainingDays;
decimal years = bond.DurationInYears;
```

### 4. Immutability
Value objects são imutáveis para garantir thread-safety e consistência.

```csharp
var isin = Isin.Create("BRXYZ1234567");
// isin.Value não pode ser alterado
```

### 5. Encapsulation
Setters privados e modificações apenas através de métodos de negócio.

```csharp
public sealed class Bond
{
    public BondSymbol Symbol { get; private set; }

    public void UpdateSymbol(string newSymbol)
    {
        Symbol = BondSymbol.Create(newSymbol);
        LastUpdatedAt = DateTime.UtcNow;
    }
}
```

## Próximos Passos

### Agregados Pendentes

1. **BondDetail** - Detalhes do título
   - Taxas (benchmark, fixed)
   - Prazos e períodos de carência
   - Relacionamentos com BondBase, BondEmitter, MarketIndex

2. **BondEmitter** - Emissor do título
   - Dados do emissor
   - Rating de crédito
   - Tipo de emissor

3. **BondBase** - Base/tipo do título
   - Símbolos base (CDB, CRI, CRA, DEB)
   - Tipo de custódia
   - Garantias (FGC)

4. **MarketIndex** - Índices de mercado
   - Tipos de índices (PRE, CDI, IPCA, etc.)
   - Nomes e descrições

5. **Trade** - Negociação de títulos
   - Ordens de compra/venda
   - Status da negociação
   - Preços e taxas

### Infraestrutura Pendente

- [ ] Repositórios (Infrastructure layer)
- [ ] Entity Framework Core mapping
- [ ] Queries (CQRS pattern)
- [ ] Domain Services
- [ ] Application Services
- [ ] DTOs e AutoMapper

### Testes Pendentes

- [ ] Testes de integração
- [ ] Testes de performance
- [ ] Testes de arquitetura (ArchUnit)

## Convenções e Boas Práticas

### Nomenclatura

- **Entidades**: Substantivos no singular (Bond, Trade, Order)
- **Value Objects**: Substantivos descritivos (Isin, BondSymbol, Rate)
- **Exceções**: Sufixo "Exception" (InvalidBondException)
- **Interfaces**: Prefixo "I" (IAuditable, IRepository)
- **Enums**: Sufixo "Type" quando apropriado (MarketIndexType)

### Validações

- Validações de formato nos Value Objects
- Validações de negócio nas Entidades
- Exceções específicas de domínio
- Mensagens de erro claras e descritivas

### Testes

- Um arquivo de teste por classe
- Nomenclatura: `{ClassName}Tests.cs`
- Padrão AAA (Arrange, Act, Assert)
- Nomes descritivos: `Method_Scenario_ExpectedBehavior`

### Documentação

- XML comments em todos os membros públicos
- Documentação de agregados em arquivos .md
- Exemplos de uso na documentação
- Diagramas quando necessário

## Referências

- **Evans, Eric** - Domain-Driven Design: Tackling Complexity in the Heart of Software
- **Vernon, Vaughn** - Implementing Domain-Driven Design
- **Martin, Robert C.** - Clean Architecture
- **Microsoft** - .NET Microservices Architecture Guide

## Contribuindo

Ao adicionar novos componentes ao domínio:

1. Seguir os padrões estabelecidos
2. Implementar testes unitários completos
3. Documentar em XML comments
4. Atualizar documentação em .md
5. Validar com testes de arquitetura
6. Revisar com o time antes de merge

---

**Status do Domínio:** 🟢 Em desenvolvimento ativo

**Última atualização:** 2026-01-19

**Próxima revisão:** Após implementação de BondDetail
