---
version: 1
slug: "frontend-src-pages-login-tsx"
primary_target: "frontend/src/pages/Login.tsx"
related_targets: ["frontend/src/pages/Register.tsx","frontend/src/components/AppShell.tsx"]
---

# Surface brief: authentication screens and app shell

Scope: `/login`, `/register`, the authenticated shell (header, navigation, user menu, delete-account dialog) and the placeholder pages it frames. Visitor mode: **Operate**. Build path: code-led (Operate).

Audience and job: a person in Brazil signing in on a phone or laptop to look at their month; an evaluator trying the demo account. Task: get in (or create an account) in under a minute, then find Painel, Movimentações and Categorias without thinking.

Constraints: pt-BR copy, WCAG AA, keyboard-complete, 360px to desktop, no fintech gradient, no illustrations, no emoji.

Process note: the direction roll ran degraded (roll service unreachable through the network proxy, no challengers, no quality-bar boards). The run was unattended (subagent, no question tool, no decision page reader), so the assigned grounded candidate was built without a user choice. Grounded list, by resonance: 1 Cédulas do Real (banknote tints), 2 Aloísio Magalhães identity programs, 3 feira-livre price placards, 4 boleto bancário, 5 Metrô de São Paulo wayfinding, 6 azulejos of Athos Bulcão and Portinari (assigned), 7 Burle Marx gardens and the Copacabana calçadão.

## Direction contract

THESIS: "Painel" means both a dashboard and an azulejo tile panel. The app is built like one of Athos Bulcão's Brasília panels: a small set of square modules, rotated, adding up to a whole month. It refuses the category default of a gradient hero card with a big balance number and purple glow.

OWN-WORLD: glazed tile white ground (cool, never cream), cobalt glaze as the committed shell colour (header and auth panel), ink near cobalt-black, Bulcão ochre as the second glaze for selection and the active-navigation mark; income green and expense red only as state. Public Sans, self-hosted, tabular figures. Flat glaze: hairline borders, small radii (4 to 6px), one soft shadow for floating layers only.

STORY: the visitor sees a calm, unmistakably built product, signs in with one clear action (or the demo), and lands in a shell whose three destinations are always visible.

FIRST VIEWPORT: desktop login is two columns: left 5/12 a cobalt panel holding a 7-by-5 grid of tile modules (the shape of a month) with the wordmark above and one sentence below; right, a 400px form column centred: heading "Entrar", email, senha, primary "Entrar", secondary demo button when configured, link to cadastro. Mobile: a cobalt tile band (one week, 7 tiles) with the wordmark, then the form.

FORM: grounded candidate 6 of 7 (azulejo panels). seed:cab3e26782614dbf. Signature move: the tile module, a square with a quarter-disc and a bar, placed in seeded rotations; in later tasks the dashboard month renders as this tile grid with each day's tile carrying data.

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance
