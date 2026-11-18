/* eslint-disable react-refresh/only-export-components -- the provider and its hook belong together */
import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from "react";
import type { ReactNode } from "react";
import { api, ApiError, onSessionExpired } from "../api/client";
import type { RegisterDto, User } from "../api/types";

export interface AuthContextValue {
  /** The signed-in user, or null. */
  user: User | null;
  /** True until the session restore (`GET /api/Auth/me`) has answered. */
  loading: boolean;
  /** True when the user was signed out by a 401 (not by "Sair"). Cleared on the next login. */
  sessionExpired: boolean;
  /** True right after the account was deleted, so the login screen can confirm it. */
  accountDeleted: boolean;
  login(email: string, password: string): Promise<void>;
  /** Creates the account, then signs in with the same credentials. */
  register(dto: RegisterDto): Promise<void>;
  logout(): Promise<void>;
  /** Permanently deletes the account and all its data. */
  deleteAccount(): Promise<void>;
}

/** The account was created but the automatic sign-in that follows failed (e.g. rate limit). */
export class SignInAfterRegisterError extends Error {
  readonly cause: unknown;
  constructor(cause: unknown) {
    super("Account created, but signing in failed");
    this.name = "SignInAfterRegisterError";
    this.cause = cause;
  }
}

const AuthContext = createContext<AuthContextValue | null>(null);

const ME = "/api/Auth/me";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [sessionExpired, setSessionExpired] = useState(false);
  const [accountDeleted, setAccountDeleted] = useState(false);
  const userRef = useRef<User | null>(null);
  userRef.current = user;

  useEffect(() => {
    let active = true;
    api
      .get<User>(ME)
      .then((me) => {
        if (active) setUser(me);
      })
      .catch(() => {
        if (active) setUser(null);
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, []);

  useEffect(
    () =>
      onSessionExpired(() => {
        if (userRef.current) setSessionExpired(true);
        setUser(null);
      }),
    [],
  );

  const login = useCallback(async (email: string, password: string) => {
    await api.post("/api/Auth/login", { email, password });
    const me = await api.get<User>(ME);
    setSessionExpired(false);
    setAccountDeleted(false);
    setUser(me);
  }, []);

  const register = useCallback(
    async (dto: RegisterDto) => {
      await api.post<User>("/api/Auth/register", dto);
      try {
        await login(dto.email, dto.password);
      } catch (error) {
        throw new SignInAfterRegisterError(error);
      }
    },
    [login],
  );

  const logout = useCallback(async () => {
    try {
      await api.post("/api/Auth/logout");
    } catch (error) {
      // A 401 means the cookie is already gone; anything else still ends the local session.
      if (!(error instanceof ApiError)) throw error;
    } finally {
      setSessionExpired(false);
      setUser(null);
    }
  }, []);

  const deleteAccount = useCallback(async () => {
    await api.delete(ME);
    setSessionExpired(false);
    setAccountDeleted(true);
    setUser(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({ user, loading, sessionExpired, accountDeleted, login, register, logout, deleteAccount }),
    [user, loading, sessionExpired, accountDeleted, login, register, logout, deleteAccount],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const value = useContext(AuthContext);
  if (!value) throw new Error("useAuth must be used inside <AuthProvider>");
  return value;
}
