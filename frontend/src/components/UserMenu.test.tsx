import { fireEvent, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { demoUser } from "../test/fetchMock";
import { UserMenu } from "./UserMenu";

function renderMenu(user = demoUser) {
  const onLogout = vi.fn();
  const onDeleteAccount = vi.fn();
  render(
    <>
      <UserMenu user={user} onLogout={onLogout} onDeleteAccount={onDeleteAccount} />
      <a href="#fora">Fora do menu</a>
    </>,
  );
  return { onLogout, onDeleteAccount, button: screen.getByRole("button", { name: /Ana Souza/ }) };
}

describe("UserMenu", () => {
  it("offers account deletion to a regular user", async () => {
    const user = userEvent.setup();
    const { button } = renderMenu();
    await user.click(button);
    expect(screen.getByRole("button", { name: "Excluir minha conta" })).toBeVisible();
  });

  it("hides account deletion for the shared demo user, whatever the email casing", async () => {
    const user = userEvent.setup();
    const { button } = renderMenu({ ...demoUser, email: "Demo@FinanceController.app" });
    await user.click(button);
    expect(screen.getByRole("button", { name: "Sair" })).toBeVisible();
    expect(screen.queryByRole("button", { name: "Excluir minha conta" })).toBeNull();
  });

  it("UserMenu does not close when the pointer lands on an item", async () => {
    const user = userEvent.setup();
    const { onLogout, button } = renderMenu();

    button.focus();
    await user.keyboard("{Enter}");
    const logout = screen.getByRole("button", { name: "Sair" });
    expect(logout).toBeVisible();

    // Safari and Firefox on macOS do not focus a button on mouse down, so focus leaves
    // the menu button with no relatedTarget before the click on the item lands.
    fireEvent.focusOut(button, { relatedTarget: null });
    expect(logout).toBeVisible();

    await user.click(logout);
    expect(onLogout).toHaveBeenCalledTimes(1);
  });

  it("closes when focus moves outside the menu", async () => {
    const user = userEvent.setup();
    const { button } = renderMenu();

    await user.click(button);
    expect(screen.getByRole("button", { name: "Sair" })).toBeVisible();
    fireEvent.focusOut(button, { relatedTarget: screen.getByRole("link", { name: "Fora do menu" }) });
    expect(button).toHaveAttribute("aria-expanded", "false");
  });

  it("closes on a pointer down outside and on Escape", async () => {
    const user = userEvent.setup();
    const { button } = renderMenu();

    await user.click(button);
    fireEvent.pointerDown(document.body);
    expect(button).toHaveAttribute("aria-expanded", "false");

    await user.click(button);
    await user.keyboard("{Escape}");
    expect(button).toHaveAttribute("aria-expanded", "false");
    expect(button).toHaveFocus();
  });
});
