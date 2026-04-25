# 💰 Finance Controller MVC

Um sistema completo de controle financeiro pessoal construído com **C#**, **ASP.NET Core** e arquitetura em camadas, aplicando as melhores práticas do mercado.

---

## 🎯 Sobre o Projeto

Finance Controller é uma aplicação fullstack para gerenciar receitas, despesas e categorias financeiras. O projeto foi desenvolvido seguindo princípios de **Clean Architecture**, **SOLID** e **DDD**, com foco em aprendizado prático e excelência técnica.

### Principais Objetivos
- ✅ Gerenciar receitas e despesas de forma simples e intuitiva
- ✅ Aprender arquitetura limpa, testes automatizados e DevOps
- ✅ Implementar autenticação segura com JWT
- ✅ Criar um pipeline de CI/CD profissional
- ✅ Escalar para um projeto pronto para produção

---

## 🛠️ Tecnologias

### Backend
- **Linguagem**: C# 12+
- **Framework**: ASP.NET Core 8+
- **ORM**: Entity Framework Core
- **Banco de Dados**: SQL Server Express
- **Autenticação**: JWT (JSON Web Tokens)
- **Validação**: FluentValidation
- **Mapeamento**: AutoMapper
- **API**: RESTful com Swagger/OpenAPI

### Frontend (Em Desenvolvimento)
- **Framework**: React, Angular ou Vue (A definir)
- **Estilos**: Responsivo e acessível

### DevOps (Roadmap)
- **Containerização**: Docker
- **CI/CD**: GitHub Actions
- **Deploy**: Azure / Vercel
- **Monitoramento**: Prometheus + Grafana

---

## 📁 Estrutura do Projeto

```
Finance-Controller-MVC/
├── Finance.API/                 # REST API & Controllers
│   ├── Controllers/             # Endpoints
│   ├── Middleware/              # Exception Handling
│   └── Program.cs               # Configuração da API
│
├── Finance.Application/         # Lógica de Negócio
│   ├── Services/                # Serviços de aplicação
│   ├── Interfaces/              # Contratos
│   ├── Validators/              # Validação (FluentValidation)
│   ├── ViewModel/               # DTOs
│   └── Mappings/                # AutoMapper Profiles
│
├── Finance.Domain/              # Entidades & Modelos
│   └── Models/                  # Domain Models
│
├── Finance.Infrastructure/      # Persistência & Dados
│   ├── Repositories/            # Implementação de repositories
│   ├── Data/                    # DbContext
│   └── Migrations/              # EF Core Migrations
│
└── Finance.Web/                 # Frontend (Projeto Futuro)
    └── Views/                   # Interface de usuário
```

### Arquitetura em Camadas

```
┌─────────────────────────────────┐
│      Finance.API (Presentation) │  ← Controllers, Middleware
├─────────────────────────────────┤
│    Finance.Application (Domain) │  ← Services, Validators
├─────────────────────────────────┤
│  Finance.Domain (Core Business) │  ← Models, Interfaces
├─────────────────────────────────┤
│  Finance.Infrastructure (Data)  │  ← Repositories, DbContext
└─────────────────────────────────┘
```

---

## 🚀 Começando

### Pré-requisitos
- **.NET SDK 8.0+** - [Download](https://dotnet.microsoft.com/download)
- **SQL Server Express** - [Download](https://www.microsoft.com/pt-br/sql-server/sql-server-express)
- **Git**

### Instalação

1. **Clone o repositório**
```bash
git clone https://github.com/seu-usuario/Finance-Controller-MVC.git
cd Finance-Controller-MVC
```

2. **Restaure as dependências**
```bash
dotnet restore
```

3. **Configure o banco de dados**
   - Atualize a string de conexão em `Finance.API/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=seu-servidor\\SQLEXPRESS;Database=FinanceControllerDB;Trusted_Connection=true;Encrypt=False;"
   }
   ```

4. **Execute as migrations**
```bash
cd Finance.Infrastructure
dotnet ef database update --startup-project ../Finance.API
```

5. **Inicie a API**
```bash
cd Finance.API
dotnet run
```

A API estará disponível em `https://localhost:5150` (ou a porta configurada).

### Documentação da API
Acesse a documentação interativa do Swagger:
```
https://localhost:5150/swagger/index.html
```

---

## 📚 Funcionalidades Principais

### Controllers Implementados

#### 🔐 Auth Controller
- `POST /api/auth/register` - Registrar novo usuário
- `POST /api/auth/login` - Fazer login

#### 💰 Category Controller
- `GET /api/categories` - Listar categorias
- `POST /api/categories` - Criar categoria
- `PUT /api/categories/{id}` - Atualizar categoria
- `DELETE /api/categories/{id}` - Deletar categoria

#### 💳 Expense Controller
- `GET /api/expenses` - Listar despesas
- `POST /api/expenses` - Criar despesa
- `PUT /api/expenses/{id}` - Atualizar despesa
- `DELETE /api/expenses/{id}` - Deletar despesa

#### 💵 Income Controller
- `GET /api/incomes` - Listar receitas
- `POST /api/incomes` - Criar receita
- `PUT /api/incomes/{id}` - Atualizar receita
- `DELETE /api/incomes/{id}` - Deletar receita

---

## 🧪 Testes

### Executar testes unitários
```bash
dotnet test
```

### Executar com cobertura de código
```bash
dotnet test /p:CollectCoverage=true
```

---

## 🎓 Roadmap Gamificado

Este projeto segue um **roadmap estruturado em sprints**, com foco em aprendizado prático e boas práticas:

### 📋 Sprints
- **Sprint 1**: O Despertar do Dev (MVP, Setup, Autenticação)
- **Sprint 2**: O Caminho do Código Limpo (SOLID, DDD, Testes)
- **Sprint 3**: O Frontend do Herói (SPA Responsiva)
- **Sprint 4**: O DevOps Desbravador (Docker, CI/CD)
- **Sprint 5**: O Mestre da Escalabilidade (Cache, Monitoramento)
- **Sprint Final**: O Guardião da Produção (Segurança, Deploy)

📖 Veja o roadmap completo em [FinanceController-Roadmap.md](./FinanceController-Roadmap.md)

---

## 🔐 Segurança

- ✅ Autenticação com JWT
- ✅ Senhas hasheadas com bcrypt
- ✅ Validação de entrada com FluentValidation
- ✅ CORS configurado
- ⚠️ Review de segurança (OWASP) planejado para Sprint Final

### Variáveis de Ambiente Obrigatórias
Crie um arquivo `.env` com:
```env
JWT_KEY=sua-chave-secreta-super-segura
DATABASE_CONNECTION=Server=...
```

---

## 📖 Padrões de Projeto Implementados

- **Repository Pattern** - Abstração de dados
- **Unit of Work** - Gerenciamento de transações
- **Dependency Injection** - Inversão de controle
- **DTO (Data Transfer Object)** - Transfer de dados entre camadas
- **Validator Pattern** - Validação centralizada
- **Exception Handling Middleware** - Tratamento global de erros

