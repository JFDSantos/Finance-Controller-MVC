# Deploy do Finance Controller (Render + Neon + GitHub Actions)

Tudo roda em planos gratuitos: **Neon** (Postgres), **Render** (API + frontend em uma imagem Docker) e **GitHub Actions** (CI e deploy).

Fluxo: push na `main` -> workflow `CI` verde -> workflow `Deploy` aplica as migrations no Neon (conexão direta), chama o Deploy Hook do Render e espera `/health` responder 200.

> Aviso: as páginas de documentação do Render e do Neon não puderam ser consultadas ao escrever este guia (rede restrita). Os nomes de chaves e menus abaixo vêm de conhecimento prévio; confira nas páginas listadas em "Referências" se algo estiver diferente na tela.

## 1. Neon (banco de dados)

1. Crie uma conta em neon.com e um projeto (região mais próxima, por exemplo `aws-us-east-1`, ou a que o Neon oferecer para São Paulo, se houver).
2. No painel do projeto, botão **Connect**. Anote **duas** strings:
   - **Pooled** (host contém `-pooler`): usada pelo app no Render.
   - **Direct** (desmarque "Connection pooling"): usada apenas para migrations (o pooler não é adequado para DDL/locks de migration).
3. O Neon entrega uma URL `postgres://usuario:senha@host/banco?sslmode=require`. O app e o bundle de migrations esperam o formato Npgsql `chave=valor`:

```
URL:     postgres://USUARIO:SENHA@ep-xxxx-pooler.REGIAO.aws.neon.tech/BANCO?sslmode=require
Npgsql:  Host=ep-xxxx-pooler.REGIAO.aws.neon.tech;Port=5432;Database=BANCO;Username=USUARIO;Password=SENHA;SSL Mode=Require
```

Para a conexão direta, use o host sem `-pooler`. Se a senha tiver caracteres como `;` ou `=`, coloque-a entre aspas simples no Npgsql: `Password='...'`.

## 2. Render (serviço web)

1. Crie conta em render.com e conecte sua conta do GitHub.
2. **New > Blueprint**, escolha este repositório. O Render lê `render.yaml` (serviço Docker `finance-controller`, plano `free`, região `oregon`, health check `/health`, auto deploy desligado).
3. Preencha as variáveis marcadas como `sync: false` quando solicitado (tabela abaixo).
4. Em **Settings > Deploy Hook** do serviço, copie a URL do hook (guarde em segredo; quem tem a URL dispara deploys).

Não há região do Render no Brasil; `oregon` é a região padrão do plano gratuito. Se preferir menor latência para o Brasil, troque `region` em `render.yaml` por uma região leste dos EUA oferecida pelo Render (confira a lista na documentação).

### Variáveis de ambiente

| Variável | Obrigatória | Valor / como gerar |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | sim | `Production` (já fixa no `render.yaml`) |
| `ConnectionStrings__DefaultConnection` | sim | String Npgsql **pooled** do Neon (seção 1) |
| `Jwt__Key` | sim | Mínimo 32 caracteres. Gere com `openssl rand -base64 48`. O app não inicia sem ela |
| `Demo__Enabled` | não | `true` para criar a conta demo; padrão `false` |
| `Demo__Password` | só se demo | Senha da conta demo |
| `VITE_DEMO_EMAIL` | só se demo | `demo@financecontroller.app` (usada no build da imagem) |
| `VITE_DEMO_PASSWORD` | só se demo | Mesma senha de `Demo__Password` (usada no build da imagem) |
| `RateLimit__LoginPerMinute` | não | Padrão `5` |
| `RateLimit__RegisterPerHour` | não | Padrão `10` |

## 3. GitHub (segredos, variáveis e ambiente)

1. **Settings > Environments > New environment**: `production`. (Opcional: exigir aprovação manual.)
2. No ambiente `production` (ou em Settings > Secrets and variables > Actions), cadastre:
   - Secret `DATABASE_URL_DIRECT`: string Npgsql **direta** do Neon, com `SSL Mode=Require`.
   - Secret `RENDER_DEPLOY_HOOK_URL`: URL do Deploy Hook do Render.
   - Variable (ou secret) `APP_URL`: URL pública, por exemplo `https://finance-controller.onrender.com` (sem barra final).
3. O workflow `Deploy` nunca imprime esses valores.

## 4. Conta demo (opcional)

Defina no Render `Demo__Enabled=true`, `Demo__Password=<senha>`, `VITE_DEMO_EMAIL=demo@financecontroller.app` e `VITE_DEMO_PASSWORD=<mesma senha>`. O Render repassa variáveis de ambiente ao build Docker como build args; o `Dockerfile` as recebe como `ARG` e o frontend exibe o botão "Experimentar com a conta demo" somente quando as duas existem.

**Essas credenciais são públicas por desenho**: ficam embutidas no JavaScript servido ao navegador. Use uma senha exclusiva e não dê privilégios a essa conta.

## 5. Primeiro deploy

Ordem: migrations primeiro, depois o app. O workflow `Deploy` já faz nessa ordem. Para o primeiro deploy:

1. Garanta que os segredos do GitHub e as variáveis do Render estão preenchidos.
2. Faça merge/push na `main` e espere o `CI` ficar verde, ou rode `Deploy` manualmente em **Actions > Deploy > Run workflow**.
3. O Blueprint do Render pode tentar um build inicial ao ser criado; se ele falhar por falta de variáveis, ignore e dispare o deploy pelo workflow.

## 6. Como verificar

1. `curl https://SEU-APP.onrender.com/health` deve retornar `{"status":"ok"}` (200).
2. Abra a URL, **registre** uma conta, faça **login** e confira o **dashboard** (a primeira requisição pode demorar, veja limitações).
3. Se a demo estiver ativa, teste o botão "Experimentar com a conta demo".

## 7. Rotação de segredos

- **`Jwt__Key`**: gere outra com `openssl rand -base64 48`, atualize no Render e reinicie/redeploy. Todas as sessões ativas são invalidadas (os usuários precisam entrar de novo).
- **Senha demo**: altere `Demo__Password` e `VITE_DEMO_PASSWORD` no Render e faça novo deploy (o build precisa rodar de novo para o frontend receber a nova senha). Se a conta demo já existe no banco com a senha antiga, redefina-a no banco ou remova o usuário demo antes do deploy.
- **Deploy Hook**: regenere no Render e atualize `RENDER_DEPLOY_HOOK_URL`.

## 8. Limitações conhecidas

- Render free: a instância hiberna após ~15 min sem tráfego; a primeira requisição depois disso leva em torno de 30 a 60 s. O passo de espera do deploy tolera até 10 min.
- Neon free: o banco "escala a zero" quando ocioso; a primeira consulta depois disso tem alguns segundos extras.
- Não há verificação de e-mail nem recuperação de senha.
- O app confia em `X-Forwarded-For` (rate limit por IP) somente porque só é acessível através do proxy do Render. Não exponha a porta 8080 diretamente em outro ambiente.
- Sem Deploy Hook automático: o auto deploy do Render está desligado; só o workflow publica.

## 9. Custos

Neon free, Render free (web service) e GitHub Actions (repositório público ou cota gratuita): R$ 0. Confira os limites atuais dos planos nas páginas de preços.

## Referências

- Render Blueprint spec: https://render.com/docs/blueprint-spec
- Render Deploy Hooks: https://render.com/docs/deploy-hooks
- Render Docker: https://render.com/docs/docker
- Render plano gratuito: https://render.com/docs/free
- Neon, conexão: https://neon.com/docs/connect/connect-from-any-app
- Neon, pooling: https://neon.com/docs/connect/connection-pooling
- EF Core migrations bundles: https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying#bundles
