# MeuBarbeiro

Projeto da disciplina LDAMD com arquitetura distribuida orientada a eventos para conectar clientes e barbeiros.

## Analise do PDF

O enunciado exige:

- dois perfis claros: cliente e prestador de servico;
- backend REST;
- persistencia em banco;
- middleware orientado a mensagens;
- aplicativo Flutter para cliente;
- aplicativo Flutter para prestador;
- fluxo assincrono real entre os componentes.

O dominio `MeuBarbeiro` atende integralmente a esses pontos:

- cliente busca barbearias por cidade;
- cliente consulta horarios disponiveis e solicita servicos;
- barbeiro recebe notificacao de nova solicitacao;
- barbeiro aceita ou recusa;
- cliente acompanha a mudanca de status;
- cliente avalia o atendimento ao final.

Observacao importante: o PDF cita Flask/Node.js como opcoes de backend, mas voce informou que o professor liberou outras linguagens. Por isso a estrutura abaixo foi preparada em `C# .NET`, mantendo os mesmos requisitos arquiteturais da disciplina.

## Estrutura do repositorio

```text
MeuBarbeiro/
├── backend/
│   ├── src/
│   │   ├── MeuBarbeiro.Api
│   │   ├── MeuBarbeiro.Application
│   │   ├── MeuBarbeiro.Contracts
│   │   ├── MeuBarbeiro.Domain
│   │   ├── MeuBarbeiro.Infrastructure
│   │   └── MeuBarbeiro.Worker
│   └── tests/
│       └── MeuBarbeiro.Api.Tests
├── docs/
├── infra/
├── mobile/
│   ├── apps/
│   │   ├── meu_barbeiro_cliente
│   │   └── meu_barbeiro_prestador
│   └── packages/
│       └── meu_barbeiro_core
├── infra/postman/
├── melos.yaml
└── MeuBarbeiro.sln
```

## Arquitetura proposta

- `MeuBarbeiro.Api`: expoe endpoints REST e orquestra casos de uso.
- `MeuBarbeiro.Application`: regras de aplicacao, interfaces e contratos internos.
- `MeuBarbeiro.Domain`: entidades e enums do negocio.
- `MeuBarbeiro.Infrastructure`: persistencia SQLite, RabbitMQ e implementacoes tecnicas.
- `MeuBarbeiro.Worker`: consumidor de eventos para processamento assincrono e notificacoes.
- `meu_barbeiro_cliente`: app do cliente.
- `meu_barbeiro_prestador`: app do barbeiro.
- `meu_barbeiro_core`: componentes compartilhados entre os apps Flutter.

## Fluxo principal

1. O cliente busca barbearias por cidade.
2. O cliente escolhe servicos e agenda um horario.
3. A API salva a solicitacao e publica `AppointmentRequested`.
4. O worker e o app do barbeiro consomem o evento de forma assincrona.
5. O barbeiro aceita ou recusa.
6. A API atualiza o status e publica `AppointmentStatusUpdated`.
7. O app do cliente atualiza a tela sem depender de acao manual.

## Roadmap por sprint

- Sprint 1: proposta do dominio, diagrama, CRUD REST, schema SQLite e colecao Postman.
- Sprint 2: RabbitMQ, produtores/consumidores, documentacao dos eventos e evidencia assincrona.
- Sprint 3: app Flutter do cliente com listagem, detalhes e solicitacao.
- Sprint 4: app Flutter do barbeiro, notificacoes e fluxo completo de ponta a ponta.

## Como subir a base

### Backend

```bash
dotnet restore
dotnet build
dotnet run --project backend/src/MeuBarbeiro.Api
```

### Worker

```bash
dotnet run --project backend/src/MeuBarbeiro.Worker
```

### RabbitMQ

```bash
docker compose -f infra/docker-compose.yml up -d
```

### RabbitMQ com Podman

```bash
podman machine init
podman machine start
podman compose -f infra/podman-compose.yml up -d
```

Ou, sem compose:

```bash
chmod +x infra/run-rabbitmq-podman.sh
./infra/run-rabbitmq-podman.sh
```

Mais detalhes em [docs/podman-rabbitmq.md](/Users/matheusfernandes/Documents/MeuBarbeiro/docs/podman-rabbitmq.md:1).

### Flutter

```bash
flutter pub get --directory mobile/apps/meu_barbeiro_cliente
flutter pub get --directory mobile/apps/meu_barbeiro_prestador
flutter run --project-dir mobile/apps/meu_barbeiro_cliente
flutter run --project-dir mobile/apps/meu_barbeiro_prestador
```
