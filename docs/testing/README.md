# 🧪 Testing — Guias de Testes

Documentação sobre estratégias, ferramentas e boas práticas de testes.

---

## 📚 Documentos

### Unit Testing (Planejado)
**Status**: 🔄 Planejado

Como escrever testes unitários:
- Setup de projeto de testes
- Mocking com Moq
- Testando Services
- Testando Repositories
- Fixtures e helpers
- Cobertura de testes

### Integration Testing (Planejado)
**Status**: 🔄 Planejado

Testes de integração:
- Setup com TestDatabase
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
```bash
# Criar projeto de testes (futuro)
dotnet new xunit -n Finance.Tests
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
- **Framework**: xUnit + TestContainers

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
→ Use migrations em memory
→ Ou use containers (TestContainers)

---

## 📚 Leitura Recomendada

1. **xUnit Framework**: https://xunit.net/
2. **Moq Documentation**: https://github.com/moq/moq4
3. **Testing Best Practices**: Microsoft Docs
4. **Your SOLID**: [SOLID Principles](../architecture/SOLID-Principles.md) — Código testável!

---

## 🎓 Roadmap de Testes

### Sprint 6: Unit Tests Básicos
- [ ] Setup projeto Finance.Tests
- [ ] Testes para CategoryService
- [ ] Testes para Validators
- [ ] Mock fixtures

### Sprint 7: Integration Tests
- [ ] Testes para ExpenseService completo
- [ ] Testes para Controllers
- [ ] Test database setup
- [ ] 80%+ coverage

### Sprint 8+: CI/CD Tests
- [ ] GitHub Actions com testes
- [ ] Coverage reports
- [ ] Falha build se <80% coverage

---

**Próximos documentos**: Unit Testing, Integration Testing  
**Última Atualização**: 26 de Abril de 2026
