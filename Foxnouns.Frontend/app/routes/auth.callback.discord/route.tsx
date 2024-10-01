import {
	ActionFunctionArgs,
	json,
	redirect,
	LoaderFunctionArgs,
	MetaFunction,
} from "@remix-run/node";
import { type ApiError, ErrorCode, firstErrorFor } from "~/lib/api/error";
import serverRequest, { writeCookie } from "~/lib/request.server";
import { AuthResponse, CallbackResponse } from "~/lib/api/auth";
import {
	Form as RemixForm,
	Link,
	useActionData,
	useLoaderData,
	ShouldRevalidateFunction,
	useNavigate,
} from "@remix-run/react";
import { Trans, useTranslation } from "react-i18next";
import { Form, Button, Alert } from "react-bootstrap";
import ErrorAlert from "~/components/ErrorAlert";
import i18n from "~/i18next.server";
import { tokenCookieName } from "~/lib/utils";
import { useEffect } from "react";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	return [{ title: `${data?.meta.title || "Log in"} • pronouns.cc` }];
};

export const shouldRevalidate: ShouldRevalidateFunction = ({ actionResult }) => {
	return !actionResult;
};

export const loader = async ({ request }: LoaderFunctionArgs) => {
	const t = await i18n.getFixedT(request);
	const url = new URL(request.url);

	const code = url.searchParams.get("code");
	const state = url.searchParams.get("state");

	if (!code || !state)
		throw { status: 400, code: ErrorCode.BadRequest, message: "Missing code or state" } as ApiError;

	const resp = await serverRequest<CallbackResponse>("POST", "/auth/discord/callback", {
		body: { code, state },
	});

	if (resp.has_account) {
		return json(
			{
				meta: { title: t("log-in.callback.title.discord-success") },
				hasAccount: true,
				user: resp.user!,
				ticket: null,
				remoteUser: null,
			},
			{
				headers: {
					"Set-Cookie": writeCookie(tokenCookieName, resp.token!),
				},
			},
		);
	}

	return json({
		meta: { title: t("log-in.callback.title.discord-register") },
		hasAccount: false,
		user: null,
		ticket: resp.ticket!,
		remoteUser: resp.remote_username!,
	});
};

export const action = async ({ request }: ActionFunctionArgs) => {
	const data = await request.formData();
	const username = data.get("username") as string | null;
	const ticket = data.get("ticket") as string | null;

	if (!username || !ticket)
		return json({
			error: {
				status: 403,
				code: ErrorCode.BadRequest,
				message: "Invalid username or ticket",
			} as ApiError,
			user: null,
		});

	try {
		const resp = await serverRequest<AuthResponse>("POST", "/auth/discord/register", {
			body: { username, ticket },
		});

		return redirect("/auth/welcome", {
			headers: {
				"Set-Cookie": writeCookie(tokenCookieName, resp.token),
			},
			status: 303,
		});
	} catch (e) {
		JSON.stringify(e);

		return json({ error: e as ApiError });
	}
};

export default function DiscordCallbackPage() {
	const { t } = useTranslation();
	const data = useLoaderData<typeof loader>();
	const actionData = useActionData<typeof action>();
	const navigate = useNavigate();

	useEffect(() => {
		setTimeout(() => {
			if (data.hasAccount) {
				navigate(`/@${data.user!.username}`);
			}
		}, 2000);
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, []);

	if (data.hasAccount) {
		const username = data.user!.username;

		return (
			<>
				<h1>{t("log-in.callback.success")}</h1>
				<p>
					<Trans
						t={t}
						i18nKey={"log-in.callback.success-link"}
						values={{ username: data.user!.username }}
					>
						{/* @ts-expect-error react-i18next handles interpolation here */}
						Welcome back, <Link to={`/@${data.user!.username}`}>@{{ username }}</Link>!
					</Trans>
					<br />
					{t("log-in.callback.redirect-hint")}
				</p>
			</>
		);
	}

	return (
		<RemixForm method="POST">
			<Form as="div">
				{actionData?.error && <RegisterError error={actionData.error} />}
				<Form.Group className="mb-3" controlId="remote-username">
					<Form.Label>{t("log-in.callback.remote-username.discord")}</Form.Label>
					<Form.Control type="text" readOnly={true} value={data.remoteUser!} />
				</Form.Group>
				<Form.Group className="mb-3" controlId="username">
					<Form.Label>{t("log-in.callback.username")}</Form.Label>
					<Form.Control name="username" type="text" required />
				</Form.Group>
				<input type="hidden" name="ticket" value={data.ticket!} />
				<Button variant="primary" type="submit">
					{t("log-in.callback.sign-up-button")}
				</Button>
			</Form>
		</RemixForm>
	);
}

function RegisterError({ error }: { error: ApiError }) {
	const { t } = useTranslation();

	// TODO: maybe turn these messages into their own error codes?
	const ticketMessage = firstErrorFor(error, "ticket")?.message;
	const usernameMessage = firstErrorFor(error, "username")?.message;

	if (ticketMessage === "Invalid ticket") {
		return (
			<Alert variant="danger">
				<Alert.Heading as="h4">{t("error.heading")}</Alert.Heading>
				<Trans t={t} i18nKey={"log-in.callback.invalid-ticket"}>
					Invalid ticket (it might have been too long since you logged in with Discord), please{" "}
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
