# RabbitMQ com Podman

Este projeto pode usar `Podman` no lugar de Docker para subir o broker `RabbitMQ` da Sprint 2.

## Arquivos

- `infra/podman-compose.yml`: compose voltado para uso com `podman compose`
- `infra/run-rabbitmq-podman.sh`: script simples usando `podman run`

## Opcao 1: Podman Compose

No macOS, normalmente o primeiro passo e preparar a VM do Podman:

```bash
podman machine init
podman machine start
```

Depois suba o RabbitMQ:

```bash
podman compose -f infra/podman-compose.yml up -d
```

Para parar:

```bash
podman compose -f infra/podman-compose.yml down
```

## Opcao 2: Podman Run

Se preferir nao depender de compose:

```bash
chmod +x infra/run-rabbitmq-podman.sh
./infra/run-rabbitmq-podman.sh
```

Para parar e remover manualmente:

```bash
podman stop meu-barbeiro-rabbitmq
podman rm meu-barbeiro-rabbitmq
```

## Como validar

Depois de subir o container:

- AMQP: `localhost:5672`
- Painel web: `http://localhost:15672`
- usuario: `guest`
- senha: `guest`

Comandos uteis:

```bash
podman ps
podman logs meu-barbeiro-rabbitmq
```

## Integracao com o projeto

A API e o worker ja estao configurados para usar:

- host: `localhost`
- porta: `5672`
- usuario: `guest`
- senha: `guest`

Isso significa que, se o RabbitMQ estiver rodando no Podman, a configuracao atual do projeto continua funcionando sem ajustes adicionais.

## Observacao

Neste ambiente de desenvolvimento do Codex, o binario `podman` nao estava instalado no momento da configuracao. Por isso, os arquivos foram preparados, mas a execucao do Podman nao foi validada localmente aqui.
