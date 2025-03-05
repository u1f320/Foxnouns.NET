import { clearToken, TOKEN_COOKIE_NAME } from "$lib";
import { apiRequest } from "$api";
import ApiError, { ErrorCode } from "$api/error";
import type { Meta, MeUser } from "$api/models";
import type { Notification } from "$api/models/moderation";
import log from "$lib/log";
import type { LayoutServerLoad } from "./$types";

export const load = (async ({ fetch, cookies }) => {
	let token: string | null = null;
	let meUser: MeUser | null = null;
	let unreadNotifications: boolean = false;
	if (cookies.get(TOKEN_COOKIE_NAME)) {
		try {
			meUser = await apiRequest<MeUser>("GET", "/users/@me", { fetch, cookies });
			token = cookies.get(TOKEN_COOKIE_NAME) || null;

			const notifications = await apiRequest<Notification[]>("GET", "/notifications", {
				fetch,
				cookies,
			});
			unreadNotifications = notifications.filter((n) => !n.acknowledged).length > 0;
		} catch (e) {
			if (e instanceof ApiError && e.code === ErrorCode.AuthenticationRequired) clearToken(cookies);
			else log.error("Could not fetch /users/@me and token has not expired:", e);
		}
	}

	const meta = await apiRequest<Meta>("GET", "/meta", { fetch, cookies });
	return { meta, meUser, token, unreadNotifications };
}) satisfies LayoutServerLoad;
