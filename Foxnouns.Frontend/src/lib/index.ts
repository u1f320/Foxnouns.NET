// place files you want to import through the `$lib` alias in this folder.

import type { Cookies } from "@sveltejs/kit";
import { DateTime } from "luxon";

export const TOKEN_COOKIE_NAME = "__Host-pronounscc-token";

export const setToken = (cookies: Cookies, token: string) =>
	cookies.set(TOKEN_COOKIE_NAME, token, { path: "/" });
export const clearToken = (cookies: Cookies) => cookies.delete(TOKEN_COOKIE_NAME, { path: "/" });

export const DEFAULT_FLAG = "/unknown_flag.svg";

export const idTimestamp = (id: string) =>
	DateTime.fromMillis(parseInt(id, 10) / (1 << 22) + 1_640_995_200_000);

export const alertKey = (url: URL): string | undefined =>
	url.searchParams.has("alert") ? "alert." + url.searchParams.get("alert") : undefined;
