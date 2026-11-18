import type { ReactNode } from "react";
import { Icon } from "./Icon";

export function Notice({ tone = "info", children }: { tone?: "info" | "danger"; children: ReactNode }) {
  return (
    <div className={`notice notice--${tone}`}>
      <Icon name={tone === "danger" ? "alert" : "info"} size={18} />
      <div>{children}</div>
    </div>
  );
}
