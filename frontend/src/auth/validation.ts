const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
export const MAX_LENGTH = 100;
export const MIN_PASSWORD = 8;

export function validateEmail(value: string): string | undefined {
  const email = value.trim();
  if (!email) return "Informe seu email.";
  if (!EMAIL_PATTERN.test(email)) return "Informe um email válido, como nome@exemplo.com.";
  if (email.length > MAX_LENGTH) return `Use no máximo ${MAX_LENGTH} caracteres.`;
  return undefined;
}

export function validateNewPassword(value: string): string | undefined {
  if (!value) return "Crie uma senha.";
  if (value.length < MIN_PASSWORD) return `A senha precisa ter pelo menos ${MIN_PASSWORD} caracteres.`;
  if (value.length > MAX_LENGTH) return `Use no máximo ${MAX_LENGTH} caracteres.`;
  return undefined;
}

export function validateName(value: string): string | undefined {
  const name = value.trim();
  if (!name) return "Informe seu nome.";
  if (name.length > MAX_LENGTH) return `Use no máximo ${MAX_LENGTH} caracteres.`;
  return undefined;
}
