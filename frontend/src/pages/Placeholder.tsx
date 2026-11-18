import { usePageTitle } from "./usePageTitle";

export function PlaceholderPage({ title, description }: { title: string; description: string }) {
  usePageTitle(title);
  return (
    <section className="page" aria-labelledby="page-title">
      <h1 className="page__title" id="page-title">
        {title}
      </h1>
      <p className="page__lead">{description}</p>
    </section>
  );
}
