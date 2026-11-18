import { act, renderHook, waitFor } from "@testing-library/react";
import type { ReactNode } from "react";
import { afterEach, describe, expect, it } from "vitest";
import { vi } from "vitest";
import { api } from "../api/client";
import { demoUser, mockFetch } from "../test/fetchMock";
import { AuthProvider, SignInAfterRegisterError, useAuth } from "./AuthContext";

const wrapper = ({ children }: { children: ReactNode }) => <AuthProvider>{children}</AuthProvider>;
const unauthorized = { status: 401, body: { code: "UNAUTHORIZED", message: "Unauthorized" } };

afterEach(() => vi.unstubAllGlobals());

describe("AuthContext", () => {
  it("AuthContext restores session from /me", async () => {
    mockFetch({ "GET /api/Auth/me": { status: 200, body: demoUser } });

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.loading).toBe(true);
    await waitFor(() => expect(result.current.loading).toBe(false));
    expect(result.current.user).toEqual(demoUser);
  });

  it("AuthContext starts signed out when /me is 401", async () => {
    mockFetch({ "GET /api/Auth/me": unauthorized });

    const { result } = renderHook(() => useAuth(), { wrapper });

    await waitFor(() => expect(result.current.loading).toBe(false));
    expect(result.current.user).toBeNull();
  });

  it("AuthContext login then loads user", async () => {
    const fetchMock = mockFetch({
      "GET /api/Auth/me": [unauthorized, { status: 200, body: demoUser }],
      "POST /api/Auth/login": { status: 200, body: { message: "Login successful" } },
    });
    const { result } = renderHook(() => useAuth(), { wrapper });
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(() => result.current.login("ana@exemplo.com", "segredo123"));

    expect(result.current.user).toEqual(demoUser);
    const loginCall = fetchMock.mock.calls.find(([, init]) => init?.method === "POST")!;
    expect(loginCall[0]).toBe("/api/Auth/login");
    expect(loginCall[1]?.body).toBe(JSON.stringify({ email: "ana@exemplo.com", password: "segredo123" }));
  });

  it("AuthContext login failure keeps the user signed out and rethrows", async () => {
    mockFetch({
      "GET /api/Auth/me": unauthorized,
      "POST /api/Auth/login": { status: 401, body: { code: "INVALID_CREDENTIALS", message: "x" } },
    });
    const { result } = renderHook(() => useAuth(), { wrapper });
    await waitFor(() => expect(result.current.loading).toBe(false));

    await expect(result.current.login("ana@exemplo.com", "errada123")).rejects.toMatchObject({
      code: "INVALID_CREDENTIALS",
    });
    expect(result.current.user).toBeNull();
  });

  it("AuthContext register creates the account and signs in", async () => {
    const fetchMock = mockFetch({
      "GET /api/Auth/me": [unauthorized, { status: 200, body: demoUser }],
      "POST /api/Auth/register": { status: 201, body: demoUser },
      "POST /api/Auth/login": { status: 200, body: { message: "ok" } },
    });
    const { result } = renderHook(() => useAuth(), { wrapper });
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(() =>
      result.current.register({ userName: "Ana Souza", email: "ana@exemplo.com", password: "segredo123" }),
    );

    expect(result.current.user).toEqual(demoUser);
    const posts = fetchMock.mock.calls.filter(([, init]) => init?.method === "POST").map(([url]) => url);
    expect(posts).toEqual(["/api/Auth/register", "/api/Auth/login"]);
  });

  it("AuthContext register reports a failed automatic sign-in separately", async () => {
    mockFetch({
      "GET /api/Auth/me": unauthorized,
      "POST /api/Auth/register": { status: 201, body: demoUser },
      "POST /api/Auth/login": { status: 429, body: { code: "RATE_LIMITED", message: "x" } },
    });
    const { result } = renderHook(() => useAuth(), { wrapper });
    await waitFor(() => expect(result.current.loading).toBe(false));

    const error = await result.current
      .register({ userName: "Ana Souza", email: "ana@exemplo.com", password: "segredo123" })
      .catch((e: unknown) => e);

    expect(error).toBeInstanceOf(SignInAfterRegisterError);
    expect(result.current.user).toBeNull();
  });

  it("AuthContext 401 on a protected call clears the user", async () => {
    mockFetch({
      "GET /api/Auth/me": { status: 200, body: demoUser },
      "GET /api/Category": unauthorized,
    });
    const { result } = renderHook(() => useAuth(), { wrapper });
    await waitFor(() => expect(result.current.user).toEqual(demoUser));

    await act(async () => {
      await api.get("/api/Category").catch(() => undefined);
    });

    expect(result.current.user).toBeNull();
    expect(result.current.sessionExpired).toBe(true);
  });

  it("AuthContext logout clears the user", async () => {
    mockFetch({
      "GET /api/Auth/me": { status: 200, body: demoUser },
      "POST /api/Auth/logout": { status: 200, body: { message: "ok" } },
    });
    const { result } = renderHook(() => useAuth(), { wrapper });
    await waitFor(() => expect(result.current.user).toEqual(demoUser));

    await act(() => result.current.logout());

    expect(result.current.user).toBeNull();
    expect(result.current.sessionExpired).toBe(false);
  });

  it("AuthContext deleteAccount deletes /me and clears the user", async () => {
    const fetchMock = mockFetch({
      "GET /api/Auth/me": { status: 200, body: demoUser },
      "DELETE /api/Auth/me": { status: 204 },
    });
    const { result } = renderHook(() => useAuth(), { wrapper });
    await waitFor(() => expect(result.current.user).toEqual(demoUser));

    await act(() => result.current.deleteAccount());

    expect(result.current.user).toBeNull();
    expect(result.current.accountDeleted).toBe(true);
    expect(fetchMock.mock.calls.some(([url, init]) => url === "/api/Auth/me" && init?.method === "DELETE")).toBe(true);
  });

  it("AuthContext deleteAccount failure keeps the user and rethrows", async () => {
    mockFetch({
      "GET /api/Auth/me": { status: 200, body: demoUser },
      "DELETE /api/Auth/me": { status: 500, body: { code: "INTERNAL_ERROR", message: "x" } },
    });
    const { result } = renderHook(() => useAuth(), { wrapper });
    await waitFor(() => expect(result.current.user).toEqual(demoUser));

    await expect(result.current.deleteAccount()).rejects.toMatchObject({ code: "INTERNAL_ERROR" });
    expect(result.current.user).toEqual(demoUser);
  });
});
