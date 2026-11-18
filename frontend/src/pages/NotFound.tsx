import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { TilePanel } from "../components/TilePanel";
import { usePageTitle } from "./usePageTitle";

export default function NotFound() {
  const { user } = useAuth();
  usePageTitle("Página não encontrada");
  return (
    <main className="page page--centered" aria-labelledby="page-title">
      <TilePanel cols={3} rows={3} seed={404} missing={[4]} className="page__tiles" />
      <h1 className="page__title" id="page-title">
        Página não encontrada
      </h1>
      <p className="page__lead">O endereço pode estar errado ou a página mudou de lugar.</p>
      <p>
        <Link className="button button--secondary" to={user ? "/" : "/login"}>
          {user ? "Voltar ao painel" : "Ir para o login"}
        </Link>
      </p>
    </main>
  );
}
