# Finance Controller: frontend

React 19 + TypeScript + Vite single-page app. In production it is built into
`../Finance.API/wwwroot` and served by the API from the same origin, so the
`jwt` cookie (HttpOnly, Secure, SameSite=Strict) works without CORS. The app
never stores tokens in JavaScript.

Requires Node 22.

## Develop

Start the API first (https profile, `https://localhost:7113`), then:

```bash
cd frontend
npm install
npm run dev
```

Open http://localhost:5173. The dev server proxies every `/api` request to the
API (`secure: false` accepts the ASP.NET Core development certificate). To point
the proxy somewhere else, set `VITE_API_PROXY_TARGET`, for example
`VITE_API_PROXY_TARGET=http://localhost:5195 npm run dev` (the API's `http` launch profile;
the default is the `https` profile, `https://localhost:7113`).

## Test

```bash
npm test          # vitest run (jsdom + Testing Library)
npm run test:watch
npm run lint
```

## Build

```bash
npm run build
```

Type-checks, then writes the bundle to `../Finance.API/wwwroot` (emptied first;
the folder is git-ignored). Run the API and open its URL to use the built app.

## Optional demo button

The login screen shows "Experimentar com a conta demo" only when **both**
variables are defined at build time (or when starting `npm run dev`):

| Variable | Value |
| --- | --- |
| `VITE_DEMO_EMAIL` | the public demo account, `demo@financecontroller.app` |
| `VITE_DEMO_PASSWORD` | the password the API was started with (`Demo__Password`) |

The demo account is public by design, so these values end up in the bundle.
Never put any other credential in a `VITE_*` variable, and never commit them:
pass them in the environment, or in an untracked `.env.local`.

## Structure

- `src/api/`: `client.ts` (fetch wrapper, `ApiError`, session-expired event) and `types.ts`
- `src/i18n/errors.ts`: pt-BR messages for every API error code
- `src/auth/`: `AuthContext` (session restore, login, register, logout, delete account), route guards, form validation
- `src/components/`: app shell, user menu, delete-account dialog, fields, the tile panel
- `src/pages/`: Login, Register, placeholder pages, 404
- `src/styles/`: design tokens and component styles (see `DESIGN.md` at the repository root)
