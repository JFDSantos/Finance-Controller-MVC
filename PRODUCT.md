# Product

<!-- impeccable:product-schema 1 -->

> Interview note: `impeccable init` normally interviews the product owner. This record was written by an
> unattended build agent that had no way to ask questions, so every fact below comes from the written
> project brief (Task 11 of the production plan) and the repository. Facts the brief did not state are
> marked **(inferred)** or listed as open decisions.

## Platform

web

## Stack

Decided by the project plan, not by the design agent: ASP.NET Core API (Clean Architecture, PostgreSQL,
cookie-based JWT auth) serving a Vite + React + TypeScript single-page app from `Finance.API/wwwroot`.
Hand-written CSS with design tokens; no UI framework. Node 22.

## Users

People in Brazil who want a clear monthly view of their own money: what came in, what went out, and
where it went. They open the app at home or on the phone to log a movement, check the month, or
understand a category. Secondary audience: recruiters and engineers evaluating the project as a
portfolio piece built to production standards.

## Product Purpose

Finance Controller records income and expenses ("movimentações"), groups them into user-defined
categories, and summarises each month on a dashboard (balance, totals, 12-month series). Success is
that a person can answer "how did this month go?" in seconds and trust the numbers they see.

## Positioning

A personal ledger that stays a ledger: numbers first, no gamification, no bank-integration upsell,
no advice. It is honest about what it is (manual entries, one currency) and executes that small scope
with production-grade craft. **(inferred from the brief's tone and out-of-scope list)**

## Operating Context

- Logged-in use is task work: register a movement, review a month, adjust categories.
- Sessions are cookie-based; a public demo account (`demo@financecontroller.app`) may be enabled per
  deployment so evaluators can try the app without registering.
- Used on phones (from 360px wide) as much as on desktop.

## Capabilities and Constraints

- Auth: register, login, logout, session restore, delete account (deletes all categories and movements).
- Categories, movements and the monthly dashboard arrive in later tasks; this record covers them as
  planned capability, not shipped.
- Language: Brazilian Portuguese for all UI copy, with correct accents.
- Currency: BRL only, formatted `R$ 1.234,50`. Dates `dd/mm/aaaa`.
- Out of scope: CSV export, goals, multi-currency.
- Terminology: "Painel" (dashboard), "Movimentações" (movements), "Categorias", "Receita" (income),
  "Despesa" (expense), "Saldo" (balance).

## Brand Commitments

- Name: Finance Controller. No logo asset exists; a typographic wordmark is acceptable. **(inferred)**
- Voice: calm, clear, trustworthy, a little warm; numbers first.
- Binding anti-references from the brief: no generic purple/blue fintech gradient template, no
  stock-illustration clichés, no emoji decoration.

## Evidence on Hand

- No testimonials, customers, user counts or press exist. Never fabricate them.
- The demo account's seeded data is synthetic and must be presented as a demo.

## Product Principles

1. Numbers first: the amount and its sign are the most important thing on any screen.
2. Trust through plainness: say exactly what happens, especially for destructive actions.
3. Small scope, finished: every state (loading, empty, error, rate-limited) is designed.
4. Same everywhere: the phone experience is the real one, not a reduced desktop.

## Accessibility & Inclusion

WCAG 2.1 AA contrast, full keyboard operation, visible focus, errors announced to assistive technology
(`aria-live`, `aria-describedby`), respects `prefers-reduced-motion`. Dark/light via
`prefers-color-scheme` is optional; if offered it must be correct in both.
