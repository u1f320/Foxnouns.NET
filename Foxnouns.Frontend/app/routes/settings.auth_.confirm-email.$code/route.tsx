import { LoaderFunctionArgs, json } from "@remix-run/node";
import { baseRequest } from "~/lib/request.server";
import { useTranslation } from "react-i18next";
import { useEffect } from "react";
import { useNavigate } from "@remix-run/react";

export const loader = async ({ params }: LoaderFunctionArgs) => {
	const state = params.code!;

	const resp = await baseRequest("POST", "/auth/email/callback", {
		body: { state },
		isInternal: true,
	});
	if (resp.status !== 204) {
		// TODO: handle non-204 status (this indicates that the email was not linked to an account)
	}

	return json({ ok: true });
};

export default function ConfirmEmailPage() {
	const { t } = useTranslation();
	const navigate = useNavigate();

	useEffect(() => {
		setTimeout(() => {
			navigate("/settings/auth");
		}, 2000);
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, []);

	return (
		<>
			<h3>{t("settings.auth.email-link-success")}</h3>
			<p>{t("settings.auth.redirect-to-auth-hint")}</p>
		</>
	);
}
