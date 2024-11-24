import { ErrorCode } from "$api/error";
import type { Modifier } from "sveltekit-i18n";

type TranslateFn = (key: string, payload?: any, props?: Modifier.Props<{}> | undefined) => any;

export default function errorDescription(t: TranslateFn, code: ErrorCode): string {
	switch (code) {
		case ErrorCode.InternalServerError:
			return t("error.internal-server-error");
		case ErrorCode.Forbidden:
			return t("error.forbidden");
		case ErrorCode.BadRequest:
			return t("error.bad-request");
		case ErrorCode.AuthenticationError:
			return t("error.authentication-error");
		case ErrorCode.AuthenticationRequired:
			return t("error.authentication-required");
		case ErrorCode.MissingScopes:
			// This error should never be returned by site tokens, so ask the user if they messed with their cookies
			return t("error.missing-scopes");
		case ErrorCode.GenericApiError:
			return t("error.generic-error");
		case ErrorCode.UserNotFound:
			return t("error.user-not-found");
		case ErrorCode.MemberNotFound:
			return t("error.member-not-found");
		case ErrorCode.AccountAlreadyLinked:
			return t("error.account-already-linked");
		case ErrorCode.LastAuthMethod:
			return t("error.last-auth-method");
		case ErrorCode.Non204Response:
			return t("error.generic-error");
	}

	return t("error.generic-error");
}
