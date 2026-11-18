import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, describe, expect, it, vi } from "vitest";
import { AuthProvider } from "../auth/AuthContext";
import { demoUser, mockFetch } from "../test/fetchMock";
import Register from "./Register";

const unauthorized = { status: 401, body: { code: "UNAUTHORIZED", message: "Unauthorized" } };

function renderRegister() {
  return render(
    <MemoryRouter initialEntries={["/register"]}>
      <AuthProvider>
        <Routes>
          <Route path="/register" element={<Register />} />
          <Route path="/" element={<h1>Painel</h1>} />
        </Routes>
      </AuthProvider>
    </MemoryRouter>,
  );
}

afterEach(() => vi.unstubAllGlobals());

describe("Register page", () => {
  it("blocks a password shorter than 8 characters without calling the API", async () => {
    const fetchMock = mockFetch({ "GET /api/Auth/me": unauthorized });
    const user = userEvent.setup();
    renderRegister();
    const name = await screen.findByLabelText("Nome");

    await user.type(name, "Ana Souza");
    await user.type(screen.getByLabelText("Email"), "ana@exemplo.com");
    await user.type(screen.getByLabelText("Senha"), "curta12");
    await user.click(screen.getByRole("button", { name: "Criar conta" }));

    const password = screen.getByLabelText("Senha");
    expect(password).toHaveAttribute("aria-invalid", "true");
    expect(password).toHaveAccessibleDescription(/pelo menos 8 caracteres/);
    expect(password).toHaveFocus();
    expect(fetchMock.mock.calls.filter(([, init]) => init?.method === "POST")).toHaveLength(0);
  });

  it("shows EMAIL_TAKEN from the server as a field error", async () => {
    mockFetch({
      "GET /api/Auth/me": unauthorized,
      "POST /api/Auth/register": { status: 409, body: { code: "EMAIL_TAKEN", message: "taken" } },
    });
    const user = userEvent.setup();
    renderRegister();

    await user.type(await screen.findByLabelText("Nome"), "Ana Souza");
    await user.type(screen.getByLabelText("Email"), "ana@exemplo.com");
    await user.type(screen.getByLabelText("Senha"), "segredo123");
    await user.click(screen.getByRole("button", { name: "Criar conta" }));

    await waitFor(() =>
      expect(screen.getByLabelText("Email")).toHaveAccessibleDescription(/já está cadastrado/),
    );
  });

  it("creates the account, signs in and goes to the dashboard", async () => {
    mockFetch({
      "GET /api/Auth/me": [unauthorized, { status: 200, body: demoUser }],
      "POST /api/Auth/register": { status: 201, body: demoUser },
      "POST /api/Auth/login": { status: 200, body: { message: "ok" } },
    });
    const user = userEvent.setup();
    renderRegister();

    await user.type(await screen.findByLabelText("Nome"), "Ana Souza");
    await user.type(screen.getByLabelText("Email"), "ana@exemplo.com");
    await user.type(screen.getByLabelText("Senha"), "segredo123");
    await user.click(screen.getByRole("button", { name: "Criar conta" }));

    expect(await screen.findByRole("heading", { name: "Painel" })).toBeInTheDocument();
  });
});
