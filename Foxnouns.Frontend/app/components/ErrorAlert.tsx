import { TFunction } from "i18next";
import { Alert } from "react-bootstrap";
import { Trans, useTranslation } from "react-i18next";
import {
	ApiError,
	ErrorCode,
	ValidationError,
	validationErrorType,
	ValidationErrorType,
} from "~/lib/api/error";

export default function ErrorAlert({ error }: { error: ApiError }) {
	const { t } = useTranslation();

	return (
		<Alert variant="danger">
			<Alert.Heading as="h4">{t("error.heading")}</Alert.Heading>
			{errorCodeDesc(t, error.code)}
			{error.errors && (
				<ul>
					{error.errors.map((e, i) => (
						<ValidationErrors key={i} errorKey={e.key} errors={e.errors} />
					))}
				</ul>
			)}
		</Alert>
	);
}

function ValidationErrors({ errorKey, errors }: { errorKey: string; errors: ValidationError[] }) {
	return (
		<li>
			<strong>
				<code>{errorKey}</code>
			</strong>
			:
			<ul>
				{errors.map((e, i) => (
					<li key={i}>
						<ValidationErrorEntry error={e} />
					</li>
				))}
			</ul>
		</li>
	);
}

function ValidationErrorEntry({ error }: { error: ValidationError }) {
	const { t } = useTranslation();

	const {
		min_length: minLength,
		max_length: maxLength,
		actual_length: actualLength,
		message: reason,
		actual_value: actualValue,
		allowed_values: allowedValues,
	} = error;

	switch (validationErrorType(error)) {
		case ValidationErrorType.LengthError:
			if (error.actual_length! > error.max_length!) {
				return (
					<Trans
						t={t}
						i18nKey={"error.validation.too-long"}
						values={{ maxLength: error.max_length!, actualLength: error.actual_length! }}
					>
						Value is too long, maximum length is {{ maxLength }}, current length is{" "}
						{{ actualLength }}.
					</Trans>
				);
			}

			if (error.actual_length! < error.min_length!) {
				return (
					<Trans
						t={t}
						i18nKey={"error.validation.too-short"}
						values={{ minLength: error.min_length!, actualLength: error.actual_length! }}
					>
						Value is too short, minimum length is {{ minLength }}, current length is{" "}
						{{ actualLength }}.
					</Trans>
				);
			}

			break;

		case ValidationErrorType.DisallowedValueError:
			return (
				<Trans
					t={t}
					i18nKey={"error.validation.disallowed-value"}
					values={{
						actualValue: error.actual_value!.toString(),
						allowedValues: error.allowed_values!.map((v) => v.toString()).join(", "),
					}}
				>
					{/* @ts-expect-error i18next handles interpolation */}
					The value <code>{{ actualValue }}</code> is not allowed here. Allowed values are:{" "}
					{/* @ts-expect-error i18next handles interpolation */}
					<code>{{ allowedValues }}</code>
				</Trans>
			);

		default:
			if (error.actual_value) {
				return (
					<Trans
						t={t}
						i18nKey={"error.validation.generic"}
						values={{ actualValue: error.actual_value!.toString(), reason: error.message }}
					>
						{/* @ts-expect-error i18next handles interpolation */}
						The value <code>{{ actualValue }}</code> is not allowed here. Reason: {{ reason }}
					</Trans>
				);
			}

			return <>{t("error.validation.generic-no-value", { reason: error.message })}</>;
	}
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
