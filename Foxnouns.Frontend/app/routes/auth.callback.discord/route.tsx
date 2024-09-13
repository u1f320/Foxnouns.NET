import { json, LoaderFunctionArgs } from "@remix-run/node";
import { type ApiError, ErrorCode } from "~/lib/api/error";
import serverRequest, { writeCookie } from "~/lib/request.server";
import { CallbackResponse } from "~/lib/api/auth";
import { Form as RemixForm, Link, useLoaderData } from "@remix-run/react";
import { Trans, useTranslation } from "react-i18next";
import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";

export const loader = async ({ request }: LoaderFunctionArgs) => {
	const url = new URL(request.url);

	const code = url.searchParams.get("code");
	const state = url.searchParams.get("state");

	if (!code || !state)
		throw { status: 400, code: ErrorCode.BadRequest, message: "Missing code or state" } as ApiError;

	const resp = await serverRequest<CallbackResponse>("POST", "/auth/discord/callback", {
		body: { code, state }
	});

	if (resp.has_account) {
		return json(
			{ hasAccount: true, user: resp.user!, ticket: null, remoteUser: null },
			{
				headers: {
					"Set-Cookie": writeCookie("pronounscc-token", resp.token!)
				}
			}
		);
	}

	return json({
		hasAccount: false,
		user: null,
		ticket: resp.ticket!,
		remoteUser: resp.remote_username!
	});
};

// TODO: action function

export default function DiscordCallbackPage() {
	const { t } = useTranslation();
	const data = useLoaderData<typeof loader>();

	if (data.hasAccount) {
		const username = data.user!.username;
		
		return (
			<>
				<h1>{t("log-in.callback.success")}</h1>
				<p>
					<Trans t={t} i18nKey={"log-in.callback.success-link"} values={{ username: data.user!.username }}>
						{/* @ts-expect-error react-i18next handles interpolation here */}
						Welcome back, <Link to={`/@${data.user!.username}`}>@{{username}}</Link>!
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
