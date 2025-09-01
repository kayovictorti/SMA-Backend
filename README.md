# SMA Monitoring

Sistema de Monitoramento de Ambientes (monorepo: backend .NET + frontend React).

Este README explica como configurar e executar o projeto localmente (backend e frontend) e descreve as escolhas de arquitetura e padrões de projeto adotados.

## Visão Geral
- Backend: ASP.NET Core (.NET 8), EF Core (SQLite), AutoMapper, Options + HttpClientFactory.
- Frontend: React + Vite + TailwindCSS + React Query + Axios.
- Testes: NUnit (via `dotnet test`) com Moq.

Estrutura resumida:
- `backend/` — API, camadas Application/Infrastructure/Domain.
- `frontend/` — SPA em React, Vite dev server com proxy para a API.
- `tests/` — testes de unidade da camada de aplicação.

## Pré‑requisitos
- .NET SDK 8
- Node.js 18+ (recomendado 20+)
- NPM (ou Yarn/Pnpm, se preferir)

Recomendado (para HTTPS local no ASP.NET):
- `dotnet dev-certs https --trust` (Windows/Mac).

## Backend (API)
Local: `backend/SMA.Api`

1) Configuração
- Arquivo de configuração principal: `backend/SMA.Api/appsettings.json`.
- Configuração de IoT (opcional):
  - `Iot.BaseUrl`: URL do serviço externo (mock/integração) para registrar/desregistrar devices.
  - `Iot.CallbackUrl`: URL pública do endpoint de eventos desta API, usado pelo serviço IoT para postar eventos (por padrão, `https://localhost:7006/api/events`).
- Banco de dados: SQLite. O caminho é resolvido para `backend/data/sma.db` automaticamente; diretórios são criados em tempo de execução.
- Migrations: executadas automaticamente no start.

2) Executando
- Via CLI: dentro de `backend/SMA.Api` execute:
  - `dotnet run --environment Production`
  - Endereços padrão (ver `launchSettings.json`/`appsettings.json`):
    - HTTPS: `https://localhost:7006`
    - HTTP: `http://localhost:5030`
- Swagger: disponível em `/swagger`

3) Observações
- Certifique-se de que o certificado de desenvolvimento HTTPS esteja confiável (veja pré‑requisitos) para evitar erros no proxy do frontend.

## Frontend (SPA)
Local: `frontend/`

1) Instale dependências
- `npm install`

2) Configure o proxy (se necessário)
- O Vite está configurado para proxy em `frontend/vite.config.ts` para `https://localhost:7006` (API). Se você alterar a porta/ambiente da API, ajuste a constante `target` nesse arquivo ou rode a API com HTTPS nessa URL.

3) Rode em desenvolvimento
- `npm run dev`
- Acesse o endereço indicado pelo Vite (por padrão, `http://localhost:5173`).

## Testes
- Na raiz ou em `tests/SMA.Tests`: `dotnet test`
- Os testes cobrem serviços da camada de aplicação usando mocks para isolar integrações externas (ex.: `IIotIntegrationClient`).

## Endpoints principais (resumo)
- `POST /api/devices` — cria um dispositivo; tenta registrar no IoT se configurado.
- `GET /api/devices` — lista dispositivos.
- `GET /api/devices/{id}` — obtém dispositivo por id.
- `PUT /api/devices/{id}` — atualiza dispositivo.
- `DELETE /api/devices/{id}` — exclui (soft delete) e tenta desregistrar no IoT.
- `POST /api/events` — ingere eventos (usado pelo IoT externo via `Iot.CallbackUrl`).

## Justificativa técnica e padrões

Arquitetura em camadas (inspirada em Clean Architecture):
- Domain: entidades e regras de negócio centrais (`SMA.Domain`). Mantém o modelo independente de frameworks.
- Application: casos de uso/serviços (`SMA.Application`). Contém DTOs, regras de orquestração e mapeamentos para o domínio. Facilita testabilidade.
- Infrastructure: persistência (EF Core/SQLite), repositórios e integrações externas (`SMA.Infrastructure`). Implementa interfaces da Application para repositórios e clientes HTTP.
- API: camada de apresentação HTTP (`SMA.Api`). Controllers finos que delegam para a Application.

Principais escolhas/padrões:
- Injeção de Dependência: serviços, repositórios e clientes HTTP registrados em `DependencyInjection.cs`. Promove baixo acoplamento e testabilidade.
- Options Pattern + HttpClientFactory: `IotOptions` e `IotIntegrationClient` usam `IOptions<T>` e HttpClient gerenciado pela factory, permitindo configuração centralizada, resiliente e facilmente mockável.
- Repositório + EF Core: repositórios para `Device` e `Event` encapsulam o `DbContext`. A camada Application não conhece EF diretamente, apenas interfaces. Facilita testes.
- Soft Delete global: `BaseEntity` define `DeletionDate`; filtros globais em `AppDbContext` escondem registros deletados sem perda física imediata.
- AutoMapper: mapeamentos entre Requests/DTOs/Entidades nas camadas API e Application reduzem boilerplate e evitam vazamento de modelos entre camadas.
- Observabilidade e tolerância a falhas: logs de aviso/erro nos pontos de integração (ex.: quando IoT falha ou não está configurado) para manter a experiência do usuário sem travar o fluxo principal.

Frontend (decisões):
- React + Vite: desenvolvimento rápido com HMR e configuração mínima.
- React Query: gerenciamento de cache de dados do backend (devices/events), simplificando fetching, invalidação e estados de carregamento/erro.
- TailwindCSS: produtividade em estilos utilitários e consistência visual simples.
- Axios + Proxy Vite: experiência local sem CORS, roteando `/api` para a API HTTPS.
- Tipos e validação: `zod` garante parse/validação dos dados recebidos do backend.

