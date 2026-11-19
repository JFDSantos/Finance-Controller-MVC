# 🧪 Testing — Guias de Testes

Documentação sobre estratégias, ferramentas e boas práticas de testes.

---

## 📚 Documentos

### Estado atual (19/11/2026)

- `Finance.Tests/Unit`: services (com repositórios simulados via Moq), regras de categoria, JWT, resumo e validadores.
- `Finance.Tests/Integration`: API inteira com `WebApplicationFactory` contra um **PostgreSQL real** (autenticação, isolamento entre usuários, regras HTTP, schema, resumo).
- `frontend/`: Vitest + Testing Library (`npm test`).
- CI: o job `backend` sobe um `postgres:16` e roda `dotnet test`; o job `frontend` roda lint, testes e build.

### Unit Testing
**Status**: ✅ Em uso (guia detalhado planejado)

Como escrever testes unitários:
- Setup de projeto de testes
- Mocking com Moq
- Testando Services
- Testando Repositories
- Fixtures e helpers
- Cobertura de testes

### Integration Testing
**Status**: ✅ Em uso (guia detalhado planejado)

Testes de integração:
- Postgres de teste lido de `TEST_DB_CONNECTION`
- Testando fluxos completos
- Testando Controllers com HttpClient
- Testando validadores

### Test Coverage (Planejado)
**Status**: 🔄 Planejado

Métricas e cobertura:
- Ferramentas (OpenCover, Coverlet)
- Meta: 80%+ coverage
- Relatórios de cobertura
- CI/CD gates

---

## 🎯 Meta de Cobertura

| Camada | Meta | Status |
|--------|------|--------|
| Services | 90% | 🔄 Planejado |
| Repositories | 85% | 🔄 Planejado |
| Controllers | 70% | 🔄 Planejado |
| **Total** | **80%+** | 🔄 Planejado |

---

## ⚡ Quick Reference

### Projeto de Testes
O projeto `Finance.Tests` já existe (xUnit). Os testes de integração precisam de um Postgres:
```bash
export TEST_DB_CONNECTION="Host=localhost;Database=finance_test;Username=postgres;Password=postgres"
# ou: source scripts/dev-env.sh (prepara um Postgres local e exporta a variável)
```

### Estrutura de Teste
```csharp
// Arrange: preparar dados
// Act: executar função
// Assert: verificar resultado
```

### Executar Testes
```bash
# Todos os testes
dotnet test

# Teste específico
dotnet test --filter "Test.Specific"

# Com cobertura
dotnet test /p:CollectCoverage=true

# Frontend
cd frontend && npm test
```

---

## 📋 Tipos de Testes

### Unit Tests
- Testam uma unidade em isolamento
- Services com mocks de repositories
- Validadores com dados de teste
- **Framework**: xUnit + Moq

### Integration Tests
- Testam múltiplas camadas
- Usam banco de dados de teste
- Testam fluxos completos
- **Framework**: xUnit + `WebApplicationFactory` + PostgreSQL real (`TEST_DB_CONNECTION`)

### E2E Tests (Futuro)
- Testam API completa
- Cliente real fazendo requisições
- **Framework**: Postman/REST Client

---

## 🤔 Troubleshooting

### Problema: "Não sei por onde começar"
→ Leia: Seção "Unit Testing" (em breve)
→ Exemplo: Veja `Finance.Application/Services/CategoryService.cs`

### Problema: "Mock complexo"
→ Use Moq: `var mock = new Mock<IRepository<T>>()`
→ Setup: `mock.Setup(x => x.AddAsync(It.IsAny<T>())).Returns(...)`

### Problema: "Banco de testes quebrou"
→ Confira se `TEST_DB_CONNECTION` aponta para um Postgres acessível
→ Recrie o banco de teste (`dropdb finance_test && createdb finance_test`)

---

## 📚 Leitura Recomendada

1. **xUnit Framework**: https://xunit.net/
2. **Moq Documentation**: https://github.com/moq/moq4
3. **Testing Best Practices**: Microsoft Docs
4. **Your SOLID**: [SOLID Principles](../architecture/SOLID-Principles.md) — Código testável!

---

## 🎓 Roadmap de Testes

### Sprint 6: Unit Tests Básicos
- [x] Setup projeto Finance.Tests
- [x] Testes para CategoryService
- [x] Testes para Validators
- [x] Mock fixtures

### Sprint 7: Integration Tests
- [x] Testes de integração da API (controllers, isolamento, regras HTTP)
- [x] Test database setup (Postgres real)
- [ ] 80%+ coverage (não medido)

### Sprint 8+: CI/CD Tests
- [x] GitHub Actions com testes
- [ ] Coverage reports
- [ ] Falha build se <80% coverage

---

**Próximos documentos**: Unit Testing, Integration Testing  
**Última Atualização**: 19 de Novembro de 2026
