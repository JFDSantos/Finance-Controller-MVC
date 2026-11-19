# 📚 Documentação de Desenvolvimento — Finance-Controller-MVC

**Bem-vindo à central de documentação do projeto!**

Este diretório contém toda a documentação de processos, arquitetura, testes e DevOps do Finance-Controller-MVC.

---

## 📂 Estrutura de Pastas

```
docs/
├── README.md (você está aqui)
├── Roadmap.md                ← Plano em sprints e progresso
├── architecture/             ← Padrões, design e diagramas
│   ├── README.md             ← Camadas e explicação dos diagramas
│   ├── SOLID-Principles.md
│   └── diagrams/             ← architecture, data-model, deploy-flow (.html e .svg)
├── development/README.md     ← Guias de desenvolvimento
├── testing/README.md         ← Estratégia e execução de testes
├── devops/
│   ├── README.md             ← Visão de CI/CD e infraestrutura
│   └── DEPLOY.md             ← Passo a passo de deploy (Render + Neon + GitHub Actions)
└── guides/README.md          ← Guias rápidos e troubleshooting
```

Documentos planejados (ainda não escritos): Git Workflow, Code Standards, Common Issues, Performance Tips, Security Checklist.

---

## 🎯 Quick Navigation

### 🏗️ **Arquitetura & Design**
- [Arquitetura e diagramas](architecture/README.md) — Camadas, modelo de dados e fluxo de deploy
- [SOLID Principles](architecture/SOLID-Principles.md) — Princípios fundamentais (SRP, OCP, LSP, ISP, DIP)
- [Roadmap](Roadmap.md) — Plano geral do projeto em fases

### 💻 **Desenvolvimento Local**
- [Rodando localmente](../README.md#-rodando-localmente) — Pré-requisitos, variáveis de ambiente, migrations, API e frontend
- [Development](development/README.md) — Guias e checklists

### 🧪 **Testes**
- [Testing](testing/README.md) — Testes unitários, de integração (Postgres real) e do frontend

### 🚀 **DevOps & Deploy**
- [DEPLOY.md](devops/DEPLOY.md) — Render, Neon, segredos e primeiro deploy
- [DevOps](devops/README.md) — CI/CD e infraestrutura

### 📋 **Guias Rápidos**
- [Guides](guides/README.md) — Snippets e troubleshooting

---

## 🚀 Começar Rápido

### 1️⃣ **Entender Arquitetura**
```bash
# Leia SOLID primeiro (base de tudo)
# Veja: docs/architecture/SOLID-Principles.md

# Veja plano geral
# Veja: docs/Roadmap.md
```
```bash
# Seguir Git workflow
git checkout -b feature/sua-feature
# Editar código, testar, fazer commit
git push origin feature/sua-feature
# Criar Pull Request

# Git Workflow: documento planejado
```

---

## 📊 Status da Documentação

| Seção | Status | Última Atualização |
|-------|--------|------------------|
| **SOLID Principles** | ✅ Completo | 26/04/2026 |
| **Roadmap** | ✅ Atualizado | 19/11/2026 |
| **Arquitetura e diagramas** | ✅ Completo | 19/11/2026 |
| **Setup Local (README principal)** | ✅ Completo | 19/11/2026 |
| **Testing** | ✅ Atualizado | 19/11/2026 |
| **Deploy (DEPLOY.md)** | ✅ Completo | 19/11/2026 |
| **Git Workflow / Code Standards** | 🔄 Planejado | - |
| **Common Issues / Performance Tips / Security Checklist** | 🔄 Planejado | - |

-------|--------|------------------|
| **SOLID Principles** | ✅ Completo | 26/04/2026 |
| **Roadmap** | ✅ Completo | 26/04/2026 |
| **Setup Local** | 🔄 Em Progresso | - |
| **Git Workflow** | 🔄 Em Progresso | - |
| **Code Standards** | 🔄 Em Progresso | - |
| **Unit Testing** | 🔄 Planejado | - |
| **Integration Testing** | 🔄 Planejado | - |
| **CI/CD Pipeline** | 🔄 Planejado | - |
| **Docker Setup** | 🔄 Planejado | - |
| **Common Issues** | 🔄 Planejado | - |
| **Performance Tips** | 🔄 Planejado | - |
| **Security Checklist** | 🔄 Planejado | - |

---

## 🎓 Roadmap de Documentação

### Sprint Atual (SOLID)
- ✅ Documentação de SOLID Principles completa
- ✅ Todos os 5 princípios explicados com exemplos
- ✅ Analogias financeiras para compreensão

### Próximos Sprints
- 📋 **Sprint 6-7**: Guias de Testes Unitários e Integração
- 📋 **Sprint 8**: Arquitetura Avançada & Design Patterns
- 📋 **Sprint 9**: DevOps & CI/CD Pipeline
- 📋 **Sprint 10**: Deployment & Monitoramento

---

## 🤝 Contribuir para a Documentação

Se encontrar erros ou tiver sugestões, contribua!

1. Crie um branch: `git checkout -b docs/melhoria-xyz`
2. Edite o arquivo relevante
3. Faça commit: `git commit -m "docs: melhorar seção XYZ"`
4. Push e crie Pull Request

---

## 📞 Suporte

- **Dúvidas sobre SOLID?** Veja [architecture/SOLID-Principles.md](architecture/SOLID-Principles.md)
- **Problema de setup?** Veja [Rodando localmente](../README.md#-rodando-localmente) e [guides/README.md](guides/README.md)
- **Precisa escrever testes?** Veja [testing/README.md](testing/README.md)

---

**Última Atualização**: 19 de Novembro de 2026  
**Mantido por**: Jeferson Santos  
**Versão**: 1.0
