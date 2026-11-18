import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { afterEach, describe, expect, it, vi } from "vitest";
import { mockFetch } from "../test/fetchMock";
import Categories from "./Categories";

const defaults = [
  { id: 1, name: "Moradia" },
  { id: 2, name: "Alimentação" },
];

function renderPage() {
  return render(
    <MemoryRouter>
      <Categories />
    </MemoryRouter>,
  );
}

function bodyOf(fetchMock: ReturnType<typeof mockFetch>, method: string) {
  const call = fetchMock.mock.calls.find(([, init]) => init?.method === method);
  return call ? { url: String(call[0]), body: call[1]?.body ? JSON.parse(String(call[1].body)) : undefined } : undefined;
}

afterEach(() => vi.unstubAllGlobals());

describe("Categories page", () => {
  it("lists categories in alphabetical order", async () => {
    mockFetch({ "GET /api/Category": { status: 200, body: defaults } });
    renderPage();

    const list = await screen.findByRole("list", { name: "Suas categorias" });
    const names = within(list).getAllByRole("listitem").map((item) => item.querySelector(".category__name")?.textContent);
    expect(names).toEqual(["Alimentação", "Moradia"]);
  });

  it("Categories page creates a category", async () => {
    const fetchMock = mockFetch({
      "GET /api/Category": { status: 200, body: defaults },
      "POST /api/Category": { status: 201, body: { id: 9, name: "Viagens" } },
    });
    const user = userEvent.setup();
    renderPage();

    const input = await screen.findByLabelText("Nova categoria");
    await user.type(input, "  Viagens ");
    await user.click(screen.getByRole("button", { name: "Adicionar" }));

    const list = screen.getByRole("list", { name: "Suas categorias" });
    expect(await within(list).findByText("Viagens")).toBeInTheDocument();
    expect(bodyOf(fetchMock, "POST")).toEqual({ url: "/api/Category", body: { name: "Viagens" } });
    expect(input).toHaveValue("");
  });

  it("blocks an empty or duplicate name without calling the API", async () => {
    const fetchMock = mockFetch({ "GET /api/Category": { status: 200, body: defaults } });
    const user = userEvent.setup();
    renderPage();

    const input = await screen.findByLabelText("Nova categoria");
    await user.click(screen.getByRole("button", { name: "Adicionar" }));
    expect(input).toHaveAccessibleDescription("Informe um nome para a categoria.");

    await user.type(input, "moradia");
    await user.click(screen.getByRole("button", { name: "Adicionar" }));
    expect(input).toHaveAccessibleDescription("Já existe uma categoria com esse nome.");
    expect(bodyOf(fetchMock, "POST")).toBeUndefined();
  });

  it("renames a category inline", async () => {
    const fetchMock = mockFetch({
      "GET /api/Category": { status: 200, body: defaults },
      "PUT /api/Category/1": { status: 200, body: { id: 1, name: "Casa" } },
    });
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole("button", { name: "Renomear Moradia" }));
    const input = screen.getByLabelText("Novo nome para Moradia");
    expect(input).toHaveFocus();
    await user.clear(input);
    await user.type(input, "Casa{Enter}");

    expect(await screen.findByText("Casa")).toBeInTheDocument();
    expect(bodyOf(fetchMock, "PUT")).toEqual({ url: "/api/Category/1", body: { name: "Casa" } });
    expect(screen.getByRole("button", { name: "Renomear Casa" })).toHaveFocus();
  });

  it("Categories page shows CATEGORY_IN_USE message on 409", async () => {
    mockFetch({
      "GET /api/Category": { status: 200, body: defaults },
      "DELETE /api/Category/1": { status: 409, body: { code: "CATEGORY_IN_USE", message: "in use" } },
    });
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole("button", { name: "Excluir Moradia" }));
    const dialog = await screen.findByRole("dialog", { name: "Excluir a categoria “Moradia”?" });
    await user.click(within(dialog).getByRole("button", { name: "Excluir categoria" }));

    expect(
      await within(dialog).findByText(
        "Esta categoria tem movimentações e não pode ser excluída. Mova ou exclua as movimentações antes.",
      ),
    ).toBeInTheDocument();
    expect(screen.getByRole("list", { name: "Suas categorias" })).toHaveTextContent("Moradia");
  });

  it("deletes an unused category", async () => {
    mockFetch({
      "GET /api/Category": { status: 200, body: defaults },
      "DELETE /api/Category/2": { status: 204 },
    });
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole("button", { name: "Excluir Alimentação" }));
    const dialog = await screen.findByRole("dialog");
    await user.click(within(dialog).getByRole("button", { name: "Excluir categoria" }));

    await waitFor(() =>
      expect(screen.getByRole("list", { name: "Suas categorias" })).not.toHaveTextContent("Alimentação"),
    );
  });

  it("shows a load error with a retry", async () => {
    mockFetch({
      "GET /api/Category": [
        { status: 500, body: { code: "INTERNAL_ERROR" } },
        { status: 200, body: defaults },
      ],
    });
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole("button", { name: "Tentar novamente" }));
    expect(await screen.findByText("Moradia")).toBeInTheDocument();
  });
});
