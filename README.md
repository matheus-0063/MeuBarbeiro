# MeuBarbeiro

Sistema de agendamento para barbearias. O repositório mantém o backend em C#/.NET e agora possui apenas o esqueleto vazio para um frontend web em React.

Os antigos aplicativos Flutter, o pacote compartilhado e a configuração Melos foram removidos.

## Estrutura

~~~text
MeuBarbeiro/
├── backend/                   # API, domínio, infraestrutura e testes .NET
├── frontend/                  # Local reservado ao futuro frontend React
│   ├── public/                # Arquivos estáticos públicos
│   └── src/
│       ├── app/               # Composição da aplicação
│       ├── components/        # Componentes visuais reutilizáveis
│       ├── features/          # Funcionalidades por domínio
│       ├── pages/             # Telas e rotas
│       ├── services/          # Integrações, como API HTTP
│       ├── styles/            # Estilos globais e tema
│       └── types/             # Tipos TypeScript compartilhados
├── collections/               # Coleções para testar a API
├── docs/                      # Documentação do projeto
├── infra/                     # Infraestrutura local
└── MeuBarbeiro.sln
~~~

## Backend

~~~bash
dotnet restore MeuBarbeiro.sln
dotnet run --project backend/src/MeuBarbeiro.Api --launch-profile http
~~~

A API local inicia em http://localhost:5039. O Swagger fica em http://localhost:5039/swagger.

## Quando iniciar o React

Crie o projeto React dentro de frontend e use as pastas existentes desde o primeiro componente. A divisão e a ordem recomendada de construção estão explicadas na conversa deste task.
