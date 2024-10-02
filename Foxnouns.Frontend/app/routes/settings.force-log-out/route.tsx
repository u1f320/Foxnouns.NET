import { ActionFunction, redirect } from "@remix-run/node";
import { fastRequest, getToken, writeCookie } from "~/lib/request.server";
import { tokenCookieName } from "~/lib/utils";
import { Button, Form } from "react-bootstrap";
import { useTranslation } from "react-i18next";
import { Form as RemixForm, Link } from "@remix-run/react";

export const action: ActionFunction = async ({ request }) => {
	const token = getToken(request);
	if (!token)
		return redirect("/", {
			status: 303,
			headers: { "Set-Cookie": writeCookie(tokenCookieName, "token", 0) },
		});

	await fastRequest("POST", "/auth/force-log-out", { token, isInternal: true });

	return redirect("/", {
		status: 303,
		headers: { "Set-Cookie": writeCookie(tokenCookieName, "token", 0) },
	});
};

export const loader = () => {
	return null;
};

export default function ForceLogoutPage() {
	const { t } = useTranslation();

	return (
		<>
			<h4>{t("settings.general.log-out-everywhere")}</h4>
			<p className="text-has-newline">{t("settings.general.log-out-everywhere-confirm")}</p>
			<RemixForm method="POST">
				<Form as="div">
					<Button type="submit" variant="danger">
						{t("yes")}
					</Button>
					{/* @ts-expect-error as=Link */}
					<Button variant="link" as={Link} to="/settings">
						{t("no")}
					</Button>
				</Form>
			</RemixForm>
		</>
	);
}
