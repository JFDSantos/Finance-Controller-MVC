import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it, vi } from "vitest";
import type { Movement } from "../api/types";
import { mockFetch } from "../test/fetchMock";
import { MovementForm } from "./MovementForm";

const categories = [
  { id: 1, name: "Moradia" },
  { id: 2, name: "Salário" },
];

const rent: Movement = {
  id: 41,
  type: "Expense",
  description: "Aluguel",
  movimentDate: "2028-01-05",
  isRecurring: true,
  categoryId: 1,
  categoryName: "Moradia",
  value: 1200,
};

function writes(fetchMock: ReturnType<typeof mockFetch>) {
  return fetchMock.mock.calls.filter(([, init]) => init?.method && init.method !== "GET");
}

function renderForm(props: Partial<Parameters<typeof MovementForm>[0]> = {}) {
  const onSaved = vi.fn();
  render(
    <MovementForm categories={categories} defaultDate="2028-01-15" onSaved={onSaved} onCancel={vi.fn()} {...props} />,
  );
  return { onSaved };
}

afterEach(() => vi.unstubAllGlobals());

describe("MovementForm", () => {
  it("MovementForm blocks zero and negative values", async () => {
    const fetchMock = mockFetch({});
    const user = userEvent.setup();
    renderForm();

    await user.type(screen.getByLabelText("Descrição"), "Mercado");
    await user.selectOptions(screen.getByLabelText("Categoria"), "1");
    const amount = screen.getByLabelText("Valor");

    for (const input of ["0", "0,00", "-10", "12,345", "dez reais"]) {
      await user.clear(amount);
      await user.type(amount, input);
      await user.click(screen.getByRole("button", { name: "Salvar" }));
      expect(amount).toHaveAttribute("aria-invalid", "true");
      expect(amount).toHaveFocus();
    }
    expect(amount).toHaveAccessibleDescription(/1\.234,50/);
    expect(writes(fetchMock)).toHaveLength(0);
  });

  it("MovementForm blocks empty description", async () => {
    const fetchMock = mockFetch({});
    const user = userEvent.setup();
    renderForm();

    await user.type(screen.getByLabelText("Descrição"), "   ");
    await user.selectOptions(screen.getByLabelText("Categoria"), "1");
    await user.type(screen.getByLabelText("Valor"), "50");
    await user.click(screen.getByRole("button", { name: "Salvar" }));

    const description = screen.getByLabelText("Descrição");
    expect(description).toHaveAttribute("aria-invalid", "true");
    expect(description).toHaveAccessibleDescription("Informe uma descrição.");
    expect(description).toHaveFocus();
    expect(writes(fetchMock)).toHaveLength(0);
  });

  it("MovementForm limits the date to the supported years", async () => {
    const fetchMock = mockFetch({});
    const user = userEvent.setup();
    renderForm();

    const date = screen.getByLabelText("Data");
    expect(date).toHaveAttribute("min", "2000-01-01");
    expect(date).toHaveAttribute("max", "2100-12-31");

    await user.type(screen.getByLabelText("Descrição"), "Mercado");
    await user.selectOptions(screen.getByLabelText("Categoria"), "1");
    await user.type(screen.getByLabelText("Valor"), "50");

    for (const typo of ["0202-03-04", "2205-03-04", "1999-12-31", "2101-01-01"]) {
      fireEvent.change(date, { target: { value: typo } });
      await user.click(screen.getByRole("button", { name: "Salvar" }));
      expect(date, typo).toHaveAttribute("aria-invalid", "true");
      expect(date, typo).toHaveAccessibleDescription("Informe uma data entre 2000 e 2100.");
      expect(date, typo).toHaveFocus();
    }
    expect(writes(fetchMock)).toHaveLength(0);
  });

  it.each(["2000-01-01", "2100-12-31"])("MovementForm accepts the edge date %s", async (edge) => {
    const fetchMock = mockFetch({ "POST /api/Expense": { status: 201, body: { id: 9 } } });
    const user = userEvent.setup();
    const { onSaved } = renderForm();

    await user.type(screen.getByLabelText("Descrição"), "Mercado");
    await user.selectOptions(screen.getByLabelText("Categoria"), "1");
    await user.type(screen.getByLabelText("Valor"), "50");
    fireEvent.change(screen.getByLabelText("Data"), { target: { value: edge } });
    await user.click(screen.getByRole("button", { name: "Salvar" }));
    await waitFor(() => expect(onSaved).toHaveBeenCalledWith(expect.objectContaining({ movimentDate: edge })));
    expect(writes(fetchMock)).toHaveLength(1);
  });

  it("MovementForm submits IsRecurring flag", async () => {
    const fetchMock = mockFetch({ "POST /api/Income": { status: 201, body: { id: 9 } } });
    const user = userEvent.setup();
    const { onSaved } = renderForm();

    await user.click(screen.getByRole("radio", { name: "Receita" }));
    await user.type(screen.getByLabelText("Descrição"), "  Salário de janeiro ");
    await user.selectOptions(screen.getByLabelText("Categoria"), "2");
    await user.type(screen.getByLabelText("Valor"), "1.234,50");
    await user.click(screen.getByRole("checkbox", { name: /Recorrente/ }));
    await user.click(screen.getByRole("button", { name: "Salvar" }));

    await waitFor(() => expect(onSaved).toHaveBeenCalled());
    const [[url, init]] = writes(fetchMock);
    expect(url).toBe("/api/Income");
    expect(init?.method).toBe("POST");
    expect(JSON.parse(String(init?.body))).toEqual({
      description: "Salário de janeiro",
      movimentDate: "2028-01-15",
      isRecurring: true,
      categoryId: 2,
      value: 1234.5,
    });
    expect(onSaved).toHaveBeenCalledWith({ type: "Income", movimentDate: "2028-01-15", categoryId: 2 });
  });

  it("MovementForm locks type in edit mode", async () => {
    const fetchMock = mockFetch({ "PUT /api/Expense/41": { status: 200, body: {} } });
    const user = userEvent.setup();
    const { onSaved } = renderForm({ movement: rent });

    expect(screen.getByRole("radio", { name: "Despesa" })).toBeChecked();
    expect(screen.getByRole("radio", { name: "Despesa" })).toBeDisabled();
    expect(screen.getByRole("radio", { name: "Receita" })).toBeDisabled();
    expect(screen.getByLabelText("Descrição")).toHaveValue("Aluguel");
    expect(screen.getByLabelText("Valor")).toHaveValue("1200,00");
    expect(screen.getByRole("checkbox", { name: /Recorrente/ })).toBeChecked();

    await user.click(screen.getByRole("checkbox", { name: /Recorrente/ }));
    await user.click(screen.getByRole("button", { name: "Salvar alterações" }));

    await waitFor(() => expect(onSaved).toHaveBeenCalled());
    const [[url, init]] = writes(fetchMock);
    expect(url).toBe("/api/Expense/41");
    expect(JSON.parse(String(init?.body))).toMatchObject({ isRecurring: false, value: 1200, categoryId: 1 });
  });

  it("shows a server-side CATEGORY_NOT_FOUND on the category field", async () => {
    mockFetch({
      "POST /api/Expense": {
        status: 400,
        body: {
          code: "VALIDATION_FAILED",
          errors: [{ propertyName: "CategoryId", errorMessage: "Category not found", errorCode: "CATEGORY_NOT_FOUND" }],
        },
      },
    });
    const user = userEvent.setup();
    const { onSaved } = renderForm();

    await user.type(screen.getByLabelText("Descrição"), "Mercado");
    await user.selectOptions(screen.getByLabelText("Categoria"), "1");
    await user.type(screen.getByLabelText("Valor"), "80");
    await user.click(screen.getByRole("button", { name: "Salvar" }));

    await waitFor(() =>
      expect(screen.getByLabelText("Categoria")).toHaveAccessibleDescription(/não existe mais/),
    );
    expect(onSaved).not.toHaveBeenCalled();
  });
});
