# Modelo C4 - Sistema de E-commerce Microserviços

Este diretório contém o modelo arquitetural C4 do sistema de e-commerce desenvolvido para o Desafio Avanade, criado no formato Structurizr DSL.

## 📋 Sobre o Modelo C4

O modelo C4 (Context, Containers, Components, Code) é uma abordagem para documentar arquitetura de software através de diferentes níveis de abstração:

- **Contexto**: Visão geral do sistema e seus usuários
- **Containers**: Aplicações e sistemas de dados que compõem o software
- **Componentes**: Detalhes internos de cada container
- **Código**: Classes e interfaces (opcional)

## 🏗️ Arquitetura Documentada

### Sistema Principal
- **E-commerce System**: Sistema de microserviços para gerenciamento de estoque e vendas

### Atores
- **Cliente**: Usuário final que realiza compras
- **Administrador**: Gerencia produtos, estoque e pedidos

### Containers
- **API Gateway**: Ponto de entrada centralizado (ASP.NET Core + Ocelot)
- **Stock Service**: Microserviço de estoque (ASP.NET Core + EF Core)
- **Sales Service**: Microserviço de vendas (ASP.NET Core + EF Core)
- **Shared Library**: Biblioteca compartilhada (.NET Library)

### Sistemas Externos
- **RabbitMQ**: Sistema de mensageria assíncrona

## 📊 Diagramas Disponíveis

### 1. Context Diagram
Mostra o sistema em seu contexto, incluindo usuários e sistemas externos.

### 2. Container Diagram
Detalha os containers principais e suas interações.

### 3. Component Diagrams
- **API Gateway Components**: Autenticação, roteamento, rate limiting
- **Stock Service Components**: Controllers, services, repositories
- **Sales Service Components**: Controllers, services, HTTP clients

### 4. Dynamic Views
- **Fluxo de Autenticação**: Login de usuário com JWT
- **Fluxo de Criação de Pedido**: Processo completo de criação de pedido

### 5. Deployment Views
- **Development**: Ambiente de desenvolvimento local
- **Production**: Ambiente de produção com clusters

## 🔧 Como Visualizar

### Opção 1: Structurizr Online (Recomendado)
1. Acesse https://structurizr.com/dsl
2. Cole o conteúdo do arquivo `c4-model.dsl`
3. Clique em "Render"

### Opção 2: Structurizr CLI
```bash
# Instalar Structurizr CLI
npm install -g @structurizr/cli

# Gerar diagramas
structurizr export -workspace c4-model.dsl -format plantuml
```

### Opção 3: VS Code Extension
1. Instale a extensão "Structurizr DSL"
2. Abra o arquivo `c4-model.dsl`
3. Use o preview integrado

## 📝 Principais Características Documentadas

### Comunicação
- **HTTP Síncrono**: Cliente → API Gateway → Microserviços
- **HTTP Interno**: Sales Service → API Gateway → Stock Service
- **RabbitMQ Assíncrono**: Eventos de vendas e atualizações de estoque

### Segurança
- **Autenticação JWT**: Centralizada no API Gateway
- **Autorização**: Baseada em roles (Admin/Customer)

### Padrões Arquiteturais
- **API Gateway Pattern**: Ponto de entrada único
- **Database per Service**: Isolamento de dados
- **Repository Pattern**: Acesso a dados
- **Service Layer**: Lógica de negócio
- **Event-Driven**: Comunicação assíncrona

### Tecnologias
- **.NET 9**: Framework principal
- **Ocelot**: API Gateway
- **Entity Framework Core**: ORM
- **RabbitMQ**: Message Broker
- **JWT**: Autenticação
- **AutoMapper**: Mapeamento de objetos

## 🎯 Benefícios da Documentação C4

1. **Comunicação Clara**: Linguagem visual padronizada
2. **Diferentes Níveis**: Do contexto aos detalhes técnicos
3. **Atualizável**: Código como documentação
4. **Compartilhável**: Formato padrão da indústria
5. **Versionável**: Controle de versão junto com o código

## 🔄 Manutenção

Para manter o modelo atualizado:

1. **Mudanças na Arquitetura**: Atualize os containers e relacionamentos
2. **Novos Componentes**: Adicione aos diagramas de componentes
3. **Novas Integrações**: Atualize os relacionamentos
4. **Deploy Changes**: Modifique os deployment environments

## 📚 Referências

- [Modelo C4](https://c4model.com/)
- [Structurizr DSL](https://github.com/structurizr/dsl)
- [Documentação Structurizr](https://structurizr.com/help/dsl)
