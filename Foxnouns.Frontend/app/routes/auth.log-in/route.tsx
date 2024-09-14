import {
	MetaFunction,
	json,
	LoaderFunctionArgs,
	redirect,
	ActionFunctionArgs,
} from "@remix-run/node";
import { Form as RemixForm, useActionData, useLoaderData } from "@remix-run/react";
import { Form, Button, ButtonGroup, ListGroup, Row, Col } from "react-bootstrap";
import { useTranslation } from "react-i18next";
import i18n from "~/i18next.server";
import serverRequest, { getToken, writeCookie } from "~/lib/request.server";
import { AuthResponse, AuthUrls } from "~/lib/api/auth";
import { ApiError, ErrorCode } from "~/lib/api/error";
import ErrorAlert from "~/components/ErrorAlert";
import { User } from "~/lib/api/user";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	return [{ title: `${data?.meta.title || "Log in"} - pronouns.cc` }];
};

export const loader = async ({ request }: LoaderFunctionArgs) => {
	const t = await i18n.getFixedT(request);
	const token = getToken(request);
	if (token) {
		try {
			await serverRequest<User>("GET", "/users/@me", { token });
			return redirect("/?err=already-logged-in", 303);
		} catch (e) {
			// ignore
		}
	}

	const urls = await serverRequest<AuthUrls>("POST", "/auth/urls");

	return json({
		meta: { title: t("log-in.title") },
		urls,
	});
};

export const action = async ({ request }: ActionFunctionArgs) => {
	const body = await request.formData();
	const email = body.get("email") as string | null;
	const password = body.get("password") as string | null;

	console.log(email, password);

	try {
		const resp = await serverRequest<AuthResponse>("POST", "/auth/email/login", {
			body: { email, password },
		});

		return redirect("/", {
			status: 303,
			headers: {
				"Set-Cookie": writeCookie("pronounscc-token", resp.token),
			},
		});
	} catch (e) {
		return json({ error: e as ApiError });
	}
};

export default function LoginPage() {
	const { t } = useTranslation();
	const { urls } = useLoaderData<typeof loader>();
	const actionData = useActionData<typeof action>();

	return (
		<>
			<Row>
				<Col md className="mb-4">
					<h2>{t("log-in.form-title")}</h2>
					{actionData?.error && <LoginError error={actionData.error} />}
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

							<ButtonGroup>
								<Button variant="primary" type="submit">
									{t("log-in.log-in-button")}
								</Button>
								<Button as="a" href="/auth/register" variant="secondary">
									{t("log-in.register-with-email")}
								</Button>
							</ButtonGroup>
						</Form>
					</RemixForm>
				</Col>
				<Col md>
					<h2>{t("log-in.3rd-party.title")}</h2>
					<p>{t("log-in.3rd-party.desc")}</p>
					<ListGroup>
						{urls.discord && (
							<ListGroup.Item action href={urls.discord}>
								{t("log-in.3rd-party.discord")}
							</ListGroup.Item>
						)}
						{urls.google && (
							<ListGroup.Item action href={urls.google}>
								{t("log-in.3rd-party.google")}
							</ListGroup.Item>
						)}
						{urls.tumblr && (
							<ListGroup.Item action href={urls.tumblr}>
								{t("log-in.3rd-party.tumblr")}
							</ListGroup.Item>
						)}
					</ListGroup>
				</Col>
			</Row>
		</>
	);
}

function LoginError({ error }: { error: ApiError }) {
	const { t } = useTranslation();

	if (error.code !== ErrorCode.UserNotFound) return <ErrorAlert error={error} />;

	return <>{t("log-in.invalid-credentials")}</>;
}
