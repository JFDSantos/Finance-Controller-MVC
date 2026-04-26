# 💻 Development — Guias de Desenvolvimento

Documentação prática para desenvolvedores trabalhando no Finance-Controller-MVC.

---

## 📚 Documentos

### Setup Local (Planejado)
**Status**: 🔄 Planejado

Como configurar ambiente local:
- Pré-requisitos (.NET 6+, SQL Server/PostgreSQL)
- Clonar repositório
- Restaurar dependências
- Configurar connection strings
- Rodar migrations
- Verificar se está funcionando

### Git Workflow (Planejado)
**Status**: 🔄 Planejado

Processo Git para o projeto:
- Branches: main, develop, feature/*, bugfix/*
- Commit messages: Conventional Commits
- Pull Requests: template e review checklist
- Merge strategy: squash ou rebase

### Code Standards (Planejado)
**Status**: 🔄 Planejado

Padrões de código C# para o projeto:
- Naming conventions (C# style guide)
- Folder structure
- File organization
- Comments e documentation
- Linting/Formatting (EditorConfig)

---

## ⚡ Quick Reference

### Rodar Projeto Localmente
```bash
# Restaurar dependências
dotnet restore

# Aplicar migrations
dotnet ef database update

# Rodar projeto
dotnet run --project Finance.API
```

### Verificar SOLID
```bash
# Ler documentação SOLID antes de novo feature!
# Veja: docs/architecture/SOLID-Principles.md
```

### Fazer novo Feature
```bash
# 1. Crie branch
git checkout -b feature/nova-feature

# 2. Implemente respeitando SOLID
# 3. Escreva testes
# 4. Faça commit
git commit -m "feat: nova-feature"

# 5. Push
git push origin feature/nova-feature

# 6. Crie Pull Request
```

---

## 📋 Checklist Pré-Commit

Antes de fazer commit de novo código:

- [ ] Testes passando (`dotnet test`)
- [ ] Sem warnings de build
- [ ] Código segue naming conventions
- [ ] SOLID principles respeitados
- [ ] Testes unitários escritos (80%+ coverage)
- [ ] Documentação atualizada
- [ ] Commit message clara e descritiva

---

## 🤔 Troubleshooting

### Problema: "Connection string não encontrada"
→ Veja: [Setup Local](./Setup-Local.md) — Seção appsettings

### Problema: "Migration falhou"
→ Veja: [Setup Local](./Setup-Local.md) — Seção Migrations

### Problema: "Não sei como estruturar novo feature"
→ Veja: [Code Standards](./Code-Standards.md)  
→ Veja: [SOLID Principles](../architecture/SOLID-Principles.md)

---

## 📚 Leitura Recomendada

1. **SOLID Principles**: [docs/architecture/SOLID-Principles.md](../architecture/SOLID-Principles.md)
2. **Project Structure**: Explore `Finance.API/`, `Finance.Application/`, etc.
3. **Design Patterns**: Repository, Unit of Work (você tem ambos!)

---

**Próximos documentos**: Setup Local, Git Workflow, Code Standards  
**Última Atualização**: 26 de Abril de 2026
