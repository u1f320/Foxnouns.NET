import { ActionFunctionArgs, json, LoaderFunctionArgs, redirect } from "@remix-run/node";
import i18n from "~/i18next.server";
import serverRequest, { fastRequest, getToken } from "~/lib/request.server";
import { AuthMethod } from "~/lib/api/user";
import { useTranslation } from "react-i18next";
import { useLoaderData, Form } from "@remix-run/react";
import { Button } from "react-bootstrap";

export const action = async ({ request }: ActionFunctionArgs) => {
	const data = await request.formData();
	const token = getToken(request);

	const id = data.get("remove-id") as string;

	await fastRequest("DELETE", `/auth/methods/${id}`, { token, isInternal: true });

	return redirect("/settings/auth", 303);
};

export const loader = async ({ request, params }: LoaderFunctionArgs) => {
	const t = await i18n.getFixedT(request);
	const token = getToken(request);

	const method = await serverRequest<AuthMethod>("GET", `/auth/methods/${params.id}`, {
		token,
		isInternal: true,
	});
	return json({ method, meta: { title: t("settings.auth.remove-auth-method-title") } });
};

export default function RemoveAuthMethodPage() {
	const { t } = useTranslation();
	const { method } = useLoaderData<typeof loader>();

	let methodName;
	switch (method.type) {
		case "EMAIL":
			methodName = "email";
			break;
		case "DISCORD":
			methodName = "Discord";
			break;
		case "FEDIVERSE":
			methodName = "Fediverse";
			break;
		case "GOOGLE":
			methodName = "Google";
			break;
		case "TUMBLR":
			methodName = "Tumblr";
			break;
	}

	return (
		<>
			<h3>{t("settings.auth.remove-auth-method-title")}</h3>
			<p>
				{t("settings.auth.remove-auth-method-hint", {
					username: method.remote_username || method.remote_id,
					methodName,
				})}
			</p>
			<p>
				<Form method="POST">
					<input type="hidden" name="remove-id" value={method.id} />
					<Button type="submit" color="primary">
						{t("settings.auth.remove-auth-method")}
					</Button>
				</Form>
			</p>
		</>
	);
}
