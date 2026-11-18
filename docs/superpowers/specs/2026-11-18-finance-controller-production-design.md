# Finance Controller: design para publicar na web

Data: 2026-11-18. Status: rascunho para aprovação.

## Intenção e porquê

O Finance Controller nasceu como projeto de estudo. Agora ele precisa virar uma
vitrine de portfólio publicada na web, construída com padrão de produção, como se
usuários reais fossem guardar suas finanças nele. O dono quer aprender e mostrar
arquitetura limpa, testes, CI/CD e deploy, e não apenas "funcionar na minha máquina".

## Estado atual (verificado no código)

- Solution com 5 projetos: `Finance.Domain`, `Finance.Application`,
  `Finance.Infrastructure`, `Finance.API` (completo) e `Finance.Web` (template padrão
  do Razor Pages, `Program.cs` quebrada).
- API: controllers `Auth`, `Category`, `Income`, `Expense`. Auth por JWT em cookie
  (`HttpOnly`, `Secure`, `SameSite=Strict`, 1 hora). Senhas com BCrypt.
- Services genéricos (`BaseService`) e repositories genéricos (`BaseRepository`),
  FluentValidation, AutoMapper, `UnitOfWork`.
- Banco: SQL Server Express local, 3 migrations.
- Sem testes, sem Dockerfile, sem CI.

## Problemas encontrados que o projeto precisa resolver

1. **Dados sem dono.** `Moviment` não tem `UserId`. `GET /Income` devolve os dados de
   todos os usuários.
2. **Registro público escolhe o papel.** `UserCreateDto` aceita `role`, então qualquer
   pessoa se cadastra como `admin`.
3. **O token não carrega o id do usuário.** `JWTService` emite claims de email, papel e
   nome, sem identificador. Sem isso não há como filtrar por dono.
4. **`UpdateAsync` mapeia um DTO para uma entidade nova (`Id = 0`) e copia os valores
   com `SetValues`.** O resultado precisa ser verificado e coberto por teste; é um
   candidato a bug de chave.
5. **Segredos no git.** A chave JWT está em `appsettings.json` e há um fallback fixo em
   `Program.cs`.
6. **Nomes e tipos inconsistentes.** Campos em minúsculo em `Moviment` e `User`, e
   `IsAppellant`, cujo sentido não está documentado.

## Objetivos

- Aplicação pública, funcional, com HTTPS, usável por mais de uma pessoa sem vazar dados.
- Frontend React com o **nível 2** de funcionalidades (abaixo).
- PostgreSQL como banco.
- Pipeline que roda testes a cada push e publica a partir da `main`.
- Documentação e um diagrama do sistema.

## Não objetivos

- Redis, mensageria, Kubernetes, Terraform e Prometheus/Grafana (Sprint 10 do Roadmap).
- Exportar CSV, dark mode, metas por categoria (nível 3).
- App mobile nativo, múltiplas moedas, importação de extrato bancário.
- Login social (Google, GitHub).

## Anti-objetivos (seria um fracasso mesmo que "funcione")

- Um usuário enxergar ou alterar dados de outro.
- Segredo real no repositório ou no histórico futuro.
- Um deploy que só roda se alguém lembrar de um passo manual.

## Escopo funcional (nível 2)

1. Cadastro e login (cookie JWT já existente) e logout.
2. CRUD de categorias, receitas e despesas, todos restritos ao usuário logado.
3. Lista de movimentações com filtro por mês e por categoria.
4. Dashboard: saldo do mês, total de receitas, total de despesas, gráfico por categoria
   e gráfico mês a mês.

## Abordagens consideradas

| | Descrição | Troca |
|---|---|---|
| A | Razor/MVC no `Finance.Web`, um app só | Mais simples, mas dashboard e gráficos custam mais e rende menos no portfólio. |
| B | API e React em deploys separados | Desenho clássico, mas o cookie `SameSite=Strict` não vale entre domínios e exige trocar a autenticação ou configurar CORS e domínio. |
| C | React compilado e servido pela própria API, mesma origem | Cookie continua funcionando, um deploy só, mostra React. O Dockerfile tem dois estágios. |

**Escolhida pelo dono: C.**

## Decisões do dono

- Frontend: React (opção C).
- Banco: PostgreSQL.
- Escopo funcional: nível 2.
- Skill `impeccable` para o design do frontend e skill `diagram-design` para o
  diagrama do sistema, ambos já baixados de GitHub para `~/.claude/skills`.
- Contexto: vitrine de portfólio, construída com padrão de produção.

## Escolhas minhas (o dono pode corrigir)

- **Frontend:** Vite, React e TypeScript. Biblioteca de gráficos leve (Recharts).
  A lista de movimentações e os formulários consomem a API existente via `fetch` com
  `credentials: "same-origin"`.
- **Servir o frontend:** o build do Vite vai para `wwwroot` do `Finance.API`, com
  `UseDefaultFiles`, `UseStaticFiles` e fallback para `index.html`. O `Finance.Web` é
  removido da solution.
- **Banco:** Npgsql com EF Core. As migrations de SQL Server são descartadas e uma
  `InitialCreate` nova é gerada para Postgres. Como não há dados reais ainda, não há
  migração de dados.
- **Dono dos dados:** `Moviment` e `Category` ganham `UserId` (FK para `User`). O id do
  usuário entra no JWT como `ClaimTypes.NameIdentifier`. Services e repositories filtram
  por esse id. Acesso a um registro de outro usuário responde 404.
- **Registro:** o `role` deixa de vir do cliente. Todo cadastro público nasce como
  `user`. Admin só por seed ou por comando manual.
- **`IsAppellant`:** assumo que significa "movimentação recorrente" e renomeio para
  `IsRecurring`. Se o dono quis dizer outra coisa, corrige antes do plano.
- **Nomes:** propriedades de `Moviment` e `User` passam para PascalCase na mesma
  migration inicial.
- **Segredos:** a chave JWT e a connection string vêm de variáveis de ambiente. Sem
  fallback no código; a aplicação não inicia se a chave faltar ou for curta. A chave
  atual é considerada comprometida e é substituída.
- **Endpoints de agregação para o dashboard:** a API ganha um endpoint de resumo
  mensal (saldo, totais, soma por categoria, série dos últimos 12 meses) calculado no
  banco, para o frontend não baixar tudo.
- **Segurança básica de produção:** HTTPS e HSTS fora de Development, rate limiting no
  login, Swagger só em Development, `ExceptionHandlingMiddleware` sem stack trace em
  produção, `/health` para o orquestrador.
- **Testes:** xUnit. Unitários para services e validators com repositories simulados;
  integração dos controllers com `WebApplicationFactory` e Postgres de teste
  (Testcontainers). Os testes de isolamento entre usuários são obrigatórios.
- **CI/CD:** GitHub Actions: restore, build, teste e build do front em todo push;
  na `main`, build da imagem Docker e deploy.
- **Deploy:** container único (Dockerfile multi-estágio: Node para o front, SDK .NET
  para a API, runtime ASP.NET para rodar). Hospedagem no Fly.io ou Render, com
  PostgreSQL gerenciado no Neon ou Supabase (planos gratuitos). Migrations aplicadas
  no deploy por um passo explícito, não no startup da aplicação.
- **Design:** `/impeccable init` gera o `PRODUCT.md` do projeto; as telas são feitas
  com `craft` e revisadas com `critique` e `polish`.
- **Diagrama:** com `diagram-design`, três diagramas: arquitetura em camadas, modelo
  de dados (ER) e fluxo de deploy. Vão para `docs/architecture/`.
- **Idioma da interface:** português do Brasil.

## Restrições

- .NET 8, projetos existentes mantidos (arquitetura em camadas e SOLID do Roadmap).
- Sem custo recorrente: só planos gratuitos.
- O ambiente de desenvolvimento do agente é um container Linux; não há SQL Server aqui,
  o que reforça a troca para Postgres.

## O que fica para quem for construir

Estrutura de pastas do frontend, escolha de componentes e rotas, nomes dos endpoints de
resumo, formato exato dos DTOs de resumo, detalhes do Dockerfile e do workflow, política
de senha mínima, e a escolha final entre Fly.io e Render.

## Ordem sugerida de entrega

1. Fundação: segredos, Postgres, migration inicial, renomeações, `UserId`, correção do
   registro, testes de isolamento.
2. Endpoints de resumo e testes de integração.
3. Frontend com `impeccable`.
4. Dockerfile, CI/CD e primeiro deploy.
5. Diagramas e documentação final.
