#!/bin/bash

# Script para executar todos os microserviços do Desafio Avanade

echo "🚀 Iniciando Microserviços E-commerce"
echo "====================================="

# Função para verificar se uma porta está em uso
check_port() {
    if lsof -i :$1 >/dev/null 2>&1; then
        echo "⚠️  Porta $1 está em uso. Finalizando processo..."
        kill $(lsof -t -i:$1) 2>/dev/null || true
        sleep 2
    fi
}

# Limpar portas se estiverem em uso
check_port 5000
check_port 5001
check_port 5002

echo ""
echo "🔧 Restaurando dependências..."
dotnet restore

echo ""
echo "🏗️  Compilando solução..."
dotnet build

echo ""
echo "🧪 Executando testes..."
dotnet test

if [ $? -ne 0 ]; then
    echo "❌ Testes falharam. Abortando execução."
    exit 1
fi

echo ""
echo "✅ Testes passaram com sucesso!"
echo ""
echo "🌟 Iniciando microserviços..."
echo ""

# Abrir em segundo plano
echo "📊 Iniciando Stock Service (porta 5001)..."
cd src/StockService
ASPNETCORE_ENVIRONMENT=Development dotnet run --urls="https://localhost:5001;http://localhost:5051" &
STOCK_PID=$!
cd ../..

sleep 3

echo "🛒 Iniciando Sales Service (porta 5002)..."
cd src/SalesService
ASPNETCORE_ENVIRONMENT=Development dotnet run --urls="https://localhost:5002;http://localhost:5052" &
SALES_PID=$!
cd ../..

sleep 3

echo "🌐 Iniciando API Gateway (porta 5000)..."
cd src/ApiGateway
ASPNETCORE_ENVIRONMENT=Development dotnet run --urls="https://localhost:5000;http://localhost:5050" &
GATEWAY_PID=$!
cd ../..

# Aguardar um pouco para os serviços iniciarem
sleep 5

echo ""
echo "🎉 Todos os serviços estão executando!"
echo ""
echo "📋 URLs disponíveis:"
echo "   🌐 API Gateway:    https://localhost:5000/swagger"
echo "   📊 Stock Service:  https://localhost:5001/swagger"
echo "   🛒 Sales Service:  https://localhost:5002/swagger"
echo ""
echo "🔑 Credenciais de teste:"
echo "   Admin: admin/admin123"
echo "   Cliente: customer1/customer123"
echo ""
echo "⏹️  Pressione Ctrl+C para parar todos os serviços"

# Função para parar os serviços
cleanup() {
    echo ""
    echo "🛑 Parando serviços..."
    kill $GATEWAY_PID 2>/dev/null || true
    kill $SALES_PID 2>/dev/null || true
    kill $STOCK_PID 2>/dev/null || true
    echo "✅ Todos os serviços foram parados."
    exit 0
}

# Capturar Ctrl+C
trap cleanup INT

# Aguardar indefinidamente
wait
