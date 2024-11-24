import { clearToken, TOKEN_COOKIE_NAME } from "$lib";
import { apiRequest } from "$api";
import ApiError, { ErrorCode } from "$api/error";
import type { Meta, User } from "$api/models";
import log from "$lib/log";
import type { LayoutServerLoad } from "./$types";

export const load = (async ({ fetch, cookies }) => {
	let meUser: User | null = null;
	if (cookies.get(TOKEN_COOKIE_NAME)) {
		try {
			meUser = await apiRequest<User>("GET", "/users/@me", { fetch, cookies });
		} catch (e) {
			if (e instanceof ApiError && e.code === ErrorCode.AuthenticationRequired) clearToken(cookies);
			else log.error("Could not fetch /users/@me and token has not expired:", e);
		}
	}

	const meta = await apiRequest<Meta>("GET", "/meta", { fetch, cookies });
	return { meta, meUser };
}) satisfies LayoutServerLoad;
