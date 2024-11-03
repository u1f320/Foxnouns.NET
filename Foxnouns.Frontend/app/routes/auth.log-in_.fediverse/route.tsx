import {
	LoaderFunctionArgs,
	json,
	MetaFunction,
	ActionFunctionArgs,
	redirect,
} from "@remix-run/node";
import i18n from "~/i18next.server";
import { useTranslation } from "react-i18next";
import { Form as RemixForm, useActionData } from "@remix-run/react";
import { Button, Form } from "react-bootstrap";
import serverRequest from "~/lib/request.server";
import { ApiError, ErrorCode } from "~/lib/api/error";
import ErrorAlert from "~/components/ErrorAlert";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	return [{ title: `${data?.meta.title || "Log in with a Fediverse account"} • pronouns.cc` }];
};

export const loader = async ({ request }: LoaderFunctionArgs) => {
	const t = await i18n.getFixedT(request);

	return json({ meta: { title: t("log-in.fediverse.choose-title") } });
};

export const action = async ({ request }: ActionFunctionArgs) => {
	const body = await request.formData();
	const instance = body.get("instance") as string | null;
	if (!instance)
		return json({
			error: {
				status: 403,
				code: ErrorCode.BadRequest,
				message: "Invalid instance name",
			} as ApiError,
		});

	try {
		const resp = await serverRequest<{ url: string }>(
			"GET",
			`/auth/fediverse?instance=${encodeURIComponent(instance)}`,
			{
				isInternal: true,
			},
		);

		return redirect(resp.url);
	} catch (e) {
		return json({ error: e as ApiError });
	}
};

export default function AuthFediversePage() {
	const { t } = useTranslation();

	const data = useActionData<typeof action>();

	return (
		<>
			<h2>{t("log-in.fediverse.choose-form-title")}</h2>
			{data?.error && <ErrorAlert error={data.error} />}
			<RemixForm method="POST">
				<Form as="div">
					<Form.Group className="mb-3" controlId="instance">
						<Form.Label>{t("log-in.fediverse-instance-label")}</Form.Label>
						<Form.Control name="instance" type="text" />
					</Form.Group>
					<Button variant="primary" type="submit">
						{t("log-in.fediverse-log-in-button")}
					</Button>
				</Form>
			</RemixForm>
		</>
	);
}
