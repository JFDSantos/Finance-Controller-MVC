---
name: Finance Controller
description: Personal finance ledger in pt-BR, built like an azulejo tile panel on a cobalt shell.
colors:
  glaze: "#f3f6fb"
  surface: "#ffffff"
  ink: "#172036"
  ink-muted: "#4a5573"
  line: "#d3dbea"
  field-border: "#7483a3"
  cobalt: "#1d44a6"
  cobalt-deep: "#15347f"
  cobalt-grout: "#13307a"
  cobalt-wash: "#e6edfb"
  on-cobalt: "#ffffff"
  on-cobalt-muted: "#c9d6f5"
  ochre: "#e3a51f"
  ochre-wash: "#fbefd2"
  danger: "#b3261e"
  danger-deep: "#8f1d17"
  danger-wash: "#fcebea"
  income: "#18794e"
  expense: "#b3261e"
  chart-income: "#16916e"
  chart-expense: "#b3261e"
  chart-expense-wash: "#f2cfcc"
  chart-category: "#1d44a6"
  chart-grid: "#e6ebf3"
  chart-baseline: "#aab5cb"
typography:
  page-title:
    fontFamily: "Public Sans Variable, Public Sans, system-ui, sans-serif"
    fontSize: "2rem"
    fontWeight: 700
    lineHeight: 1.2
    letterSpacing: "-0.02em"
  auth-title:
    fontFamily: "Public Sans Variable, Public Sans, system-ui, sans-serif"
    fontSize: "1.75rem"
    fontWeight: 700
    lineHeight: 1.2
    letterSpacing: "-0.02em"
  dialog-title:
    fontFamily: "Public Sans Variable, Public Sans, system-ui, sans-serif"
    fontSize: "1.5rem"
    fontWeight: 700
    lineHeight: 1.2
  lead:
    fontFamily: "Public Sans Variable, Public Sans, system-ui, sans-serif"
    fontSize: "1.125rem"
    fontWeight: 400
    lineHeight: 1.5
  body:
    fontFamily: "Public Sans Variable, Public Sans, system-ui, sans-serif"
    fontSize: "1rem"
    fontWeight: 400
    lineHeight: 1.5
    fontFeature: "tnum"
  label:
    fontFamily: "Public Sans Variable, Public Sans, system-ui, sans-serif"
    fontSize: "0.875rem"
    fontWeight: 600
    lineHeight: 1.5
  wordmark:
    fontFamily: "Public Sans Variable, Public Sans, system-ui, sans-serif"
    fontSize: "1.0625rem"
    fontWeight: 700
    lineHeight: 1
    letterSpacing: "-0.01em"
rounded:
  sm: "4px"
  md: "6px"
  lg: "10px"
  pill: "999px"
spacing:
  "1": "4px"
  "2": "8px"
  "3": "12px"
  "4": "16px"
  "5": "24px"
  "6": "32px"
  "7": "48px"
  "8": "64px"
components:
  button-primary:
    backgroundColor: "{colors.cobalt}"
    textColor: "{colors.on-cobalt}"
    rounded: "{rounded.md}"
    padding: "0 24px"
    height: "44px"
  button-primary-hover:
    backgroundColor: "{colors.cobalt-deep}"
  button-secondary:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.cobalt}"
    rounded: "{rounded.md}"
    padding: "0 24px"
    height: "44px"
  button-secondary-hover:
    backgroundColor: "{colors.cobalt-wash}"
    textColor: "{colors.cobalt-deep}"
  button-danger:
    backgroundColor: "{colors.danger}"
    textColor: "{colors.on-cobalt}"
    rounded: "{rounded.md}"
    padding: "0 24px"
    height: "44px"
  button-disabled:
    backgroundColor: "{colors.glaze}"
    textColor: "{colors.ink-muted}"
  input:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.md}"
    padding: "0 12px"
    height: "44px"
  shell-header:
    backgroundColor: "{colors.cobalt}"
    textColor: "{colors.on-cobalt}"
  nav-link:
    textColor: "{colors.on-cobalt-muted}"
    padding: "0 12px"
    height: "64px"
  nav-link-active:
    textColor: "{colors.on-cobalt}"
  notice-info:
    backgroundColor: "{colors.cobalt-wash}"
    textColor: "{colors.ink}"
    rounded: "{rounded.md}"
    padding: "12px 16px"
  notice-danger:
    backgroundColor: "{colors.danger-wash}"
    textColor: "{colors.danger-deep}"
    rounded: "{rounded.md}"
    padding: "12px 16px"
  menu-panel:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.lg}"
    padding: "8px"
  dialog:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.lg}"
    padding: "24px"
  button-quiet:
    backgroundColor: "transparent"
    textColor: "{colors.ink-muted}"
    rounded: "{rounded.md}"
    padding: "0 12px"
    height: "44px"
  button-quiet-hover:
    backgroundColor: "{colors.cobalt-wash}"
    textColor: "{colors.cobalt-deep}"
  icon-button:
    backgroundColor: "transparent"
    textColor: "{colors.ink-muted}"
    rounded: "{rounded.md}"
    size: "44px"
  segmented-option-selected:
    backgroundColor: "{colors.cobalt}"
    textColor: "{colors.on-cobalt}"
    height: "44px"
  segmented-option:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.ink-muted}"
    height: "44px"
  tag-recurring:
    backgroundColor: "{colors.cobalt-wash}"
    textColor: "{colors.cobalt-deep}"
    rounded: "{rounded.sm}"
    padding: "2px 8px"
  ledger:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.lg}"
  state-panel:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.lg}"
    padding: "48px 24px"
  summary-panel:
    backgroundColor: "{colors.cobalt-grout}"
    rounded: "{rounded.lg}"
    padding: "6px"
  summary-tile:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.ink}"
    padding: "16px"
  summary-tile-balance:
    backgroundColor: "{colors.cobalt}"
    textColor: "{colors.on-cobalt}"
    padding: "24px 16px"
  chart-panel:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.lg}"
    padding: "16px"
  chart-toggle-pressed:
    backgroundColor: "{colors.cobalt-wash}"
    textColor: "{colors.cobalt-deep}"
    rounded: "{rounded.md}"
    height: "44px"
---

# Design System: Finance Controller

## Overview

**Creative North Star: "The Tile Panel"**

In Brazilian Portuguese a *painel* is both a dashboard and an azulejo tile panel, like the ones Athos Bulcão set into Brasília's buildings: a handful of square modules, each a simple figure, rotated and repeated until they add up to a wall. Finance Controller is built the same way. The shell is cobalt glaze, the ground is glazed white, ochre is the second glaze that marks where you are, and the one figure that carries the identity is the tile module (a square holding a quarter-disc, a half-disc or a diagonal, turned in steps of 90 degrees). A month is seven tiles wide.

This is an Operate surface: the world contributes type, palette, density and that one module, never the layout or the controls. Navigation reads as navigation, forms are plain labelled fields, and the numbers (tabular figures everywhere) stay the loudest thing on any data screen. The product brief rules out the purple/blue fintech gradient, stock illustration and emoji decoration; the flat cobalt here is glaze, not gradient, and nothing glows.

Light only for now. The scene is a phone in daylight or a laptop in the evening at home; the glazed white ground serves both. A dark theme is optional per PRODUCT.md and has not been designed: do not ship one by inverting tokens.

**Key Characteristics:**
- Cobalt shell (header, auth panel) over a cool glazed-white ground; never cream.
- One signature module (the tile) used as identity on the auth panel, the mark and the 404 panel, and reserved for data in the dashboard.
- Public Sans, self-hosted, with tabular numerals on by default.
- Flat glaze: hairline borders, 4 to 10px radii, a single soft shadow only for layers that float.
- Every state designed: pending labels, field errors, form errors, notices, disabled, empty.

## Colors

A restrained-but-committed palette: neutrals tinted toward cobalt, cobalt owning the shell, ochre as the single second glaze, red and green only when they mean money or danger.

### Primary
- **Cobalt Glaze** (`cobalt`): the shell. Header background, auth panel tiles, primary buttons, links, the focus ring on light surfaces, the caret and native control accent.
- **Deep Cobalt** (`cobalt-deep`): hover and pressed state of cobalt actions and links.
- **Grout Cobalt** (`cobalt-grout`): the auth panel's background, which shows between tiles as grout lines.
- **Cobalt Wash** (`cobalt-wash`): info notices, hover rows in menus, secondary-button hover.

### Secondary
- **Bulcão Ochre** (`ochre`): the active-navigation bar, the user avatar, roughly one tile in six, the mark's accent quarter, and the focus ring on cobalt. Never body text.
- **Ochre Wash** (`ochre-wash`): text selection.

### Tertiary
- **Alarm Red** (`danger`, `danger-deep`, `danger-wash`): field and form errors, the destructive menu item and the delete button. Also the expense colour (`expense`).
- **Ledger Green** (`income`): income amounts only (used from the movements and dashboard tasks onward).

### Chart marks
Chart colours do one job each and were checked with the dataviz palette validator against the white chart surface (light only; no dark theme exists).
- **Income mark** (`chart-income`, `#16916e`) and **Expense mark** (`chart-expense`, `#b3261e`): the two series of the 12-month chart. The income mark is a lighter step of the ledger green so the pair separates under colour-vision deficiency (worst CVD ΔE 10.6, normal-vision ΔE 28.7, both ≥ 3:1 on white). The ledger's own `income` green (`#18794e`) against the red only reached ΔE 6.5 (warn band), which is why the mark is not the text colour.
- **Category bars** (`chart-category`, cobalt): expenses by category is one series of nominal categories, so every bar takes the same hue; the names are direct labels. Never colour categories by rank or by value.
- **Spent meter** (`chart-expense` over `chart-expense-wash`): the filled and unfilled steps of the same red.
- **Grid** (`chart-grid`) and **baseline** (`chart-baseline`): solid hairlines, recessive.
- Text in charts always wears ink tokens, never a series colour; identity comes from a swatch beside it.

### Neutral
- **Glazed White** (`glaze`): page ground, checkbox rows, disabled buttons.
- **Surface White** (`surface`): inputs, menus, dialogs, secondary buttons.
- **Cobalt Ink** (`ink`): primary text (14.9:1 on glaze).
- **Muted Ink** (`ink-muted`): leads, hints, notes, secondary labels (6.8:1 on glaze).
- **Hairline** (`line`): dividers and the menu/panel border.
- **Field Edge** (`field-border`): input borders, 3.8:1 on white so the control boundary meets WCAG 1.4.11.
- **On Cobalt** (`on-cobalt`, `on-cobalt-muted`): text on the shell; the muted tint (5.9:1 on cobalt) for inactive nav and the auth tagline.

### Named Rules
**The Glaze Not Gradient Rule.** Every colour field is flat. No gradients, glows, glass or blur anywhere; depth comes from grout and hairlines.

**The Second Glaze Rule.** Ochre marks one thing at a time: where you are (active nav), who you are (avatar), or a single tile in a panel. It never carries text or a button.

**The Meaning-Only Red Rule.** Red appears only for errors, destructive actions and expenses. A disabled destructive button turns neutral glaze, never faded red.

## Typography

**Body Font:** Public Sans Variable (self-hosted via `@fontsource-variable/public-sans`, with system-ui fallback)

**Character:** a civic, identity-program grotesque: plain, sturdy, slightly wide, with honest figures. One family does every job; hierarchy comes from size and weight steps, not from a second face.

### Hierarchy
- **Page title** (700, 1.75rem on phones / 2rem from 48rem, 1.2, -0.02em): one per page, the page's name.
- **Auth title** (700, 1.75rem, 1.2, -0.02em): "Entrar", "Criar conta".
- **Dialog title** (700, 1.5rem, 1.2): the question the dialog asks.
- **Lead** (400, 1.125rem in pages / 1rem in auth, 1.5, muted ink, max 60ch): the one sentence under a title.
- **Body** (400, 1rem, 1.5): everything else. `font-variant-numeric: tabular-nums` is set on `body`.
- **Label** (600, 0.875rem): field labels; nav links use 600 at 0.9375rem.
- **Small** (0.8125rem, muted): notes such as the demo disclaimer.
- **Wordmark** (700, 1.0625rem, -0.01em): "Finance Controller" beside the mark.

### Named Rules
**The Tabular Rule.** Figures are tabular everywhere so amounts align in columns. Money is formatted `R$ 1.234,50`, dates `dd/mm/aaaa`.

**The No Eyebrow Rule.** Headings stand alone: no kicker, eyebrow or section number above them.

## Layout

Mobile first, one spacing scale on a 4px base (`spacing` tokens). The side gutter is 16px below 48rem and 32px above. App content sits in a 70rem column. Controls and nav items are at least 44px tall.

- **Auth screens:** below 60rem, a cobalt band (wordmark plus one week of tiles, 7 across) sits above the form; from 60rem the screen splits 5/12 cobalt panel (wordmark, a 7-by-5 month of tiles, the tagline) and 7/12 form column. The form column is at most 25rem wide and vertically centred.
- **Shell:** below 48rem the header has two rows: brand and user button, then the three nav links as tabs under a hairline. From 48rem it is one 64px row: brand, nav, user menu. Navigation is always visible; there is no hamburger.
- **Rhythm:** 24px between form fields and groups, 8px between a label and its control, 32px above the switch link, more space above a heading than below it.

## Elevation & Depth

Flat by default. Surfaces separate by colour (cobalt against glaze, white against glaze) and by hairlines. One shadow exists, for layers that float above the page: the user menu panel and the dialog.

### Shadow Vocabulary
- **Float** (`box-shadow: 0 10px 28px rgb(23 32 54 / 0.16), 0 2px 6px rgb(23 32 54 / 0.08)`): menus and dialogs only.
- **Backdrop** (`rgb(19 26 46 / 0.55)`): the modal dialog's `::backdrop`.

### Named Rules
**The Grout Rule.** Tiles never get shadows or borders. They are separated by grout: the surface colour showing through a 6% gap (6 units on a 100-unit tile).

## Shapes

Small, square-shouldered radii: 4px for small inner controls (reveal button), 6px for buttons, inputs and notices, 10px for floating panels and dialogs, a full pill only for the user button. The tile module itself is a perfect square with no radius; its figures are the only curves that carry identity (quarter-disc of the tile's full width, half-disc on an edge, corner-to-corner diagonal).

## Components

### Buttons
- **Shape:** gently squared (6px), 44px tall, 24px side padding, 600 weight.
- **Primary:** cobalt with white text; hover deep cobalt; pressed nudges down 1px.
- **Secondary:** white with a pale cobalt-grey edge and cobalt text; hover cobalt wash with a cobalt edge.
- **Danger:** red with white text; hover deep red. Used only for the final destructive action.
- **Pending:** the button keeps its colour at 75% opacity, its label changes to the gerund ("Entrando…", "Criando conta…", "Excluindo…"), and it is disabled.
- **Disabled (not pending):** neutral glaze background, muted ink, hairline edge.

### Inputs / Fields
- **Style:** white field, 1px field-edge border, 6px radius, 44px tall, label above in 600.
- **Focus:** border and a 2px outline in cobalt.
- **Error:** red border (red focus outline), message below with a drawn alert icon, wired through `aria-describedby` and `aria-invalid`. The error replaces the hint.
- **Password:** an eye toggle inside the field (`aria-pressed`, "Mostrar senha" / "Ocultar senha").

### Notices
Flat wash blocks with a drawn icon: info in cobalt wash (session expired, account deleted, account created), danger in red wash (form-level errors). No coloured side stripe.

### Navigation
On the cobalt header: inactive links in muted on-cobalt, hover and active in white, the active link carries a 3px ochre bar on its bottom edge. On phones the links become a second header row of tabs.

### User menu
A pill button (ochre initial avatar, name hidden below 48rem, chevron that turns 180 degrees when open) opening a white floating panel: the user's name and email, then "Sair" and the red "Excluir minha conta". Disclosure pattern: Escape and outside clicks close it and return focus.

### Dialog
Native modal `<dialog>`: 10px radius, float shadow, 24 to 32px padding. The delete-account dialog states exactly what is lost, names the account, requires a checkbox before the red button enables, shows server errors in place, and returns focus to the user-menu button when it closes.

### Select
A native `<select>` (`SelectField`) dressed exactly like a text field: same edge, height, focus, hint and error wiring, with the drawn chevron at the right instead of the browser arrow. Used for category choice and the month and year pickers.

### Segmented control
A radio group drawn as joined buttons (`Segmented`): 44px, field-edge borders, the chosen option in cobalt with white text. The real radio covers each option, so arrow keys, touch and screen readers get native behaviour; focus draws a cobalt ring 2px outside the option. Used for the type filter (Todas / Receitas / Despesas) and the movement type (Despesa / Receita). Disabled (edit mode, where the API cannot change a movement's type), unchosen options turn glaze with a strike-through and the chosen one turns cobalt wash, with a hint saying why.

### Quiet, icon and link buttons
- **Quiet** (`button--quiet`): row actions with a label ("Renomear", "Excluir"); transparent, muted ink, cobalt wash on hover, red wash for the destructive one. Below 30rem the label is hidden and the button is a 44px square; the accessible name always carries the item ("Renomear Moradia").
- **Icon** (`icon-button`): 44px square, drawn icon only, `aria-label` names action and item. The framed variant (white with the secondary-button edge) is used for previous/next month.
- **Link button** (`link-button`): a text action inside running copy ("Limpar filtros", "Voltar ao mês atual"), cobalt and underlined like a link.

### Ledger (movements list)
One semantic table on every width (explicit ARIA roles keep it a table when CSS changes its display). From 48rem: a white panel with hairline rows, columns Data, Descrição, Categoria, Valor (right-aligned, tabular) and the two row actions. Below 48rem each row becomes a two-line entry: description and amount on top, date, category and actions below. Amounts carry a sign and a hidden type word as well as colour (green `+R$` income, red `−R$` expense), so meaning never rests on colour alone. Recurring movements carry a small cobalt-wash **Recorrente** tag with the repeat icon. While a new filter loads the old rows stay, dimmed to 60%.

Above the ledger, the period heading (the month, first letter capitalised) with the count, and the **totals** for the filtered list: Receitas, Despesas and Saldo in bold tabular figures (only the matching total when a type filter is on). On phones the totals sit in their own white panel.

### Filters
Period (previous/next buttons around month and year selects; a "Voltar ao mês atual" link when away from it), Categoria select and the Tipo segmented control, on one row from 48rem and stacked on phones, closed by a hairline. The filters live in the address (`?mes=2028-01&categoria=3&tipo=despesas`).

### States
- **Empty:** a white panel with a 7×1 strip of tiles (one week; the tile module as identity, never animated on working screens), the title "Nenhuma movimentação em …", one sentence and the primary action. When filters cause the emptiness, no tiles: the title says so and offers "Limpar filtros".
- **Error:** a danger notice with the pt-BR message and a secondary "Tentar novamente" button.
- **Loading:** skeleton rows (flat glaze bars in a white panel) that fade in only after 300ms, so fast answers never flash.
- **Success:** a calm info notice in a `role="status"` region ("Despesa salva em fevereiro de 2028."), which also explains why the view moved to another month.

### Forms in dialogs
Create and edit a movement in the wide dialog (34rem): Tipo, Descrição, Valor and Data side by side from 30rem, Categoria, then the Recorrente checkbox with a note that it marks a repeating movement and creates nothing automatically. Destructive confirmations (`ConfirmDialog`) put Cancelar first (it takes initial focus), name exactly what is lost, and when the server says retrying cannot help (`CATEGORY_IN_USE`) the red button goes away and Cancelar reads "Entendi". Focus returns to the opener, or to the section heading when the opener's row is gone.

### Category list
A white panel with hairline rows: name on the left, quiet Renomear and Excluir on the right. Renaming happens in place: the row turns glaze and becomes a field with Cancelar and Salvar; Enter saves, Escape cancels, focus returns to that row's Renomear button. Creating sits above the list as one field and an "Adicionar" button.

### Dashboard (Painel)
The month selector (the same Período control as Movimentações, in `?mes=`) scopes the summary and the category chart; the 12-month chart always ends at the current month and highlights the chosen month when it falls inside it. Under the month heading:

- **Summary panel:** the month drawn as a small azulejo panel. Three tiles in a cobalt-grout frame (6px grout, 10px outer radius, square tiles): **Saldo** on a cobalt tile in white (2 to 2.5rem, proportional figures), **Receitas** and **Despesas** on white tiles. Each tile carries one `TileGlyph` (a single module): a half-disc rising for income and a positive balance, hanging for expense and a negative balance, a diagonal for a zero balance, so direction reads by shape as well as by the sign and the word (“Positivo”, “Negativo”, “Zerado”). Zero amounts print no sign. The Despesas tile has the **spent meter**: ten small tiles, each a tenth of the month's income, filled by what was spent, beside “67% das receitas” (or “Sem receitas no mês”). One column on phones, balance on its own row from 30rem, three across from 48rem.
- **Chart panel** (`ChartPanel`): white panel, title, one muted sentence, and a **Ver como tabela** toggle (`aria-pressed`, cobalt wash when on) that swaps the chart for its table twin with the same figures.
- **Para onde foi o dinheiro** (`CategoryChart`): ranked horizontal bars, largest first, ties by name; name and amount and share above each 10px bar (square at the baseline, 4px rounded data end, at least 2px). Past eight categories the smallest fold into **Outros** (always last, with “N categorias”).
- **Últimos 12 meses** (`MonthlyChart`): paired columns (income left, expense right, 2px surface gap, at most 20px each, 4px rounded tops) on one axis with compact ticks (“R$ 2,5 mil”). Above the plot a **readout** is the legend and the direct label: swatch, series name, value, and the saldo, for the month in focus (the chosen month, or the one under the pointer or keyboard). The whole month band is the hit target; the plot is one tab stop and the arrow keys, Home and End walk the months, with a cobalt-outlined band. X labels thin to every second or third month on narrow widths, always keeping the chosen one. An all-zero year keeps the axis and says “Nenhuma movimentação nos últimos 12 meses.” The table puts the currency in the headers (“Receitas (R$)”) so four columns fit at 360px.
- **States:** skeletons shaped like the tiles and like twelve columns (after 300ms); while another month loads, the previous one stays at 60% opacity. An empty month replaces the summary and the category chart with the 7×1 tile strip, “Nenhuma movimentação em …”, one sentence and **Ir para Movimentações** (to the same month). Summary and series fail independently, each with a danger notice and “Tentar novamente”.

### Tile Panel (signature)
An SVG grid of 100-unit square tiles with 6-unit grout, each tile one of three figures in one of four rotations, chosen by a seeded generator so the panel is identical on every render. Figures are glaze white, about one in six ochre, on cobalt tile grounds. It plays one quarter-turn entrance per page load (900ms, exponential ease-out, 22ms stagger), skipped under `prefers-reduced-motion`. Sizes in use: 7×5 (auth, desktop), 7×1 (auth, phone), 3×3 with the centre tile missing (404). The mark is a 2×2 of the same module whose quarter-discs meet in one disc, one quarter ochre. `TileGlyph` is a single module (cobalt-grout or glaze ground, one figure) used beside data on the dashboard.

## Do's and Don'ts

### Do:
- **Do** put the shell on cobalt (`cobalt`) and the work on glazed white (`glaze`) and white surfaces.
- **Do** use the tile module for data on the dashboard: the summary is a tile panel and the spent meter counts in tiles. (A per-day 7-column month grid remains a possible future addition; it needs per-day data the summary endpoint does not return.)
- **Do** keep figures tabular and money in `R$ 1.234,50`.
- **Do** give every async action a pending label and every failure a pt-BR message from `errorMessage`.
- **Do** keep touch targets at 44px and focus rings visible: cobalt on light, ochre on cobalt.

### Don't:
- **Don't** use gradients, glows, glass or stock illustration; the product brief rules out the purple/blue fintech template and emoji decoration.
- **Don't** add a kicker or eyebrow above headings, or a coloured border-left on notices or cards.
- **Don't** use ochre for text or buttons, or red for anything that is not an error, a destructive action or an expense.
- **Don't** scatter the tile panel as ornament across working screens; it is identity on the auth, mark and empty/error surfaces and data on the dashboard, nothing else.
- **Don't** add a second typeface or a dark theme by token inversion.
