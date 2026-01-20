# 🎯 Apex

> Sistema de alta performance para gestão de ordens e catálogo de produtos de Renda Fixa

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MySQL](https://img.shields.io/badge/MySQL-8.0-4479A1?logo=mysql&logoColor=white)](https://www.mysql.com/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?logo=rabbitmq&logoColor=white)](https://www.rabbitmq.com/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)

## 📋 Sobre

O Apex é o núcleo de processamento de ordens de Renda Fixa da Warren Brasil, construído para suportar alto volume de operações diárias com performance e resiliência. O sistema gerencia o ciclo completo de negociação de títulos públicos e privados, tanto no mercado primário quanto secundário.

### Responsabilidades

- ⚡ **Processamento de Ordens**: Gestão completa do fluxo de ordens de compra e venda
- 📚 **Catálogo de Produtos**: Gerenciamento centralizado de produtos de RF disponíveis
- ✅ **Validações**: Elegibilidade, limites de investimento e disponibilidade
- 🔄 **Orquestração**: Coordenação de fluxos entre sistemas internos e vendor
- 🔌 **APIs**: Endpoints para consulta de produtos e acompanhamento de ordens

## 🏗️ Arquitetura

### Stack Tecnológica

| Componente | Tecnologia | Uso |
|------------|------------|-----|
| **Runtime** | .NET 9 (LTS) | Framework principal |
| **Banco de Dados** | MySQL 8 | Persistência transacional |
| **Mensageria Interna** | RabbitMQ | Comunicação assíncrona entre workers |
| **Event Streaming** | Apache Kafka | Integração com outros domínios |
| **Containerização** | Docker | Deployment e ambientes |
| **Cloud** | AWS / OCI | Infraestrutura |

## 🚀 Getting Started

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) e Docker Compose
- MySQL 8.0+
- RabbitMQ 3.12+

### Instalação

1. **Clone o repositório**
```bash
git clone https://github.com/warren-brasil/apex.git
cd apex
```

2. **Configure as variáveis de ambiente**
```bash
cp .env.example .env
# Edite o .env com suas configurações
```

3. **Suba a infraestrutura local**
```bash
docker-compose up -d
```

4. **Execute as migrations**
```bash
dotnet ef database update
```

5. **Rode o projeto**
```bash
dotnet run --project src/Apex.Api
```

A API estará disponível em `https://localhost:5001`

### Testando a API

**Criar um cliente:**
```bash
curl -X POST https://localhost:5001/api/customers \
  -H "Content-Type: application/json" \
  -d '{
    "apiId": "API_WARREN_001",
    "document": "12345678901",
    "company": 1,
    "sinacorId": "123456789"
  }'
```

**Buscar cliente por ID:**
```bash
curl https://localhost:5001/api/customers/1
```

**Buscar cliente por API ID:**
```bash
curl https://localhost:5001/api/customers/by-api-id/API_WARREN_001
```

## 📁 Estrutura do Projeto
```
apex/
├── src/
│   ├── Apex.Api/              # API REST
│   ├── Apex.Application/      # Casos de uso e orquestrações
│   ├── Apex.Domain/           # Entidades e regras de negócio
│   ├── Apex.Infrastructure/   # Persistência e integrações
│   └── Apex.Workers/          # Background workers
├── tests/
│   ├── Apex.UnitTests/
│   ├── Apex.IntegrationTests/
│   └── Apex.E2ETests/
├── docs/                      # Documentação técnica
├── scripts/                   # Scripts de deploy e migrations
└── docker-compose.yml
```

## 🔧 Desenvolvimento

### Padrões e Práticas

- **Clean Architecture**: Separação clara de responsabilidades
- **DDD**: Modelagem orientada ao domínio financeiro
- **CQRS**: Separação de leitura e escrita para performance
- **Repository Pattern**: Abstração de acesso a dados

### Testes
```bash
# Todos os testes
dotnet test

# Apenas testes unitários
dotnet test --filter Category=Unit

# Com coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportsDirectory=./coverage
```

### Qualidade de Código
```bash
# Análise estática
dotnet format --verify-no-changes

# Security scan
dotnet list package --vulnerable
```

## 📊 Performance

O sistema foi projetado para suportar:

- ✅ **10.000+ ordens/dia** com latência < 100ms
- ✅ **Concorrência alta** em operações simultâneas
- ✅ **Resiliência** com circuit breakers e retries
- ✅ **Observabilidade** com métricas e traces completos

### Otimizações

- Bulk operations para gravação em lote
- Connection pooling otimizado
- Índices estratégicos no MySQL
- Cache distribuído para catálogo
- Processamento assíncrono via workers

## 📚 Documentação

### API
- [OpenAPI Spec](https://localhost:5001/openapi/v1.json) - Especificação OpenAPI da API
- [Customers Endpoints](./docs/api/customers-endpoints.md) - Documentação dos endpoints de Customers

### Domínio
- [Bond Aggregate](./docs/domain/bond-aggregate.md) - Documentação do agregado Bond
- [Customer Aggregate](./docs/domain/customer-aggregate.md) - Documentação do agregado Customer
- [Bond Related Entities](./docs/domain/bond-related-entities.md) - Entidades relacionadas a Bond

### Application Layer
- [Customer Use Cases](./docs/application/customer-use-cases.md) - Casos de uso de Customer (CQRS)

### Outros
- [Wiki](./docs/wiki) - Documentação técnica detalhada
- [ADRs](./docs/adr) - Architecture Decision Records
- [Runbooks](./docs/runbooks) - Guias operacionais

## 🤝 Contribuindo

1. Crie uma branch a partir de `main`: `git checkout -b feature/nova-funcionalidade`
2. Commit suas mudanças: `git commit -m 'feat: adiciona nova funcionalidade'`
3. Push para a branch: `git push origin feature/nova-funcionalidade`
4. Abra um Pull Request

### Commits Semânticos

- `feat`: Nova funcionalidade
- `fix`: Correção de bug
- `docs`: Documentação
- `refactor`: Refatoração
- `test`: Testes
- `perf`: Performance
- `chore`: Manutenção

## 📞 Suporte

- **Squad**: Mercados
- **Slack**: #squad-mercados

## 📄 Licença

Propriedade da Warren Brasil - Uso interno apenas

---
