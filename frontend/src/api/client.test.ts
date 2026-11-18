import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { api, ApiError, onSessionExpired } from "./client";

function jsonResponse(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

let fetchMock: ReturnType<typeof vi.fn>;

beforeEach(() => {
  fetchMock = vi.fn();
  vi.stubGlobal("fetch", fetchMock);
});

afterEach(() => {
  vi.unstubAllGlobals();
});

describe("api client", () => {
  it("client sends JSON and same-origin credentials", async () => {
    fetchMock.mockResolvedValue(jsonResponse(201, { id: 1 }));

    const result = await api.post<{ id: number }>("/api/Auth/register", { email: "a@b.co" });

    expect(result).toEqual({ id: 1 });
    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe("/api/Auth/register");
    expect(init.method).toBe("POST");
    expect(init.credentials).toBe("same-origin");
    expect(new Headers(init.headers).get("Content-Type")).toBe("application/json");
    expect(init.body).toBe(JSON.stringify({ email: "a@b.co" }));
  });

  it("client sends PUT with a JSON body", async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { ok: true }));

    await api.put("/api/Category/1", { name: "Casa" });

    const [, init] = fetchMock.mock.calls[0];
    expect(init.method).toBe("PUT");
    expect(new Headers(init.headers).get("Content-Type")).toBe("application/json");
    expect(init.body).toBe(JSON.stringify({ name: "Casa" }));
  });

  it("client does not send Content-Type when there is no body", async () => {
    fetchMock.mockResolvedValue(jsonResponse(200, { message: "ok" }));
    fetchMock.mockResolvedValueOnce(jsonResponse(200, { id: 1 }));

    await api.get("/api/Auth/me");
    await api.post("/api/Auth/logout");

    for (const [, init] of fetchMock.mock.calls) {
      expect(init.credentials).toBe("same-origin");
      expect(new Headers(init.headers).has("Content-Type")).toBe(false);
      expect(init.body).toBeUndefined();
    }
    expect(fetchMock.mock.calls[0][1].method).toBe("GET");
    expect(fetchMock.mock.calls[1][1].method).toBe("POST");
  });

  it("client throws ApiError with code on non-2xx", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(409, { code: "EMAIL_TAKEN", message: "Email already registered." }),
    );

    const error = await api.post("/api/Auth/register", {}).catch((e: unknown) => e);

    expect(error).toBeInstanceOf(ApiError);
    expect(error).toMatchObject({ status: 409, code: "EMAIL_TAKEN" });
  });

  it("client keeps field errors from a validation failure", async () => {
    fetchMock.mockResolvedValue(
      jsonResponse(400, {
        code: "VALIDATION_FAILED",
        message: "Validation failed.",
        errors: [{ propertyName: "Password", errorMessage: "too short", errorCode: "MinimumLengthValidator" }],
      }),
    );

    const error = (await api.post("/api/Auth/register", {}).catch((e: unknown) => e)) as ApiError;

    expect(error.code).toBe("VALIDATION_FAILED");
    expect(error.fieldErrors).toEqual([
      { propertyName: "Password", errorMessage: "too short", errorCode: "MinimumLengthValidator" },
    ]);
  });

  it("client maps non-JSON error body to UNKNOWN", async () => {
    fetchMock.mockResolvedValue(new Response("<html>Bad gateway</html>", { status: 502 }));

    const error = await api.get("/api/Auth/me").catch((e: unknown) => e);

    expect(error).toBeInstanceOf(ApiError);
    expect(error).toMatchObject({ status: 502, code: "UNKNOWN" });
  });

  it("client maps a JSON error body without code to UNKNOWN", async () => {
    fetchMock.mockResolvedValue(jsonResponse(400, { title: "Bad Request" }));

    const error = await api.get("/api/Auth/me").catch((e: unknown) => e);

    expect(error).toMatchObject({ status: 400, code: "UNKNOWN" });
  });

  it("client maps a network failure to NETWORK_ERROR", async () => {
    fetchMock.mockRejectedValue(new TypeError("Failed to fetch"));

    const error = await api.get("/api/Auth/me").catch((e: unknown) => e);

    expect(error).toBeInstanceOf(ApiError);
    expect(error).toMatchObject({ status: 0, code: "NETWORK_ERROR" });
  });

  it("client returns undefined on 204", async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 204 }));

    const result = await api.delete("/api/Auth/me");

    expect(result).toBeUndefined();
    expect(fetchMock.mock.calls[0][1].method).toBe("DELETE");
    expect(fetchMock.mock.calls[0][1].credentials).toBe("same-origin");
  });

  it("client raises session expired on a 401 outside login", async () => {
    const listener = vi.fn();
    const unsubscribe = onSessionExpired(listener);
    fetchMock.mockResolvedValue(jsonResponse(401, { code: "UNAUTHORIZED", message: "Unauthorized" }));

    await api.get("/api/Category").catch(() => undefined);
    expect(listener).toHaveBeenCalledTimes(1);

    await api.post("/api/Auth/login", { email: "a@b.co", password: "x" }).catch(() => undefined);
    expect(listener).toHaveBeenCalledTimes(1);

    unsubscribe();
    await api.get("/api/Category").catch(() => undefined);
    expect(listener).toHaveBeenCalledTimes(1);
  });
});
