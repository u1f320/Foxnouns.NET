import { Link, useFetcher } from "@remix-run/react";
import Meta from "~/lib/api/meta";
import { User } from "~/lib/api/user";

import { Nav, NavDropdown } from "react-bootstrap";
import { useTranslation } from "react-i18next";
import BaseNavbar from "~/components/nav/BaseNavbar";

export default function MainNavbar({ user }: { meta: Meta; user?: User }) {
	const fetcher = useFetcher();
	const { t } = useTranslation();

	const userMenu = user ? (
		<NavDropdown title={<>@{user.username}</>} align="end">
			<NavDropdown.Item as={Link} to={`/@${user.username}`}>
				{t("navbar.view-profile")}
			</NavDropdown.Item>
			<NavDropdown.Item as={Link} to="/settings">
				{t("navbar.settings")}
			</NavDropdown.Item>
			<NavDropdown.Divider />
			<fetcher.Form method="POST" action="/auth/log-out">
				<NavDropdown.Item as="button" type="submit">
					{t("navbar.log-out")}
				</NavDropdown.Item>
			</fetcher.Form>
		</NavDropdown>
	) : (
		<Nav.Link to="/auth/log-in" as={Link}>
			{t("navbar.log-in")}
		</Nav.Link>
	);

	return <BaseNavbar>{userMenu}</BaseNavbar>;
}
