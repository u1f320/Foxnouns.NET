import { LoaderFunctionArgs, json, redirect, MetaFunction } from "@remix-run/node";
import i18n from "~/i18next.server";
import serverRequest, { getToken } from "~/lib/request.server";
import { MeUser } from "~/lib/api/user";
import { Link, Outlet, useLocation } from "@remix-run/react";
import { Nav } from "react-bootstrap";
import { useTranslation } from "react-i18next";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	return [{ title: `${data?.meta.title || "Settings"} • pronouns.cc` }];
};

export const loader = async ({ request }: LoaderFunctionArgs) => {
	const t = await i18n.getFixedT(request);
	const token = getToken(request);

	if (token) {
		try {
			const user = await serverRequest<MeUser>("GET", "/users/@me", { token });
			return json({ user, meta: { title: t("settings.title") } });
		} catch (e) {
			return redirect("/auth/log-in");
		}
	}

	return redirect("/auth/log-in");
};

export default function SettingsLayout() {
	const { t } = useTranslation();
	const { pathname } = useLocation();

	return (
		<>
			<Nav variant="pills" justify fill className="flex-column flex-md-row">
				<Nav.Link active={pathname === "/settings"} as={Link} to="/settings">
					{t("settings.nav.general-information")}
				</Nav.Link>
				<Nav.Link
					active={pathname.startsWith("/settings/profile")}
					as={Link}
					to="/settings/profile"
				>
					{t("settings.nav.profile")}
				</Nav.Link>
				<Nav.Link
					active={pathname.startsWith("/settings/members")}
					as={Link}
					to="/settings/members"
				>
					{t("settings.nav.members")}
				</Nav.Link>
				<Nav.Link active={pathname.startsWith("/settings/auth")} as={Link} to="/settings/auth">
					{t("settings.nav.authentication")}
				</Nav.Link>
				<Nav.Link active={pathname === "/settings/export"} as={Link} to="/settings/export">
					{t("settings.nav.export")}
				</Nav.Link>
			</Nav>
			<div className="my-3">
				<Outlet />
			</div>
		</>
	);
}
