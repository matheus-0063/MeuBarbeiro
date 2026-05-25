# Sprint 2 - Documentacao dos Eventos

## Visao geral

O sistema MeuBarbeiro utiliza `RabbitMQ` como middleware orientado a mensagens para garantir comunicacao assincrona entre o backend REST e o processo consumidor (`MeuBarbeiro.Worker`).

A topologia utilizada foi:

- Exchange: `meu-barbeiro.events`
- Tipo: `direct`
- Fila 1: `appointments.requested`
- Fila 2: `appointments.status-updated`

## Tabela de eventos

| Evento | Produtor | Consumidor | Exchange | Routing Key | Fila | Momento do fluxo |
| --- | --- | --- | --- | --- | --- | --- |
| `AppointmentRequestedIntegrationEvent` | `MeuBarbeiro.Api` / `AppointmentService` | `MeuBarbeiro.Worker` | `meu-barbeiro.events` | `appointments.requested` | `appointments.requested` | Quando o cliente cria uma solicitacao |
| `AppointmentStatusUpdatedIntegrationEvent` | `MeuBarbeiro.Api` / `AppointmentService` | `MeuBarbeiro.Worker` | `meu-barbeiro.events` | `appointments.status-updated` | `appointments.status-updated` | Quando o barbeiro atualiza o status da solicitacao |

## Payloads JSON

### 1. AppointmentRequestedIntegrationEvent

```json
{
  "appointmentId": "6633f74e-4639-4db8-bd07-e305956ba038",
  "clientId": "7f954a7a-fb61-4af6-b4b4-6915204887d2",
  "barberId": "3b5996f8-8e41-4aa1-a7e5-1d7ebcb64527",
  "barbershopId": "ca0242dd-a336-4627-9265-203c19004e1d",
  "scheduledAtUtc": "2026-05-25T18:00:00Z",
  "totalPrice": 55.0
}
```

### 2. AppointmentStatusUpdatedIntegrationEvent

```json
{
  "appointmentId": "6633f74e-4639-4db8-bd07-e305956ba038",
  "barberId": "3b5996f8-8e41-4aa1-a7e5-1d7ebcb64527",
  "status": "Accepted",
  "updatedAtUtc": "2026-05-25T18:05:00Z"
}
```

## Evidencia de processamento

O consumidor persiste uma auditoria de processamento no SQLite, na tabela `EventProcessingAudits`, contendo:

- nome do evento
- fila consumida
- payload recebido
- horario de processamento
- status (`Processed` ou `Failed`)
- mensagem de erro, quando aplicavel

Essa tabela serve como evidencia objetiva da comunicacao assincrona real no fluxo.
