# Sprint 2 - Evidencia de Execucao

## Subida do RabbitMQ

```bash
docker run -d --hostname rabbit --name rabbitmq \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:3-management
```

Painel web:

- URL: `http://localhost:15672`
- Usuario: `guest`
- Senha: `guest`

## Subida da API

```bash
dotnet run --project backend/src/MeuBarbeiro.Api
```

## Subida do Worker

```bash
dotnet run --project backend/src/MeuBarbeiro.Worker
```

## Fluxo para demonstracao

### 1. Criar uma solicitacao

Executar `POST /api/v1/appointment`.

Evidencia esperada:

- API persiste no SQLite
- API publica `AppointmentRequestedIntegrationEvent`
- fila `appointments.requested` recebe a mensagem
- worker consome e grava auditoria

### 2. Atualizar o status

Executar `PATCH /api/v1/appointment/{id}/status`.

Evidencia esperada:

- API atualiza no SQLite
- API publica `AppointmentStatusUpdatedIntegrationEvent`
- fila `appointments.status-updated` recebe a mensagem
- worker consome e grava auditoria

## Onde comprovar

- logs da API
- logs do worker
- painel RabbitMQ
- tabela `EventProcessingAudits` no SQLite
