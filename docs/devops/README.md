# 🚀 DevOps — CI/CD, Deploy & Infraestrutura

Documentação sobre pipeline de deploy, containerização e infraestrutura.

---

## 📚 Documentos

### CI/CD Pipeline (Planejado)
**Status**: 🔄 Planejado

Configurar GitHub Actions:
- Trigger: push em branches
- Build .NET
- Rodar testes
- Verificar cobertura
- Publicar artefatos
- Notificações

### Docker Setup (Planejado)
**Status**: 🔄 Planejado

Containerizar aplicação:
- Dockerfile para API
- Docker Compose para stack (API + DB)
- Multi-stage builds
- Tamanho otimizado de imagem
- Push para registry

### Deployment Guide (Planejado)
**Status**: 🔄 Planejado

Deploy em produção:
- Ambientes: dev, staging, prod
- Secrets management
- Database migrations
- Health checks
- Rollback strategy
- Monitoring

---

## 🏗️ Infraestrutura Alvo

```
┌─────────────────────────────────────┐
│       GitHub (Repository)           │
│  ↓ Trigger GitHub Actions           │
├─────────────────────────────────────┤
│    GitHub Actions (CI Pipeline)     │
│  • Build & Test                     │
│  • Code Coverage Check              │
│  • Build Docker Image               │
│  ↓                                   │
├─────────────────────────────────────┤
│    Docker Registry (ghcr.io)        │
│  ↓ Pull Image                       │
├─────────────────────────────────────┤
│    Produção (Azure/Heroku/AWS)      │
│  • API Container                    │
│  • Database (PostgreSQL)            │
│  • Load Balancer                    │
│  • Monitoring (Application Insights)│
└─────────────────────────────────────┘
```

---

## 🎯 Planilha de DevOps

| Componente | Status | Sprint |
|-----------|--------|--------|
| GitHub Actions | 🔄 Planejado | 9 |
| Docker API | 🔄 Planejado | 9 |
| Docker Compose | 🔄 Planejado | 9 |
| Deploy Staging | 🔄 Planejado | 9 |
| Deploy Produção | 🔄 Planejado | 10 |
| Monitoring | 🔄 Planejado | 10 |
| Alertas | 🔄 Planejado | 10 |

---

## ⚡ Quick Reference

### Local Docker
```bash
# Build imagem
docker build -t finance-api:latest .

# Rodar container
docker run -p 5001:5001 finance-api:latest

# Docker Compose
docker-compose up
```

### GitHub Actions
```yaml
# .github/workflows/ci.yml (futuro)
on: [push]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-dotnet@v1
      - run: dotnet test
```

---

## 🤔 Troubleshooting

### Problema: "Imagem Docker muito grande"
→ Use multi-stage builds
→ Remova files desnecessários
→ Use .dockerignore

### Problema: "Build GitHub Actions demora"
→ Cache dependências NuGet
→ Use layers do Docker
→ Considere matrix builds

### Problema: "Prod diferente de local"
→ Use Docker Compose para igualar amb.
→ Environment variables para config
→ Versionamento de imagens

---

## 📚 Leitura Recomendada

1. **GitHub Actions**: https://docs.github.com/en/actions
2. **Docker**: https://docs.docker.com/
3. **Docker Compose**: https://docs.docker.com/compose/
4. **Microsoft Deploy .NET**: https://docs.microsoft.com/en-us/dotnet/core/docker/build-container

---

## 🎓 Roadmap de DevOps

### Sprint 9: CI/CD & Docker
- [ ] Criar Dockerfile
- [ ] Docker Compose com DB
- [ ] GitHub Actions pipeline
- [ ] Deploy staging automático
- [ ] Testes em CI

### Sprint 10: Produção
- [ ] Deploy produção
- [ ] SSL/HTTPS
- [ ] Monitoring & Alertas
- [ ] Logs estruturados
- [ ] Backup strategy

---

**Próximos documentos**: CI/CD Pipeline, Docker Setup, Deployment Guide  
**Última Atualização**: 26 de Abril de 2026
