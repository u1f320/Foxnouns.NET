import { LoaderFunctionArgs, redirect, json, MetaFunction } from "@remix-run/node";
import i18n from "~/i18next.server";
import serverRequest, { getToken } from "~/lib/request.server";
import { User } from "~/lib/api/user";
import { useTranslation } from "react-i18next";
import { Link, useLoaderData } from "@remix-run/react";
import { Button } from "react-bootstrap";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	return [{ title: `${data?.meta.title || "Welcome"} - pronouns.cc` }];
};

export const loader = async ({ request }: LoaderFunctionArgs) => {
	const t = await i18n.getFixedT(request);
	const token = getToken(request);
	let user: User;

	if (token) {
		try {
			user = await serverRequest<User>("GET", "/users/@me", { token });
		} catch (e) {
			return redirect("/auth/log-in");
		}
	} else {
		return redirect("/auth/log-in");
	}

	return json({ meta: { title: t("welcome.title") }, user });
};

export default function WelcomePage() {
	const { t } = useTranslation();
	const { user } = useLoaderData<typeof loader>();

	return (
		<div>
			<h1>{t("welcome.header")}</h1>
			<p>{t("welcome.blurb")}</p>
			<h2>{t("welcome.customize-profile")}</h2>
			<p>{t("welcome.customize-profile-blurb")}</p>
			<h2>{t("welcome.create-members")}</h2>
			<p>{t("welcome.create-members-blurb")}</p>
			<h2>{t("welcome.custom-preferences")}</h2>
			<p>{t("welcome.custom-preferences-blurb")}</p>
			<Link to={`/@${user.username}`}>
				<Button as="span" variant="primary">
					{t("welcome.profile-button")}
				</Button>
			</Link>
		</div>
	);
}
