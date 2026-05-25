# Sprint 2 - Relatorio de Integracao

## 1. Ferramenta de MOM escolhida

Para a Sprint 2 foi utilizado o `RabbitMQ` como middleware orientado a mensagens. A escolha foi motivada por tres fatores principais:

- facilidade de configuracao local;
- boa aderencia ao modelo de filas e roteamento por exchange;
- compatibilidade com o requisito da disciplina de demonstrar comunicacao assincrona real entre produtor e consumidor.

## 2. Padrao adotado

Foi adotada uma arquitetura baseada em `exchange direct`, com duas filas independentes para eventos distintos do dominio:

- `appointments.requested`
- `appointments.status-updated`

Esse desenho permite separar claramente os dois momentos principais do negocio:

1. criacao da solicitacao pelo cliente;
2. atualizacao de status da solicitacao pelo barbeiro.

## 3. Integracao implementada

O backend REST atua como produtor de eventos por meio da `AppointmentService`. Quando uma solicitacao e criada ou tem seu status alterado, a service publica um evento no RabbitMQ. O `MeuBarbeiro.Worker` atua como consumidor, processando as mensagens de forma assincrona e persistindo registros de auditoria no banco SQLite.

Com isso, o sistema deixa de depender apenas de chamadas REST sincronas e passa a demonstrar um fluxo assíncrono real entre componentes do backend.

## 4. Dificuldades encontradas

A principal dificuldade tecnica foi a evolucao do schema do SQLite apos a adicao da tabela `EventProcessingAudits`, pois o banco local ja existia desde a Sprint 1. Como `EnsureCreated()` nao atualiza tabelas em bancos ja existentes, foi necessario adicionar uma etapa explicita de garantia de schema para criar a tabela de auditoria quando ausente.

Outro ponto de atencao foi a configuracao do consumidor com `ack` e `nack`, evitando que falhas silenciosas mascarassem o processamento das mensagens.

## 5. Resultado final

Ao final da integracao, o projeto passou a possuir:

- produtor de eventos implementado;
- consumidor assincrono implementado;
- topologia RabbitMQ configurada;
- auditoria persistida no SQLite;
- evidencias em logs e banco para demonstrar o fluxo assincrono.

Isso atende aos objetivos centrais da Sprint 2 e prepara a base para a notificacao e sincronizacao com os aplicativos Flutter nas sprints seguintes.
