#!/usr/bin/env bash
set -euo pipefail

CONTAINER_NAME="meu-barbeiro-rabbitmq"
IMAGE_NAME="docker.io/library/rabbitmq:3-management"

if ! command -v podman >/dev/null 2>&1; then
  echo "Podman nao encontrado no PATH."
  exit 1
fi

if podman container exists "${CONTAINER_NAME}"; then
  echo "Container ${CONTAINER_NAME} ja existe. Iniciando..."
  podman start "${CONTAINER_NAME}"
  exit 0
fi

echo "Criando container ${CONTAINER_NAME}..."
podman run -d \
  --name "${CONTAINER_NAME}" \
  -p 5672:5672 \
  -p 15672:15672 \
  -e RABBITMQ_DEFAULT_USER=guest \
  -e RABBITMQ_DEFAULT_PASS=guest \
  "${IMAGE_NAME}"

echo "RabbitMQ iniciado."
echo "AMQP: http://localhost:5672"
echo "Painel: http://localhost:15672"
