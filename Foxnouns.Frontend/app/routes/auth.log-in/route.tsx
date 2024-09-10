import { MetaFunction, json, LoaderFunctionArgs, ActionFunction, redirect } from "@remix-run/node";
import { Form as RemixForm } from "@remix-run/react";
import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import { useTranslation } from "react-i18next";
import i18n from "~/i18next.server";
import serverRequest, { writeCookie } from "~/lib/request.server";
import { AuthResponse } from "~/lib/api/auth";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	return [{ title: `${data?.meta.title || "Log in"} - pronouns.cc` }];
};

export const loader = async ({ request }: LoaderFunctionArgs) => {
	const t = await i18n.getFixedT(request);

	return json({
		meta: { title: t("log-in.title") },
	});
};

export const action: ActionFunction = async ({ request }) => {
	const body = await request.formData();
	const email = body.get("email") as string | null;
	const password = body.get("password") as string | null;

	console.log(email, password);

	const resp = await serverRequest<AuthResponse>("POST", "/auth/email/login", {
		body: { email, password },
	});

	return redirect("/", {
		status: 303,
		headers: {
			"Set-Cookie": writeCookie("pronounscc-token", resp.token),
		},
	});
};

export default function LoginPage() {
	const { t } = useTranslation();

	return (
		<RemixForm action="/auth/log-in" method="POST">
			<Form as="div">
				<Form.Group className="mb-3" controlId="email">
					<Form.Label>{t("log-in.email")}</Form.Label>
					<Form.Control name="email" type="email" required />
				</Form.Group>
				<Form.Group className="mb-3" controlId="password">
					<Form.Label>{t("log-in.password")}</Form.Label>
					<Form.Control name="password" type="password" required />
				</Form.Group>

				<Button variant="primary" type="submit">
					{t("log-in.log-in-button")}
				</Button>
			</Form>
		</RemixForm>
	);
}
