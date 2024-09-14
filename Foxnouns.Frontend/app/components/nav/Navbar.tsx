import { Link, useFetcher } from "@remix-run/react";
import Meta from "~/lib/api/meta";
import { User, UserSettings } from "~/lib/api/user";
import Logo from "./Logo";

import { Nav, Navbar, NavDropdown } from "react-bootstrap";
import { BrightnessHigh, BrightnessHighFill, MoonFill } from "react-bootstrap-icons";
import { useTranslation } from "react-i18next";

export default function MainNavbar({
	user,
	settings,
}: {
	meta: Meta;
	user?: User;
	settings: UserSettings;
}) {
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

	const ThemeIcon =
		settings.dark_mode === null
			? BrightnessHigh
			: settings.dark_mode
				? MoonFill
				: BrightnessHighFill;

	const theme = settings.dark_mode ? "dark" : "light";

	return (
		<Navbar expand="lg" className={`mb-4 mx-2 bg-${theme}`} color={theme} variant={theme}>
			<Navbar.Brand to="/" as={Link}>
				<Logo />
			</Navbar.Brand>
			<Navbar.Toggle aria-controls="main-navbar" />
			<Navbar.Collapse id="main-navbar">
				<Nav className="ms-auto">
					{userMenu}
					<fetcher.Form method="POST" action="/dark-mode">
						<NavDropdown
							title={
								<>
									<ThemeIcon /> {t("navbar.theme")}
								</>
							}
							align="end"
						>
							<NavDropdown.Item as="button" name="theme" value="auto" type="submit">
								{t("navbar.theme-auto")}
							</NavDropdown.Item>
							<NavDropdown.Item as="button" name="theme" value="dark" type="submit">
								{t("navbar.theme-dark")}
							</NavDropdown.Item>
							<NavDropdown.Item as="button" name="theme" value="light" type="submit">
								{t("navbar.theme-light")}
							</NavDropdown.Item>
						</NavDropdown>
					</fetcher.Form>
				</Nav>
			</Navbar.Collapse>
		</Navbar>
	);
}
