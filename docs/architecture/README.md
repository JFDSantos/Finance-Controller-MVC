# 🏗️ Arquitetura & Design

Documentação sobre padrões arquiteturais, princípios de design e estrutura do projeto Finance-Controller-MVC.

---

## 📚 Documentos

### [SOLID Principles](SOLID-Principles.md)
**Status**: ✅ Completo  
**Última Atualização**: 26/04/2026

Documentação abrangente dos 5 princípios SOLID aplicados ao Finance-Controller-MVC:

- **S** — Single Responsibility Principle (Responsabilidade Única)
- **O** — Open/Closed Principle (Aberto/Fechado)
- **L** — Liskov Substitution Principle (Substituição de Liskov)
- **I** — Interface Segregation Principle (Segregação de Interface)
- **D** — Dependency Inversion Principle (Inversão de Dependência)

Contém:
- ✅ Definições claras
- ✅ Exemplos do código real
- ✅ Analogias do domínio financeiro
- ✅ Anti-patterns
- ✅ Validação com quizzes
- ✅ Melhorias futuras

**Para quem?** Todos no time devem ler para entender a base arquitetural.

---

### Layer Architecture (Em Progresso)
**Status**: 🔄 Planejado

Documentar as 4 camadas do projeto:
- Domain Layer (modelos ricos)
- Application Layer (services)
- Infrastructure Layer (repositories)
- API Layer (controllers)

---

### Design Patterns (Planejado)
**Status**: 🔄 Planejado

Padrões de design utilizados:
- Repository Pattern
- Unit of Work Pattern
- Specification Pattern (futuro)
- Factory Pattern (futuro)
- Strategy Pattern (futuro)

---

## 🚀 Quick Start

Se é novo no projeto:

1. **Leia**: [SOLID Principles](SOLID-Principles.md) (30 min)
2. **Entenda**: Quais princípios aplicamos em cada camada
3. **Explore**: O código seguindo a documentação
4. **Implemente**: Novos features mantendo os padrões

---

## 📊 Arquitetura Atual

```
┌─────────────────────────────────────┐
│        Finance.API (Controllers)     │
│  ↓ depende de                        │
├─────────────────────────────────────┤
│   Finance.Application (Services)     │
│  ↓ depende de                        │
├─────────────────────────────────────┤
│  Finance.Infrastructure (Repos)      │
│  ↓ depende de                        │
├─────────────────────────────────────┤
│   Finance.Domain (Models)            │
└─────────────────────────────────────┘

Cada camada é isolada e trocável
(Dependency Inversion: abstração em cima, concrição embaixo)
```

---

## 🎯 Princípios em Ação

### Single Responsibility (SRP)
- Repository: APENAS acesso a dados
- Service: APENAS lógica de negócio
- Controller: APENAS HTTP protocol

### Open/Closed (OCP)
- BaseRepository: genérico, extensível via classes concretas
- BaseService: virtual methods, override em subclasses

### Liskov Substitution (LSP)
- Trocar CategoryService por ExpenseService sem quebras
- UserService com criptografia ainda respeita contrato

### Interface Segregation (ISP)
- IBaseService é bem segregada
- Futuro: IReadOnlyService para AuditService

### Dependency Inversion (DIP)
- Controllers recebem IService, não Service
- Services recebem IRepository, não Repository concretos
- DI Container em Program.cs gerencia tudo

---

## 📝 Checklist para Novos Features

Ao implementar novo feature:

- [ ] Respeita SRP? (Cada classe tem uma responsabilidade)
- [ ] É aberto para extensão? (Não modifica código existente)
- [ ] Substitui tipos sem surpresas? (LSP válido)
- [ ] Interfaces são pequenas? (ISP respeitado)
- [ ] Depende de abstrações? (DIP válido)
- [ ] Tem testes? (Coverage 80%+)
- [ ] Documentado? (Código + comments)

---

## 🔗 Links Úteis

- [SOLID Principles](SOLID-Principles.md) — Arquivo completo
- [Roadmap](../Roadmap.md) — Plano geral
- [GitHub Repo](https://github.com/jeferson-santos/Finance-Controller-MVC) — Código-fonte

---

**Mantido por**: Jeferson Santos  
**Última Atualização**: 26 de Abril de 2026
