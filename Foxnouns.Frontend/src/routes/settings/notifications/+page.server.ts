import { apiRequest } from "$api";
import type { Notification } from "$api/models/moderation";
import { alertKey } from "$lib";

export const load = async ({ url, fetch, cookies }) => {
	const notifications = await apiRequest<Notification[]>("GET", "/notifications", {
		fetch,
		cookies,
	});
	return { notifications, alertKey: alertKey(url) };
};
