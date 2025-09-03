workspace "Desafio Avanade - E-commerce Microservices" "Arquitetura de microserviços para plataforma de e-commerce com API Gateway, gerenciamento de estoque e vendas" {
    
    model {
        // External Actors
        customer = person "Cliente" "Usuário final que realiza compras na plataforma" {
            tags "Customer"
        }
        
        admin = person "Administrador" "Administrador do sistema que gerencia produtos e pedidos" {
            tags "Admin"
        }
        
        // Main Software System
        ecommerceSystem = softwareSystem "E-commerce System" "Sistema de microserviços para gerenciamento de estoque e vendas" {
            
            // API Gateway Container
            apiGateway = container "API Gateway" "Ponto de entrada centralizado para todas as requisições. Provê autenticação JWT, rate limiting e roteamento" "ASP.NET Core, Ocelot" {
                tags "API Gateway"
                
                authController = component "Auth Controller" "Controlador responsável pela autenticação e geração de tokens JWT" "ASP.NET Core Controller"
                ocelotMiddleware = component "Ocelot Gateway" "Middleware para roteamento e proxy reverso" "Ocelot"
                jwtMiddleware = component "JWT Middleware" "Middleware para validação de tokens JWT" "ASP.NET Core JWT"
                rateLimitMiddleware = component "Rate Limiting" "Middleware para controle de taxa de requisições" "Ocelot Rate Limiting"
            }
            
            // Stock Service Container
            stockService = container "Stock Service" "Microserviço responsável pelo gerenciamento de produtos e estoque" "ASP.NET Core, Entity Framework" {
                tags "Microservice"
                
                productsController = component "Products Controller" "API REST para gerenciamento de produtos" "ASP.NET Core Controller"
                productService = component "Product Service" "Lógica de negócio para produtos e estoque" "Service Layer"
                productRepository = component "Product Repository" "Acesso a dados dos produtos" "Repository Pattern"
                stockUpdateConsumer = component "Stock Update Consumer" "Consumidor de mensagens RabbitMQ para atualizações de estoque" "Hosted Service"
                stockDatabase = component "Stock Database" "Base de dados de produtos e estoque" "In-Memory Database"
            }
            
            // Sales Service Container
            salesService = container "Sales Service" "Microserviço responsável pelo gerenciamento de pedidos e vendas" "ASP.NET Core, Entity Framework" {
                tags "Microservice"
                
                ordersController = component "Orders Controller" "API REST para gerenciamento de pedidos" "ASP.NET Core Controller"
                orderService = component "Order Service" "Lógica de negócio para pedidos e vendas" "Service Layer"
                orderRepository = component "Order Repository" "Acesso a dados dos pedidos" "Repository Pattern"
                stockServiceClient = component "Stock Service Client" "Cliente HTTP para comunicação com Stock Service via Gateway" "HTTP Client"
                salesDatabase = component "Sales Database" "Base de dados de pedidos e vendas" "In-Memory Database"
            }
            
            // Shared Library
            sharedLibrary = container "Shared Library" "Biblioteca compartilhada com componentes comuns" ".NET Library" {
                tags "Shared"
                
                dtos = component "DTOs" "Data Transfer Objects compartilhados" "DTOs"
                models = component "Models" "Modelos de domínio compartilhados" "Domain Models"
                jwtService = component "JWT Service" "Serviço para geração e validação de tokens JWT" "JWT Service"
                messagingService = component "RabbitMQ Service" "Serviço para publicação e consumo de mensagens" "RabbitMQ Client"
            }
            
            // RabbitMQ Container
            rabbitMQ = container "RabbitMQ" "Sistema de mensageria para comunicação assíncrona entre microserviços" "RabbitMQ" {
                tags "Message Broker"
            }
        }
        
        // Relationships between People and Software Systems
        customer -> ecommerceSystem "Usa para realizar compras e consultar pedidos" "HTTPS"
        admin -> ecommerceSystem "Gerencia produtos, estoque e pedidos" "HTTPS"
        
        // Relationships between Containers
        customer -> apiGateway "Faz requisições" "HTTPS/REST"
        admin -> apiGateway "Faz requisições administrativas" "HTTPS/REST"
        
        apiGateway -> stockService "Roteia requisições de estoque" "HTTP/REST"
        apiGateway -> salesService "Roteia requisições de vendas" "HTTP/REST"
        
        salesService -> apiGateway "Consulta produtos e reserva estoque" "HTTP/REST"
        
        stockService -> rabbitMQ "Consome mensagens de atualização de estoque" "AMQP"
        salesService -> rabbitMQ "Publica eventos de vendas" "AMQP"
        
        apiGateway -> sharedLibrary "Usa componentes comuns"
        stockService -> sharedLibrary "Usa componentes comuns"
        salesService -> sharedLibrary "Usa componentes comuns"
        
        // Component Level Relationships - API Gateway
        authController -> jwtService "Gera e valida tokens JWT"
        ocelotMiddleware -> jwtMiddleware "Valida autenticação"
        ocelotMiddleware -> rateLimitMiddleware "Controla taxa de requisições"
        
        // Component Level Relationships - Stock Service
        productsController -> productService "Delega operações de negócio"
        productService -> productRepository "Acessa dados"
        productRepository -> stockDatabase "Persiste dados"
        stockUpdateConsumer -> messagingService "Consome mensagens"
        stockUpdateConsumer -> productService "Atualiza estoque"
        
        // Component Level Relationships - Sales Service
        ordersController -> orderService "Delega operações de negócio"
        orderService -> orderRepository "Acessa dados"
        orderRepository -> salesDatabase "Persiste dados"
        orderService -> stockServiceClient "Verifica disponibilidade"
        orderService -> messagingService "Publica eventos"
        stockServiceClient -> apiGateway "Comunica via Gateway"
        
        // Shared Library Dependencies
        messagingService -> rabbitMQ "Conecta ao broker"
        
        // Deployment Environments
        developmentEnvironment = deploymentEnvironment "Development" {
            deploymentNode "Developer Machine" "Máquina de desenvolvimento" "Windows/macOS/Linux" {
                deploymentNode "Docker Desktop" "Containerização local" "Docker" {
                    rabbitMQContainer = containerInstance rabbitMQ
                }
                
                deploymentNode ".NET Runtime" "Runtime .NET 9" ".NET 9" {
                    apiGatewayInstance = containerInstance apiGateway
                    stockServiceInstance = containerInstance stockService
                    salesServiceInstance = containerInstance salesService
                    sharedLibraryInstance = containerInstance sharedLibrary
                }
            }
        }
        
        productionEnvironment = deploymentEnvironment "Production" {
            deploymentNode "Load Balancer" "Balanceador de carga" "Nginx/HAProxy" {
                deploymentNode "API Gateway Cluster" "Cluster do API Gateway" "Kubernetes" {
                    apiGatewayProd1 = containerInstance apiGateway
                    apiGatewayProd2 = containerInstance apiGateway
                }
            }
            
            deploymentNode "Microservices Cluster" "Cluster dos microserviços" "Kubernetes" {
                deploymentNode "Stock Service Nodes" "Nós do Stock Service" {
                    stockServiceProd1 = containerInstance stockService
                    stockServiceProd2 = containerInstance stockService
                }
                
                deploymentNode "Sales Service Nodes" "Nós do Sales Service" {
                    salesServiceProd1 = containerInstance salesService
                    salesServiceProd2 = containerInstance salesService
                }
                
                deploymentNode "Shared Services" "Serviços compartilhados" {
                    sharedLibraryProd = containerInstance sharedLibrary
                }
            }
            
            deploymentNode "Message Broker Cluster" "Cluster RabbitMQ" "RabbitMQ Cluster" {
                rabbitMQProd1 = containerInstance rabbitMQ
                rabbitMQProd2 = containerInstance rabbitMQ
                rabbitMQProd3 = containerInstance rabbitMQ
            }
        }
    }
    
    views {
        // System Context Diagram
        systemContext ecommerceSystem "SystemContext" {
            include *
            autoLayout
            title "Sistema de E-commerce - Contexto do Sistema"
            description "Visão geral do sistema de microserviços de e-commerce mostrando os usuários e sistemas externos"
        }
        
        // Container Diagram
        container ecommerceSystem "Containers" {
            include *
            autoLayout
            title "Sistema de E-commerce - Diagrama de Containers"
            description "Visão dos containers principais: API Gateway, Stock Service, Sales Service e sistemas externos"
        }
        
        // Component Diagram - API Gateway
        component apiGateway "ApiGatewayComponents" {
            include *
            autoLayout
            title "API Gateway - Componentes"
            description "Componentes internos do API Gateway: autenticação, roteamento e controles"
        }
        
        // Component Diagram - Stock Service
        component stockService "StockServiceComponents" {
            include *
            autoLayout
            title "Stock Service - Componentes"
            description "Componentes do microserviço de estoque: controllers, services, repositories e consumers"
        }
        
        // Component Diagram - Sales Service
        component salesService "SalesServiceComponents" {
            include *
            autoLayout
            title "Sales Service - Componentes"
            description "Componentes do microserviço de vendas: controllers, services, repositories e clients"
        }
        
        // Dynamic Views - Container Level
        dynamic ecommerceSystem "UserLogin" "Fluxo de Login do Usuário" {
            customer -> apiGateway "1. POST /api/auth/login"
            apiGateway -> sharedLibrary "2. Valida credenciais e gera JWT"
            sharedLibrary -> apiGateway "3. Token JWT"
            apiGateway -> customer "4. Retorna token JWT"
            autoLayout
            title "Fluxo de Autenticação"
        }
        
        dynamic ecommerceSystem "CreateOrder" "Fluxo de Criação de Pedido" {
            customer -> apiGateway "1. POST /api/sales/orders (com JWT)"
            apiGateway -> salesService "2. Roteia para Sales Service"
            salesService -> apiGateway "3. Consulta produto via Gateway"
            apiGateway -> stockService "4. Roteia para Stock Service"
            stockService -> apiGateway "5. Retorna dados do produto"
            apiGateway -> salesService "6. Dados do produto"
            salesService -> apiGateway "7. Reserva estoque via Gateway"
            apiGateway -> stockService "8. Reserva estoque"
            stockService -> apiGateway "9. Confirmação de reserva"
            apiGateway -> salesService "10. Estoque reservado"
            salesService -> rabbitMQ "11. Publica evento de venda"
            stockService -> rabbitMQ "12. Consome atualização de estoque"
            salesService -> apiGateway "13. Pedido criado com sucesso"
            apiGateway -> customer "14. Confirmação do pedido"
            autoLayout
            title "Fluxo de Criação de Pedido"
        }
        
        // Deployment View - Development
        deployment ecommerceSystem "Development" "DevelopmentDeployment" {
            include *
            autoLayout
            title "Ambiente de Desenvolvimento"
            description "Deploy local para desenvolvimento com Docker e .NET Runtime"
        }
        
        // Deployment View - Production
        deployment ecommerceSystem "Production" "ProductionDeployment" {
            include *
            autoLayout
            title "Ambiente de Produção"
            description "Deploy em produção com clusters Kubernetes e alta disponibilidade"
        }
        
        // Styles
        styles {
            element "Person" {
                color #ffffff
                fontSize 22
                shape Person
            }
            
            element "Customer" {
                background #08427b
            }
            
            element "Admin" {
                background #d73027
            }
            
            element "Software System" {
                background #1168bd
                color #ffffff
            }
            
            element "Message Broker" {
                background #ff6b35
                shape Cylinder
            }
            
            element "Container" {
                background #438dd5
                color #ffffff
            }
            
            element "API Gateway" {
                background #2e8b57
                shape WebBrowser
            }
            
            element "Microservice" {
                background #1e90ff
                shape Component
            }
            
            element "Shared" {
                background #daa520
                shape Component
            }
            
            element "Component" {
                background #85bbf0
                color #000000
            }
            
            element "Database" {
                shape Cylinder
                background #ff6b6b
            }
            
            relationship "Relationship" {
                dashed false
            }
            
            relationship "Asynchronous" {
                dashed true
            }
        }
    }
}