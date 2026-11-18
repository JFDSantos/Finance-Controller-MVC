import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";
import type { Location } from "react-router-dom";
import { PageLoading } from "../components/PageLoading";
import { useAuth } from "./AuthContext";

export interface AuthRedirectState {
  from?: Location;
  registeredEmail?: string;
}

/** Renders children only for a signed-in user; otherwise sends them to /login and remembers where they were going. */
export function RequireAuth({ children }: { children: ReactNode }) {
  const { user, loading } = useAuth();
  const location = useLocation();
  if (loading) return <PageLoading />;
  if (!user) return <Navigate to="/login" replace state={{ from: location } satisfies AuthRedirectState} />;
  return <>{children}</>;
}

/** Keeps signed-in users away from /login and /register. */
export function RedirectIfAuthenticated({ children }: { children: ReactNode }) {
  const { user, loading } = useAuth();
  const location = useLocation();
  if (loading) return <PageLoading />;
  if (user) {
    const from = (location.state as AuthRedirectState | null)?.from;
    const target = from && from.pathname !== "/login" && from.pathname !== "/register" ? from : "/";
    return <Navigate to={target} replace />;
  }
  return <>{children}</>;
}
