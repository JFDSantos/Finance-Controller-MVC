# Finance Controller: produção Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Publicar o Finance Controller na web com Postgres, dados isolados por usuário, frontend React (nível 2) e CI/CD.

**Architecture:** Mantém as 4 camadas .NET 8 (Domain, Application, Infrastructure, API). `Finance.API` passa a servir o build do React (`frontend/`) como arquivos estáticos na mesma origem e expõe a API em `/api/*`. O isolamento por usuário vem de `ICurrentUser` + filtro global do EF + checagens explícitas.

**Tech Stack:** .NET 8, EF Core 9 + Npgsql, FluentValidation, AutoMapper, xUnit + Moq + `WebApplicationFactory`, Vite + React + TypeScript + Recharts + Vitest, Docker, GitHub Actions, Fly.io, Neon.

**Spec:** `docs/superpowers/specs/2026-11-18-finance-controller-production-design.md`

## Global Constraints

- .NET `net8.0` em todos os projetos; pacotes EF Core na versão `9.0.10` já usada.
- Jwt: `Jwt:Key` vem de variável de ambiente (`Jwt__Key`), mínimo 32 caracteres, sem fallback; a API não inicia sem ela. Expiração do token e do cookie: 8 horas, sem renovação.
- Cookie: `jwt`, `HttpOnly`, `Secure`, `SameSite=Strict`.
- Rotas da API sob `/api/[controller]`. Escritas exigem `Content-Type: application/json` (senão 415).
- Erros da API em inglês com `code` estável; o frontend traduz para pt-BR. A interface é pt-BR.
- `Value`: sempre positivo, `decimal(18,2)`. `MovimentDate`: `DateOnly` (coluna `date`). `IsAppellant` vira `IsRecurring` (só uma flag).
- `EMovimentType.Base` removido; ficam `Income = 1` e `Expense = 2`.
- Propriedades de `Moviment`, `User`, `Category` em PascalCase. Todo cadastro público nasce com `EUserRole.user`; o cliente não escolhe o papel.
- Categorias por usuário, nome único por usuário (índice em `UserId` + `Name`); excluir categoria em uso retorna 409 (`CATEGORY_IN_USE`).
- Rate limit do login: 5 tentativas por minuto por IP.
- Migrations usam connection string direta (`DATABASE_URL_DIRECT`); o app usa a string com pooler. Sem Redis, Kubernetes, Terraform, CSV, dark mode ou metas.
- Sem reescrever histórico do git; nenhum segredo real commitado.

## Review Focus

- Usuário B cria movimentação com `CategoryId` de uma categoria do usuário A: deve falhar com 400 (`CATEGORY_NOT_FOUND`). Teste na Task 6.
- Login com corpo vazio ou campos nulos: 400, nunca 500. Task 8.
- Cookie `jwt` expirado ou adulterado em rota protegida: 401, nunca 500. Task 8.
- Mês sem nenhuma movimentação no resumo e na série: zeros, nunca erro nem lista vazia. Task 9.
- Datas de borda (31/01, 29/02, dezembro para janeiro) caem no mês certo. Task 9.
- Valor `0`, negativo, ou com mais de 2 casas decimais: 400. Task 3.
- Excluir a conta apaga categorias e movimentações do usuário e nada dos outros. Task 8.
- Cadastro com email já existente (diferença de maiúsculas incluída): 409 (`EMAIL_TAKEN`). Task 8.

---

### Task 1: Ambiente, solution e projeto de testes

**Files:**
- Modify: `Finance Controller MVC.sln` (remove `Finance.Web`; adiciona `Finance.Tests`)
- Delete: pasta `Finance.Web/`
- Create: `Finance.Tests/Finance.Tests.csproj`, `Finance.Tests/Integration/ApiFactory.cs`
- Modify: `Finance.API/Program.cs` (apenas `public partial class Program { }` no fim)

**Interfaces:**
- Produces: `ApiFactory : WebApplicationFactory<Program>` com `Task<HttpClient> CreateLoggedInClientAsync(string email, string password = "Senha#12345")` (registra e faz login; devolve cliente com cookie) e `Task ResetDatabaseAsync()`. Lê `TEST_DB_CONNECTION`; define `Jwt__Key` fixa de teste com 40 caracteres.

- [ ] **Step 1: Instalar toolchain** (o container não tem `dotnet`). Instalar .NET SDK 8 com `dotnet-install.sh --channel 8.0` e PostgreSQL 16 via `apt-get`. Subir o Postgres local e criar o banco `finance_test`. Exportar `TEST_DB_CONNECTION="Host=localhost;Database=finance_test;Username=postgres;Password=postgres"`. Se um host for bloqueado pelo proxy, parar e avisar o dono (usar `update_environment`); não seguir sem `dotnet`.
- [ ] **Step 2:** `dotnet sln remove Finance.Web/Finance.Web.csproj`, apagar `Finance.Web/`, `dotnet new xunit -n Finance.Tests`, adicionar Moq, `Microsoft.AspNetCore.Mvc.Testing` 8.0.x e referências a Application, Infrastructure e API. Adicionar o projeto à solution.
- [ ] **Step 3:** Escrever `ApiFactory` conforme Interfaces e um teste de fumaça `Smoke_ApiStarts` que faz `GET /health` e espera 200 (ainda vai falhar).
- [ ] **Step 4:** Run: `dotnet build` e `dotnet test --filter Smoke_ApiStarts`. Expected: build OK; teste FAIL (404, `/health` ainda não existe). A Task 9 o faz passar.
- [ ] **Step 5: Commit** `chore: remove Finance.Web, add test project and tooling`.

### Task 2: Segredos e JWT com id do usuário

**Files:**
- Modify: `Finance.API/Program.cs`, `Finance.API/appsettings.json` (remover `Jwt.Key`), `Finance.Application/Services/JWTService.cs`
- Test: `Finance.Tests/Unit/JwtServiceTests.cs`

**Interfaces:**
- Produces: claim `ClaimTypes.NameIdentifier` = `user.Id` em todo token; `JwtOptions { string Key }` ligado a `Jwt`, validado no startup (`ValidateOnStart`, `Key.Length >= 32`).
- Consumes: `UserSelectDto` (`Id`, `email`, `user`, `role`), renomeado na Task 3.

- [ ] **Step 1: Failing tests** `GenerateToken_IncludesNameIdentifierClaim` (decodifica o token e espera o claim igual a `"7"` para `Id = 7`) e `Startup_FailsWhenJwtKeyMissing` (`ApiFactory` sem `Jwt__Key` lança `OptionsValidationException`).
- [ ] **Step 2:** Run: `dotnet test --filter "JwtServiceTests|Startup_Fails"`. Expected: FAIL.
- [ ] **Step 3:** Em `Program.cs`, ler a chave só de configuração (remover o fallback `"chave-super-secreta-123!"`) e usar `JwtOptions`. `JWTService` lê `JwtOptions`, usa `AddHours(8)` e inclui o claim. Remover o valor da chave de `appsettings.json`; documentar `Jwt__Key` no README.
- [ ] **Step 4:** Run os mesmos testes. Expected: PASS.
- [ ] **Step 5: Commit** `feat: require Jwt key from environment and add user id claim`.

### Task 3: Modelo de domínio, DTOs e validadores

**Files:**
- Modify: `Finance.Domain/Models/{Moviment,User,Category}.cs`, `Enums/EMovimentType.cs`, `Finance.Application/ViewModel/*.cs`, `Validators/*.cs`, `Mappings/FinanceMappingProfile .cs`
- Create: `Finance.Domain/Models/IOwnedEntity.cs`
- Test: `Finance.Tests/Unit/ValidatorTests.cs`

**Interfaces:**
- Produces: `interface IOwnedEntity { int UserId { get; set; } }` implementada por `Moviment` e `Category`. `Moviment`: `Id, Type, Description, MovimentDate (DateOnly), IsRecurring, CategoryId, Category?, Value, UserId`. `Category`: `Id, Name, UserId, Moviments?`. `User`: `Id, UserName, Password, Email, Role` (`UserName` evita colidir com o nome do tipo). `UserCreateDto` perde `role`. `IncomeCreateDto`/`ExpenseCreateDto`: `Description, MovimentDate (DateOnly), IsRecurring, CategoryId, Value`. `*SelectDto` ganham `IsRecurring` no lugar de `IsAppellant`. `UserSelectDto`: `Id, UserName, Email, Role`.
- Consumes: nada.

- [ ] **Step 1: Failing tests** em `ValidatorTests`: `IncomeValidator_RejectsZeroValue`, `IncomeValidator_RejectsNegativeValue`, `IncomeValidator_RejectsThreeDecimalPlaces` (`10.005m`), `IncomeValidator_AcceptsTwoDecimalPlaces`, `UserValidator_RejectsPasswordShorterThan8`. Os validadores recebem `ICategoryRepository` mockado; a regra de categoria entra na Task 6.
- [ ] **Step 2:** Run: `dotnet test --filter ValidatorTests`. Expected: FAIL (não compila ou falha).
- [ ] **Step 3:** Aplicar as renomeações e tipos da seção Interfaces em Domain, DTOs e `FinanceMappingProfile` (mapeamentos `UserName`/`Password`/`Email` e `IsRecurring`). Regra de `Value`: `GreaterThan(0)` e `PrecisionScale(18, 2, false)`. Senha: `MinimumLength(8)`. Remover `Base` do enum.
- [ ] **Step 4:** Run: `dotnet build && dotnet test --filter ValidatorTests`. Expected: PASS.
- [ ] **Step 5: Commit** `refactor: pascal case models, DateOnly, IsRecurring, owned entities`.

### Task 4: Postgres e migration inicial

**Files:**
- Modify: `Finance.Infrastructure/Finance.Infrastructure.csproj` (trocar `SqlServer` por `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.x), `Data/FinanceContext.cs`, `Finance.API/Program.cs` (`UseNpgsql`), `appsettings*.json`
- Delete: `Finance.Infrastructure/Migrations/*`
- Create: `Finance.Application/Interfaces/ICurrentUser.cs`, nova migration `InitialCreate`
- Test: `Finance.Tests/Integration/SchemaTests.cs`

**Interfaces:**
- Produces: `interface ICurrentUser { int UserId { get; } }` (lança `UnauthorizedAccessException` sem usuário). `FinanceContext(DbContextOptions<FinanceContext> options, ICurrentUser currentUser)` com filtro global `e.UserId == currentUser.UserId` em `Moviment` e `Category`; índice único `Category(UserId, Name)`; índice único `User(Email)` em minúsculas (coluna `citext` ou comparação normalizada: guardar `Email` já em minúsculas, decisão do implementador); `Value` com `HasPrecision(18, 2)`; FKs com `DeleteBehavior.Restrict` entre `Moviment`→`Category`, e `Cascade` de `User` para `Moviment` e `Category`.
- Consumes: `IOwnedEntity` (Task 3).

- [ ] **Step 1: Failing test** `Migrations_ApplyToEmptyPostgres` (`Database.MigrateAsync()` no banco de teste; depois espera as tabelas `Moviments`, `Categories`, `User`) e `CategoryName_UniquePerUser` (dois inserts com mesmo `UserId`+`Name` lançam `DbUpdateException`; usuários diferentes passam).
- [ ] **Step 2:** Run: `dotnet test --filter SchemaTests`. Expected: FAIL.
- [ ] **Step 3:** Trocar o provider, remover migrations antigas, configurar o contexto conforme Interfaces e gerar `InitialCreate` (`dotnet ef migrations add InitialCreate --project Finance.Infrastructure --startup-project Finance.API`). Registrar `ICurrentUser` como `HttpCurrentUser` (lê `ClaimTypes.NameIdentifier` de `IHttpContextAccessor`) em `Finance.API/Auth/HttpCurrentUser.cs`.
- [ ] **Step 4:** Run: `dotnet test --filter SchemaTests`. Expected: PASS.
- [ ] **Step 5: Commit** `feat: switch to PostgreSQL with ownership model and initial migration`.

### Task 5: Repositories e services com isolamento por usuário (corrige `UpdateAsync`)

**Files:**
- Modify: `Finance.Infrastructure/Repositories/*.cs`, `Finance.Application/Services/BaseService.cs`, `UserService.cs`
- Test: `Finance.Tests/Unit/BaseServiceTests.cs`, `Finance.Tests/Integration/IsolationTests.cs`

**Interfaces:**
- Produces: `BaseRepository(FinanceContext context, ICurrentUser currentUser)`. `GetByIdAsync` devolve `null` para entidade `IOwnedEntity` de outro usuário (o `FindAsync` ignora o filtro global, então a checagem é explícita). `AddAsync` define `UserId = currentUser.UserId` em `IOwnedEntity`. `BaseService.UpdateAsync(int id, TCreateDto dto)` mapeia o DTO sobre a entidade existente (`_mapper.Map(dto, existing)`), preservando `Id` e `UserId`; `UserService.UpdateAsync` repete o padrão e refaz o hash da senha.
- Consumes: `ICurrentUser`, `IOwnedEntity`.

- [ ] **Step 1: Failing tests.** Unit: `UpdateAsync_PreservesIdAndUserId` (repositório mockado devolve entidade com `Id=5, UserId=2`; após update continuam iguais). Integration (`IsolationTests`, usuários A e B logados): `UserB_CannotGetUserA_Income` (404), `UserB_CannotUpdateUserA_Income` (404), `UserB_CannotDeleteUserA_Income` (404 e o registro continua para A), `GetAll_ReturnsOnlyOwnRecords`, `UserB_CannotSeeUserA_Categories`.
- [ ] **Step 2:** Run: `dotnet test --filter "BaseServiceTests|IsolationTests"`. Expected: FAIL.
- [ ] **Step 3:** Implementar conforme Interfaces. `UpdateAsync` do repositório marca a entidade rastreada como modificada; ajustar construtores dos repositories concretos.
- [ ] **Step 4:** Run os mesmos testes. Expected: PASS (os de integração dependem das rotas `/api` da Task 8; se ainda não existirem, marcar com `Skip` e removê-lo na Task 8).
- [ ] **Step 5: Commit** `feat: per-user isolation and fix UpdateAsync key overwrite`.

### Task 6: Categoria da movimentação e exclusão bloqueada

**Files:**
- Modify: `Finance.Application/Validators/{Income,Expense}CreateDtoValidator.cs`, `Services/CategoryService.cs`, `Finance.API/Middleware/ExceptionHandlingMiddleware.cs`
- Create: `Finance.Application/Exceptions/ConflictException.cs` (`ConflictException(string code, string message)`)
- Test: `Finance.Tests/Unit/CategoryRulesTests.cs`

**Interfaces:**
- Produces: validadores recebem `ICategoryRepository`; regra `CategoryId` com `MustAsync(repo.GetByIdAsync(id) != null)` e `WithErrorCode("CATEGORY_NOT_FOUND")` (o filtro global garante que categoria de outro usuário não existe). `CategoryService.DeleteAsync` lança `ConflictException("CATEGORY_IN_USE", ...)` quando há movimentações, usando `IMovementReadRepository.AnyByCategoryAsync(int categoryId)` (Task 9; declarar a interface aqui). O middleware responde `{ code, message, errors? }` com 404 `NOT_FOUND`, 400 `VALIDATION_FAILED` (cada erro com `errorCode`), 409 pelo `code` da exceção, 401 `UNAUTHORIZED`, 500 `INTERNAL_ERROR` (sem stack trace).

- [ ] **Step 1: Failing tests:** `Validator_RejectsUnknownCategory` (repo devolve `null` → erro com `CATEGORY_NOT_FOUND`), `Validator_AcceptsOwnedCategory`, `DeleteCategory_WithMovements_ThrowsConflictCategoryInUse`, `DeleteCategory_Empty_Succeeds`, `Middleware_Maps_ConflictTo409WithCode`.
- [ ] **Step 2:** Run: `dotnet test --filter CategoryRulesTests`. Expected: FAIL.
- [ ] **Step 3:** Implementar conforme Interfaces.
- [ ] **Step 4:** Run os mesmos testes. Expected: PASS.
- [ ] **Step 5: Commit** `feat: category ownership validation and in-use delete guard`.

### Task 7: Rotas `/api`, JSON obrigatório e `ForwardedHeaders`

**Files:**
- Modify: todos os controllers (`[Route("api/[controller]")]`), `Finance.API/Program.cs`
- Create: `Finance.API/Middleware/RequireJsonMiddleware.cs`
- Test: `Finance.Tests/Integration/HttpRulesTests.cs`

**Interfaces:**
- Produces: `RequireJsonMiddleware` devolve 415 (`UNSUPPORTED_MEDIA_TYPE`) para `POST/PUT/PATCH/DELETE` em `/api` com corpo e sem `application/json`. `UseForwardedHeaders` (`XForwardedFor | XForwardedProto`, limpando `KnownNetworks`/`KnownProxies`) antes de `UseHttpsRedirection`. Remover o `UseHttpsRedirection` em ambiente de teste não é necessário: `ApiFactory` usa `BaseAddress` `https://localhost`.

- [ ] **Step 1: Failing tests:** `Post_WithFormContentType_Returns415`, `Controllers_AreUnder_ApiPrefix` (`GET /api/Category` sem login → 401; `GET /Category` → 404).
- [ ] **Step 2:** Run: `dotnet test --filter HttpRulesTests`. Expected: FAIL.
- [ ] **Step 3:** Implementar; manter `[Authorize]` onde já existe e adicionar `[Authorize]` ao `CategoryController` se faltar.
- [ ] **Step 4:** Run: `dotnet test --filter HttpRulesTests`. Expected: PASS.
- [ ] **Step 5: Commit** `feat: /api prefix, JSON-only writes, forwarded headers`.

### Task 8: Auth: registro, login, `me`, exclusão de conta, rate limit, categorias padrão

**Files:**
- Modify: `Finance.API/Controllers/AuthController.cs`, `Finance.Application/Services/UserService.cs`, `Finance.Application/Interfaces/IUserService.cs`, `Program.cs`
- Create: `Finance.Application/ViewModel/LoginDto.cs` (`Email`, `Password`), `Finance.Application/Services/DefaultCategories.cs`
- Test: `Finance.Tests/Integration/AuthTests.cs`

**Interfaces:**
- Produces: `POST /api/Auth/register` (`UserCreateDto` sem `role`) → 201 `UserSelectDto`; cria o usuário com `EUserRole.user` e as categorias `Alimentação, Moradia, Transporte, Saúde, Lazer, Salário, Outros`, tudo no mesmo commit. `POST /api/Auth/login` (`LoginDto`, JSON) → 200 e cookie; 400 se faltar campo; 401 `INVALID_CREDENTIALS` se errado. `POST /api/Auth/logout`. `GET /api/Auth/me` → `UserSelectDto`. `DELETE /api/Auth/me` → 204, apaga o usuário (cascata) e o cookie. `IUserService.RegisterAsync(UserCreateDto dto): Task<UserSelectDto>` e `DeleteAccountAsync(int userId): Task`. Email normalizado para minúsculas; duplicado → `ConflictException("EMAIL_TAKEN", ...)`. Rate limiter `login` (fixed window, 5/min, particionado por IP) aplicado a `POST /api/Auth/login`; excedido → 429.

- [ ] **Step 1: Failing tests:** `Register_AlwaysCreatesRoleUser_EvenIfRoleSent`, `Register_SeedsDefaultCategories` (7 categorias), `Register_DuplicateEmail_CaseInsensitive_Returns409EmailTaken`, `Login_EmptyBody_Returns400`, `Login_WrongPassword_Returns401`, `Protected_WithTamperedCookie_Returns401`, `Protected_WithExpiredToken_Returns401`, `Me_ReturnsCurrentUser`, `DeleteMe_RemovesOwnDataOnly` (A e B com dados; após excluir A, B mantém tudo; login de A falha), `Login_SixthAttemptInAMinute_Returns429`.
- [ ] **Step 2:** Run: `dotnet test --filter AuthTests`. Expected: FAIL.
- [ ] **Step 3:** Implementar conforme Interfaces; remover `Skip` dos testes da Task 5.
- [ ] **Step 4:** Run: `dotnet test`. Expected: todos PASS.
- [ ] **Step 5: Commit** `feat: registration hardening, me endpoints, account deletion, login rate limit`.

### Task 9: Lista filtrada, resumo mensal e série de 12 meses

**Files:**
- Create: `Finance.Application/Interfaces/{IMovementReadRepository,ISummaryService,IMovementQueryService}.cs`, `Finance.Application/Services/{SummaryService,MovementQueryService}.cs`, `Finance.Application/ViewModel/{MonthlySummaryDto,CategoryTotalDto,MonthPointDto,MovementListItemDto}.cs`, `Finance.Infrastructure/Repositories/MovementReadRepository.cs`, `Finance.API/Controllers/{SummaryController,MovementController}.cs`
- Test: `Finance.Tests/Integration/SummaryTests.cs`, `Finance.Tests/Unit/SummaryServiceTests.cs`

**Interfaces:**
- Produces:
  - `MonthlySummaryDto(int Year, int Month, decimal Income, decimal Expense, decimal Balance, IReadOnlyList<CategoryTotalDto> ByCategory)`; `CategoryTotalDto(int CategoryId, string CategoryName, EMovimentType Type, decimal Total)`; `MonthPointDto(int Year, int Month, decimal Income, decimal Expense)`; `MovementListItemDto(int Id, EMovimentType Type, string? Description, DateOnly MovimentDate, bool IsRecurring, int CategoryId, string CategoryName, decimal Value)`.
  - `IMovementReadRepository`: `Task<IReadOnlyList<Moviment>> ListAsync(DateOnly? from, DateOnly? toExclusive, int? categoryId, EMovimentType? type)`, `Task<IReadOnlyList<CategoryTotalDto>> TotalsByCategoryAsync(DateOnly from, DateOnly toExclusive)`, `Task<IReadOnlyList<MonthPointDto>> TotalsByMonthAsync(DateOnly from, DateOnly toExclusive)`, `Task<bool> AnyByCategoryAsync(int categoryId)`. Tudo agregado no banco.
  - `ISummaryService`: `GetMonthlyAsync(int year, int month)`, `GetSeriesAsync(int months)` (12 por padrão, máximo 24; sempre completa, zeros nos meses vazios, em ordem cronológica terminando no mês atual do servidor).
  - `GET /api/Summary?year=&month=`, `GET /api/Summary/series?months=12`, `GET /api/Movement?year=&month=&categoryId=&type=` (ordenado por data decrescente). `month` fora de 1 a 12 → 400.

- [ ] **Step 1: Failing tests:** `Summary_EmptyMonth_ReturnsZerosAndEmptyByCategory`, `Summary_BoundaryDates_Jan31_Feb1_Dec31_Jan1_LandInCorrectMonth`, `Summary_LeapDay_2028_02_29_CountedInFebruary`, `Summary_Balance_IsIncomeMinusExpense`, `Summary_OnlyCountsOwnData`, `Series_ReturnsTwelveMonthsZeroFilledInOrder`, `Series_MonthsAbove24_Returns400`, `Movement_FilterByCategoryAndMonth`, `Movement_InvalidMonth_Returns400`.
- [ ] **Step 2:** Run: `dotnet test --filter "SummaryTests|SummaryServiceTests"`. Expected: FAIL.
- [ ] **Step 3:** Implementar; registrar no `Program.cs`. Intervalos são `[from, toExclusive)` com `DateOnly`, sem fuso.
- [ ] **Step 4:** Run: `dotnet test`. Expected: PASS.
- [ ] **Step 5: Commit** `feat: movement list filter, monthly summary and 12-month series`.

### Task 10: Produção: health, estáticos, HSTS, Swagger só em dev, conta demo

**Files:**
- Modify: `Finance.API/Program.cs`, `Finance.API/Middleware/ExceptionHandlingMiddleware.cs`
- Create: `Finance.API/Health/HealthEndpoint.cs`, `Finance.Infrastructure/Seed/DemoSeeder.cs`
- Test: `Finance.Tests/Integration/ProductionTests.cs`

**Interfaces:**
- Produces: `GET /health` → 200 `{ "status": "ok" }` sem tocar no banco. Arquivos estáticos de `wwwroot` com `UseDefaultFiles`/`UseStaticFiles` e `MapFallbackToFile("index.html")` que **não** captura `/api/*` nem `/health` (fallback para esses caminhos devolve 404 JSON). HSTS e redirect HTTPS fora de Development; Swagger só em Development. Conexão com retry (`EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)`). `DemoSeeder.SeedAsync(FinanceContext context, string demoPassword)` idempotente: cria `demo@financecontroller.app` com ~3 meses de receitas e despesas variadas, incluindo recorrentes. Roda no startup só se `Demo:Enabled=true` e `Demo:Password` estiver definido.

- [ ] **Step 1: Failing tests:** `Health_Returns200WithoutDatabase` (string de conexão inválida mesmo assim 200), `UnknownApiRoute_Returns404Json_NotIndexHtml`, `Swagger_NotAvailableInProduction`, `Error500_DoesNotLeakStackTrace`, `DemoSeeder_IsIdempotent` (duas execuções, mesma contagem), `DemoSeeder_Disabled_ByDefault`.
- [ ] **Step 2:** Run: `dotnet test --filter ProductionTests`. Expected: FAIL.
- [ ] **Step 3:** Implementar conforme Interfaces. O smoke test da Task 1 passa aqui.
- [ ] **Step 4:** Run: `dotnet test`. Expected: tudo PASS, incluindo `Smoke_ApiStarts`.
- [ ] **Step 5: Commit** `feat: health, static hosting, production hardening, demo account`.

### Task 11: Frontend base: Vite, cliente da API, autenticação

**Files:**
- Create: `frontend/` (Vite + React + TS), `frontend/src/api/client.ts`, `frontend/src/api/types.ts`, `frontend/src/i18n/errors.ts`, `frontend/src/auth/AuthContext.tsx`, `frontend/src/pages/{Login,Register}.tsx`, `PRODUCT.md`, `DESIGN.md`
- Modify: `.gitignore` (`Finance.API/wwwroot/`, `frontend/node_modules/`)
- Test: `frontend/src/api/client.test.ts`, `frontend/src/i18n/errors.test.ts`

**Interfaces:**
- Produces: `api.get<T>(path)`, `api.post<T>(path, body)`, `api.put<T>(path, body)`, `api.delete(path)` com `credentials: "same-origin"` e `Content-Type: application/json`; erro tipado `ApiError { status: number; code: string }`; `errorMessage(code: string): string` (pt-BR; códigos desconhecidos → "Algo deu errado. Tente novamente."). `AuthContext` expõe `{ user, login(email, password), register(dto), logout(), deleteAccount() }`; 401 em qualquer chamada leva ao login. Vite: `build.outDir = "../Finance.API/wwwroot"`, `server.proxy["/api"]` para `https://localhost:5150`.

- [ ] **Step 1: Reinstalar os skills** se a sessão for nova: copiar `impeccable` e `diagram-design` de `pbakaus/impeccable` e `cathrynlavery/diagram-design` para `~/.claude/skills` (ver notas da sessão). Rodar `/impeccable init` para escrever `PRODUCT.md` (público: pessoas que controlam finanças pessoais; tom: claro e confiável; interface em pt-BR).
- [ ] **Step 2: Failing tests (Vitest):** `client sends JSON and same-origin credentials`, `client throws ApiError with code on non-2xx`, `errorMessage maps EMAIL_TAKEN to Portuguese`, `errorMessage falls back for unknown code`.
- [ ] **Step 3:** Implementar cliente, mensagens e telas de login e cadastro. Usar `/impeccable craft` para as telas e `/impeccable document` para `DESIGN.md`.
- [ ] **Step 4:** Run: `cd frontend && npm test -- --run && npm run build`. Expected: testes PASS; build gera `Finance.API/wwwroot/index.html`.
- [ ] **Step 5: Commit** `feat: frontend scaffold with auth screens`.

### Task 12: Frontend: categorias e movimentações

**Files:**
- Create: `frontend/src/pages/{Categories,Movements}.tsx`, `frontend/src/components/{MovementForm,MonthFilter}.tsx`, `frontend/src/lib/format.ts`
- Test: `frontend/src/lib/format.test.ts`, `frontend/src/components/MovementForm.test.tsx`

**Interfaces:**
- Produces: `formatBRL(value: number): string` ("R$ 1.234,50"), `formatDate(iso: string): string` ("31/01/2028"), `monthLabel(year: number, month: number): string`. Telas: CRUD de categorias (mostra aviso pt-BR para `CATEGORY_IN_USE`), lista de movimentações filtrada por mês e categoria (`GET /api/Movement`), criar/editar/excluir receita e despesa com a flag "Recorrente".
- Consumes: cliente da Task 11 e rotas da Task 9.

- [ ] **Step 1: Failing tests:** `formatBRL formats thousands and cents`, `formatDate formats ISO date without timezone shift`, `MovementForm blocks zero and negative values`, `MovementForm submits IsRecurring flag`.
- [ ] **Step 2:** Run: `cd frontend && npm test -- --run`. Expected: FAIL.
- [ ] **Step 3:** Implementar telas com `/impeccable craft`; revisar com `/impeccable critique` e `/impeccable polish` (estados vazios, carregamento, erro e acessibilidade por teclado).
- [ ] **Step 4:** Run: `npm test -- --run && npm run build`. Expected: PASS.
- [ ] **Step 5: Commit** `feat: categories and movements screens`.

### Task 13: Frontend: dashboard

**Files:**
- Create: `frontend/src/pages/Dashboard.tsx`, `frontend/src/components/{SummaryCards,CategoryChart,MonthlyChart}.tsx`, `frontend/src/lib/chartData.ts`
- Test: `frontend/src/lib/chartData.test.ts`

**Interfaces:**
- Produces: `toCategorySlices(rows: CategoryTotal[]): { name: string; value: number }[]` (só despesas, ordenadas por valor decrescente), `toMonthlySeries(points: MonthPoint[]): { label: string; income: number; expense: number }[]`. Dashboard com saldo, receitas e despesas do mês, seletor de mês, gráfico por categoria e gráfico mês a mês (Recharts). Estado vazio explica o que fazer.
- Consumes: `GET /api/Summary` e `GET /api/Summary/series` (Task 9).

- [ ] **Step 1: Failing tests:** `toCategorySlices ignores income rows and sorts desc`, `toMonthlySeries keeps chronological order and pt-BR month labels`, `toMonthlySeries handles all-zero months`.
- [ ] **Step 2:** Run: `cd frontend && npm test -- --run`. Expected: FAIL.
- [ ] **Step 3:** Implementar. Usar a skill `dataviz` para escolher cores e leitura dos gráficos, e `/impeccable polish` na tela inteira. Verificar visualmente com a conta demo no browser (Playwright) em largura de celular e de desktop.
- [ ] **Step 4:** Run: `npm test -- --run && npm run build`. Expected: PASS.
- [ ] **Step 5: Commit** `feat: dashboard with summary and charts`.

### Task 14: Docker, CI e deploy

**Files:**
- Create: `Dockerfile`, `.dockerignore`, `fly.toml`, `.github/workflows/ci.yml`, `.github/workflows/deploy.yml`, `docs/devops/DEPLOY.md`

**Interfaces:**
- Produces: imagem multi-estágio (Node 22 compila `frontend/` para `wwwroot`; SDK 8 publica; runtime `aspnet:8.0` roda, usuário não-root, porta 8080). `ci.yml` em todo push e PR: serviço Postgres 16, `dotnet build`, `dotnet test` com `TEST_DB_CONNECTION`, `npm ci`, `npm test -- --run`, `npm run build`. `deploy.yml` na `main` depois do CI verde: gerar bundle de migrations (`dotnet ef migrations bundle`), executá-lo com `DATABASE_URL_DIRECT`, depois `flyctl deploy --remote-only`. Segredos do GitHub: `FLY_API_TOKEN`, `DATABASE_URL_DIRECT`. Segredos no Fly: `Jwt__Key`, `ConnectionStrings__DefaultConnection` (pooler), `Demo__Enabled`, `Demo__Password`. `docs/devops/DEPLOY.md` lista cada segredo e como gerar a chave JWT (`openssl rand -base64 48`).

- [ ] **Step 1:** Escrever o `Dockerfile` e rodar `docker build` onde Docker existir; no container do agente, se não houver Docker, validar com `dotnet publish` mais `npm run build` e deixar o build da imagem para o CI.
- [ ] **Step 2:** Escrever os workflows. Validar a sintaxe YAML localmente e abrir a execução do CI no primeiro push.
- [ ] **Step 3:** Run (CI): push na branch. Expected: `ci.yml` verde.
- [ ] **Step 4:** O deploy exige ações do dono: criar conta Fly.io e banco Neon, e cadastrar os segredos. Entregar o passo a passo em `DEPLOY.md` e pedir ao dono. Depois do primeiro deploy, abrir a URL pública, fazer login na conta demo e conferir `/health`.
- [ ] **Step 5: Commit** `ci: docker image, CI workflow and Fly.io deploy`.

### Task 15: Diagramas e documentação final

**Files:**
- Create: `docs/architecture/diagrams/{architecture,data-model,deploy-flow}.html` (e `.svg`)
- Modify: `README.md`, `docs/Roadmap.md`, `docs/architecture/README.md`

**Interfaces:**
- Produces: três diagramas feitos com a skill `diagram-design`: arquitetura em camadas com o React servido pela API; modelo de dados (ER: `User`, `Category`, `Moviment` com `Income`/`Expense`); fluxo de deploy (push → CI → migrations → Fly → Neon). README atualizado (Postgres, variáveis de ambiente, como rodar local, link da URL pública, limitações conhecidas: sem verificação de email nem recuperação de senha). Roadmap marca os sprints concluídos.

- [ ] **Step 1:** Gerar os três diagramas com `diagram-design` (responder à primeira configuração com os tokens de `DESIGN.md` para manter a mesma identidade visual) e conferir a leitura em tela cheia e em largura de celular.
- [ ] **Step 2:** Atualizar README, Roadmap e README de arquitetura; remover referências ao SQL Server e a `Finance.Web`.
- [ ] **Step 3:** Run: `dotnet test && (cd frontend && npm test -- --run)`. Expected: tudo PASS.
- [ ] **Step 4: Commit** `docs: system diagrams and production documentation`.

---

## Self-Review

- **Cobertura do spec:** segredos (T2), Postgres/migration (T4), `UserId`/isolamento/`UpdateAsync` (T3 a T5), registro sem papel (T8), categorias padrão e regras (T6, T8), `IsRecurring` (T3), datas (T3, T9), resumo e série (T9), rate limit, `ForwardedHeaders`, JSON-only, health, estáticos, conta demo, exclusão de conta (T7, T8, T10), frontend nível 2 (T11 a T13), Docker, CI/CD, Neon com string direta (T14), diagramas e docs (T15).
- **Desvio do spec, para o dono confirmar:** o spec cita Testcontainers; o plano usa um Postgres real lido de `TEST_DB_CONNECTION` (serviço do GitHub Actions no CI, Postgres local no container), porque Testcontainers exige Docker e o container do agente pode não ter.
- **Risco de ambiente:** o container não tem `dotnet`. A Task 1 instala o SDK; se o proxy bloquear o download, o trabalho para até o dono liberar o host.
