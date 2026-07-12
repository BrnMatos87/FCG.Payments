# FCG.Payments

Microsserviço responsável pelo processamento dos pagamentos da plataforma FIAP Cloud Games.

## Responsabilidades

- Processamento de pagamentos
- Registro das transações
- Atualização do status do pagamento
- Publicação de eventos de pagamento
- Consumo de eventos de pedidos

## Arquitetura

```
FCG.Payments
├── src
│   ├── FCG.Payments.Api
│   ├── FCG.Payments.Application
│   ├── FCG.Payments.Domain
│   ├── FCG.Payments.Infrastructure
│   └── FCG.Payments.Worker
├── tests
├── k8s
├── Dockerfile.Api
├── Dockerfile.Worker
├── docker-compose.yml
├── docker-compose.full.yml
├── NuGet.config
└── README.md
```

## Camadas

### API

- Controllers
- Swagger
- Autenticação JWT
- Endpoints HTTP

### Application

- Commands
- Handlers
- DTOs
- Casos de uso
- Interfaces

### Domain

- Entidades
- Regras de negócio
- Objetos de Valor
- Interfaces

### Infrastructure

- Entity Framework Core
- SQL Server
- RabbitMQ
- MassTransit
- Repositórios
- Migrations

### Worker

- Consumo de eventos
- Processamento assíncrono
- Publicação de eventos
- Integração entre microsserviços

---

# Tecnologias

- .NET 8
- ASP.NET Core
- Worker Service
- Entity Framework Core
- SQL Server
- RabbitMQ
- MassTransit
- Docker
- Kubernetes
- Swagger
- xUnit

---

# Dependência Compartilhada

```xml
<PackageReference Include="FCG.BuildingBlocks" Version="1.0.1" />
```

---

# Execução Local

Restore

```powershell
dotnet restore --configfile .\NuGet.config
```

Build

```powershell
dotnet build
```

Testes

```powershell
dotnet test
```

Executar API

```powershell
dotnet run --project .\src\FCG.Payments.Api
```

Executar Worker

```powershell
dotnet run --project .\src\FCG.Payments.Worker
```

---

# Docker

## API

Build

```powershell
docker build -f Dockerfile.Api -t brnmatos/fcg-payments-api:1.0.1 .
```

Executar

```powershell
docker run -p 5003:8080 brnmatos/fcg-payments-api:1.0.1
```

---

## Worker

Build

```powershell
docker build -f Dockerfile.Worker -t brnmatos/fcg-payments-worker:1.0.1 .
```

Executar

```powershell
docker run brnmatos/fcg-payments-worker:1.0.1
```

---

# Docker Hub

```powershell
docker push brnmatos/fcg-payments-api:1.0.1
docker push brnmatos/fcg-payments-worker:1.0.1
```

---

# Docker Compose

## Infraestrutura

```powershell
docker compose up -d
```

## Ambiente Completo

```powershell
docker compose -f docker-compose.full.yml up -d --build
```

---

# Kubernetes

## Estrutura

```
k8s
├── namespace.yaml
├── configmap.yaml
├── secret.yaml
├── sqlserver.yaml
├── rabbitmq.yaml
├── api-deployment.yaml
├── api-service.yaml
├── worker-deployment.yaml
```

---

## Deploy

```powershell
kubectl apply -f .\k8s\namespace.yaml
kubectl apply -f .\k8s\
```

---

## Logs

API

```powershell
kubectl logs -f deployment/fcg-payments-api -n fcg
```

Worker

```powershell
kubectl logs -f deployment/fcg-payments-worker -n fcg
```

---

## Swagger

Consultar serviço

```powershell
kubectl get svc payments-api -n fcg
```

Acesso

```
http://localhost:5003/swagger
```

ou

```
http://<EXTERNAL-IP>:5003/swagger
```

---

## SQL Server

```powershell
kubectl port-forward service/payments-sqlserver 1438:1433 -n fcg
```

SQL Server Management Studio

```
Servidor:
localhost,1438
```

---

## RabbitMQ

```powershell
kubectl port-forward service/rabbitmq 15672:15672 -n fcg
```

```
http://localhost:15672
```

---
## Eventos

### Consumidos

- OrderPlacedEvent

### Publicados

- PaymentProcessedEvent

# Fluxo

```
Catalog API
      │
      ▼
OrderPlacedEvent
      │
      ▼
RabbitMQ
      │
      ▼
Payments Worker
      │
      ▼
Processamento do pagamento
      │
      ▼
PaymentProcessedEvent
      │
      ▼
RabbitMQ
      │
      ▼
Notifications Worker
```

---

# Comunicação

- payments-api
- payments-sqlserver
- rabbitmq

---

# Segurança

- ConfigMap para configurações
- Secret para credenciais
- SQL Server ClusterIP
- RabbitMQ ClusterIP
- API LoadBalancer

---

# CI/CD

```
Restore
   ↓
Build
   ↓
Tests
   ↓
Docker Build
   ↓
Docker Push
   ↓
Kubernetes
```

---

# Troubleshooting

Verificar Pods

```powershell
kubectl get pods -n fcg
```

Verificar Deployments

```powershell
kubectl get deployments -n fcg
```

Verificar Services

```powershell
kubectl get services -n fcg
```

Descrever Pod

```powershell
kubectl describe pod <pod-name> -n fcg
```

Logs da API

```powershell
kubectl logs -f deployment/fcg-payments-api -n fcg
```

Logs do Worker

```powershell
kubectl logs -f deployment/fcg-payments-worker -n fcg
```

---

# Autor

**Bruno Matos**

Pós-graduação FIAP - Tech Challenge