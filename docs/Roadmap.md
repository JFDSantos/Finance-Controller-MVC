# Roadmap Gamificado: Finance Controller

Bem-vindo ao seu desafio de aprendizado! Aqui está um roadmap gamificado, dividido em sprints/etapas, para você evoluir como dev fullstack e devops, aprendendo as melhores práticas do mercado.

---

## 🎯 FASE 1: SOLID PRINCIPLES (Sprints 1-5) ✅ COMPLETE

### 🏆 Sprint 1: Single Responsibility Principle (SRP)
**Status:** ✅ CONCLUÍDO (26/04/2026)
- ✅ Repository layer: Only data access responsibility
- ✅ Service layer: Only business logic responsibility
- ✅ Controller layer: Only HTTP protocol responsibility
- ✅ Quiz: 2/2 correct

**Conquistas:**
- Entendeu SRP conceitualmente
- Identificou SRP no código existente

---

### 🏆 Sprint 2: Open/Closed Principle (OCP)
**Status:** ✅ CONCLUÍDO (26/04/2026)
- ✅ Criado BaseRepository<T,int> genérico (~150 linhas eliminadas)
- ✅ Criado BaseService<T,TCreateDto,TSelectDto> com methods virtual
- ✅ Refatorado CategoryService, ExpenseService, IncomeService, UserService
- ✅ Implementado password encryption via override pattern
- ✅ Quiz: 2/2 correct

**Conquistas:**
- Código aberto para extensão, fechado para modificação
- Eliminado duplicação de código

---

### 🏆 Sprint 3: Liskov Substitution Principle (LSP)
**Status:** ✅ CONCLUÍDO (26/04/2026)
- ✅ Validado: Todas services são substituíveis sem surpresas
- ✅ Entendido: UserService com encryption respeita contrato
- ✅ Analogia bancária: Qualquer "caixa" funciona entregando mesma promessa
- ✅ Quiz: 2/2 correct (GuestUserService vs IncomeOptimizedService)

**Conquistas:**
- Compreensão de substituibilidade sem surpresas
- Reconheceu diferença entre enriquecimento vs mudança de contrato

---

### 🏆 Sprint 4: Interface Segregation Principle (ISP)
**Status:** ✅ CONCLUÍDO (26/04/2026)
- ✅ Analisado: IBaseService é segregado apropriadamente
- ✅ Entendido: Quando future-proof com Read/Write segregation
- ✅ Demonstrado pragmatismo: YAGNI until needed
- ✅ Quiz: 2/2 correct + pragmatic thinking

**Conquistas:**
- Reconheceu balance entre arquitetura ideal vs prática
- Compreensão de quando aplicar padrões

---

### 🏆 Sprint 5: Dependency Inversion Principle (DIP)
**Status:** ✅ CONCLUÍDO (26/04/2026)
- ✅ Validado: Código já respeita DIP completamente
- ✅ Program.cs: Todas dependências registradas como interfaces
- ✅ Controllers: Dependem de IService, não Services concretos
- ✅ Services: Dependem de IRepository, não concretos
- ✅ Quiz: 3/3 correct (100% mastery)

**Conquistas:**
- Domínio completo de Inversion of Control
- Entendeu ciclo completo: Alto nível → Abstração ← Baixo nível

---

### 📚 SOLID Documentation
**Status:** ✅ DOCUMENTAÇÃO COMPLETA
- Arquivo: [SOLID-Principles.md](architecture/SOLID-Principles.md)
- Contém: Todos 5 princípios, exemplos, validação, antigos, futuro
- Referência: Para futuros devs e revisão

---

## 🏃 Sprint 6-7: Testes Unitários & Integração
**Status:** ✅ CONCLUÍDO (19/11/2026)
- ✅ Testes unitários dos services com repositórios simulados (`Finance.Tests/Unit`: BaseService, CategoryService, regras de categoria, JWT, resumo)
- ✅ Testes dos validadores FluentValidation (`ValidatorTests`)
- ✅ Testes de integração da API com `WebApplicationFactory` contra um PostgreSQL real (`Finance.Tests/Integration`: autenticação, isolamento entre usuários, regras HTTP, nomes de categoria, resumo, schema, produção)
- ✅ Testes rodam no CI a cada push, com um serviço `postgres:16`
- ⏳ Cobertura de 80%+: não medida ainda (`dotnet test /p:CollectCoverage=true` está disponível pelo coverlet)

**Desvio do plano:** em vez de Testcontainers, os testes usam um Postgres lido de `TEST_DB_CONNECTION` (serviço do GitHub Actions no CI, Postgres local em desenvolvimento).

**Objetivo:** Solidificar SOLID com testes

---

## 🏃 Sprint 8: O Frontend do Herói
**Status:** ✅ CONCLUÍDO (19/11/2026), nível 2
- ✅ SPA em React + TypeScript + Vite, servida pela própria API a partir de `wwwroot`
- ✅ Cadastro, login, logout e exclusão de conta; CRUD de categorias, receitas e despesas
- ✅ Lista de movimentações com filtros; dashboard com saldo, totais, gráfico por categoria e gráfico mês a mês (SVG feito à mão)
- ✅ Responsiva (até 360px) e com atenção a acessibilidade (rótulos, anúncios de leitura, contraste)
- ⏳ Dark mode: fora do escopo (nível 3)

**Objetivo:** Interface moderna e responsiva

---

## 🚀 Sprint 9: O DevOps Desbravador
**Status:** 🟡 PARCIAL (19/11/2026): pipeline pronto, deploy pendente da configuração das contas
- ✅ Dockerfile multi-stage (build do frontend, publish da API, imagem runtime sem root)
- ✅ CI no GitHub Actions: `backend` (com Postgres), `frontend` (lint, testes, build) e `docker` (build da imagem)
- ✅ Workflow de deploy: guarda (só `main` deste repositório), migrations bundle no Neon pela conexão direta, Deploy Hook do Render e espera de `/health`
- ✅ Segredos fora do repositório: environment `production` no GitHub e variáveis do serviço no Render ([DEPLOY.md](devops/DEPLOY.md))
- ⏳ Primeiro deploy: depende de o dono criar as contas no Render e no Neon e cadastrar os segredos

**Objetivo:** Deploy automatizado

---

## 🧠 Sprint 10: O Mestre da Escalabilidade
**Status:** 🔄 FUTURA
- [ ] Adicionar cache (Redis), mensageria (RabbitMQ)
- [ ] Implementar monitoramento e alertas (Prometheus, Grafana)
- [ ] Infraestrutura como código (Terraform)
- [ ] Orquestração com Kubernetes (opcional)

**Objetivo:** Sistema escalável e resiliente

---

## 🏆 Sprint Final: O Guardião da Produção
**Status:** 🔄 FUTURA
- [ ] Revisar segurança (OWASP)
- [ ] Testes de carga e performance
- [ ] Documentar tudo (README, Wiki, Swagger): README, diagramas e guia de deploy já feitos em 19/11/2026; Swagger existe só em Development
- [ ] Deploy em produção
- [ ] Monitorar, corrigir bugs, coletar feedback

**Objetivo:** Projeto publicado e pronto para produção

---

## 📊 Progress Summary

| Fase | Sprints | Status | Progresso |
|------|---------|--------|-----------|
| **SOLID Principles** | 1-5 | ✅ COMPLETE | 100% |
| **Testes & Integração** | 6-7 | ✅ CONCLUÍDO | 100% (cobertura não medida) |
| **Frontend** | 8 | ✅ CONCLUÍDO | 100% (nível 2) |
| **DevOps** | 9 | 🟡 PARCIAL | Docker e CI/CD prontos; primeiro deploy pendente |
| **Escalabilidade** | 10 | 🔄 FUTURA | 0% |
| **Produção** | Final | 🔄 FUTURA | 0% |

*Atualizado em 19/11/2026.*

---

## 🎓 Dicas de Gameficação
- ✅ Marque cada sprint como "concluída" ao terminar
- 🏆 Crie badges/conquistas pessoais por princípio
- 📈 Compartilhe progresso em redes sociais ou com amigos
- 🔄 Refaça etapas para fixar conceitos

**Bons estudos e boa jornada!** 🚀
