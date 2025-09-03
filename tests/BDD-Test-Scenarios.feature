# Cenários de Testes BDD para o Desafio Técnico - Microserviços E-commerce

Feature: Sistema de Microserviços para E-commerce
  Como usuário do sistema de e-commerce
  Quero gerenciar produtos e vendas através de microserviços
  Para ter uma experiência escalável e robusta

  Background:
    Dado que o sistema de microserviços está rodando
    E os serviços de estoque e vendas estão disponíveis
    E o API Gateway está configurado
    E o RabbitMQ está ativo para comunicação entre serviços

  # ===============================================
  # CENÁRIOS DE AUTENTICAÇÃO JWT
  # ===============================================

  Scenario: Login com credenciais válidas
    Quando eu envio uma requisição POST para "/api/auth/login" com username "admin" e password "admin123"
    Então eu devo receber um status code 200
    E eu devo receber um token JWT válido
    E o token deve conter as informações do usuário

  Scenario: Login com credenciais inválidas
    Quando eu envio uma requisição POST para "/api/auth/login" com username "invalid" e password "wrong"
    Então eu devo receber um status code 401
    E eu devo receber uma mensagem de erro "Invalid credentials"

  Scenario: Acesso a endpoint protegido sem token
    Quando eu envio uma requisição GET para "/api/stock/products" sem token de autenticação
    Então eu devo receber um status code 401
    E eu devo receber uma mensagem de "Unauthorized"

  Scenario: Acesso a endpoint protegido com token expirado
    Dado que eu tenho um token JWT expirado
    Quando eu envio uma requisição GET para "/api/stock/products" com o token expirado
    Então eu devo receber um status code 401
    E eu devo receber uma mensagem de token expirado

  # ===============================================
  # CENÁRIOS DO MICROSERVIÇO DE ESTOQUE
  # ===============================================

  ## Cadastro de Produtos - Cenários Positivos
  Scenario: Cadastro de um novo produto com dados válidos
    Dado que eu estou autenticado como admin
    Quando eu envio uma requisição POST para "/api/stock/products" com os seguintes dados:
      | name        | description                    | price | stockQuantity |
      | Smartphone  | Smartphone Android com 128GB  | 1299.99 | 50          |
    Então eu devo receber um status code 201
    E o produto deve ser criado com um ID único
    E os dados do produto devem estar corretos

  Scenario: Consultar todos os produtos cadastrados
    Dado que existem produtos cadastrados no sistema
    E eu estou autenticado
    Quando eu envio uma requisição GET para "/api/stock/products"
    Então eu devo receber um status code 200
    E eu devo receber uma lista de produtos
    E cada produto deve conter id, name, description, price e stockQuantity

  Scenario: Consultar produto específico por ID válido
    Dado que existe um produto com ID 1
    E eu estou autenticado
    Quando eu envio uma requisição GET para "/api/stock/products/1"
    Então eu devo receber um status code 200
    E eu devo receber os dados corretos do produto

  Scenario: Verificar estoque suficiente para compra
    Dado que existe um produto com ID 1 e estoque de 50 unidades
    E eu estou autenticado
    Quando eu envio uma requisição GET para "/api/stock/products/1/stock/check/10"
    Então eu devo receber um status code 200
    E a resposta deve indicar que há estoque suficiente

  ## Cadastro de Produtos - Cenários Negativos

  Scenario: Tentar cadastrar produto com nome vazio
    Dado que eu estou autenticado como admin
    Quando eu envio uma requisição POST para "/api/stock/products" com name vazio
    Então eu devo receber um status code 400
    E eu devo receber uma mensagem de erro de validação para o campo "name"

  Scenario: Tentar cadastrar produto com preço negativo
    Dado que eu estou autenticado como admin
    Quando eu envio uma requisição POST para "/api/stock/products" com price "-100"
    Então eu devo receber um status code 400
    E eu devo receber uma mensagem de erro "Price must be greater than zero"

  Scenario: Tentar cadastrar produto com estoque negativo
    Dado que eu estou autenticado como admin
    Quando eu envio uma requisição POST para "/api/stock/products" com stockQuantity "-5"
    Então eu devo receber um status code 400
    E eu devo receber uma mensagem de erro "Stock quantity cannot be negative"

  Scenario: Consultar produto com ID inexistente
    Dado que não existe produto com ID 999
    E eu estou autenticado
    Quando eu envio uma requisição GET para "/api/stock/products/999"
    Então eu devo receber um status code 404
    E eu devo receber uma mensagem "Product not found"

  ## Casos Extremos - Estoque

  Scenario: Verificar estoque insuficiente para compra
    Dado que existe um produto com ID 1 e estoque de 5 unidades
    E eu estou autenticado
    Quando eu envio uma requisição GET para "/api/stock/products/1/stock/check/10"
    Então eu devo receber um status code 200
    E a resposta deve indicar que não há estoque suficiente

  Scenario: Verificar estoque com quantidade zero
    Dado que existe um produto com ID 1
    E eu estou autenticado
    Quando eu envio uma requisição GET para "/api/stock/products/1/stock/check/0"
    Então eu devo receber um status code 400
    E eu devo receber uma mensagem "Quantity must be greater than zero"

  Scenario: Cadastrar produto com descrição muito longa
    Dado que eu estou autenticado como admin
    Quando eu envio uma requisição POST para "/api/stock/products" com description de mais de 1000 caracteres
    Então eu devo receber um status code 400
    E eu devo receber uma mensagem de erro de validação para o campo "description"

  # ===============================================
  # CENÁRIOS DO MICROSERVIÇO DE VENDAS
  # ===============================================

  ## Criação de Pedidos - Cenários Positivos

  Scenario: Criar pedido com produtos em estoque
    Dado que eu estou autenticado como "user123"
    E existem produtos com estoque suficiente:
      | productId | quantity | availableStock |
      | 1         | 2        | 50            |
      | 2         | 1        | 25            |
    Quando eu envio uma requisição POST para "/api/sales/orders" com:
      | customerId | user123                    |
      | items      | [{"productId":1,"quantity":2},{"productId":2,"quantity":1}] |
    Então eu devo receber um status code 201
    E o pedido deve ser criado com status "Pending"
    E deve ser enviada uma mensagem via RabbitMQ para atualizar o estoque

  Scenario: Consultar todos os pedidos do usuário
    Dado que eu estou autenticado como "user123"
    E existem pedidos para o usuário "user123"
    Quando eu envio uma requisição GET para "/api/sales/orders/my-orders"
    Então eu devo receber um status code 200
    E eu devo receber uma lista com os pedidos do usuário

  Scenario: Consultar pedido específico por ID
    Dado que existe um pedido com ID 1 para o usuário autenticado
    Quando eu envio uma requisição GET para "/api/sales/orders/1"
    Então eu devo receber um status code 200
    E eu devo receber os dados completos do pedido

  ## Criação de Pedidos - Cenários Negativos

  Scenario: Tentar criar pedido com produto sem estoque suficiente
    Dado que eu estou autenticado
    E existe um produto com ID 1 e estoque de 2 unidades
    Quando eu envio uma requisição POST para "/api/sales/orders" com quantidade 5 do produto 1
    Então eu devo receber um status code 400
    E eu devo receber uma mensagem "Insufficient stock for product"

  Scenario: Tentar criar pedido com produto inexistente
    Dado que eu estou autenticado
    Quando eu envio uma requisição POST para "/api/sales/orders" com produto ID 999
    Então eu devo receber um status code 400
    E eu devo receber uma mensagem "Product not found"

  Scenario: Consultar pedido inexistente
    Dado que não existe pedido com ID 999
    E eu estou autenticado
    Quando eu envio uma requisição GET para "/api/sales/orders/999"
    Então eu devo receber um status code 404
    E eu devo receber uma mensagem "Order not found"

  Scenario: Tentar consultar pedido de outro usuário
    Dado que existe um pedido com ID 1 para usuário "other"
    E eu estou autenticado como "user123"
    Quando eu envio uma requisição GET para "/api/sales/orders/1"
    Então eu devo receber um status code 403
    E eu devo receber uma mensagem "Access denied"

  ## Casos Extremos - Vendas

  Scenario: Criar pedido sem itens
    Dado que eu estou autenticado
    Quando eu envio uma requisição POST para "/api/sales/orders" com lista de itens vazia
    Então eu devo receber um status code 400
    E eu devo receber uma mensagem "Order must contain at least one item"

  Scenario: Criar pedido com quantidade zero
    Dado que eu estou autenticado
    Quando eu envio uma requisição POST para "/api/sales/orders" com:
      | customerId | user123                    |
      | items      | [{"productId":1,"quantity":0}] |
    Então eu devo receber um status code 400
    E eu devo receber uma mensagem "Item quantity must be greater than zero"

  Scenario: Cancelar pedido pelo cliente
    Dado que existe um pedido com ID 1 com status "Pending" para o usuário autenticado
    Quando eu envio uma requisição PUT para "/api/sales/orders/1" com status "Cancelled"
    Então eu devo receber um status code 200
    E o status do pedido deve ser "Cancelled"
    E deve ser enviada mensagem via RabbitMQ para restaurar o estoque

  Scenario: Tentar atualizar pedido já processado
    Dado que existe um pedido com ID 1 e status "Shipped"
    Quando eu envio uma requisição PUT para "/api/sales/orders/1" com status "Cancelled"
    Então eu devo receber um status code 400
    E eu devo receber uma mensagem "Cannot modify shipped order"

  # ===============================================
  # CENÁRIOS DE COMUNICAÇÃO ENTRE MICROSERVIÇOS
  # ===============================================

  Scenario: Comunicação via RabbitMQ - Atualização de estoque após venda
    Dado que existe um produto com ID 1 e estoque de 10 unidades
    E o RabbitMQ está funcionando
    Quando um pedido é confirmado com 3 unidades do produto 1
    Então uma mensagem deve ser enviada via RabbitMQ
    E o serviço de estoque deve processar a mensagem
    E o estoque do produto 1 deve ser reduzido para 7 unidades

  Scenario: Falha na comunicação com RabbitMQ
    Dado que o RabbitMQ está indisponível
    Quando eu tento criar um pedido
    Então o pedido deve ser criado com status "Pending"
    E deve ser logado um erro de comunicação
    E o sistema deve continuar funcionando

  # ===============================================
  # CENÁRIOS DO API GATEWAY
  # ===============================================

  Scenario: Roteamento correto para microserviço de estoque
    Dado que eu estou autenticado
    Quando eu envio uma requisição GET para "/api/stock/products" via API Gateway
    Então a requisição deve ser roteada para o microserviço de estoque
    E eu devo receber a resposta correta

  Scenario: Roteamento correto para microserviço de vendas
    Dado que eu estou autenticado
    Quando eu envio uma requisição GET para "/api/sales/orders" via API Gateway
    Então a requisição deve ser roteada para o microserviço de vendas
    E eu devo receber a resposta correta

  Scenario: Rate limiting no API Gateway
    Dado que eu estou autenticado
    Quando eu envio mais de 100 requisições por minuto para qualquer endpoint
    Então algumas requisições devem ser bloqueadas com status code 429
    E eu devo receber uma mensagem "Rate limit exceeded"

  Scenario: Endpoint não encontrado no API Gateway
    Quando eu envio uma requisição GET para "/api/invalid/endpoint"
    Então eu devo receber um status code 404
    E eu devo receber uma mensagem "Route not found"

  Scenario: Microserviço indisponível
    Dado que o microserviço de estoque está indisponível
    Quando eu envio uma requisição GET para "/api/stock/products"
    Então eu devo receber um status code 502
    E eu devo receber uma mensagem "Service unavailable"

  # ===============================================
  # CENÁRIOS DE CASOS EXTREMOS E PERFORMANCE
  # ===============================================

  Scenario: Criação simultânea de pedidos para mesmo produto
    Dado que existe um produto com ID 1 e estoque de 5 unidades
    Quando 3 usuários tentam criar pedidos simultaneamente com 3 unidades cada
    Então apenas 1 pedido deve ser confirmado
    E os outros 2 devem receber erro de estoque insuficiente

  Scenario: Sistema sob alta carga
    Dado que o sistema está sob alta carga
    Quando múltiplos usuários fazem requisições simultaneamente
    Então o sistema deve manter a consistência dos dados
    E as respostas devem estar dentro do tempo limite aceitável

  # ===============================================
  # CENÁRIOS DE SEGURANÇA
  # ===============================================

  Scenario: Tentativa de SQL Injection
    Dado que eu estou autenticado
    Quando eu envio uma requisição com payload malicioso contendo SQL injection
    Então a requisição deve ser bloqueada
    E não deve afetar o banco de dados

  Scenario: Validação de autorização por role
    Dado que eu estou autenticado como "User"
    Quando eu tento acessar endpoint administrativo "/api/stock/products" com método POST
    Então eu devo receber um status code 403
    E eu devo receber uma mensagem "Insufficient permissions"

  Scenario: Token JWT com dados manipulados
    Dado que eu tenho um token JWT com dados alterados
    Quando eu uso este token para fazer uma requisição
    Então eu devo receber um status code 401
    E eu devo receber uma mensagem "Invalid token"
