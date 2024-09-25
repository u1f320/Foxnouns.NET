import { ReactNode } from "react";
import { Nav, Navbar } from "react-bootstrap";
import { Link } from "@remix-run/react";
import Logo from "~/components/nav/Logo";

export default function BaseNavbar({ children }: { children?: ReactNode; }) {
	return (
		<Navbar expand="lg" className={`mb-4 mx-2`}>
			<Navbar.Brand to="/" as={Link}>
				<Logo />
			</Navbar.Brand>
			{children && (
				<>
					<Navbar.Toggle aria-controls="main-navbar" />
					<Navbar.Collapse id="main-navbar">
						<Nav className="ms-auto">{children}</Nav>
					</Navbar.Collapse>
				</>
			)}
		</Navbar>
	);
}
