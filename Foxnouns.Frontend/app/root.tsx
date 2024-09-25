import {
	json,
	Links,
	Meta as MetaComponent,
	Outlet,
	Scripts,
	ScrollRestoration,
	useLoaderData,
	useRouteError,
	useRouteLoaderData,
} from "@remix-run/react";
import { LoaderFunctionArgs } from "@remix-run/node";
import { useChangeLanguage } from "remix-i18next/react";
import { useTranslation } from "react-i18next";

import serverRequest, { getCookie, writeCookie } from "./lib/request.server";
import Meta from "./lib/api/meta";
import Navbar from "./components/nav/Navbar";
import { User, UserSettings } from "./lib/api/user";
import { ApiError, ErrorCode } from "./lib/api/error";

import "./app.scss";
import getLocalSettings from "./lib/settings.server";
import { LANGUAGE } from "~/env.server";
import { errorCodeDesc } from "./components/ErrorAlert";
import { Container } from "react-bootstrap";
import { ReactNode } from "react";
import BaseNavbar from "~/components/nav/BaseNavbar";

export const loader = async ({ request }: LoaderFunctionArgs) => {
	const meta = await serverRequest<Meta>("GET", "/meta");

	const token = getCookie(request, "pronounscc-token");
	let setCookie = "";

	let meUser: User | undefined;
	let settings = getLocalSettings(request);
	if (token) {
		try {
			meUser = await serverRequest<User>("GET", "/users/@me", { token });

			settings = await serverRequest<UserSettings>("GET", "/users/@me/settings", { token });
		} catch (e) {
			// If we get an unauthorized error, clear the token, as it's not valid anymore.
			if ((e as ApiError).code === ErrorCode.AuthenticationRequired) {
				setCookie = writeCookie("pronounscc-token", token, 0);
			}
		}
	}

	return json(
		{ meta, meUser, settings, locale: LANGUAGE },
		{
			headers: { "Set-Cookie": setCookie },
		},
	);
};

export function Layout({ children }: { children: ReactNode }) {
	const { locale } = useRouteLoaderData<typeof loader>("root") || {
		meta: {
			users: {
				total: 0,
				active_month: 0,
				active_week: 0,
				active_day: 0,
			},
			members: 0,
			version: "",
			hash: "",
		},
	};
	const { i18n } = useTranslation();
	i18n.language = locale || "en";
	useChangeLanguage(locale || "en");

	return (
		<html
			lang={locale || "en"}
			dir={i18n.dir()}
		>
		<head>
			<meta charSet="utf-8" />
			<link rel="icon" type="image/svg+xml" href="/favicon.svg" />
			<meta name="viewport" content="width=device-width, initial-scale=1" />
			<MetaComponent />
			<Links />
		</head>
		<body>
		{children}
		<ScrollRestoration />
		<Scripts />
			</body>
		</html>
	);
}

export function ErrorBoundary() {
	const data = useRouteLoaderData<typeof loader>("root");

	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	const error: any = useRouteError();
	const { t } = useTranslation();

	console.log(error);

	const errorElem =
		"code" in error && "message" in error ? (
			<ApiErrorElem error={error as ApiError} />
		) : (
			<>{t("error.errors.generic-error")}</>
		);

	return (
		<html lang="en">
			<head>
				<title>{t("error.title")}</title>
				<MetaComponent />
				<Links />
			</head>
			<body>
				{data?.meUser && data?.meta ? (
					<Navbar meta={data.meta} user={data.meUser} />
				) : (
					<BaseNavbar />
				)}
				<Container>{errorElem}</Container>
				<Scripts />
			</body>
		</html>
	);
}

function ApiErrorElem({ error }: { error: ApiError }) {
	const { t } = useTranslation();
	const errorDesc = errorCodeDesc(t, error.code);

	return (
		<>
			<h1>{t("error.heading")}</h1>
			<p>{errorDesc}</p>
			<details>
				<summary>{t("error.more-info")}</summary>
				<pre>
					<code>{JSON.stringify(error, null, "  ")}</code>
				</pre>
			</details>
		</>
	);
}

export default function App() {
	const { meta, meUser } = useLoaderData<typeof loader>();

	return (
		<>
			<Navbar meta={meta} user={meUser} />
			<Container>
				<Outlet />
			</Container>
		</>
	);
}
