import { Link, useFetcher } from "@remix-run/react";
import Meta from "~/lib/api/meta";
import { User, UserSettings } from "~/lib/api/user";
import Logo from "./Logo";

import Nav from "react-bootstrap/Nav";
import Navbar from "react-bootstrap/Navbar";
import NavDropdown from "react-bootstrap/NavDropdown";
import { BrightnessHigh, BrightnessHighFill, MoonFill } from "react-bootstrap-icons";

export default function MainNavbar({
	user,
	settings,
}: {
	meta: Meta;
	user?: User;
	settings: UserSettings;
}) {
	const fetcher = useFetcher();

	const userMenu = user ? (
		<NavDropdown title={<>@{user.username}</>} align="end">
			<NavDropdown.Item as={Link} to={`/@${user.username}`}>
				View profile
			</NavDropdown.Item>
			<NavDropdown.Item as={Link} to="/settings">
				Settings
			</NavDropdown.Item>
			<NavDropdown.Divider />
			<NavDropdown.Item as={Link} to="/auth/logout">
				Log out
			</NavDropdown.Item>
		</NavDropdown>
	) : (
		<Nav.Link to="/auth/login" as={Link}>
			Log in or sign up
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
									<ThemeIcon /> Theme
								</>
							}
							align="end"
						>
							<NavDropdown.Item as="button" name="theme" value="auto" type="submit">
								Automatic
							</NavDropdown.Item>
							<NavDropdown.Item as="button" name="theme" value="dark" type="submit">
								Dark mode
							</NavDropdown.Item>
							<NavDropdown.Item as="button" name="theme" value="light" type="submit">
								Light mode
							</NavDropdown.Item>
						</NavDropdown>
					</fetcher.Form>
				</Nav>
			</Navbar.Collapse>
		</Navbar>
	);
}
