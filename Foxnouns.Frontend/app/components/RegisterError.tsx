import { ApiError, firstErrorFor } from "~/lib/api/error";
import { Trans, useTranslation } from "react-i18next";
import { Alert } from "react-bootstrap";
import { Link } from "@remix-run/react";
import ErrorAlert from "~/components/ErrorAlert";

export default function RegisterError({ error }: { error: ApiError }) {
	const { t } = useTranslation();

	// TODO: maybe turn these messages into their own error codes?
	const ticketMessage = firstErrorFor(error, "ticket")?.message;
	const usernameMessage = firstErrorFor(error, "username")?.message;

	if (ticketMessage === "Invalid ticket") {
		return (
			<Alert variant="danger">
				<Alert.Heading as="h4">{t("error.heading")}</Alert.Heading>
				<Trans t={t} i18nKey={"log-in.callback.invalid-ticket"}>
					Invalid ticket (it might have been too long since you logged in), please{" "}
					<Link to="/auth/log-in">try again</Link>.
				</Trans>
			</Alert>
		);
	}

	if (usernameMessage === "Username is already taken") {
		return (
			<Alert variant="danger">
				<Alert.Heading as="h4">{t("log-in.callback.invalid-username")}</Alert.Heading>
				{t("log-in.callback.username-taken")}
			</Alert>
		);
	}

	return <ErrorAlert error={error} />;
}
