// place files you want to import through the `$lib` alias in this folder.

import type { Cookies } from "@sveltejs/kit";

export const TOKEN_COOKIE_NAME = "__Host-pronounscc-token";

export const setToken = (cookies: Cookies, token: string) =>
	cookies.set(TOKEN_COOKIE_NAME, token, { path: "/" });
export const clearToken = (cookies: Cookies) => cookies.delete(TOKEN_COOKIE_NAME, { path: "/" });

// TODO: change this to something we actually clearly have the rights to use
export const DEFAULT_AVATAR = "https://pronouns.cc/default/512.webp";
