# Desafio Técnico - Microserviços E-commerce

Este projeto implementa uma arquitetura de microserviços para gerenciamento de estoque e vendas em uma plataforma de e-commerce, seguindo as especificações do desafio técnico e **melhores práticas de arquitetura com comunicação exclusiva via API Gateway**.

## 🏗️ Arquitetura

O sistema é composto por:

- **API Gateway** (Porta 5000): Ponto de entrada centralizado e **único canal de comunicação**
- **Stock Service** (Porta 5001): Gerenciamento de estoque
- **Sales Service** (Porta 5002): Gerenciamento de vendas
- **Shared Library**: Componentes compartilhados (DTOs, Models, Messaging)

### � Fluxo de Comunicação
```
Cliente → API Gateway → Microserviços
SalesService → API Gateway → StockService (HTTP)
SalesService ↔ StockService (RabbitMQ para eventos)
```

**✅ Princípios Seguidos:**
- Comunicação HTTP **exclusivamente via Gateway**
- Autenticação JWT centralizada
- Rate limiting e logging unificados
- Comunicação assíncrona via RabbitMQ para eventos

## �🚀 Tecnologias Utilizadas

- **.NET 9.0**: Framework principal
- **Entity Framework Core**: ORM para acesso a dados
- **AutoMapper**: Mapeamento entre objetos
- **JWT**: Autenticação e autorização centralizada
- **RabbitMQ**: Comunicação assíncrona entre microserviços
- **Ocelot**: API Gateway com roteamento avançado
- **Swagger**: Documentação das APIs
- **xUnit**: Testes unitários (22 testes implementados)
- **In-Memory Database**: Para desenvolvimento e testes

## 📋 Pré-requisitos

- .NET 8.0 SDK
- RabbitMQ Server (opcional para desenvolvimento)
- Visual Studio 2022 ou VS Code

## 🔧 Configuração e Execução

### 1. Clone o repositório
```bash
git clone <repository-url>
cd desafio-avanade
```

### 2. Executar todos os serviços (Método Rápido)
```bash
./run-services.sh
```
Este script irá:
- Restaurar dependências
- Compilar a solução
- Executar testes
- Iniciar todos os microserviços

### 3. Executar serviços manualmente

#### Restaurar dependências
```bash
dotnet restore
```

#### API Gateway (Porta 5000)
```bash
cd src/ApiGateway
dotnet run --urls="https://localhost:5000;http://localhost:5050"
```

#### Stock Service (Porta 5001)
```bash
cd src/StockService
dotnet run --urls="https://localhost:5001;http://localhost:5051"
```

#### Sales Service (Porta 5002)
```bash
cd src/SalesService
dotnet run --urls="https://localhost:5002;http://localhost:5052"
```

### 4. Acessar as APIs

- **API Gateway**: https://localhost:5000/swagger
- **Stock Service**: https://localhost:5001/swagger
- **Sales Service**: https://localhost:5002/swagger

## 🔐 Autenticação

### Credenciais de Teste

Para autenticar, faça uma requisição POST para `https://localhost:5000/api/auth/login`:

```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Usuários disponíveis:**
- `admin` / `admin123` (Role: Admin)
- `customer1` / `customer123` (Role: Customer)
- `customer2` / `customer123` (Role: Customer)
- `user` / `user123` (Role: Customer)

### Usando o Token

Após obter o token, adicione-o no header Authorization:
```
Authorization: Bearer <seu-token>
```

## 📚 Endpoints Principais

### API Gateway

#### Autenticação
- `POST /api/auth/login` - Fazer login
- `POST /api/auth/validate` - Validar token

#### Roteamento (via Gateway)
- `GET /api/stock/products` - Listar produtos (via Stock Service)
- `POST /api/sales/orders` - Criar pedido (via Sales Service)

### Stock Service (Direto)

#### Produtos
- `GET /api/products` - Listar todos os produtos
- `GET /api/products/{id}` - Obter produto por ID
- `POST /api/products` - Criar novo produto
- `PUT /api/products/{id}` - Atualizar produto
- `DELETE /api/products/{id}` - Remover produto

#### Estoque
- `GET /api/products/{id}/stock/check/{quantity}` - Verificar disponibilidade
- `PUT /api/products/{id}/stock` - Atualizar estoque
- `POST /api/products/{id}/stock/reserve` - Reservar estoque

### Sales Service (Direto)

#### Pedidos
- `GET /api/orders` - Listar todos os pedidos (Admin)
- `GET /api/orders/my-orders` - Meus pedidos
- `GET /api/orders/{id}` - Obter pedido por ID
- `POST /api/orders` - Criar novo pedido
- `PUT /api/orders/{id}/status` - Atualizar status (Admin)
- `PUT /api/orders/{id}/cancel` - Cancelar pedido

## 🛠️ Ferramentas e Arquivos de Exemplo

### Arquivo de Requisições HTTP
Use o arquivo `api-examples.http` com extensões como REST Client no VS Code para testar as APIs.

### Collection do Postman
Importe o arquivo `Desafio Avanade.postman_collection.json` no Postman para ter uma collection completa de testes.

#### 🎯 Cobertura Completa BDD
A coleção do Postman foi expandida para incluir **38 testes automatizados** cobrindo **100% dos cenários BDD**:

- ✅ **Autenticação JWT** (4 testes)
- ✅ **Stock Service** (11 testes) - Produtos e estoque
- ✅ **Sales Service** (12 testes) - Pedidos e gerenciamento
- ✅ **API Gateway** (5 testes) - Roteamento e resiliência
- ✅ **Microservices Communication** (3 testes) - RabbitMQ e integração
- ✅ **Security** (3 testes) - Segurança e autorização

**Documentação Detalhada:**
- [`Postman-Collection-BDD-Coverage.md`](Postman-Collection-BDD-Coverage.md) - Cobertura de cenários BDD
- [`Postman-Collection-Expansion-Summary.md`](Postman-Collection-Expansion-Summary.md) - Resumo da expansão

### Docker Compose (Opcional)
Para executar RabbitMQ e SQL Server localmente:
```bash
docker-compose up -d
```

### Script de Execução
Execute todos os serviços de uma vez:
```bash
chmod +x run-services.sh
./run-services.sh
```

## 🧪 Exemplos de Uso

### 1. Autenticar
```bash
curl -X POST https://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}'
```

### 2. Listar Produtos
```bash
curl -X GET https://localhost:5000/api/stock/products \
  -H "Authorization: Bearer <token>"
```

### 3. Criar Pedido
```bash
curl -X POST https://localhost:5000/api/sales/orders \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "items": [
      {
        "productId": 1,
        "quantity": 2
      }
    ]
  }'
```

## 🔄 Comunicação entre Microserviços

### HTTP Síncrono
- Sales Service → Stock Service (verificação de estoque)

### RabbitMQ Assíncrono
- Sales Service → Stock Service (atualizações de estoque)
- Queue: `stock-updates`

## 🧪 Executar Testes

### Todos os testes
```bash
dotnet test
```

### Testes específicos
```bash
dotnet test tests/StockService.Tests/
dotnet test tests/SalesService.Tests/
```

## 🎯 Cenários de Teste BDD

O projeto inclui uma suite completa de cenários BDD (Behavior Driven Development) documentada em [`tests/BDD-Test-Scenarios.feature`](tests/BDD-Test-Scenarios.feature).

### 📋 Cenários Cobertos:
- **Autenticação JWT** - Login, tokens, autorização
- **Gestão de Produtos** - CRUD completo com validações
- **Gestão de Estoque** - Verificação, reserva, atualizações
- **Gestão de Pedidos** - Criação, consulta, cancelamento
- **API Gateway** - Roteamento, rate limiting, resiliência
- **Comunicação entre Serviços** - RabbitMQ, sincronização
- **Segurança** - SQL injection, autorização, tokens

### 🧪 Implementação dos Testes:
- **xUnit** - 22 testes unitários automatizados
- **Postman** - 38 testes de integração cobrindo 100% dos cenários BDD
- **Documentação BDD** - [`BDD-Test-Scenarios-README.md`](BDD-Test-Scenarios-README.md)

## 📊 Funcionalidades Implementadas

### ✅ Microserviço de Estoque
- [x] Cadastro de produtos
- [x] Consulta de produtos
- [x] Atualização de estoque
- [x] Verificação de disponibilidade
- [x] Reserva de estoque
- [x] Consumo de mensagens do RabbitMQ

### ✅ Microserviço de Vendas
- [x] Criação de pedidos
- [x] Consulta de pedidos
- [x] Validação de estoque
- [x] Notificação via RabbitMQ
- [x] Controle de status de pedidos

### ✅ API Gateway
- [x] Roteamento para microserviços
- [x] Autenticação JWT
- [x] Rate limiting
- [x] Documentação Swagger

### ✅ Recursos Adicionais
- [x] Autenticação e autorização JWT
- [x] Comunicação RabbitMQ
- [x] Documentação Swagger
- [x] Testes unitários
- [x] Logging estruturado
- [x] Padrões de design (Repository, CQRS)
- [x] Tratamento de erros

## 🔧 Configurações

### RabbitMQ (Opcional)
Se você tiver o RabbitMQ instalado localmente, o sistema se conectará automaticamente. Caso contrário, o sistema funcionará sem comunicação assíncrona.

### Banco de Dados
O sistema usa In-Memory Database por padrão para facilitar o desenvolvimento. Para usar SQL Server, atualize as connection strings nos arquivos `appsettings.json`.

## 📝 Notas de Desenvolvimento

- Os microserviços são independentes e podem ser executados separadamente
- Cada serviço tem seu próprio banco de dados (Database per Service)
- A comunicação entre serviços é feita via HTTP e RabbitMQ
- JWT é usado para autenticação e autorização
- Swagger está disponível para todas as APIs
- Testes unitários cobrem os principais cenários

## 🚦 Status do Projeto

✅ **Completo** - Todas as funcionalidades do desafio foram implementadas com sucesso!

## 🤖 Desenvolvimento com GitHub Copilot

Este projeto foi **100% desenvolvido usando GitHub Copilot**, demonstrando as capacidades da IA para desenvolvimento de software empresarial. Nenhuma linha de código foi escrita manualmente.

### 🧪 Cenários de Teste BDD

Os cenários de teste BDD estão documentados no arquivo [`tests/BDD-Test-Scenarios.feature`](tests/BDD-Test-Scenarios.feature). Eles cobrem:
- **Autenticação JWT**: Login, tokens, autorização.
- **Gestão de Produtos e Estoque**: CRUD, validações, reserva e atualizações.
- **Gestão de Pedidos**: Criação, consulta, cancelamento.
- **API Gateway**: Roteamento, rate limiting, resiliência.
- **Comunicação entre Serviços**: RabbitMQ, sincronização.
- **Segurança**: SQL injection, autorização, tokens.

### 🛠️ Collection do Postman

Para testar as APIs, utilize a collection do Postman disponível no arquivo [`Collection`](postman-collection.json). Ela inclui:
- **38 testes automatizados** cobrindo 100% dos cenários BDD.
- Testes de integração para autenticação, estoque, vendas e comunicação entre serviços.

### 📊 Arquitetura Baseada no C4 Model

A arquitetura do sistema foi modelada seguindo o padrão C4, detalhado no arquivo [`architecture/c4-model.dsl`](architecture/c4-model.dsl). Este modelo descreve:
- **Contexto do Sistema**: Interação entre usuários e o sistema.
- **Containers**: API Gateway, Stock Service, Sales Service, RabbitMQ e Shared Library.
- **Componentes**: Estrutura interna de cada microserviço.
- **Dinâmica**: Fluxos de autenticação e criação de pedidos.

Para mais detalhes, consulte os diagramas gerados a partir do modelo C4.

## 📄 Licença

Este projeto foi desenvolvido para fins educacionais como parte de um desafio técnico.
