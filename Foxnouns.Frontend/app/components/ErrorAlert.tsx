import { TFunction } from "i18next";
import Alert from "react-bootstrap/Alert";
import { useTranslation } from "react-i18next";
import { ApiError, ErrorCode } from "~/lib/api/error";

export default function ErrorAlert({ error }: { error: ApiError }) {
	const { t } = useTranslation();

	return (
		<Alert variant="danger">
			<Alert.Heading as="h4">{t("error.heading")}</Alert.Heading>
			{errorCodeDesc(t, error.code)}
		</Alert>
	);
}

export const errorCodeDesc = (t: TFunction, code: ErrorCode) => {
	switch (code) {
		case ErrorCode.AuthenticationError:
			return t("error.errors.authentication-error");
		case ErrorCode.AuthenticationRequired:
			return t("error.errors.authentication-required");
		case ErrorCode.BadRequest:
			return t("error.errors.bad-request");
		case ErrorCode.Forbidden:
			return t("error.errors.forbidden");
		case ErrorCode.GenericApiError:
			return t("error.errors.generic-error");
		case ErrorCode.InternalServerError:
			return t("error.errors.internal-server-error");
		case ErrorCode.MemberNotFound:
			return t("error.errors.member-not-found");
		case ErrorCode.UserNotFound:
			return t("error.errors.user-not-found");
	}

	return t("error.errors.generic-error");
};
