import { describe, expect, it } from "vitest";
import { errorMessage, fieldErrorMessage, FALLBACK_MESSAGE, STABLE_ERROR_CODES } from "./errors";

describe("errorMessage", () => {
  it("errorMessage maps EMAIL_TAKEN to Portuguese", () => {
    expect(errorMessage("EMAIL_TAKEN")).toBe("Este email já está cadastrado.");
  });

  it("errorMessage maps INVALID_CREDENTIALS and RATE_LIMITED usefully", () => {
    expect(errorMessage("INVALID_CREDENTIALS")).toBe("Email ou senha incorretos.");
    expect(errorMessage("RATE_LIMITED")).toMatch(/aguarde/i);
  });

  it("errorMessage maps every stable code to a non-fallback message", () => {
    expect(STABLE_ERROR_CODES).toEqual([
      "EMAIL_TAKEN",
      "VALIDATION_FAILED",
      "INVALID_CREDENTIALS",
      "RATE_LIMITED",
      "UNSUPPORTED_MEDIA_TYPE",
      "NOT_FOUND",
      "UNAUTHORIZED",
      "INTERNAL_ERROR",
      "CATEGORY_IN_USE",
      "CATEGORY_NOT_FOUND",
      "INVALID_MONTH",
      "INVALID_YEAR",
      "INVALID_RANGE",
      "INVALID_VALUE",
    ]);
    for (const code of STABLE_ERROR_CODES) {
      const message = errorMessage(code);
      expect(message, code).not.toBe(FALLBACK_MESSAGE);
      expect(message.length, code).toBeGreaterThan(10);
      expect(message, code).toMatch(/\.$/);
    }
  });

  it("errorMessage falls back for unknown code", () => {
    expect(errorMessage("SOMETHING_NEW")).toBe("Algo deu errado. Tente novamente.");
    expect(errorMessage("UNKNOWN")).toBe("Algo deu errado. Tente novamente.");
    expect(errorMessage("")).toBe("Algo deu errado. Tente novamente.");
  });

  it("errorMessage explains a network failure", () => {
    expect(errorMessage("NETWORK_ERROR")).toMatch(/conex/i);
  });
});

describe("fieldErrorMessage", () => {
  it("translates FluentValidation codes for known fields", () => {
    expect(fieldErrorMessage("Password", "MinimumLengthValidator")).toBe(
      "A senha precisa ter pelo menos 8 caracteres.",
    );
    expect(fieldErrorMessage("email", "NotEmptyValidator")).toBe("Informe seu email.");
    expect(fieldErrorMessage("UserName", "MaximumLengthValidator")).toBe(
      "Use no máximo 100 caracteres.",
    );
  });

  it("returns a generic message for unknown combinations", () => {
    expect(fieldErrorMessage("Other", "Whatever")).toBe("Verifique este campo.");
  });
});
