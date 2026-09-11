# FCG.Payments

Microsserviço .NET 8 responsável pelo processamento de pagamentos da FIAP Cloud Games.

## Arquitetura atual

```text
Catalog API
  -> OrderPlacedEvent
  -> RabbitMQ
  -> Payments Worker
     |-> SQL Server
     |-> PaymentProcessedEvent -> RabbitMQ -> Catalog Worker
     `-> HTTP POST -> Azure Function FCG.Notifications

Cliente -> Kong -> Payments API -> SQL Server
```

RabbitMQ continua necessário entre Catalog e Payments. A integração de Payments com Notifications não usa RabbitMQ: ela envia uma requisição HTTP para a Azure Function.

## Responsabilidades

- consumir `OrderPlacedEvent` pelo Payments Worker;
- processar e registrar pagamentos no SQL Server exclusivo do serviço;
- publicar `PaymentProcessedEvent` no RabbitMQ para o Catalog;
- enviar o mesmo contrato necessário para Notifications via HTTP;
- disponibilizar consulta de pagamento pela API.

## Endpoint

| Método | Rota | Acesso integrado |
|---|---|---|
| GET | `/api/payments/orders/{orderId}` | protegido por JWT no Kong e na API |

## Mensageria com Catalog

```text
Fila de entrada: payments-order-placed
Evento consumido: OrderPlacedEvent
Evento publicado: PaymentProcessedEvent
Destino consumidor: Catalog Worker
```

Principais configurações:

- `RabbitMq__Host`
- `RabbitMq__Port`
- `RabbitMq__VirtualHost`
- `RabbitMq__Username`
- `RabbitMq__Password`
- `RabbitMq__OrderPlacedQueue`

Esse fluxo permanece assíncrono e não deve ser removido ao configurar Notifications.

## Integração HTTP com Notifications

Após processar o pagamento, Payments executa:

```text
POST {Notifications__BaseUrl}/api/notifications/payment-processed
x-functions-key: {Notifications__FunctionKey}  # quando configurada
Body: PaymentProcessedEvent em JSON
```

O cliente é registrado com `IHttpClientFactory`. A resposta HTTP é validada, e falhas são registradas e propagadas ao fluxo chamador. Uma política de retry, quando desejada, deve ser configurada explicitamente no host/consumer.

Configurações:

| Variável | Finalidade | Sensível |
|---|---|---|
| `Notifications__BaseUrl` | URL base da Azure Function | não |
| `Notifications__FunctionKey` | Function Key enviada em `x-functions-key` | sim |

Essas configurações existem na API e no Worker porque ambos registram a infraestrutura compartilhada. No fluxo principal, a chamada ocorre durante o processamento iniciado pelo Worker.

Para desenvolvimento com a Function executando no host:

- aplicação local: `http://localhost:7071`;
- container: `http://host.docker.internal:7071`.

No Kubernetes integrado, use a URL pública/privada alcançável da Azure Function. Não utilize `fcg-notifications-functions`.

## JWT e API Gateway

A API valida tokens emitidos por Users:

```text
Issuer: FCG.Users.Api
Audience: FCG.CloudGames
Algorithm: HS256
```

Na solução integrada, o Kong é o único ponto de entrada externo e protege `/api/payments`. A configuração declarativa oficial fica em `FCG.Orchestration/kong/kong.yml`.

## Configuração

Principais variáveis:

- `ConnectionStrings__DefaultConnection`
- `Jwt__SecretKey`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__ExpirationMinutes`
- configurações `RabbitMq__*`
- `Notifications__BaseUrl`
- `Notifications__FunctionKey`

Não versione connection strings, credenciais do RabbitMQ, JWT secret nem Function Key.

## Execução local

```powershell
dotnet restore --configfile .\NuGet.config
dotnet build
dotnet test
dotnet run --project .\src\FCG.Payments.Api
dotnet run --project .\src\FCG.Payments.Worker
```

Swagger local:

```text
http://localhost:5003/swagger
```

## Docker

```powershell
docker build -f Dockerfile.Api -t brnmatos/fcg-payments-api:1.0.2 .
docker build -f Dockerfile.Worker -t brnmatos/fcg-payments-worker:1.0.2 .
```

O `docker-compose.full.yml` permite desenvolvimento isolado com API, Worker, SQL Server e RabbitMQ. A Function deve estar disponível no host ou na URL configurada.

Para a solução completa da Fase 3, use o Compose do `FCG.Orchestration`. Nele, Payments API é interna e o Kong recebe o tráfego externo.

## Kubernetes

Os manifests locais suportam execução isolada. Na arquitetura integrada, use o Kustomize do `FCG.Orchestration`, que configura:

- `payments-api` como `ClusterIP` na porta `8080`;
- Payments API e Worker com requests/limits;
- probes na API;
- SQL Server e RabbitMQ com armazenamento persistente;
- URL da Function em ConfigMap;
- Function Key, JWT, banco e RabbitMQ em Secret não versionado;
- Kong como único `LoadBalancer` das APIs.

Exemplo de implantação integrada:

```powershell
Set-Location ..\FCG.Orchestration
Copy-Item k8s/shared-secret.example.yaml k8s/shared-secret.yaml
kubectl kustomize .
kubectl apply -k .
```

## Fluxo completo

```text
1. Catalog publica OrderPlacedEvent no RabbitMQ.
2. Payments Worker consome o pedido e registra o pagamento.
3. Payments publica PaymentProcessedEvent no RabbitMQ para Catalog.
4. Payments envia POST /api/notifications/payment-processed para a Azure Function.
5. Catalog Worker atualiza a biblioteca do usuário.
6. Notifications processa a confirmação de pagamento.
```

## Relação com os requisitos da Fase 3

- microsserviço containerizado com API e Worker independentes;
- acesso externo centralizado no Kong;
- RabbitMQ preservado apenas no fluxo de negócio assíncrono;
- Notifications desacoplada do broker e executada como Azure Function HTTP Trigger;
- configurações sensíveis separadas das configurações públicas.

Na stack de observabilidade escolhida para a Fase 3, a instrumentação obrigatória é Users e Catalog. Payments não é target do Prometheus nesta opção; isso não impede instrumentação futura.
