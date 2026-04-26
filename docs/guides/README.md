# 📋 Guides — Guias Rápidos & Troubleshooting

Coleção de guias rápidos, resoluções de problemas comuns e checklists.

---

## 📚 Documentos

### Common Issues (Planejado)
**Status**: 🔄 Planejado

Problemas comuns e soluções:
- Connection string não encontrada
- Migration falhou
- Build error em DI Container
- Teste não encontra mock
- Compilação lenta
- Mais...

### Performance Tips (Planejado)
**Status**: 🔄 Planejado

Otimizações de performance:
- Query optimization (EF Core)
- N+1 queries
- Caching strategies
- Async operations
- Database indexes
- Profiling tools

### Security Checklist (Planejado)
**Status**: 🔄 Planejado

Segurança checklist:
- Secrets management
- SQL Injection prevention
- Authentication & Authorization
- OWASP Top 10
- Dependency scanning
- Security headers

---

## ⚡ Quick Snippets

### Copiar Connection String
```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FinanceDb;Trusted_Connection=true;"
  }
}
```

### Setup DI Container
```csharp
// Program.cs — exemplo pronto
services.AddScoped<IBaseRepository<Category, int>, CategoryRepository>();
services.AddScoped<IBaseService<Category, CategoryCreateDto, CategorySelectDto>, CategoryService>();
```

### Mock no Teste
```csharp
// Teste unitário — exemplo
var mockRepo = new Mock<IBaseRepository<Category, int>>();
mockRepo.Setup(x => x.AddAsync(It.IsAny<Category>()))
    .ReturnsAsync(new Category { Id = 1, Name = "Test" });

var service = new CategoryService(mockRepo.Object, uow, mapper, validator);
```

---

## 📋 Checklists

### Pre-Commit Checklist
- [ ] `dotnet build` passa
- [ ] `dotnet test` passa
- [ ] Sem warnings
- [ ] Código segue naming conventions
- [ ] SOLID respeitado
- [ ] Testes > 80% coverage
- [ ] Commit message descritiva

### Code Review Checklist
- [ ] SOLID principles respeitados
- [ ] Testes escritos e passando
- [ ] Documentation atualizada
- [ ] Performance aceitável
- [ ] Sem code smells
- [ ] Segurança validada

### Release Checklist
- [ ] Todos testes passando
- [ ] Coverage > 80%
- [ ] Documentação atualizada
- [ ] Changelog feito
- [ ] Version incrementado
- [ ] README atualizado
- [ ] Deploy scripts validados

---

## 🤔 Quick Troubleshoot

### "Erro X ao compilar"
1. Limpar cache: `dotnet clean`
2. Restaurar: `dotnet restore`
3. Tentar build novamente

### "EF Core migration error"
1. Ver migrations: `dotnet ef migrations list`
2. Remover última: `dotnet ef migrations remove`
3. Criar nova: `dotnet ef migrations add NomeMigration`
4. Update: `dotnet ef database update`

### "Teste falha aleatoriamente"
1. Pode ser: estado compartilhado entre testes
2. Solução: use fixtures isoladas
3. Ou: banco de dados de teste compartilhado

### "DI Container erro"
1. Service não registrado? Adicione em Program.cs
2. Dependência circular? Redesign
3. Tipo errado registrado? Verifique interface

---

## 🔗 Links Rápidos

| Tópico | Link |
|--------|------|
| SOLID Principles | [docs/architecture/SOLID-Principles.md](../architecture/SOLID-Principles.md) |
| Setup Local | [docs/development/Setup-Local.md](../development/Setup-Local.md) (em breve) |
| Unit Testing | [docs/testing/Unit-Testing.md](../testing/Unit-Testing.md) (em breve) |
| GitHub Actions | [docs/devops/CI-CD-Pipeline.md](../devops/CI-CD-Pipeline.md) (em breve) |
| Docker | [docs/devops/Docker-Setup.md](../devops/Docker-Setup.md) (em breve) |

---

## 📞 Precisa de Ajuda?

### Procure por:
1. **SOLID questions**: [SOLID Principles](../architecture/SOLID-Principles.md)
2. **Setup issues**: Veja Security Checklist (em breve)
3. **Performance issues**: [Performance Tips](./Performance-Tips.md) (em breve)
4. **Bugs obscuros**: [Common Issues](./Common-Issues.md) (em breve)

---

**Próximos documentos**: Common Issues, Performance Tips, Security Checklist  
**Última Atualização**: 26 de Abril de 2026
