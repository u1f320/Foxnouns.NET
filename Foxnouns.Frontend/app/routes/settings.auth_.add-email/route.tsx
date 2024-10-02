import { ActionFunctionArgs, LoaderFunctionArgs, MetaFunction } from "@remix-run/node";
import i18n from "~/i18next.server";
import { json, useActionData, useNavigate, useRouteLoaderData } from "@remix-run/react";
import { loader as settingsLoader } from "~/routes/settings/route";
import { useTranslation } from "react-i18next";
import { useEffect } from "react";
import { Button, Card, Form } from "react-bootstrap";
import { Form as RemixForm } from "@remix-run/react/dist/components";
import { ApiError, ErrorCode } from "~/lib/api/error";
import { fastRequest, getToken } from "~/lib/request.server";
import ErrorAlert from "~/components/ErrorAlert";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	return [{ title: `${data?.meta.title || "Authentication"} • pronouns.cc` }];
};

export const loader = async ({ request }: LoaderFunctionArgs) => {
	const t = await i18n.getFixedT(request);
	return { meta: { title: t("settings.auth.title") } };
};

export const action = async ({ request }: ActionFunctionArgs) => {
	const token = getToken(request)!;
	const body = await request.formData();
	const email = body.get("email") as string | null;
	const password = body.get("password-1") as string | null;
	const password2 = body.get("password-2") as string | null;

	if (!email || !password || !password2) {
		return json({
			error: {
				status: 400,
				code: ErrorCode.BadRequest,
				message: "One or more required fields are missing.",
			} as ApiError,
			ok: false,
		});
	}

	if (password !== password2) {
		return json({
			error: {
				status: 400,
				code: ErrorCode.BadRequest,
				message: "Passwords do not match.",
			} as ApiError,
			ok: false,
		});
	}

	await fastRequest("POST", "/auth/email/add", {
		body: { email, password },
		token,
		isInternal: true,
	});

	return json({ error: null, ok: true });
};

export default function AddEmailPage() {
	const { t } = useTranslation();
	const { user } = useRouteLoaderData<typeof settingsLoader>("routes/settings")!;
	const actionData = useActionData<typeof action>();
	const navigate = useNavigate();
	const emails = user.auth_methods.filter((m) => m.type === "EMAIL");

	useEffect(() => {
		if (emails.length >= 3) {
			navigate("/settings/auth");
		}
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, []);

	return (
		<RemixForm method="POST">
			<Card body>
				<Card.Title>
					{emails.length === 0
						? t("settings.auth.form.add-first-email")
						: t("settings.auth.form.add-extra-email")}
				</Card.Title>
				{emails.length === 0 && !actionData?.ok && <p>{t("settings.auth.no-email")}</p>}
				{actionData?.ok && <p>{t("settings.auth.new-email-pending")}</p>}
				{actionData?.error && <ErrorAlert error={actionData.error} />}
				<Form as="div">
					<Form.Group className="mb-3" controlId="email-address">
						<Form.Label>{t("settings.auth.form.email-address")}</Form.Label>
						<Form.Control type="email" name="email" required disabled={actionData?.ok} />
					</Form.Group>
					<Form.Group className="mb-3" controlId="password-1">
						<Form.Label>{t("settings.auth.form.password-1")}</Form.Label>
						<Form.Control type="password" name="password-1" required disabled={actionData?.ok} />
					</Form.Group>
					<Form.Group className="mb-3" controlId="password-2">
						<Form.Label>{t("settings.auth.form.password-2")}</Form.Label>
						<Form.Control type="password" name="password-2" required disabled={actionData?.ok} />
					</Form.Group>
					<Button variant="primary" type="submit">
						{t("settings.auth.form.add-email-button")}
					</Button>
				</Form>
			</Card>
		</RemixForm>
	);
}
