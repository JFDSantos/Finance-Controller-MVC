import type { StableErrorCode } from "../api/types";

export const FALLBACK_MESSAGE = "Algo deu errado. Tente novamente.";

const MESSAGES: Record<StableErrorCode, string> = {
  EMAIL_TAKEN: "Este email já está cadastrado.",
  VALIDATION_FAILED: "Alguns dados não estão corretos. Revise os campos e tente novamente.",
  INVALID_CREDENTIALS: "Email ou senha incorretos.",
  RATE_LIMITED: "Muitas tentativas seguidas. Aguarde um momento e tente novamente.",
  UNSUPPORTED_MEDIA_TYPE: "Não foi possível enviar os dados. Atualize a página e tente novamente.",
  NOT_FOUND: "Não encontramos o que você procurava. Pode ter sido excluído.",
  UNAUTHORIZED: "Sua sessão expirou. Entre novamente para continuar.",
  INTERNAL_ERROR: "Tivemos um problema no servidor. Tente novamente em instantes.",
  CATEGORY_IN_USE:
    "Esta categoria tem movimentações e não pode ser excluída. Mova ou exclua as movimentações antes.",
  CATEGORY_NAME_TAKEN: "Você já tem uma categoria com esse nome.",
  CATEGORY_NOT_FOUND: "Esta categoria não existe mais. Escolha outra.",
  INVALID_MONTH: "Escolha um mês entre janeiro e dezembro.",
  INVALID_YEAR: "Escolha um ano válido.",
  INVALID_RANGE: "O período escolhido não é válido. Confira as datas de início e fim.",
  INVALID_VALUE: "Um dos valores informados não é válido. Revise e tente novamente.",
  INVALID_DATE: "Informe uma data entre 2000 e 2100.",
  DEMO_READ_ONLY: "A conta de demonstração é compartilhada e não pode ser excluída.",
};

const CLIENT_MESSAGES: Record<string, string> = {
  NETWORK_ERROR: "Sem conexão com o servidor. Verifique sua internet e tente novamente.",
};

export const STABLE_ERROR_CODES = Object.keys(MESSAGES) as StableErrorCode[];

/** pt-BR message for an API error code; unknown codes get the generic fallback. */
export function errorMessage(code: string): string {
  return (
    (MESSAGES as Record<string, string>)[code] ?? CLIENT_MESSAGES[code] ?? FALLBACK_MESSAGE
  );
}

const FIELD_LABEL_MESSAGES: Record<string, Record<string, string>> = {
  email: {
    NotEmptyValidator: "Informe seu email.",
    EmailValidator: "Informe um email válido, como nome@exemplo.com.",
  },
  password: {
    NotEmptyValidator: "Informe sua senha.",
    MinimumLengthValidator: "A senha precisa ter pelo menos 8 caracteres.",
  },
  username: {
    NotEmptyValidator: "Informe seu nome.",
  },
};

const GENERIC_CODE_MESSAGES: Record<string, string> = {
  MaximumLengthValidator: "Use no máximo 100 caracteres.",
  NotEmptyValidator: "Preencha este campo.",
};

/** pt-BR message for one server-side field error (FluentValidation `errorCode`). */
export function fieldErrorMessage(propertyName: string, errorCode: string): string {
  const field = propertyName.toLowerCase();
  return (
    FIELD_LABEL_MESSAGES[field]?.[errorCode] ??
    GENERIC_CODE_MESSAGES[errorCode] ??
    "Verifique este campo."
  );
}
