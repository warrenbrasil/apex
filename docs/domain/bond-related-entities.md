# Entidades Relacionadas ao Bond - Documentação

## Visão Geral

Este documento descreve as entidades que compõem o ecossistema do agregado Bond, incluindo `BondDetail`, `BondBase`, `BondEmitter` e `MarketIndex`.

## Diagrama de Relacionamentos

```
┌─────────────────┐
│   MarketIndex   │
│   (CDI, IPCA)   │
└────────┬────────┘
         │
         │ 1
         │
         │ N
┌────────▼────────┐      N      ┌──────────────┐
│   BondDetail    │◄────────────┤     Bond     │
│   (Aggregate)   │              │  (Aggregate) │
└────────┬────────┘              └──────────────┘
         │
    ┌────┼────┐
    │    │    │
    │ N  │ N  │ N
    │    │    │
┌───▼──┐ │ ┌──▼──────────┐
│Bond  │ │ │ BondEmitter │
│Base  │ │ │  (Issuer)   │
└──────┘ │ └─────────────┘
         │
         │
```

## Entidades

### 1. MarketIndex

**Tabela:** `MarketIndexes`

**Propósito:** Representa os índices de mercado usados como referência para taxas de títulos.

#### Propriedades

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `Id` | int | Identificador único (PK) |
| `Name` | string | Nome do índice (ex: "CDI", "IPCA") |
| `Description` | string | Descrição detalhada |
| `MarketIndexType` | MarketIndexType | Tipo do índice (enum) |
| `VirtualIndexName` | string? | Nome virtual para sistema |
| `CetipIndexName` | string? | Nome para integração CETIP |

#### Propriedades Calculadas

| Propriedade | Descrição |
|-------------|-----------|
| `IsPreFixed` | Verdadeiro se é pré-fixado |
| `IsPostFixed` | Verdadeiro se é pós-fixado |
| `IsInflationLinked` | Verdadeiro se vinculado à inflação (IPCA, IGP-M) |

#### Exemplos de Uso

```csharp
// Criar novo índice
var cdiIndex = MarketIndex.Create(
    name: "CDI",
    description: "Certificado de Depósito Interbancário",
    marketIndexType: MarketIndexType.Cdi,
    virtualIndexName: "CDI_VIRTUAL",
    cetipIndexName: "CDI_CETIP");

// Verificar tipo
if (cdiIndex.IsPostFixed)
{
    Console.WriteLine("CDI é um índice pós-fixado");
}

// Atualizar informações
cdiIndex.Update(
    name: "CDI",
    description: "CDI - Taxa Referencial do Mercado Interbancário",
    virtualIndexName: "CDI_V2",
    cetipIndexName: "CDI_CETIP_V2");
```

---

### 2. BondBase

**Tabela:** `BondsBase`

**Propósito:** Representa o tipo/categoria base de um título (CDB, CRI, CRA, Debênture, etc.).

#### Propriedades

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `Id` | int | Identificador único (PK) |
| `BaseSymbol` | string | Símbolo base (ex: "CDB", "CRI", "DEB") |
| `Description` | string | Descrição do tipo |
| `TypeCore` | short | Identificador para sistema legado |
| `CustodyChamber` | CustodyChamberType | CETIP ou SELIC |
| `GuaranteedByFgc` | bool | Garantido pelo FGC? |
| `HasIncomeTax` | bool | Sujeito a imposto de renda? |

#### Propriedades Calculadas

| Propriedade | Descrição |
|-------------|-----------|
| `IsCetipCustody` | Verdadeiro se custodiado na CETIP |
| `IsSelicCustody` | Verdadeiro se custodiado na SELIC |

#### Tipos Comuns de BondBase

| BaseSymbol | Descrição | FGC | Custódia |
|------------|-----------|-----|----------|
| CDB | Certificado de Depósito Bancário | ✅ Sim | CETIP |
| CRI | Certificado de Recebíveis Imobiliários | ❌ Não | CETIP |
| CRA | Certificado de Recebíveis do Agronegócio | ❌ Não | CETIP |
| DEB | Debênture | ❌ Não | CETIP |
| LCI | Letra de Crédito Imobiliário | ✅ Sim | CETIP |
| LCA | Letra de Crédito do Agronegócio | ✅ Sim | CETIP |
| LF | Letra Financeira | ❌ Não | CETIP |
| TD | Tesouro Direto | ❌ Não | SELIC |

#### Exemplos de Uso

```csharp
// Criar CDB
var cdb = BondBase.Create(
    baseSymbol: "CDB",
    description: "Certificado de Depósito Bancário",
    typeCore: 1,
    custodyChamber: CustodyChamberType.Cetip,
    guaranteedByFgc: true,
    hasIncomeTax: true);

// Criar CRI (sem garantia FGC)
var cri = BondBase.Create(
    baseSymbol: "CRI",
    description: "Certificado de Recebíveis Imobiliários",
    typeCore: 2,
    custodyChamber: CustodyChamberType.Cetip,
    guaranteedByFgc: false,
    hasIncomeTax: false); // CRI é isento de IR para PF

// Verificar garantia FGC
if (cdb.GuaranteedByFgc)
{
    Console.WriteLine("Este título é garantido pelo FGC até R$ 250.000");
}
```

---

### 3. BondEmitter

**Tabela:** `BondEmitters`

**Propósito:** Representa a entidade que emite títulos (banco, empresa, governo).

#### Propriedades

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `Id` | int | Identificador único (PK) |
| `Issuer` | string | Código do emissor |
| `Name` | string | Nome curto |
| `FullName` | string | Nome legal completo |
| `BusinessDocument` | BusinessDocument | CNPJ ou CPF |
| `Email` | string? | Email para contato |
| `CreditRating` | CreditRating | Rating de crédito |
| `ExternalId` | string? | ID em sistema externo |
| `IssuerType` | EmitterType | Tipo de emissor |

#### Propriedades Calculadas

| Propriedade | Descrição |
|-------------|-----------|
| `IsFinancialInstitution` | Verdadeiro se é instituição financeira |
| `IsCompany` | Verdadeiro se é empresa privada |
| `IsGovernment` | Verdadeiro se é entidade governamental |
| `IsHighCreditRating` | Verdadeiro se rating é alto (A+ ou melhor) |
| `IsInvestmentGrade` | Verdadeiro se é grau de investimento |

#### Value Object: BusinessDocument

**Formato:**
- **CPF:** 11 dígitos (pessoa física)
- **CNPJ:** 14 dígitos (pessoa jurídica)

**Funcionalidades:**
- Detecção automática do tipo (CPF/CNPJ) baseado no comprimento
- Validação de formato
- Formatação com máscara:
  - CPF: `000.000.000-00`
  - CNPJ: `00.000.000/0000-00`

#### Exemplos de Uso

```csharp
// Criar emissor (banco)
var banco = BondEmitter.Create(
    issuer: "BANCO_ABC",
    name: "Banco ABC",
    fullName: "Banco ABC S.A.",
    businessDocument: "12345678000190", // CNPJ
    email: "contato@bancoabc.com.br",
    creditRating: CreditRating.High,
    externalId: "EXT_123",
    issuerType: EmitterType.FinancialInstitution);

// Verificar tipo de documento
Console.WriteLine(banco.BusinessDocument.IsCnpj); // true
Console.WriteLine(banco.BusinessDocument.FormatWithMask()); // "12.345.678/0001-90"

// Criar emissor (empresa)
var empresa = BondEmitter.Create(
    issuer: "EMPRESA_XYZ",
    name: "XYZ Incorporações",
    fullName: "XYZ Incorporações Ltda.",
    businessDocument: "98765432000100",
    email: null,
    creditRating: CreditRating.Medium,
    externalId: null,
    issuerType: EmitterType.Company);

// Atualizar rating
empresa.UpdateCreditRating(CreditRating.High);

// Verificar se é grau de investimento
if (empresa.IsInvestmentGrade)
{
    Console.WriteLine("Emissor com boa classificação de crédito");
}
```

---

### 4. BondDetail

**Tabela:** `BondDetails`

**Propósito:** Representa os detalhes de uma oferta específica de título, incluindo taxas, prazos e liquidez.

#### Propriedades

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `Id` | int | Identificador único (PK) |
| `FantasyName` | string? | Nome de fantasia/display |
| `DeadlineCalendarDays` | int | Prazo em dias corridos |
| `DeadlineCalendarYears` | decimal | Prazo em anos (360 dias) |
| `InitialUnitValue` | Money | Valor inicial unitário |
| `BenchmarkPercentualRate` | Rate | Taxa de referência (CDI+, IPCA+) |
| `FixedPercentualRate` | Rate | Taxa fixa (pré-fixado) |
| `IsAvailable` | bool | Disponível para negociação? |
| `IsExemptDebenture` | bool | Debênture isenta de IR? |
| `DaysToGracePeriod` | int | Dias até carência (liquidez) |
| `MarketIndexId` | int | FK para MarketIndex |
| `BondBaseId` | int | FK para BondBase |
| `BondEmitterId` | int | FK para BondEmitter |
| `CreatedAt` | DateTime | Data de criação |
| `LastUpdatedAt` | DateTime? | Data da última atualização |

#### Propriedades Calculadas

| Propriedade | Descrição |
|-------------|-----------|
| `HasDailyLiquidity` | Liquidez diária (carência = 0) |
| `LiquidityAtMaturityOnly` | Liquidez só no vencimento |
| `IsPreFixed` | Título pré-fixado |
| `IsPostFixed` | Título pós-fixado |
| `IsHybrid` | Título híbrido (pré + pós) |

#### Tipos de Títulos por Taxa

**1. Pré-Fixado (IsPreFixed = true)**
```
FixedPercentualRate = 12.5%
BenchmarkPercentualRate = 0%

Exemplo: CDB Pré-fixado 12,5% a.a.
```

**2. Pós-Fixado (IsPostFixed = true)**
```
FixedPercentualRate = 0%
BenchmarkPercentualRate = 2.0%

Exemplo: CDB CDI + 2% a.a.
```

**3. Híbrido (IsHybrid = true)**
```
FixedPercentualRate = 5.0%
BenchmarkPercentualRate = 100% (IPCA)

Exemplo: CDB IPCA + 5% a.a.
```

#### Liquidez

| Carência (dias) | Tipo | Descrição |
|-----------------|------|-----------|
| 0 | Diária | Pode resgatar a qualquer momento |
| 1 a N | Parcial | Pode resgatar após N dias |
| = Prazo Total | No Vencimento | Só pode resgatar no vencimento |

#### Exemplos de Uso

```csharp
// CDB Pré-fixado 12,5% a.a. - 2 anos - Liquidez diária
var cdbPre = BondDetail.Create(
    fantasyName: "CDB Banco ABC 12,5% a.a.",
    deadlineCalendarDays: 720, // 2 anos
    initialUnitValue: 1000m,
    benchmarkPercentualRate: 0m,
    fixedPercentualRate: 12.5m,
    isAvailable: true,
    isExemptDebenture: false,
    daysToGracePeriod: 0, // Liquidez diária
    marketIndexId: 1, // PRE
    bondBaseId: 1, // CDB
    bondEmitterId: 1); // Banco ABC

Console.WriteLine(cdbPre.IsPreFixed); // true
Console.WriteLine(cdbPre.HasDailyLiquidity); // true
Console.WriteLine(cdbPre.GetDeadlineDescription()); // "2 anos"
Console.WriteLine(cdbPre.GetLiquidityDescription()); // "Diária"

// CRI IPCA + 8% a.a. - 5 anos - Liquidez no vencimento
var cri = BondDetail.Create(
    fantasyName: "CRI Imobiliária XYZ IPCA + 8%",
    deadlineCalendarDays: 1800, // 5 anos
    initialUnitValue: 1000m,
    benchmarkPercentualRate: 8m, // IPCA + 8%
    fixedPercentualRate: 0m,
    isAvailable: true,
    isExemptDebenture: false,
    daysToGracePeriod: 1800, // Só no vencimento
    marketIndexId: 3, // IPCA
    bondBaseId: 2, // CRI
    bondEmitterId: 2); // Imobiliária XYZ

Console.WriteLine(cri.IsPostFixed); // true
Console.WriteLine(cri.LiquidityAtMaturityOnly); // true

// Atualizar taxas
cri.UpdateRates(
    benchmarkPercentualRate: 8.5m,
    fixedPercentualRate: 0m,
    daysToGracePeriod: 1800);

// Tornar indisponível
cri.MakeUnavailable();
```

---

## Relações entre Entidades

### Bond → BondDetail

```csharp
// Bond possui BondDetailId (FK)
var bond = Bond.Create(
    symbol: "CDB123",
    isin: "BRXYZ1234567",
    issuanceAt: DateTime.UtcNow,
    expirationAt: DateTime.UtcNow.AddYears(2));

// Vincular ao BondDetail
bond.LinkToBondDetail(bondDetailId: 456);

Console.WriteLine(bond.HasBondDetail); // true
Console.WriteLine(bond.BondDetailId); // 456
```

### BondDetail → MarketIndex, BondBase, BondEmitter

```csharp
// BondDetail possui 3 FKs obrigatórias
var bondDetail = BondDetail.Create(
    fantasyName: "Título Completo",
    deadlineCalendarDays: 360,
    initialUnitValue: 1000m,
    benchmarkPercentualRate: 2m,
    fixedPercentualRate: 0m,
    isAvailable: true,
    isExemptDebenture: false,
    daysToGracePeriod: 0,
    marketIndexId: 10,  // CDI
    bondBaseId: 1,       // CDB
    bondEmitterId: 5);   // Banco XYZ

// As entidades relacionadas devem existir no banco de dados
```

---

## Testes

### Estatísticas de Testes

- **Total de testes:** 64
- **BondDetailTests:** 18 testes
- **BondTests:** 46 testes
- **Cobertura:** 100% das regras de negócio

### Executar Testes

```bash
dotnet test tests/Apex.UnitTests/Apex.UnitTests.csproj
```

---

## Regras de Negócio Implementadas

### MarketIndex
✅ Nome e descrição obrigatórios
✅ Identificação automática de tipo (pré/pós-fixado, inflação)
✅ Atualização de nomes virtuais e CETIP

### BondBase
✅ BaseSymbol limitado a 10 caracteres
✅ BaseSymbol sempre em maiúsculas
✅ Identificação de custódia (CETIP/SELIC)
✅ Flags de garantia FGC e imposto de renda

### BondEmitter
✅ Validação de CNPJ/CPF
✅ Detecção automática de tipo de documento
✅ Formatação com máscara
✅ Validação de email (formato válido)
✅ Classificação de crédito
✅ Identificação de tipo de emissor

### BondDetail
✅ Prazo deve ser maior que zero
✅ Carência não pode exceder prazo
✅ Valor inicial deve ser positivo
✅ Taxas devem estar no intervalo válido (0-1000%)
✅ Cálculo automático de anos (convenção 360 dias)
✅ Identificação automática de tipo de título (pré/pós/híbrido)
✅ Descrições formatadas de liquidez e prazo
✅ Auditoria automática (CreatedAt/LastUpdatedAt)

---

## Próximos Passos

1. Implementar repositórios para cada entidade
2. Configurar Entity Framework Core mappings
3. Criar queries (CQRS) para consultas otimizadas
4. Implementar casos de uso na camada de aplicação
5. Criar DTOs para comunicação com API
6. Implementar validações adicionais conforme necessário

---

**Última atualização:** 2026-01-19
