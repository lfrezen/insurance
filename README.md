# 🏥 Sistema de Seguros - Arquitetura Hexagonal

Sistema de gerenciamento de seguros construído com **arquitetura de microsserviços**, seguindo princípios de **Arquitetura Hexagonal (Ports & Adapters)**, **Clean Code**, **DDD** e **SOLID** usando **.NET 9**, **comunicação assíncrona com RabbitMQ**, **comunicação HTTP** e **persistência PostgreSQL**.

A solução consiste em dois microsserviços independentes:

- **ProposalService** → Gerencia propostas de seguro (criar, aprovar, rejeitar, listar). Publica eventos quando uma proposta é aprovada.
- **ContractService** → Cria contratos automaticamente ao consumir eventos de propostas aprovadas via RabbitMQ. Também oferece endpoint HTTP para criação manual.

---

## 🏗️ Arquitetura

### Arquitetura Hexagonal (Ports & Adapters)

- **Domínio**: Lógica de negócio principal, independente de preocupações externas
- **Portas (Ports)**: Interfaces definindo contratos (IRepository, IUnitOfWork, IEventPublisher, IMessageConsumer)
- **Adaptadores (Adapters)**: Implementações concretas (Repositórios EF Core, RabbitMQ Publisher/Consumer, Cliente HTTP)

### Comunicação

- **RabbitMQ**: Comunicação assíncrona orientada a eventos (ProposalService publica → ContractService consome)
  - Exchange: `insurance-events` (tipo: topic)
  - Queue: `contract-service.proposal-approved`
  - Routing Key: `proposal.approved`
- **HTTP REST**: Comunicação síncrona entre serviços quando necessário
- **PostgreSQL**: Cada serviço possui seu próprio banco de dados (ProposalServiceDb, ContractServiceDb)

---

## 🚀 Tecnologias

| Camada         | Tecnologias                                                       |
| -------------- | ----------------------------------------------------------------- |
| API            | ASP.NET Core 9, Swagger/OpenAPI, BackgroundService                |
| Aplicação      | Casos de Uso, DTOs, Result Pattern                                |
| Domínio        | Entidades, Objetos de Valor, Regras de Negócio, Portas            |
| Infraestrutura | Entity Framework Core 9, PostgreSQL 16, RabbitMQ 3.13, HttpClient |
| Mensageria     | RabbitMQ Client 7.1.2, Exchange Topic, Durable Queues             |
| Testes         | xUnit, Moq, FluentAssertions (21 testes unitários)                |

---

## 🧱 Estrutura do Projeto

```
├── src/
│   ├── ProposalService/
│   │   ├── API/                    # Controllers, configuração DI
│   │   ├── Application/            # Casos de Uso, DTOs
│   │   ├── Domain/                 # Entidades, Value Objects, Portas, Events
│   │   │   ├── Events/             # ProposalApprovedEvent
│   │   │   └── Ports/              # IEventPublisher
│   │   └── Infrastructure/         # EF Core, Repositórios, RabbitMQ Publisher
│   │       └── Messaging/          # RabbitMqEventPublisher
│   └── ContractService/
│       ├── API/                    # Controllers, configuração DI
│       │   └── Workers/            # MessageConsumerWorker (BackgroundService)
│       ├── Application/            # Casos de Uso, DTOs
│       ├── Domain/                 # Entidades, Portas
│       │   └── Ports/              # IMessageConsumer
│       └── Infrastructure/         # EF Core, Repositórios, Cliente HTTP, RabbitMQ Consumer
│           └── Messaging/          # RabbitMqProposalConsumer, ProposalApprovedEvent
├── tests/
│   ├── ProposalService.Tests/      # 14 testes unitários
│   └── ContractService.Tests/      # 7 testes unitários
├── docker/
│   └── init-db.sql                 # Inicialização do banco de dados
├── docker-compose.yml              # Orquestração Docker (postgres, rabbitmq, services)
└── README.md
```

---

## 📋 Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

---

## 🚀 Como Executar

### Opção 1: Usando Docker Compose (Recomendado)

1. **Construir e iniciar todos os serviços**

```bash
docker-compose up --build -d
```

Os serviços serão iniciados automaticamente:

- PostgreSQL na porta 5432
- RabbitMQ na porta 5672 (AMQP) e 15672 (Management UI)
- ProposalService na porta 5009
- ContractService na porta 5126

As migrations são aplicadas automaticamente ao iniciar os serviços.

2. **Acessar as interfaces**

- ProposalService Swagger: http://localhost:5009/swagger
- ContractService Swagger: http://localhost:5126/swagger
- RabbitMQ Management: http://localhost:15672 (usuário: `guest`, senha: `guest`)

### Opção 2: Executando Localmente

1. **Iniciar PostgreSQL e RabbitMQ**

```bash
docker run --name insurance-postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16-alpine
docker run --name insurance-rabbitmq -p 5672:5672 -p 15672:15672 -d rabbitmq:3.13-management-alpine
```

2. **Criar bancos de dados**

```bash
docker exec -it insurance-postgres psql -U postgres -c "CREATE DATABASE \"ProposalServiceDb\";"
docker exec -it insurance-postgres psql -U postgres -c "CREATE DATABASE \"ContractServiceDb\";"
```

3. **Aplicar migrations**

```bash
# ProposalService
cd src/ProposalService/Infrastructure
dotnet ef database update --startup-project ../API

# ContractService
cd ../../ContractService/Infrastructure
dotnet ef database update --startup-project ../API
```

4. **Executar os serviços**

Terminal 1 - ProposalService:

```bash
cd src/ProposalService/API
dotnet run
```

Terminal 2 - ContractService:

```bash
cd src/ContractService/API
dotnet run

```

---

## 📡 Exemplos de Uso da API

### 1. Criar uma Proposta

```http
POST http://localhost:5009/api/proposals
Content-Type: application/json

{
  "fullName": "João Silva",
  "cpf": "12345678900",
  "email": "joao@email.com",
  "coverageType": "Vida",
  "insuredAmount": 100000
}
```

**Resposta:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fullName": "João Silva",
  "cpf": "123.456.789-00",
  "email": "joao@email.com",
  "coverageType": "Vida",
  "insuredAmount": 100000,
  "status": "EmAnalise",
  "createdAt": "2025-10-16T10:30:00Z"
}
```

### 2. Aprovar a Proposta

```http
PATCH http://localhost:5009/api/proposals/{id}/status
Content-Type: application/json

{
  "status": "Aprovada"
}
```

**Status possíveis:** `EmAnalise`, `Aprovada`, `Rejeitada`

**O que acontece:** Quando a proposta é aprovada, o ProposalService publica um evento `ProposalApprovedEvent` no RabbitMQ. O ContractService consome esse evento automaticamente e cria o contrato.

### 3. Verificar Contrato Criado Automaticamente

Após aprovar a proposta, o contrato é criado automaticamente via RabbitMQ. Para consultar:

```http
GET http://localhost:5126/api/contracts
```

Ou buscar por ID específico:

```http
GET http://localhost:5126/api/contracts/{contractId}
```

**Resposta:**

```json
{
  "id": "ed4e5d0f-1e5c-4c75-9803-f0054f9b4a0b",
  "proposalId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "contractedAt": "2025-10-16T19:47:31Z"
}
```

### 4. Monitorar Mensagens no RabbitMQ

Acesse o RabbitMQ Management UI: http://localhost:15672

- **Usuário:** guest
- **Senha:** guest

No painel você pode visualizar:

- Exchange `insurance-events`
- Queue `contract-service.proposal-approved`
- Mensagens publicadas e consumidas

---

## 🧪 Executando Testes

Executar todos os testes unitários (21 testes):

```bash
dotnet test
```

Executar projeto de teste específico:

```bash
dotnet test tests/ProposalService.Tests
dotnet test tests/ContractService.Tests
```

Cobertura de testes:

- **ProposalService**: 14 testes (8 Domínio + 6 Aplicação)
- **ContractService**: 7 testes (1 Domínio + 6 Aplicação)

---

## 🔄 Regras de Negócio

### ProposalService

- **Transições de Status**: EmAnalise → Aprovada/Rejeitada
- **Validação**: Não é possível alterar status se a proposta já está Aprovada ou Rejeitada
- **Objetos de Valor**: Validação de CPF, validação de Email
- **Tipos de Cobertura**: Vida, Auto, Residencial, Empresarial
- **Eventos**: Publica `ProposalApprovedEvent` no RabbitMQ quando proposta é aprovada

### ContractService

- **Consumo de Eventos**: Consome `ProposalApprovedEvent` do RabbitMQ e cria contrato automaticamente
- **Validação HTTP**: Valida se a proposta existe e está aprovada via chamada HTTP ao ProposalService
- **BackgroundService**: Worker em execução contínua consumindo mensagens da fila
- **Tratamento de Erros**: ACK para sucesso, NACK sem requeue para evitar loops infinitos

---

## 🐳 Comandos Docker

```bash
# Construir e iniciar todos os serviços
docker-compose up --build -d

# Visualizar logs de todos os serviços
docker-compose logs -f

# Visualizar logs de um serviço específico
docker-compose logs -f proposal-service
docker-compose logs -f contract-service
docker-compose logs -f rabbitmq

# Parar todos os serviços
docker-compose down

# Parar e remover volumes (limpar banco de dados)
docker-compose down -v

# Reiniciar um serviço específico
docker-compose restart proposal-service
docker-compose restart contract-service

# Verificar status dos containers
docker-compose ps

# Acessar shell de um container
docker exec -it insurance-proposal-service bash
docker exec -it insurance-contract-service bash
```

---

## 📁 Migrations do Banco de Dados

### Criar nova migration

```bash
# ProposalService
cd src/ProposalService/Infrastructure
dotnet ef migrations add NomeDaMigration --startup-project ../API

# ContractService
cd src/ContractService/Infrastructure
dotnet ef migrations add NomeDaMigration --startup-project ../API
```

### Aplicar migrations

```bash
# ProposalService
cd src/ProposalService/Infrastructure
dotnet ef database update --startup-project ../API

# ContractService
cd src/ContractService/Infrastructure
dotnet ef database update --startup-project ../API
```

### Reverter migration

```bash
# Reverter para migration específica
dotnet ef database update NomeDaMigrationAnterior --startup-project ../API

# Reverter todas as migrations
dotnet ef database update 0 --startup-project ../API
```

---

## 🐛 Troubleshooting

### Erro: "Cannot connect to PostgreSQL"

```bash
# Verificar se o container está rodando
docker ps | grep postgres

# Ver logs do PostgreSQL
docker-compose logs postgres

# Reiniciar o PostgreSQL
docker-compose restart postgres
```

### Erro: "Migration not applied"

As migrations são aplicadas automaticamente ao iniciar os serviços via Docker Compose. Se necessário aplicar manualmente:

```bash
docker exec -it insurance-proposal-service dotnet ef database update
docker exec -it insurance-contract-service dotnet ef database update
```

### Erro: "RabbitMQ connection failed"

```bash
# Verificar se o RabbitMQ está rodando
docker-compose ps rabbitmq

# Ver logs do RabbitMQ
docker-compose logs rabbitmq

# Reiniciar o RabbitMQ
docker-compose restart rabbitmq
```

### Mensagem não está sendo consumida

```bash
# Verificar logs do ContractService
docker-compose logs -f contract-service

# Acessar RabbitMQ Management UI
# http://localhost:15672 (guest/guest)
# Verificar se a fila tem mensagens pendentes
```

### Erro: "Port already in use"

```bash
# Ver processos usando a porta
netstat -ano | findstr :5009

# Parar processo específico (substitua PID)
taskkill /PID <PID> /F
```

### Limpar ambiente Docker completamente

```bash
# Parar tudo e remover volumes
docker-compose down -v

# Remover imagens antigas
docker rmi insurance-proposal-service insurance-contract-service

# Rebuild completo
docker-compose up --build -d
```

---

## 🎯 Padrões de Design e Princípios

### Padrões Aplicados

- **Arquitetura Hexagonal**: Separação clara entre Domínio, Aplicação e Infraestrutura
- **Repository Pattern**: Abstração sobre acesso a dados
- **Unit of Work**: Gerenciamento de transações e coordenação de repositórios
- **Result Pattern**: Tratamento explícito de erros sem uso de exceções para fluxo de controle
- **Event-Driven Architecture**: Comunicação assíncrona via eventos de domínio
- **Publisher/Subscriber**: RabbitMQ para desacoplamento entre serviços
- **Background Service**: Worker pattern para processamento assíncrono
- **Dependency Injection**: Inversão de controle e baixo acoplamento

### Princípios SOLID

- **Single Responsibility**: Cada classe tem uma única responsabilidade
- **Open/Closed**: Aberto para extensão, fechado para modificação
- **Liskov Substitution**: Subtipos devem ser substituíveis por seus tipos base
- **Interface Segregation**: Interfaces específicas são melhores que interfaces genéricas
- **Dependency Inversion**: Dependa de abstrações, não de implementações concretas

### Domain-Driven Design

- **Entidades**: Objetos com identidade única (Proposal, Contract)
- **Objetos de Valor**: Objetos imutáveis sem identidade (CPF, Email)
- **Agregados**: Conjuntos de objetos tratados como uma unidade
- **Repositórios**: Acesso a agregados persistidos
- **Casos de Uso**: Orquestração da lógica de aplicação
- **Eventos de Domínio**: ProposalApprovedEvent para comunicação entre bounded contexts

### Clean Code

- Nomes significativos e expressivos
- Funções pequenas com responsabilidade única
- Tratamento de erros explícito
- Testes como documentação viva

---

## 📚 Documentação Adicional

### Diagrama de Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│                        HTTP Clients                          │
└──────────────┬──────────────────────┬───────────────────────┘
               │                      │
               ▼                      ▼
        ┌──────────────┐      ┌──────────────┐
        │  Proposal    │      │  Contract    │
        │   Service    │      │   Service    │
        │  :5009       │      │  :5126       │
        └──────┬───────┘      └──────┬───────┘
               │                     │
               │ Publish Event       │ Consume Event
               ▼                     ▼
        ┌─────────────────────────────────────┐
        │           RabbitMQ :5672            │
        │  Exchange: insurance-events (topic) │
        │  Queue: contract-service.proposal-  │
        │         approved                    │
        └─────────────────────────────────────┘
               │                     │
               ▼                     ▼
        ┌──────────────┐      ┌──────────────┐
        │ ProposalDb   │      │ ContractDb   │
        │ PostgreSQL   │      │ PostgreSQL   │
        └──────────────┘      └──────────────┘
```

### Portas dos Serviços

| Serviço               | Porta | Endpoint                      |
| --------------------- | ----- | ----------------------------- |
| ProposalService       | 5009  | http://localhost:5009/swagger |
| ContractService       | 5126  | http://localhost:5126/swagger |
| PostgreSQL            | 5432  | localhost:5432                |
| RabbitMQ (AMQP)       | 5672  | localhost:5672                |
| RabbitMQ (Management) | 15672 | http://localhost:15672        |

---

## 📄 Licença

Este projeto foi desenvolvido para fins educacionais e de demonstração de boas práticas de arquitetura de software.

---

## 👤 Autor

**Sistema de Seguros - Arquitetura Hexagonal**

Demonstração de microsserviços com .NET 9, Arquitetura Hexagonal, DDD, Event-Driven Architecture e Docker.

Desenvolvido com ❤️ usando .NET 9, PostgreSQL, RabbitMQ e Docker

---

## 🙏 Agradecimentos

Projeto inspirado em princípios de:

- Clean Architecture (Robert C. Martin)
- Domain-Driven Design (Eric Evans)
- Hexagonal Architecture (Alistair Cockburn)
- Microservices Patterns (Chris Richardson)
- Enterprise Integration Patterns (Gregor Hohpe)
