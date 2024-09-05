import {
	json,
	Links,
	Meta as MetaComponent,
	Outlet,
	Scripts,
	ScrollRestoration,
	useLoaderData,
} from "@remix-run/react";
import { LoaderFunction } from "@remix-run/node";
import SSRProvider from "react-bootstrap/SSRProvider";

import serverRequest, { getCookie, writeCookie } from "./lib/request.server";
import Meta from "./lib/api/meta";
import Navbar from "./components/nav/Navbar";
import { User, UserSettings } from "./lib/api/user";
import { ApiError, ErrorCode } from "./lib/api/error";

import "./app.scss";
import getLocalSettings from "./lib/settings.server";

export const loader: LoaderFunction = async ({ request }) => {
	const meta = await serverRequest<Meta>("GET", "/meta");

	const token = getCookie(request, "pronounscc-token");
	let setCookie = "";

	let meUser: User | undefined;
	let settings = getLocalSettings(request);
	if (token) {
		try {
			const user = await serverRequest<User>("GET", "/users/@me", { token });
			meUser = user;

			settings = await serverRequest<UserSettings>(
				"GET",
				"/users/@me/settings",
				{ token },
			);
		} catch (e) {
			// If we get an unauthorized error, clear the token, as it's not valid anymore.
			if ((e as ApiError).code === ErrorCode.AuthenticationRequired) {
				setCookie = writeCookie("pronounscc-token", token, 0);
			}
		}
	}

	return json(
		{ meta, meUser, settings },
		{
			headers: { "Set-Cookie": setCookie },
		},
	);
};

export function Layout({ children }: { children: React.ReactNode }) {
	const { settings } = useLoaderData<typeof loader>();

	return (
		<html lang="en" data-bs-theme={settings.dark_mode ? "dark" : "light"}>
			<head>
				<meta charSet="utf-8" />
				<meta name="viewport" content="width=device-width, initial-scale=1" />
				<MetaComponent />
				<Links />
			</head>
			<body>
				<SSRProvider>{children}</SSRProvider>
				<ScrollRestoration />
				<Scripts />
			</body>
		</html>
	);
}

export default function App() {
	const { meta, meUser, settings } = useLoaderData<typeof loader>();

	return (
		<>
			<Navbar meta={meta} user={meUser} settings={settings} />
			<Outlet />
		</>
	);
}
