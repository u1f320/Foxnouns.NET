import {
	ActionFunctionArgs,
	json,
	LoaderFunctionArgs,
	MetaFunction,
	redirect,
} from "@remix-run/node";
import i18n from "~/i18next.server";
import { type ApiError, ErrorCode } from "~/lib/api/error";
import serverRequest, { writeCookie } from "~/lib/request.server";
import { AuthResponse, CallbackResponse } from "~/lib/api/auth";
import { tokenCookieName } from "~/lib/utils";
import {
	Link,
	ShouldRevalidateFunction,
	useActionData,
	useLoaderData,
	useNavigate,
} from "@remix-run/react";
import { Trans, useTranslation } from "react-i18next";
import { useEffect } from "react";
import { Form as RemixForm } from "@remix-run/react/dist/components";
import { Button, Form } from "react-bootstrap";
import RegisterError from "~/components/RegisterError";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	return [{ title: `${data?.meta.title || "Log in"} • pronouns.cc` }];
};

export const shouldRevalidate: ShouldRevalidateFunction = ({ actionResult }) => {
	return !actionResult;
};

export const loader = async ({ request, params }: LoaderFunctionArgs) => {
	const t = await i18n.getFixedT(request);
	const url = new URL(request.url);

	const code = url.searchParams.get("code");
	if (!code) throw { status: 400, code: ErrorCode.BadRequest, message: "Missing code" } as ApiError;

	const resp = await serverRequest<CallbackResponse>("POST", "/auth/fediverse/callback", {
		body: { code, instance: params.instance! },
		isInternal: true,
	});

	if (resp.has_account) {
		return json(
			{
				meta: { title: t("log-in.callback.title.fediverse-success") },
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
		meta: { title: t("log-in.callback.title.fediverse-register") },
		hasAccount: false,
		user: null,
		instance: params.instance!,
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
		const resp = await serverRequest<AuthResponse>("POST", "/auth/fediverse/register", {
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

export default function FediverseCallbackPage() {
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
					<Form.Label>{t("log-in.callback.remote-username.fediverse")}</Form.Label>
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
