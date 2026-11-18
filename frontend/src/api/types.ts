/** The authenticated user, as returned by `GET /api/Auth/me` and `POST /api/Auth/register`. */
export interface User {
  id: number;
  userName: string;
  email: string;
  role: string;
}

export interface RegisterDto {
  userName: string;
  email: string;
  password: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

/** One entry of `errors` in the API's error body (FluentValidation shape, camelCase). */
export interface FieldError {
  propertyName: string;
  errorMessage: string;
  errorCode: string;
}

/** The JSON body the API sends with every non-2xx response. `code` can be missing on framework-level failures. */
export interface ApiErrorBody {
  code?: string;
  message?: string;
  errors?: FieldError[];
}

/** Codes the backend guarantees. Anything else is treated as unknown. */
export type StableErrorCode =
  | "EMAIL_TAKEN"
  | "VALIDATION_FAILED"
  | "INVALID_CREDENTIALS"
  | "RATE_LIMITED"
  | "UNSUPPORTED_MEDIA_TYPE"
  | "NOT_FOUND"
  | "UNAUTHORIZED"
  | "INTERNAL_ERROR"
  | "CATEGORY_IN_USE"
  | "CATEGORY_NOT_FOUND"
  | "INVALID_MONTH"
  | "INVALID_YEAR"
  | "INVALID_RANGE"
  | "INVALID_VALUE";

/** Codes produced by the client itself. */
export type ClientErrorCode = "UNKNOWN" | "NETWORK_ERROR";
