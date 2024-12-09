import { apiRequest } from "$api";
import type { PrideFlag } from "$api/models/user";

export const load = async ({ fetch, cookies }) => {
	const flags = await apiRequest<PrideFlag[]>("GET", "/users/@me/flags", { fetch, cookies });
	return { flags };
};
