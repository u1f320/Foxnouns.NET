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

export function Layout({ children }: { children: React.ReactNode }) {
	const { locale, settings } = useRouteLoaderData<typeof loader>("root") || {
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
			data-bs-theme={settings?.dark_mode ? "dark" : "light"}
			dir={i18n.dir()}
		>
			<head>
				<meta charSet="utf-8" />
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
				<title>
					<>{t("error.title")} - pronouns.cc</>
				</title>
				<MetaComponent />
				<Links />
			</head>
			<body>
				{errorElem}
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
			<h4>{t("error.heading")}</h4>
			<p>{errorDesc}</p>
		</>
	);
}

export default function App() {
	const { meta, meUser, settings } = useLoaderData<typeof loader>();

	return (
		<>
			<Navbar meta={meta} user={meUser} settings={settings} />
			<Container>
				<Outlet />
			</Container>
		</>
	);
}
