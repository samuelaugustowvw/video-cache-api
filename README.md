# Video Cache API

API REST em .NET 8 que guarda e recupera URLs de vídeos usando Redis como cache. Roda inteira em containers Docker e está publicada na AWS.

O escopo é pequeno de propósito (dois endpoints), então o foco aqui foi fazer o básico bem feito: código organizado, containers configurados com cuidado e um deploy que qualquer pessoa consegue reproduzir.

## 🔗 Aplicação no ar

- API: http://52.67.37.59
- Swagger: http://52.67.37.59/swagger
- Health check: http://52.67.37.59/health/ready

> Use `http://`, não `https://` — o ambiente não tem certificado SSL configurado.

## 🏗️ Arquitetura

```
                 Internet
                     │  HTTP :80
        ┌────────────▼──────────────────┐
        │   AWS EC2 (t3.micro)          │
        │  ┌──────────────────────────┐ │
        │  │  rede docker interna     │ │
        │  │  ┌────────┐  ┌────────┐  │ │
        │  │  │  API   │──│ Redis  │  │ │
        │  │  │ :8080  │  │ :6379  │  │ │
        │  │  └────────┘  └────────┘  │ │
        │  └──────────────────────────┘ │
        └───────────────────────────────┘
```

A API e o Redis rodam como containers separados na mesma instância EC2, conversando por uma rede interna do Docker. O Redis não abre porta nenhuma para fora — quem chega pela internet só alcança a API, na porta 80.

## Organização do código

É um projeto único de ASP.NET Core Web API. Para um domínio deste tamanho, dividir em vários projetos seria mais atrapalho do que ajuda, então preferi separar as responsabilidades por pasta:

| Pasta | O que tem lá |
|---|---|
| `Controllers` | Recebem o HTTP e devolvem a resposta |
| `Services` | A lógica, entre o controller e o Redis |
| `Repositories` | Acesso ao Redis (StackExchange.Redis) |
| `Models` | Os DTOs de entrada e saída |
| `Middlewares` | Tratamento global de exceções |

O fluxo é sempre Controller → Service → Repository. Mesmo num projeto só, manter essa separação é o que permite testar a lógica sem precisar de um Redis rodando.

## 🛠️ Stack

C# / .NET 8, ASP.NET Core Web API, Redis 7, StackExchange.Redis, Docker e Docker Compose, xUnit + Moq nos testes, GitHub Actions para CI e AWS EC2 para o deploy.

## 📌 Endpoints

**POST /api/cache** — guarda uma URL

```json
{
  "id": "video-001",
  "url": "https://youtube.com/watch?v=abc123"
}
```

Retorna `201` quando grava, `400` se o corpo vier inválido (id vazio, url malformada).

**GET /api/cache/{id}** — recupera a URL

Retorna `200` com os dados se existir, `404` se não existir.

**GET /health/live** e **GET /health/ready** — checagens de saúde. O `live` diz se o processo está de pé; o `ready` também verifica se o Redis está respondendo.

## ▶️ Rodando localmente

Só precisa do Docker instalado.

```bash
git clone https://github.com/samuelaugustowvw/video-cache-api.git
cd video-cache-api
cp .env.example .env
docker compose up --build -d
```

Pronto, abre em http://localhost:8080/swagger.

Para testar na mão:

```bash
curl -X POST http://localhost:8080/api/cache \
  -H "Content-Type: application/json" \
  -d '{"id":"video-001","url":"https://youtube.com/watch?v=abc123"}'

curl http://localhost:8080/api/cache/video-001
```

Se quiser espiar o dado direto no Redis:

```bash
docker compose exec redis redis-cli GET video:video-001
```

E os testes:

```bash
dotnet test
```

## ⚙️ Variáveis de ambiente

Nada de config sensível fica no código. O que muda entre ambientes vem por variável:

| Variável | Para que serve | Padrão |
|---|---|---|
| `ConnectionStrings__Redis` | endereço do Redis | `redis:6379` |
| `API_PORT` | porta exposta no host | `8080` |
| `ASPNETCORE_ENVIRONMENT` | ambiente da aplicação | `Production` |

## 🧠 Algumas decisões e o porquê

**Projeto único em vez de camadas.** São dois endpoints. Montar Clean Architecture aqui seria over-engineering — a separação por pastas já dá organização suficiente sem espalhar o código em quatro projetos.

**Conexão do Redis como singleton.** O `ConnectionMultiplexer` da StackExchange.Redis é feito para ser criado uma vez e reaproveitado. Criar uma conexão por requisição é o jeito clássico de esgotar sockets e derrubar a API sob carga.

**`AbortOnConnectFail = false`.** No Docker a API às vezes sobe antes do Redis. Sem essa config, ela morreria no startup e o container entraria em loop de reinício. Com ela, a API sobe e reconecta sozinha quando o Redis aparece.

**Prefixo `video:` nas chaves.** O Redis não tem tabelas, é tudo um espaço só de chaves. O prefixo funciona como namespace e evita colisão caso o projeto passe a guardar outros tipos de dado.

**Dockerfile multi-stage.** Um estágio compila (com o SDK, ~800MB), o outro só roda (com o runtime, ~220MB). A imagem final não carrega compilador nem código-fonte — fica menor e com menos superfície de ataque. O container também roda como usuário sem privilégios, não como root.

**EC2 + Docker Compose em vez de ECS/Fargate.** Fargate não entra no Free Tier. Com EC2 rodando o mesmo compose do ambiente local, o custo fica zero e o que roda na nuvem é idêntico ao que roda na minha máquina.

**Redis sem porta exposta.** Um detalhe fácil de esquecer: publicar a 6379 deixaria um Redis sem senha aberto para a internet. Aqui ele só existe na rede interna do Docker.

**Swagger ligado em produção.** Normalmente eu desligaria, mas deixei ligado de propósito para facilitar a avaliação — dá pra testar os endpoints direto pelo navegador. Num cenário real ficaria restrito a ambientes internos.

## 🚀 CI

Tem um workflow no GitHub Actions que, a cada push na `main`, faz restore, build, roda os testes e valida que a imagem Docker sobe. Deixei o deploy fora do pipeline de propósito — é uma instância única e o deploy é manual e pontual, então automatizar isso não compensava o risco de guardar credenciais da AWS no repositório.
