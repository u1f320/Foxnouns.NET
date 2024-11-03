import {
	ActionFunctionArgs,
	json,
	redirect,
	LoaderFunctionArgs,
	MetaFunction,
} from "@remix-run/node";
import { type ApiError, ErrorCode } from "~/lib/api/error";
import serverRequest, { getToken, writeCookie } from "~/lib/request.server";
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
import { Form, Button } from "react-bootstrap";
import i18n from "~/i18next.server";
import { tokenCookieName } from "~/lib/utils";
import { useEffect } from "react";
import RegisterError from "~/components/RegisterError";
import { AuthMethod } from "~/lib/api/user";
import { errorCodeDesc } from "~/components/ErrorAlert";

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

	const token = getToken(request);

	if (!code || !state)
		throw { status: 400, code: ErrorCode.BadRequest, message: "Missing code or state" } as ApiError;

	if (token) {
		try {
			const resp = await serverRequest<AuthMethod>("POST", "/auth/discord/add-account/callback", {
				body: { code, state },
				token,
				isInternal: true,
			});

			return json({
				isLinkRequest: true,
				meta: { title: t("log-in.callback.title.discord-link") },
				error: null,
				hasAccount: false,
				user: null,
				ticket: null,
				remoteUser: null,
				newAuthMethod: resp,
			});
		} catch (e) {
			return json({
				isLinkRequest: true,
				meta: { title: t("log-in.callback.title.discord-link") },
				error: e as ApiError,
				hasAccount: false,
				user: null,
				ticket: null,
				remoteUser: null,
				newAuthMethod: null,
			});
		}
	}

	const resp = await serverRequest<CallbackResponse>("POST", "/auth/discord/callback", {
		body: { code, state },
		isInternal: true,
	});

	if (resp.has_account) {
		return json(
			{
				isLinkRequest: false,
				meta: { title: t("log-in.callback.title.discord-success") },
				error: null,
				hasAccount: true,
				user: resp.user!,
				ticket: null,
				remoteUser: null,
				newAuthMethod: null,
			},
			{
				headers: {
					"Set-Cookie": writeCookie(tokenCookieName, resp.token!),
				},
			},
		);
	}

	return json({
		isLinkRequest: false,
		meta: { title: t("log-in.callback.title.discord-register") },
		error: null,
		hasAccount: false,
		user: null,
		ticket: resp.ticket!,
		remoteUser: resp.remote_username!,
		newAuthMethod: null,
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
			isInternal: true,
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

	if (data.isLinkRequest) {
		if (data.error) {
			return (
				<>
					<h1>{t("log-in.callback.link-error")}</h1>
					<p>{errorCodeDesc(t, data.error.code)}</p>
				</>
			);
		}

		const authMethod = data.newAuthMethod!;

		return (
			<>
				<h1>{t("log-in.callback.discord-link-success")}</h1>
				<p>
					{t("log-in.callback.discord-link-success-hint", {
						username: authMethod.remote_username ?? authMethod.remote_id,
					})}
				</p>
			</>
		);
	}

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
